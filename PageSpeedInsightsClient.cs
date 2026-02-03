// PsiMetricsNet48.cs
// .NET Framework 4.8
// NuGet: Newtonsoft.Json (Json.NET)
// Install-Package Newtonsoft.Json

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace PsiMetricsNet48
{
    public enum Strategy
    {
        Mobile,
        Desktop
    }

    /// <summary>
    /// Метрики PageSpeed Insights (Lighthouse) для одной страницы и одной стратегии (mobile/desktop).
    /// В комментариях: 
    ///  - "Колонка" = короткое имя для столбца в таблице
    ///  - "Лучше/хуже" = направление оптимизации
    ///  - Пороги = ориентиры (синтетика Lighthouse может немного плавать)
    /// </summary>
    public sealed class PageSpeedMetrics
    {
        // -----------------------------
        // Идентификация / служебные
        // -----------------------------

        /// <summary>
        /// Исходный URL, для которого собирались метрики.
        /// Колонка: url
        /// Лучше/хуже: не применимо
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Стратегия теста: Mobile или Desktop.
        /// Колонка: strat
        /// Лучше/хуже: не применимо
        /// </summary>
        public Strategy Strategy { get; set; }

        /// <summary>
        /// Время, когда Google прогнал Lighthouse (из lighthouseResult.fetchTime).
        /// Колонка: fetch_time
        /// Лучше/хуже: не применимо (нужно для контроля "свежести" и разброса)
        /// </summary>
        public DateTimeOffset? FetchTime { get; set; }

        // -----------------------------
        // Категории (score 0..1)
        // -----------------------------

        /// <summary>
        /// Lighthouse Performance score (0..1).
        /// Колонка: perf_score
        /// Лучше/хуже: больше = лучше (1.0 идеально)
        /// Ориентиры: 0.90+ хорошо, 0.50-0.89 средне, <0.50 плохо
        /// </summary>
        public double? PerformanceScore { get; set; }

        /// <summary>
        /// Lighthouse SEO score (0..1).
        /// Колонка: seo_score
        /// Лучше/хуже: больше = лучше
        /// Замечание: это не "ранжирование", а чеклист базовых SEO-ошибок.
        /// </summary>
        public double? SeoScore { get; set; }

        /// <summary>
        /// Lighthouse Best Practices score (0..1).
        /// Колонка: bp_score
        /// Лучше/хуже: больше = лучше
        /// </summary>
        public double? BestPracticesScore { get; set; }

        /// <summary>
        /// Lighthouse Accessibility score (0..1).
        /// Колонка: a11y_score
        /// Лучше/хуже: больше = лучше
        /// </summary>
        public double? AccessibilityScore { get; set; }

        // -----------------------------
        // Core / скорость (numericValue)
        // -----------------------------

        /// <summary>
        /// LCP (Largest Contentful Paint), мс.
        /// Колонка: lcp_ms
        /// Лучше/хуже: меньше = лучше
        /// Ориентиры (в основном для field, но полезно и тут): <=2500 хорошо, 2500-4000 надо улучшить, >4000 плохо
        /// </summary>
        public double? LargestContentfulPaintMs { get; set; }

        /// <summary>
        /// CLS (Cumulative Layout Shift), безразмерный.
        /// Колонка: cls
        /// Лучше/хуже: меньше = лучше
        /// Ориентиры: <=0.10 хорошо, 0.10-0.25 средне, >0.25 плохо
        /// </summary>
        public double? CumulativeLayoutShift { get; set; }

        /// <summary>
        /// INP (Interaction to Next Paint), мс. Может отсутствовать в некоторых прогонах.
        /// Колонка: inp_ms
        /// Лучше/хуже: меньше = лучше
        /// Ориентиры: <=200 хорошо, 200-500 средне, >500 плохо
        /// </summary>
        public double? InteractionToNextPaintMs { get; set; }

        /// <summary>
        /// TBT (Total Blocking Time), мс (часто есть почти всегда).
        /// Колонка: tbt_ms
        /// Лучше/хуже: меньше = лучше
        /// Ориентиры: <=200 хорошо, 200-600 средне, >600 плохо
        /// </summary>
        public double? TotalBlockingTimeMs { get; set; }

        /// <summary>
        /// server-response-time, мс (приближённо про "TTFB" со стороны сервера).
        /// Колонка: ttfb_ms
        /// Лучше/хуже: меньше = лучше
        /// Ориентиры: <=600 хорошо, 600-1000 средне, >1000 плохо
        /// </summary>
        public double? ServerResponseTimeMs { get; set; }

        /// <summary>
        /// FCP (First Contentful Paint), мс.
        /// Колонка: fcp_ms
        /// Лучше/хуже: меньше = лучше
        /// Ориентиры: <=1800 хорошо, 1800-3000 средне, >3000 плохо
        /// </summary>
        public double? FirstContentfulPaintMs { get; set; }

        /// <summary>
        /// Speed Index, мс.
        /// Колонка: si_ms
        /// Лучше/хуже: меньше = лучше
        /// Ориентиры: <=3400 хорошо, 3400-5800 средне, >5800 плохо
        /// </summary>
        public double? SpeedIndexMs { get; set; }

        // -----------------------------
        // Вес/запросы
        // -----------------------------

        /// <summary>
        /// total-byte-weight, байты (вес загрузки страницы по Lighthouse).
        /// Колонка: bytes
        /// Лучше/хуже: меньше = лучше
        /// Замечание: для mobile желательно держать как можно ниже (часто важнее, чем на desktop).
        /// </summary>
        public double? TotalByteWeight { get; set; }

        /// <summary>
        /// Количество сетевых запросов (network-requests.details.items.length).
        /// Колонка: req_cnt
        /// Лучше/хуже: меньше = лучше (обычно меньше запросов => быстрее)
        /// </summary>
        public int? NetworkRequestsCount { get; set; }

        // -----------------------------
        // Потенциальная экономия (savings)
        // Важно: "SavingsBytes" — это сколько можно сэкономить.
        // Для таблицы: меньше = лучше (меньше "лишнего").
        // -----------------------------

        /// <summary>
        /// unused-javascript.details.overallSavingsBytes — неиспользуемый JS, байты (потенциальная экономия).
        /// Колонка: unused_js_b
        /// Лучше/хуже: меньше = лучше (0 = идеально)
        /// </summary>
        public double? UnusedJavaScriptSavingsBytes { get; set; }

        /// <summary>
        /// unused-css-rules.details.overallSavingsBytes — неиспользуемый CSS, байты (потенциальная экономия).
        /// Колонка: unused_css_b
        /// Лучше/хуже: меньше = лучше (0 = идеально)
        /// </summary>
        public double? UnusedCssSavingsBytes { get; set; }

        /// <summary>
        /// offscreen-images.details.overallSavingsBytes — изображения "ниже первого экрана" без lazy-load/оптимизации, байты (экономия).
        /// Колонка: offscr_img_b
        /// Лучше/хуже: меньше = лучше
        /// </summary>
        public double? OffscreenImagesSavingsBytes { get; set; }

        /// <summary>
        /// modern-image-formats.details.overallSavingsBytes — экономия при переходе на современные форматы (WebP/AVIF), байты.
        /// Колонка: modern_img_b
        /// Лучше/хуже: меньше = лучше
        /// </summary>
        public double? ModernImageFormatsSavingsBytes { get; set; }

        /// <summary>
        /// uses-optimized-images.details.overallSavingsBytes — экономия на оптимизации изображений (сжатие/размеры), байты.
        /// Колонка: opt_img_b
        /// Лучше/хуже: меньше = лучше
        /// </summary>
        public double? UsesOptimizedImagesSavingsBytes { get; set; }
    }

    public sealed class PageSpeedInsightsClient : IDisposable
    {
        private readonly HttpClient _http;
        private readonly bool _disposeHttp;
        private readonly string _apiKey;

        public PageSpeedInsightsClient(string apiKey, HttpClient httpClient = null)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API key пустой.", nameof(apiKey));

            _apiKey = apiKey;

            if (httpClient == null)
            {
                _http = new HttpClient();
                _disposeHttp = true;
            }
            else
            {
                _http = httpClient;
                _disposeHttp = false;
            }
        }

        /// <summary>
        /// Получить метрики PSI/Lighthouse по URL.
        /// Запрашиваем категории: performance, seo, best-practices, accessibility.
        /// </summary>
        public async Task<PageSpeedMetrics> GetMetricsAsync(
            string url,
            Strategy strategy,
            string locale = "ru",
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL пустой.", nameof(url));

            var requestUrl = BuildRequestUrl(url, strategy, locale);

            using (var req = new HttpRequestMessage(HttpMethod.Get, requestUrl))
            {
                req.Headers.Accept.ParseAdd("application/json");

                CancellationToken token = cancellationToken;
                CancellationTokenSource linkedCts = null;

                if (timeout.HasValue)
                {
                    linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    linkedCts.CancelAfter(timeout.Value);
                    token = linkedCts.Token;
                }

                try
                {
                    using (var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false))
                    {
                        var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

                        if (!resp.IsSuccessStatusCode)
                        {
                            var msg = TryExtractErrorMessage(body) ?? ("HTTP " + (int)resp.StatusCode + " " + resp.ReasonPhrase);
                            throw new HttpRequestException("PageSpeed Insights API error: " + msg);
                        }

                        var root = JObject.Parse(body);

                        // lighthouseResult
                        var lh = root["lighthouseResult"] as JObject;

                        var metrics = new PageSpeedMetrics
                        {
                            Url = url,
                            Strategy = strategy,
                            FetchTime = GetDateTimeOffset(lh, "fetchTime"),

                            PerformanceScore = GetDouble(lh, "categories.performance.score"),
                            SeoScore = GetDouble(lh, "categories.seo.score"),
                            BestPracticesScore = GetDouble(lh, "categories.best-practices.score"),
                            AccessibilityScore = GetDouble(lh, "categories.accessibility.score"),

                            LargestContentfulPaintMs = GetDouble(lh, "audits.largest-contentful-paint.numericValue"),
                            CumulativeLayoutShift = GetDouble(lh, "audits.cumulative-layout-shift.numericValue"),
                            InteractionToNextPaintMs = GetDouble(lh, "audits.interaction-to-next-paint.numericValue"),
                            TotalBlockingTimeMs = GetDouble(lh, "audits.total-blocking-time.numericValue"),
                            ServerResponseTimeMs = GetDouble(lh, "audits.server-response-time.numericValue"),
                            FirstContentfulPaintMs = GetDouble(lh, "audits.first-contentful-paint.numericValue"),
                            SpeedIndexMs = GetDouble(lh, "audits.speed-index.numericValue"),

                            TotalByteWeight = GetDouble(lh, "audits.total-byte-weight.numericValue"),

                            NetworkRequestsCount = GetNetworkRequestsCount(lh),

                            UnusedJavaScriptSavingsBytes = GetDouble(lh, "audits.unused-javascript.details.overallSavingsBytes"),
                            UnusedCssSavingsBytes = GetDouble(lh, "audits.unused-css-rules.details.overallSavingsBytes"),

                            OffscreenImagesSavingsBytes = GetDouble(lh, "audits.offscreen-images.details.overallSavingsBytes"),
                            ModernImageFormatsSavingsBytes = GetDouble(lh, "audits.modern-image-formats.details.overallSavingsBytes"),
                            UsesOptimizedImagesSavingsBytes = GetDouble(lh, "audits.uses-optimized-images.details.overallSavingsBytes")
                        };

                        return metrics;
                    }
                }
                finally
                {
                    if (linkedCts != null) linkedCts.Dispose();
                }
            }
        }

        private string BuildRequestUrl(string url, Strategy strategy, string locale)
        {
            var strat = (strategy == Strategy.Mobile) ? "mobile" : "desktop";

            // Важно: параметр "locale" (не "local")
            // Категории добавляем повторяющимся параметром category=
            // Пример:
            // ...&category=performance&category=seo&category=best-practices&category=accessibility
            var endpoint = "https://www.googleapis.com/pagespeedonline/v5/runPagespeed";
            var qs =
                "key=" + Uri.EscapeDataString(_apiKey) +
                "&url=" + Uri.EscapeDataString(url) +
                "&strategy=" + Uri.EscapeDataString(strat) +
                "&locale=" + Uri.EscapeDataString(locale) +
                "&category=performance&category=seo&category=best-practices&category=accessibility";

            return endpoint + "?" + qs;
        }

        private static int? GetNetworkRequestsCount(JObject lighthouseResult)
        {
            if (lighthouseResult == null) return null;

            var items = lighthouseResult.SelectToken("audits.network-requests.details.items") as JArray;
            return items != null ? (int?)items.Count : null;
        }

        private static double? GetDouble(JObject obj, string jsonPath)
        {
            if (obj == null) return null;

            var token = obj.SelectToken(jsonPath);
            if (token == null) return null;

            // numericValue иногда приходит числом, иногда строкой (редко)
            if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
                return token.Value<double>();

            if (token.Type == JTokenType.String)
            {
                double parsed;
                if (double.TryParse(token.Value<string>(), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out parsed))
                    return parsed;
            }

            return null;
        }

        private static DateTimeOffset? GetDateTimeOffset(JObject obj, string propertyName)
        {
            if (obj == null) return null;

            var t = obj[propertyName];
            if (t == null || t.Type != JTokenType.String) return null;

            DateTimeOffset dto;
            if (DateTimeOffset.TryParse(t.Value<string>(), out dto))
                return dto;

            return null;
        }

        private static string TryExtractErrorMessage(string body)
        {
            try
            {
                var root = JObject.Parse(body);
                var msg = root.SelectToken("error.message");
                return msg != null ? msg.Value<string>() : null;
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            if (_disposeHttp) _http.Dispose();
        }
    }
}
