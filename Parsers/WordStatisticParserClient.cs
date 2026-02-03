// WordStatisticParserClient.cs
// .NET Framework 4.8
// NuGet: Newtonsoft.Json (Json.NET)
// Install-Package Newtonsoft.Json

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WordStatisticParserClient
{
    /// <summary>
    /// Итоговые метрики парсера по URL+keyword.
    /// Идея: в таблицу кладём в основном агрегаты + метрики по ключевому слову.
    /// Словари words/ngrams лучше хранить отдельно (в JSON/БД), чтобы не раздувать таблицу.
    /// </summary>
    public sealed class ParsedPageMetrics
    {
        // -----------------------------
        // Идентификация
        // -----------------------------

        /// <summary>
        /// URL страницы.
        /// Колонка: url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Ключевая фраза (как ты передал в API).
        /// Колонка: kw
        /// </summary>
        public string Keyword { get; set; }

        // -----------------------------
        // Структура/объём контента
        // -----------------------------

        /// <summary>
        /// Всего слов на странице.
        /// Колонка: tw
        /// Лучше/хуже: зависит от SERP (сравнивай с медианой топа), но слишком мало часто хуже.
        /// </summary>
        public int? TotalWords { get; set; }

        /// <summary>
        /// Всего предложений.
        /// Колонка: ts
        /// Лучше/хуже: нейтрально, зависит от стиля; важно для "читабельности".
        /// </summary>
        public int? TotalSentences { get; set; }

        /// <summary>
        /// Всего абзацев.
        /// Колонка: tp
        /// Лучше/хуже: больше обычно лучше (структурированнее), но смотреть по топу.
        /// </summary>
        public int? TotalParagraphs { get; set; }

        /// <summary>
        /// Слов в абзацах (только текстовые блоки).
        /// Колонка: twp
        /// Лучше/хуже: выше = больше "полезного" текста, но без переспама.
        /// </summary>
        public int? TotalWordsInParagraphs { get; set; }

        // -----------------------------
        // Заголовки
        // -----------------------------

        /// <summary>
        /// Кол-во H1.
        /// Колонка: h1
        /// Лучше/хуже: обычно 1 лучше всего; 0 или >1 часто хуже.
        /// </summary>
        public int? H1Count { get; set; }

        /// <summary>
        /// Кол-во H2.
        /// Колонка: h2
        /// Лучше/хуже: 1+ часто лучше (структура), но сравнивай с топом.
        /// </summary>
        public int? H2Count { get; set; }

        /// <summary>
        /// Кол-во H3.
        /// Колонка: h3
        /// </summary>
        public int? H3Count { get; set; }

        /// <summary>
        /// Кол-во H4.
        /// Колонка: h4
        /// </summary>
        public int? H4Count { get; set; }

        /// <summary>
        /// Кол-во H5.
        /// Колонка: h5
        /// </summary>
        public int? H5Count { get; set; }

        /// <summary>
        /// Всего слов в заголовках.
        /// Колонка: twh
        /// Лучше/хуже: выше = больше "тематики" в структуре, но не надо спамить ключом.
        /// </summary>
        public int? TotalWordsInHeaders { get; set; }

        // -----------------------------
        // Метатеги (title/description)
        // -----------------------------

        /// <summary>
        /// Всего слов в Title.
        /// Колонка: ttitle_w
        /// Лучше/хуже: зависит от ниши; обычно 5-12 слов ок, важно не переспамить.
        /// </summary>
        public int? TotalWordsInTitle { get; set; }

        /// <summary>
        /// Всего слов в Description.
        /// Колонка: tdesc_w
        /// Лучше/хуже: обычно 8-20 слов ок, важно наличие интента/УТП.
        /// </summary>
        public int? TotalWordsInDescription { get; set; }

        // -----------------------------
        // Ссылки/картинки
        // -----------------------------

        /// <summary>
        /// Кол-во изображений.
        /// Колонка: img
        /// Лучше/хуже: зависит от типа страницы; слишком мало может быть хуже (бедный контент).
        /// </summary>
        public int? ImageCount { get; set; }

        /// <summary>
        /// Кол-во внутренних ссылок.
        /// Колонка: in_l
        /// Лучше/хуже: умеренно больше часто лучше (навигация), но без "простыни" ссылок.
        /// </summary>
        public int? InnerLinks { get; set; }

        /// <summary>
        /// Кол-во внешних ссылок.
        /// Колонка: out_l
        /// Лучше/хуже: нейтрально; иногда полезно для справок, но для коммерции часто 0-умеренно.
        /// </summary>
        public int? OuterLinks { get; set; }

        /// <summary>
        /// Всего слов в анкорах ссылок.
        /// Колонка: twl
        /// Лучше/хуже: нейтрально; важно, чтобы не было переспама ключами в ссылках.
        /// </summary>
        public int? TotalWordsInLinks { get; set; }

        // -----------------------------
        // Ключевая фраза (разбор по словам)
        // -----------------------------

        /// <summary>
        /// Кол-во слов в ключевой фразе (после разбиения).
        /// Колонка: kw_n
        /// Лучше/хуже: не применимо, это характеристика запроса.
        /// </summary>
        public int KeywordWordsCount { get; set; }

        /// <summary>
        /// Сколько слов из keyphrase встретилось в Title (сумма по всем словам фразы).
        /// Колонка: kw_t
        /// Лучше/хуже: выше обычно лучше, но не обязательно 100% совпадение.
        /// </summary>
        public int KeywordWordsInTitle { get; set; }

        /// <summary>
        /// Сколько слов из keyphrase встретилось в Description.
        /// Колонка: kw_d
        /// </summary>
        public int KeywordWordsInDescription { get; set; }

        /// <summary>
        /// Сколько слов из keyphrase встретилось в Headers (H1-Hx суммарно).
        /// Колонка: kw_h
        /// </summary>
        public int KeywordWordsInHeaders { get; set; }

        /// <summary>
        /// Сколько слов из keyphrase встретилось в ALT.
        /// Колонка: kw_alt
        /// </summary>
        public int KeywordWordsInAlt { get; set; }

        /// <summary>
        /// Сколько слов из keyphrase встретилось в тексте (по твоему блоку keywords_in_text).
        /// Колонка: kw_txt
        /// Лучше/хуже: зависит от частоты; важно не уйти в спам.
        /// </summary>
        public int KeywordWordsInText { get; set; }

        /// <summary>
        /// Доля "токенов/слов" или иной внутренний коэффициент (tokens_ratio).
        /// Колонка: tok_r
        /// Лучше/хуже: интерпретация зависит от того, как ты считаешь; полезно как сравнительная метрика.
        /// </summary>
        public double? TokensRatio { get; set; }

        // -----------------------------
        // Читабельность
        // -----------------------------

        /// <summary>
        /// Kincaid score (у тебя отдельным полем).
        /// Колонка: kin
        /// Лучше/хуже: ниже обычно проще читать (для массовой аудитории часто лучше).
        /// </summary>
        public double? KincaidScore { get; set; }

        /// <summary>
        /// Flesch Reading Ease (readability_scores.flesch_reading_ease).
        /// Колонка: fre
        /// Лучше/хуже: выше = легче читать.
        /// </summary>
        public double? FleschReadingEase { get; set; }

        /// <summary>
        /// Gunning Fog (readability_scores.gunning_fog).
        /// Колонка: gfog
        /// Лучше/хуже: ниже = проще.
        /// </summary>
        public double? GunningFog { get; set; }

        /// <summary>
        /// SMOG index (readability_scores.smog_index).
        /// Колонка: smog
        /// Лучше/хуже: ниже = проще.
        /// </summary>
        public double? SmogIndex { get; set; }

        /// <summary>
        /// Automated Readability Index (readability_scores.automated_readability_index).
        /// Колонка: ari
        /// Лучше/хуже: ниже = проще.
        /// </summary>
        public double? AutomatedReadabilityIndex { get; set; }

        // -----------------------------
        // Плотность ключевых слов (если хочешь)
        // -----------------------------

        /// <summary>
        /// Плотность для ТОЧНОГО слова из фразы (например, для самого важного токена).
        /// Колонка: kd_main
        /// Лучше/хуже: зависит от топа; слишком высоко = риск переспама.
        /// </summary>
        public double? MainKeywordDensity { get; set; }

        // -----------------------------
        // Дополнительно: сырьё (по желанию)
        // -----------------------------

        /// <summary>
        /// Полный JSON-ответ (по желанию). Можно сохранять для дебага/воспроизводимости.
        /// В таблицу обычно НЕ нужно.
        /// </summary>
        public string RawJson { get; set; }
    }

    public sealed class WordParserClient : IDisposable
    {
        private readonly HttpClient _http;
        private readonly bool _disposeHttp;
        private readonly Uri _endpoint;

        public WordParserClient(string endpointBaseUrl, HttpClient httpClient = null)
        {
            if (string.IsNullOrWhiteSpace(endpointBaseUrl))
                throw new ArgumentException("endpointBaseUrl пустой.", nameof(endpointBaseUrl));

            // например: https://mydomen.ru/parse_by_url
            _endpoint = new Uri(endpointBaseUrl, UriKind.Absolute);

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
        /// POST { url: "...", keyword: "..." } -> парсит ответ и возвращает агрегированные метрики.
        /// </summary>
        public async Task<ParsedPageMetrics> ParseAsync(
            string url,
            string keyword,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default(CancellationToken),
            bool includeRawJson = false)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL пустой.", nameof(url));
            if (string.IsNullOrWhiteSpace(keyword))
                throw new ArgumentException("keyword пустой.", nameof(keyword));

            var payload = new JObject
            {
                ["url"] = url,
                ["keyword"] = keyword
            };

            using (var req = new HttpRequestMessage(HttpMethod.Post, _endpoint))
            {
                req.Headers.Accept.ParseAdd("application/json");
                req.Content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json");

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
                            throw new HttpRequestException("MyParser API error: HTTP " + (int)resp.StatusCode + " " + resp.ReasonPhrase + " body=" + body);

                        var root = JObject.Parse(body);

                        var metrics = BuildMetrics(root, url, keyword);
                        if (includeRawJson) metrics.RawJson = body;

                        return metrics;
                    }
                }
                finally
                {
                    if (linkedCts != null) linkedCts.Dispose();
                }
            }
        }

        private static ParsedPageMetrics BuildMetrics(JObject root, string url, string keyword)
        {
            // Агрегаты
            int? totalWords = GetInt(root, "total_words");
            int? totalSentences = GetInt(root, "total_sentences");
            int? totalParagraphs = GetInt(root, "total_paragraphs");
            int? totalWordsInParagraphs = GetInt(root, "total_words_in_paragraphs");

            int? imageCount = GetInt(root, "image_count");
            int? innerLinks = GetInt(root, "inner_links");
            int? outerLinks = GetInt(root, "outer_links");

            int? totalWordsInLinks = GetInt(root, "total_words_in_links");
            int? totalWordsInHeaders = GetInt(root, "total_words_in_headers");
            int? totalWordsInTitle = GetInt(root, "total_words_in_title");
            int? totalWordsInDescription = GetInt(root, "total_words_in_description");

            int? h1 = GetInt(root, "headers_count.h1");
            int? h2 = GetInt(root, "headers_count.h2");
            int? h3 = GetInt(root, "headers_count.h3");
            int? h4 = GetInt(root, "headers_count.h4");
            int? h5 = GetInt(root, "headers_count.h5");

            double? tokensRatio = GetDouble(root, "tokens_ratio");
            double? kincaid = GetDouble(root, "kincaid_score");

            double? fre = GetDouble(root, "readability_scores.flesch_reading_ease");
            double? smog = GetDouble(root, "readability_scores.smog_index");
            double? gfog = GetDouble(root, "readability_scores.gunning_fog");
            double? ari = GetDouble(root, "readability_scores.automated_readability_index");

            // Работа с keyword: разбиваем на слова и считаем совпадения по словарям words_in_*
            var kwWords = SplitKeywordToTokens(keyword);
            var kwCount = kwWords.Length;

            // words_in_title / description / headers / alt:
            var wordsInTitle = root["words_in_title"] as JObject;
            var wordsInDesc = root["words_in_description"] as JObject;
            var wordsInHeaders = root["words_in_headers"] as JObject;
            var wordsInAlt = root["words_in_alt"] as JObject;

            int kwInTitle = SumMatches(wordsInTitle, kwWords);
            int kwInDesc = SumMatches(wordsInDesc, kwWords);
            int kwInHeaders = SumMatches(wordsInHeaders, kwWords);
            int kwInAlt = SumMatches(wordsInAlt, kwWords);

            // keywords_in_text: у тебя там обычно точные ключевые токены
            var kwInTextObj = root["keywords_in_text"] as JObject;
            int kwInText = SumMatches(kwInTextObj, kwWords);

            // keyword_density: словарь {token: percent?} — у тебя выглядит как "5" для "кабардинка"
            // Сохраним плотность для "главного" токена — первого непустого.
            double? mainDensity = null;
            var densityObj = root["keyword_density"] as JObject;
            var mainToken = kwWords.Length > 0 ? kwWords[0] : null;
            if (!string.IsNullOrWhiteSpace(mainToken) && densityObj != null && densityObj[mainToken] != null)
            {
                // у тебя может быть int; приводим к double
                if (densityObj[mainToken].Type == JTokenType.Integer || densityObj[mainToken].Type == JTokenType.Float)
                    mainDensity = densityObj[mainToken].Value<double>();
            }

            return new ParsedPageMetrics
            {
                Url = url,
                Keyword = keyword,

                TotalWords = totalWords,
                TotalSentences = totalSentences,
                TotalParagraphs = totalParagraphs,
                TotalWordsInParagraphs = totalWordsInParagraphs,

                H1Count = h1,
                H2Count = h2,
                H3Count = h3,
                H4Count = h4,
                H5Count = h5,
                TotalWordsInHeaders = totalWordsInHeaders,

                TotalWordsInTitle = totalWordsInTitle,
                TotalWordsInDescription = totalWordsInDescription,

                ImageCount = imageCount,
                InnerLinks = innerLinks,
                OuterLinks = outerLinks,
                TotalWordsInLinks = totalWordsInLinks,

                KeywordWordsCount = kwCount,
                KeywordWordsInTitle = kwInTitle,
                KeywordWordsInDescription = kwInDesc,
                KeywordWordsInHeaders = kwInHeaders,
                KeywordWordsInAlt = kwInAlt,
                KeywordWordsInText = kwInText,

                TokensRatio = tokensRatio,

                KincaidScore = kincaid,
                FleschReadingEase = fre,
                SmogIndex = smog,
                GunningFog = gfog,
                AutomatedReadabilityIndex = ari,

                MainKeywordDensity = mainDensity
            };
        }

        private static string[] SplitKeywordToTokens(string keyword)
        {
            // Простейшая нормализация:
            // - lower
            // - заменяем всё не-буквы/цифры на пробел
            // - split
            // Если твой сервис лемматизирует, возможно лучше прислать keyword уже в нужной форме,
            // или здесь добавить лемматизацию (но это уже отдельная тема).
            var s = (keyword ?? "").ToLowerInvariant();

            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
            {
                if (char.IsLetterOrDigit(ch) || ch == 'ё')
                    sb.Append(ch);
                else
                    sb.Append(' ');
            }

            var parts = sb.ToString()
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            return parts;
        }

        private static int SumMatches(JObject dict, string[] tokens)
        {
            if (dict == null || tokens == null || tokens.Length == 0) return 0;

            int sum = 0;
            foreach (var t in tokens)
            {
                var v = dict[t];
                if (v != null && (v.Type == JTokenType.Integer || v.Type == JTokenType.Float))
                    sum += v.Value<int>();
            }
            return sum;
        }

        private static int? GetInt(JObject obj, string jsonPath)
        {
            if (obj == null) return null;
            var t = obj.SelectToken(jsonPath);
            if (t == null) return null;

            if (t.Type == JTokenType.Integer) return t.Value<int>();
            if (t.Type == JTokenType.Float) return (int)Math.Round(t.Value<double>());

            if (t.Type == JTokenType.String)
            {
                int parsed;
                if (int.TryParse(t.Value<string>(), out parsed))
                    return parsed;
            }
            return null;
        }

        private static double? GetDouble(JObject obj, string jsonPath)
        {
            if (obj == null) return null;
            var t = obj.SelectToken(jsonPath);
            if (t == null) return null;

            if (t.Type == JTokenType.Float || t.Type == JTokenType.Integer)
                return t.Value<double>();

            if (t.Type == JTokenType.String)
            {
                double parsed;
                if (double.TryParse(t.Value<string>(), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out parsed))
                    return parsed;
            }
            return null;
        }

        public void Dispose()
        {
            if (_disposeHttp) _http.Dispose();
        }
    }
}

