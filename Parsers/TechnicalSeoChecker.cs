// TechnicalSeoCheckerNet48.cs
// .NET Framework 4.8
//
// NuGet:
//   Install-Package HtmlAgilityPack
//
// Что делает:
// 1) Проверяет, что варианты главного URL (http/https, www/non-www, /, /index.html, /index.php) редиректятся ТОЛЬКО на заданный canonicalRoot.
// 2) Проверяет HTTP-статус главной (должен быть 200).
// 3) robots.txt: наличие, наличие Sitemap, корректность домена в Sitemap, корректность Host (если указан).
// 4) sitemap.xml: наличие и корректность домена в <loc>.
// 5) favicon.ico: наличие.
// 6) Canonical на главной: наличие и совпадение с финальным URL.
// 7) Meta robots: если есть — не должен содержать noindex/nofollow.
// 8) Заголовки ответа сервера (Content-Type/charset, Content-Encoding).
// 9) HTTPS/SSL: сертификат валиден (по умолчанию .NET) и нет mixed content (http-ресурсы на https-странице).
//
// Результат: текст на русском языке с найденными проблемами.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace TechnicalSeoCheckerNet48
{
    public sealed class TechnicalSeoChecker : IDisposable
    {
        private readonly HttpClient _http;
        private readonly bool _disposeHttp;

        public TechnicalSeoChecker(HttpClient httpClient = null)
        {
            if (httpClient == null)
            {
                // ВАЖНО: отключаем авто-редиректы, чтобы вручную видеть цепочки редиректов
                var handler = new HttpClientHandler
                {
                    AllowAutoRedirect = false,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };

                _http = new HttpClient(handler);
                _disposeHttp = true;
            }
            else
            {
                _http = httpClient;
                _disposeHttp = false;
            }

            _http.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// Синхронный запуск (если не хочешь async/await).
        /// </summary>
        public string Check(string canonicalRootUrl)
        {
            return CheckAsync(canonicalRootUrl).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Асинхронный запуск.
        /// canonicalRootUrl — эталонный URL главной, например: https://site.ru/
        /// </summary>
        public async Task<string> CheckAsync(string canonicalRootUrl, CancellationToken ct = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(canonicalRootUrl))
                throw new ArgumentException("canonicalRootUrl пустой.", nameof(canonicalRootUrl));

            var issues = new List<string>();

            // Нормализуем эталон
            var canonicalRoot = NormalizeUrl(canonicalRootUrl, forceTrailingSlash: true);

            Uri canonicalUri;
            if (!Uri.TryCreate(canonicalRoot, UriKind.Absolute, out canonicalUri))
                throw new ArgumentException("canonicalRootUrl не является абсолютным URL.", nameof(canonicalRootUrl));

            var hostExpected = canonicalUri.Host;
            var schemeExpected = canonicalUri.Scheme;

            // -----------------------------
            // 1) Проверка редиректов главной
            // -----------------------------
            var variants = BuildRootVariants(canonicalUri);

            foreach (var variant in variants)
            {
                var chain = await FollowRedirectsAsync(variant, maxHops: 10, ct: ct).ConfigureAwait(false);

                if (chain.Error != null)
                {
                    issues.Add($"[Редиректы] Не удалось проверить вариант {variant} — ошибка: {chain.Error}");
                    continue;
                }

                var finalUrl = chain.FinalUrl;
                var finalNorm = NormalizeUrl(finalUrl, forceTrailingSlash: true);

                if (!string.Equals(finalNorm, canonicalRoot, StringComparison.OrdinalIgnoreCase))
                {
                    // если не совпало — пишем, что именно не привелось к canonicalRoot
                    issues.Add($"[Редиректы] Вариант {variant} ведёт на {finalUrl}, но должен вести на {canonicalRoot}. " +
                               $"Цепочка: {string.Join(" -> ", chain.Chain)}");
                }

                // Цепочки редиректов: если больше 1 редиректа — это уже не идеально
                var hops = chain.Chain.Count - 1;
                if (hops > 5)
                {
                    issues.Add($"[Редиректы] Длинная цепочка редиректов для {variant}: {string.Join(" -> ", chain.Chain)}. " +
                               $"Желательно 0–1 редирект.");
                }

                // 302/307 — нежелательно для каноникализации
                if (chain.RedirectStatuses.Any(s =>  s == 307))
                {
                    issues.Add($"[Редиректы] Для {variant} используются временные редиректы (307). Для каноникализации лучше 301.");
                }
            }

            // -----------------------------
            // 2) HTTP-статус главной (после редиректов)
            // -----------------------------
            var mainChain = await FollowRedirectsAsync(canonicalRoot, maxHops: 10, ct: ct).ConfigureAwait(false);
            if (mainChain.Error != null)
            {
                issues.Add($"[Главная] Не удалось открыть главную {canonicalRoot}: {mainChain.Error}");
                return BuildReport(canonicalRoot, issues);
            }

            // GET главной, чтобы получить HTML/заголовки
            HttpResponseMessage mainResp = null;
            string mainHtml = null;

            try
            {
                mainResp = await SendAsync(mainChain.FinalUrl, HttpMethod.Get, ct).ConfigureAwait(false);
                if ((int)mainResp.StatusCode != 200)
                {
                    issues.Add($"[Главная] Статус главной страницы должен быть 200, сейчас {(int)mainResp.StatusCode} ({mainResp.ReasonPhrase}). URL: {mainChain.FinalUrl}");
                }

                mainHtml = await mainResp.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (AuthenticationException ex)
            {
                issues.Add($"[SSL] Ошибка проверки сертификата (AuthenticationException) при открытии {mainChain.FinalUrl}: {ex.Message}");
            }
            catch (Exception ex)
            {
                issues.Add($"[Главная] Ошибка при загрузке главной {mainChain.FinalUrl}: {ex.Message}");
            }

            // -----------------------------
            // 3) robots.txt: наличие + Sitemap + Host (если есть)
            // -----------------------------
            var robotsUrl = schemeExpected + "://" + hostExpected + "/robots.txt";
            try
            {
                var robotsResp = await SendAsync(robotsUrl, HttpMethod.Get, ct).ConfigureAwait(false);
                var robotsBody = await robotsResp.Content.ReadAsStringAsync().ConfigureAwait(false);

                if ((int)robotsResp.StatusCode != 200)
                {
                    issues.Add($"[robots.txt] robots.txt не найден или недоступен. URL: {robotsUrl}, статус {(int)robotsResp.StatusCode}.");
                }
                else
                {
                    // Ищем Sitemap:
                    var sitemapLines = ExtractRobotsDirectives(robotsBody, "sitemap");
                    if (sitemapLines.Count == 0)
                    {
                        issues.Add("[robots.txt] В robots.txt не указан Sitemap.");
                    }
                    else
                    {
                        // Проверяем, что Sitemap ведёт на правильный домен
                        foreach (var sm in sitemapLines)
                        {
                            Uri smUri;
                            if (Uri.TryCreate(sm, UriKind.Absolute, out smUri))
                            {
                                if (!string.Equals(smUri.Host, hostExpected, StringComparison.OrdinalIgnoreCase))
                                {
                                    issues.Add($"[robots.txt] Sitemap указывает на другой домен: {sm} (ожидается домен {hostExpected}).");
                                }
                            }
                        }
                    }

                    // Host: (если есть)
                    var hostLines = ExtractRobotsDirectives(robotsBody, "host");
                    if (hostLines.Count > 0)
                    {
                        foreach (var h in hostLines)
                        {
                            // Host может быть "site.ru" или "www.site.ru"
                            var clean = h.Trim().TrimEnd('/');
                            if (!string.Equals(clean, hostExpected, StringComparison.OrdinalIgnoreCase))
                            {
                                issues.Add($"[robots.txt] Host указан как {clean}, но ожидается {hostExpected}.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                issues.Add($"[robots.txt] Ошибка при проверке robots.txt ({robotsUrl}): {ex.Message}");
            }

            // -----------------------------
            // 4) sitemap.xml: наличие + корректный домен в <loc>
            // -----------------------------
            var sitemapUrl = schemeExpected + "://" + hostExpected + "/sitemap.xml";
            try
            {
                var smChain = await FollowRedirectsAsync(sitemapUrl, maxHops: 10, ct: ct).ConfigureAwait(false);
                if (smChain.Error != null)
                {
                    issues.Add($"[sitemap.xml] Не удалось проверить {sitemapUrl}: {smChain.Error}");
                }
                else
                {
                    var smResp = await SendAsync(smChain.FinalUrl, HttpMethod.Get, ct).ConfigureAwait(false);
                    var smBody = await smResp.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if ((int)smResp.StatusCode != 200)
                    {
                        issues.Add($"[sitemap.xml] sitemap.xml не найден или недоступен. URL: {sitemapUrl}, статус {(int)smResp.StatusCode}.");
                    }
                    else
                    {
                        // Быстрая проверка домена в <loc>
                        var locUrls = ExtractXmlLocUrls(smBody, maxCount: 200); // ограничим, чтобы не тащить огромный сайт
                        if (locUrls.Count == 0)
                        {
                            issues.Add("[sitemap.xml] Не удалось найти <loc> в sitemap.xml (возможно это не sitemap или нестандартный формат).");
                        }
                        else
                        {
                            // Если в loc встречаются абсолютные URL, проверяем домен
                            var wrong = locUrls
                                .Select(u => TryParseUri(u))
                                .Where(u => u != null)
                                .Where(u => !string.Equals(u.Host, hostExpected, StringComparison.OrdinalIgnoreCase))
                                .Select(u => u.ToString())
                                .Take(10)
                                .ToList();

                            if (wrong.Count > 0)
                            {
                                issues.Add($"[sitemap.xml] В sitemap.xml обнаружены URL с другим доменом (первые примеры): {string.Join(", ", wrong)}. Ожидается домен {hostExpected}.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                issues.Add($"[sitemap.xml] Ошибка при проверке sitemap.xml ({sitemapUrl}): {ex.Message}");
            }

            // -----------------------------
            // 5) favicon.ico: наличие
            // -----------------------------
            var faviconUrl = schemeExpected + "://" + hostExpected + "/favicon.ico";
            try
            {
                var favChain = await FollowRedirectsAsync(faviconUrl, maxHops: 10, ct: ct).ConfigureAwait(false);
                if (favChain.Error != null)
                {
                    issues.Add($"[favicon] Не удалось проверить favicon.ico ({faviconUrl}): {favChain.Error}");
                }
                else
                {
                    var favResp = await SendAsync(favChain.FinalUrl, HttpMethod.Get, ct).ConfigureAwait(false);
                    if ((int)favResp.StatusCode != 200)
                    {
                        issues.Add($"[favicon] favicon.ico не найден или недоступен. URL: {faviconUrl}, статус {(int)favResp.StatusCode}.");
                    }
                }
            }
            catch (Exception ex)
            {
                issues.Add($"[favicon] Ошибка при проверке favicon.ico ({faviconUrl}): {ex.Message}");
            }

            // Если HTML не загрузили — дальше проверки 6/7/8/9 по HTML не сделать
            if (string.IsNullOrWhiteSpace(mainHtml))
            {
                issues.Add("[Главная] Не удалось получить HTML главной страницы — проверки canonical/meta robots/mixed content могут быть неполными.");
                return BuildReport(canonicalRoot, issues);
            }

            // -----------------------------
            // 6) Canonical на главной
            // -----------------------------
            try
            {
                var canonicalTag = ExtractCanonicalHref(mainHtml, mainChain.FinalUrl);
                if (string.IsNullOrWhiteSpace(canonicalTag))
                {
                    issues.Add("[Canonical] На главной странице не найден <link rel=\"canonical\" ...>.");
                }
                else
                {
                    var canonicalTagNorm = NormalizeUrl(canonicalTag, forceTrailingSlash: true);
                    var finalMainNorm = NormalizeUrl(mainChain.FinalUrl, forceTrailingSlash: true);

                    if (!string.Equals(canonicalTagNorm, finalMainNorm, StringComparison.OrdinalIgnoreCase))
                    {
                        issues.Add($"[Canonical] canonical не совпадает с финальным URL. canonical={canonicalTagNorm}, final={finalMainNorm}.");
                    }
                }
            }
            catch (Exception ex)
            {
                issues.Add($"[Canonical] Ошибка при проверке canonical: {ex.Message}");
            }

            // -----------------------------
            // 7) Meta robots: нет noindex/nofollow
            // -----------------------------
            try
            {
                var metaRobots = ExtractMetaRobots(mainHtml);
                if (!string.IsNullOrWhiteSpace(metaRobots))
                {
                    var lower = metaRobots.ToLowerInvariant();
                    if (lower.Contains("noindex") || lower.Contains("nofollow"))
                    {
                        issues.Add($"[Meta robots] На главной странице обнаружено ограничение индексации: meta robots=\"{metaRobots}\" (не должно содержать noindex/nofollow).");
                    }
                }
            }
            catch (Exception ex)
            {
                issues.Add($"[Meta robots] Ошибка при проверке meta robots: {ex.Message}");
            }

            // -----------------------------
            // 8) Заголовки ответа сервера
            // -----------------------------
            try
            {
                // Content-Type
                var ctHeader = mainResp?.Content?.Headers?.ContentType;
                if (ctHeader == null)
                {
                    issues.Add("[Headers] Не удалось определить Content-Type у главной страницы.");
                }
                else
                {
                    var media = (ctHeader.MediaType ?? "").ToLowerInvariant();
                    if (!media.Contains("text/html"))
                    {
                        issues.Add($"[Headers] Content-Type главной страницы неожиданный: {ctHeader}. Ожидается text/html.");
                    }

                    // charset
                    if (string.IsNullOrWhiteSpace(ctHeader.CharSet))
                    {
                        issues.Add("[Headers] У главной страницы не указан charset в Content-Type (желательно UTF-8).");
                    }
                }

                // Content-Encoding (gzip/br)
                var compressionOk = await IsCompressionEnabledAsync(mainChain.FinalUrl, ct).ConfigureAwait(false);
                if (!compressionOk)
                {
                    issues.Add("[Headers] Не обнаружено сжатие ответа (gzip/br). Сервер не вернул Content-Encoding при запросе Accept-Encoding.");
                }
            }
            catch (Exception ex)
            {
                issues.Add($"[Headers] Ошибка при проверке заголовков: {ex.Message}");
            }

            // -----------------------------
            // 9) HTTPS/SSL + mixed content
            // -----------------------------
            try
            {
                if (!string.Equals(new Uri(mainChain.FinalUrl).Scheme, "https", StringComparison.OrdinalIgnoreCase))
                {
                    issues.Add($"[HTTPS] Финальный URL главной не HTTPS: {mainChain.FinalUrl}. Ожидается https.");
                }

                // Mixed content: ищем http:// в src/href/action/poster и т.п.
                var mixed = ExtractMixedContentUrls(mainHtml)
                    .Where(u => u.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(20)
                    .ToList();

                if (mixed.Count > 0)
                {
                    issues.Add($"[Mixed content] На HTTPS-странице найдены ресурсы по HTTP (первые примеры): {string.Join(", ", mixed)}. Это нужно заменить на https:// или относительные URL.");
                }
            }
            catch (Exception ex)
            {
                issues.Add($"[HTTPS/Mixed] Ошибка при проверке mixed content: {ex.Message}");
            }

            return BuildReport(canonicalRoot, issues);
        }

        // -----------------------------
        // Helpers
        // -----------------------------

        private async Task<HttpResponseMessage> SendAsync(string url, HttpMethod method, CancellationToken ct)
        {
            var req = new HttpRequestMessage(method, url);
            req.Headers.Accept.ParseAdd("*/*");

            // Просим сжатие явно (иначе сервер может не отдавать Content-Encoding)
            req.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");

            return await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct)
                             .ConfigureAwait(false);
        }

        private sealed class RedirectTrace
        {
            public List<string> Chain { get; set; } = new List<string>();
            public List<int> RedirectStatuses { get; set; } = new List<int>();
            public string FinalUrl { get; set; }
            public string Error { get; set; }
        }

        private async Task<RedirectTrace> FollowRedirectsAsync(string startUrl, int maxHops, CancellationToken ct)
        {
            var trace = new RedirectTrace();
            string current = startUrl;
            trace.Chain.Add(current);

            try
            {
                for (int i = 0; i < maxHops; i++)
                {
                    using (var resp = await SendAsync(current, HttpMethod.Get, ct).ConfigureAwait(false))
                    {
                        var code = (int)resp.StatusCode;

                        if (code >= 300 && code < 400)
                        {
                            trace.RedirectStatuses.Add(code);

                            // Location
                            var loc = resp.Headers.Location;
                            if (loc == null)
                            {
                                trace.Error = "Редирект без заголовка Location.";
                                trace.FinalUrl = current;
                                return trace;
                            }

                            // HttpClient иногда даёт Location как relative — приводим к absolute
                            Uri next;
                            if (loc.IsAbsoluteUri)
                                next = loc;
                            else
                                next = new Uri(new Uri(current), loc);

                            current = next.ToString();
                            trace.Chain.Add(current);
                            continue;
                        }

                        trace.FinalUrl = current;
                        return trace;
                    }
                }

                trace.Error = "Превышен лимит редиректов (возможен цикл).";
                trace.FinalUrl = current;
                return trace;
            }
            catch (AuthenticationException ex)
            {
                trace.Error = "AuthenticationException (SSL): " + ex.Message;
                trace.FinalUrl = current;
                return trace;
            }
            catch (Exception ex)
            {
                trace.Error = ex.Message;
                trace.FinalUrl = current;
                return trace;
            }
        }

        private static List<string> BuildRootVariants(Uri canonicalUri)
        {
            // canonicalUri — эталон. Строим варианты:
            // http/https + www/non-www + / + index.html + index.php
            var variants = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var host = canonicalUri.Host;
            var hasWww = host.StartsWith("www.", StringComparison.OrdinalIgnoreCase);

            var hostWww = hasWww ? host : "www." + host;
            var hostNonWww = hasWww ? host.Substring(4) : host;

            var schemes = new[] { "http", "https" };
            var hosts = new[] { hostNonWww, hostWww };

            foreach (var sch in schemes)
            {
                foreach (var h in hosts)
                {
                    variants.Add($"{sch}://{h}/");
                    variants.Add($"{sch}://{h}");
                    variants.Add($"{sch}://{h}/index.html");
                }
            }

            // Возвращаем список (стабильный порядок)
            return variants.OrderBy(v => v).ToList();
        }

        private static string NormalizeUrl(string url, bool forceTrailingSlash)
        {
            Uri u;
            if (!Uri.TryCreate(url, UriKind.Absolute, out u))
                return url;

            // Приводим:
            // - убираем #fragment
            // - оставляем path как есть, но если forceTrailingSlash и путь пустой => "/"
            // - без query для корня? (для главной лучше без query)
            var builder = new UriBuilder(u)
            {
                Fragment = ""
            };

            if (forceTrailingSlash)
            {
                if (string.IsNullOrEmpty(builder.Path) || builder.Path == "/")
                    builder.Path = "/";
                else if (!builder.Path.EndsWith("/"))
                {
                    // Для корня не трогаем, для страниц не заставляем слэш — тут используем только для root
                    // (в нашем чекере forceTrailingSlash применяется только для главной)
                }
            }

            // Для главной не хотим query
            if (builder.Path == "/")
                builder.Query = "";

            return builder.Uri.ToString().TrimEnd(); // не TrimEnd('/') — иначе сломаем "/"
        }

        private static List<string> ExtractRobotsDirectives(string robotsBody, string directiveName)
        {
            var list = new List<string>();
            if (string.IsNullOrWhiteSpace(robotsBody)) return list;

            var lines = robotsBody.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var raw in lines)
            {
                var line = raw.Trim();

                if (line.StartsWith("#")) continue;

                // Sitemap: xxx
                var idx = line.IndexOf(':');
                if (idx <= 0) continue;

                var name = line.Substring(0, idx).Trim().ToLowerInvariant();
                if (!string.Equals(name, directiveName.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
                    continue;

                var value = line.Substring(idx + 1).Trim();
                if (!string.IsNullOrWhiteSpace(value))
                    list.Add(value);
            }

            return list;
        }

        private static List<string> ExtractXmlLocUrls(string xml, int maxCount)
        {
            // Лёгкий парсер: ищем <loc>...</loc>
            var list = new List<string>();
            if (string.IsNullOrWhiteSpace(xml)) return list;

            foreach (Match m in Regex.Matches(xml, @"<loc>\s*(.*?)\s*</loc>", RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                var val = WebUtility.HtmlDecode(m.Groups[1].Value.Trim());
                if (!string.IsNullOrWhiteSpace(val))
                {
                    list.Add(val);
                    if (list.Count >= maxCount) break;
                }
            }
            return list;
        }

        private static Uri TryParseUri(string s)
        {
            Uri u;
            return Uri.TryCreate(s, UriKind.Absolute, out u) ? u : null;
        }

        private static string ExtractCanonicalHref(string html, string baseUrl)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var node = doc.DocumentNode
                .SelectSingleNode("//link[translate(@rel,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='canonical']");

            if (node == null) return null;

            var href = node.GetAttributeValue("href", null);
            if (string.IsNullOrWhiteSpace(href)) return null;

            // canonical может быть относительным
            Uri abs;
            if (Uri.TryCreate(href, UriKind.Absolute, out abs))
                return abs.ToString();

            var baseUri = new Uri(baseUrl);
            abs = new Uri(baseUri, href);
            return abs.ToString();
        }

        private static string ExtractMetaRobots(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Проверим и robots, и googlebot (часто встречается)
            var nodes = doc.DocumentNode.SelectNodes("//meta");
            if (nodes == null) return null;

            foreach (var n in nodes)
            {
                var name = (n.GetAttributeValue("name", "") ?? "").Trim().ToLowerInvariant();
                if (name == "robots" || name == "googlebot")
                {
                    var content = n.GetAttributeValue("content", null);
                    if (!string.IsNullOrWhiteSpace(content))
                        return content.Trim();
                }
            }
            return null;
        }

        private static IEnumerable<string> ExtractMixedContentUrls(string html)
        {
            // Достаём src/href/action/poster/data и т.п.
            // HtmlAgilityPack: выберем все атрибуты и заберём значения, где есть http://
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var attrs = new[] { "src", "href", "action", "poster", "data" };

            foreach (var node in doc.DocumentNode.Descendants())
            {
                if (node.Attributes == null || node.Attributes.Count == 0) continue;

                foreach (var a in attrs)
                {
                    var val = node.GetAttributeValue(a, null);
                    if (string.IsNullOrWhiteSpace(val)) continue;

                    var v = val.Trim();

                    // Пропустим mailto/tel/javascript/data:
                    if (v.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)) continue;
                    if (v.StartsWith("tel:", StringComparison.OrdinalIgnoreCase)) continue;
                    if (v.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase)) continue;
                    if (v.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) continue;

                    if (v.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                        yield return v;
                }
            }

            // Дополнительно: иногда http:// встречается в CSS/inline scripts
            // Но это уже более тяжело; при желании можно расширить.
        }

        private static string BuildReport(string canonicalRoot, List<string> issues)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Техническая проверка сайта");
            sb.AppendLine("Эталонная главная (canonical root): " + canonicalRoot);
            sb.AppendLine();

            if (issues == null || issues.Count == 0)
            {
                sb.AppendLine("Проблем не обнаружено по заданному чек-листу.");
                return sb.ToString();
            }

            sb.AppendLine("Обнаруженные проблемы:");
            foreach (var i in issues.Distinct())
                sb.AppendLine(" - " + i);

            return sb.ToString();
        }

        public void Dispose()
        {
            if (_disposeHttp) _http.Dispose();
        }

        private async Task<bool> IsCompressionEnabledAsync(string url, CancellationToken ct)
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.None // важно!
            };

            using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) })
            {
                // Идем по редиректам до финального URL
                var chain = await FollowRedirectsWithClientAsync(client, url, 10, ct).ConfigureAwait(false);
                if (chain.Error != null) return false;

                using (var req = new HttpRequestMessage(HttpMethod.Get, chain.FinalUrl))
                {
                    req.Headers.TryAddWithoutValidation("Accept-Encoding", "br, gzip, deflate");

                    using (var resp = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false))
                    {
                        // Проверяем Content-Encoding в "сыром" виде
                        IEnumerable<string> encVals;
                        var enc = new List<string>();

                        if (resp.Headers.TryGetValues("Content-Encoding", out encVals)) enc.AddRange(encVals);
                        if (resp.Content != null && resp.Content.Headers.TryGetValues("Content-Encoding", out encVals)) enc.AddRange(encVals);

                        var encAll = string.Join(",", enc).ToLowerInvariant();

                        return encAll.Contains("gzip") || encAll.Contains("br") || encAll.Contains("deflate");
                    }
                }
            }
        }

        // Вариант FollowRedirects, который использует переданный HttpClient (чтобы не плодить код)
        private async Task<RedirectTrace> FollowRedirectsWithClientAsync(HttpClient client, string startUrl, int maxHops, CancellationToken ct)
        {
            var trace = new RedirectTrace();
            string current = startUrl;
            trace.Chain.Add(current);

            try
            {
                for (int i = 0; i < maxHops; i++)
                {
                    using (var resp = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, current),
                                                             HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false))
                    {
                        var code = (int)resp.StatusCode;

                        if (code >= 300 && code < 400)
                        {
                            trace.RedirectStatuses.Add(code);
                            var loc = resp.Headers.Location;
                            if (loc == null)
                            {
                                trace.Error = "Редирект без заголовка Location.";
                                trace.FinalUrl = current;
                                return trace;
                            }

                            Uri next = loc.IsAbsoluteUri ? loc : new Uri(new Uri(current), loc);
                            current = next.ToString();
                            trace.Chain.Add(current);
                            continue;
                        }

                        trace.FinalUrl = current;
                        return trace;
                    }
                }

                trace.Error = "Превышен лимит редиректов (возможен цикл).";
                trace.FinalUrl = current;
                return trace;
            }
            catch (Exception ex)
            {
                trace.Error = ex.Message;
                trace.FinalUrl = current;
                return trace;
            }
        }

    }
}
