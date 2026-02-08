using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TechnicalSeoCheckerNet48;

namespace CheckPosition
{
    // Форма отображает детализированный анализ сайта с медианами топ-позиций и рекомендациями
    public sealed class AnalysisDataResult : Form
    {
        private const decimal DefaultWarningThresholdPercent = 10m;
        private const decimal DefaultCriticalThresholdPercent = 25m;

        private readonly DataBaseSqlite _database;
        private readonly long _siteId;
        private readonly IReadOnlyList<MetricDefinition> _metrics;
        private readonly DataGridView _grid;
        private readonly Button _refreshButton;
        private readonly NumericUpDown _warningThreshold;
        private readonly NumericUpDown _criticalThreshold;
        private readonly Label _siteLabel;

        public AnalysisDataResult(DataBaseSqlite database, long siteId)
        {
            // Сохраняем зависимости и выполняем базовую валидацию входных параметров
            _database = database ?? throw new ArgumentNullException(nameof(database));
            if (siteId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(siteId));
            }

            _siteId = siteId;
            _metrics = BuildMetricDefinitions();

            // Настраиваем базовые свойства формы
            Text = "Полный анализ сайта";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(900, 600);

            // Формируем элементы интерфейса и раскладываем их на форме
            _siteLabel = new Label { AutoSize = true, Font = new Font(Font, FontStyle.Bold), Text = "Сайт: загрузка..." };
            _refreshButton = new Button { Text = "Обновить", AutoSize = true };
            _refreshButton.Click += RefreshButton_Click;

            _warningThreshold = CreateThresholdControl(DefaultWarningThresholdPercent);
            _criticalThreshold = CreateThresholdControl(DefaultCriticalThresholdPercent);

            var warningLabel = new Label { AutoSize = true, Text = "Порог отклонения (%):" };
            var criticalLabel = new Label { AutoSize = true, Text = "Критичный порог (%):" };

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            var headerPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(8),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true
            };

            headerPanel.Controls.Add(_siteLabel);
            headerPanel.Controls.Add(new Label { AutoSize = true, Width = 20 });
            headerPanel.Controls.Add(warningLabel);
            headerPanel.Controls.Add(_warningThreshold);
            headerPanel.Controls.Add(criticalLabel);
            headerPanel.Controls.Add(_criticalThreshold);
            headerPanel.Controls.Add(_refreshButton);

            Controls.Add(_grid);
            Controls.Add(headerPanel);

            Shown += AnalysisDataResult_Shown;
        }

        private static NumericUpDown CreateThresholdControl(decimal defaultValue)
        {
            // Создаем контрол для ввода порога отклонений с ограничениями по диапазону
            return new NumericUpDown
            {
                Minimum = 0,
                Maximum = 200,
                DecimalPlaces = 1,
                Increment = 0.5m,
                Value = defaultValue,
                Width = 80
            };
        }

        private async void AnalysisDataResult_Shown(object sender, EventArgs e)
        {
            // Запускаем первичный пересчет при первом показе формы
            await RefreshAnalysisAsync().ConfigureAwait(true);
        }

        private async void RefreshButton_Click(object sender, EventArgs e)
        {
            // Позволяем пользователю пересчитать анализ по текущим порогам
            await RefreshAnalysisAsync().ConfigureAwait(true);
        }

        private async Task RefreshAnalysisAsync()
        {
            // Основной сценарий пересчета анализа и сохранения результатов в БД
            _refreshButton.Enabled = false;
            try
            {
                var siteInfo = _database.LoadSiteInfo(_siteId);
                if (siteInfo == null)
                {
                    MessageBox.Show("Не удалось найти выбранный сайт в базе данных.");
                    return;
                }

                _siteLabel.Text = string.IsNullOrWhiteSpace(siteInfo.PageUrl) ? $"ID сайта: {_siteId}" : $"Сайт: {siteInfo.PageUrl}";

                var topSites = _database.LoadTopPositionSites(3);
                if (topSites.Count == 0)
                {
                    MessageBox.Show("Нет сайтов с позициями 1-3 для расчета медиан.");
                    return;
                }

                var allSiteIds = new HashSet<long>(topSites.Select(site => site.SiteId)) { _siteId };
                var analysisData = _database.LoadAnalysisDataForSites(allSiteIds.ToList());

                var currentSiteRows = ExtractAnalysisRows(analysisData, _siteId);
                if (currentSiteRows.Count == 0)
                {
                    MessageBox.Show("Для выбранного сайта нет данных анализа. Сначала выполните анализ.");
                    return;
                }

                var topSiteRows = new List<DataRow>();
                foreach (var topSite in topSites)
                {
                    topSiteRows.AddRange(ExtractAnalysisRows(analysisData, topSite.SiteId));
                }

                var results = await BuildAnalysisResultsAsync(siteInfo, currentSiteRows, topSiteRows, topSites).ConfigureAwait(true);
                BindResults(results);
                _database.SaveSiteAnalysisResults(_siteId, results);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка пересчета анализа: " + ex.Message);
            }
            finally
            {
                _refreshButton.Enabled = true;
            }
        }

        private async Task<IReadOnlyList<SiteAnalysisResultRow>> BuildAnalysisResultsAsync(
            SiteBasicInfo siteInfo,
            IReadOnlyList<DataRow> currentSiteRows,
            IReadOnlyList<DataRow> topSiteRows,
            IReadOnlyList<SiteBasicInfo> topSites)
        {
            // Собираем результаты по всем метрикам, включая техническую проверку
            var results = new List<SiteAnalysisResultRow>();
            foreach (var metric in _metrics)
            {
                double? actualValue = GetMetricValue(currentSiteRows, metric);
                double? medianValue = CalculateMedian(GetMetricValues(topSiteRows, metric));
                double? deviationPercent = CalculateDeviationPercent(actualValue, medianValue);
                string recommendation = BuildRecommendation(metric, actualValue, medianValue, deviationPercent);

                results.Add(new SiteAnalysisResultRow(metric.DisplayName, actualValue, medianValue, deviationPercent, recommendation));
            }

            var technicalResult = await BuildTechnicalResultAsync(siteInfo, topSites).ConfigureAwait(true);
            if (technicalResult != null)
            {
                results.Add(technicalResult);
            }

            return results;
        }

        private async Task<SiteAnalysisResultRow> BuildTechnicalResultAsync(
            SiteBasicInfo siteInfo,
            IReadOnlyList<SiteBasicInfo> topSites)
        {
            // Запускаем техническую проверку и формируем запись для таблицы
            if (string.IsNullOrWhiteSpace(siteInfo.PageUrl))
            {
                return null;
            }

            var warnings = new List<string>();
            double? actualIssues = await CountTechnicalIssuesSafeAsync(siteInfo.PageUrl, warnings).ConfigureAwait(true);

            var topIssueCounts = new List<double>();
            foreach (var topSite in topSites)
            {
                if (string.IsNullOrWhiteSpace(topSite.PageUrl))
                {
                    continue;
                }

                var count = await CountTechnicalIssuesSafeAsync(topSite.PageUrl, warnings).ConfigureAwait(true);
                if (count.HasValue)
                {
                    topIssueCounts.Add(count.Value);
                }
            }

            if (warnings.Count > 0)
            {
                MessageBox.Show(string.Join(Environment.NewLine, warnings), "Техническая проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            double? medianIssues = CalculateMedian(topIssueCounts);
            double? deviationPercent = CalculateDeviationPercent(actualIssues, medianIssues);

            var metric = new MetricDefinition("technical_issues", "Технические ошибки (количество)", MetricDirection.LowerIsBetter, null);
            string recommendation = BuildRecommendation(metric, actualIssues, medianIssues, deviationPercent);

            return new SiteAnalysisResultRow(metric.DisplayName, actualIssues, medianIssues, deviationPercent, recommendation);
        }

        private static List<DataRow> ExtractAnalysisRows(DataTable table, long siteId)
        {
            // Фильтруем строки анализа по идентификатору сайта
            return table.Rows.Cast<DataRow>().Where(row => GetLong(row["site_id"]) == siteId).ToList();
        }

        private static long GetLong(object value)
        {
            // Безопасно преобразуем значение к long
            if (value == null || value == DBNull.Value)
            {
                return 0;
            }

            if (value is long longValue)
            {
                return longValue;
            }

            if (value is int intValue)
            {
                return intValue;
            }

            return long.TryParse(Convert.ToString(value), out var parsed) ? parsed : 0;
        }

        private void BindResults(IReadOnlyList<SiteAnalysisResultRow> results)
        {
            // Создаем таблицу представления для грида и заполняем ее рассчитанными значениями
            var table = new DataTable();
            table.Columns.Add("Параметр", typeof(string));
            table.Columns.Add("Факт", typeof(string));
            table.Columns.Add("Медиана", typeof(string));
            table.Columns.Add("Отклонение", typeof(string));
            table.Columns.Add("Рекомендации", typeof(string));

            foreach (var result in results)
            {
                string actual = FormatValue(result.ActualValue);
                string median = FormatValue(result.MedianValue);
                string deviation = FormatDeviation(result.DeviationPercent);
                table.Rows.Add(result.ParameterName, actual, median, deviation, result.Recommendation ?? string.Empty);
            }

            _grid.DataSource = table;
            _grid.AutoResizeColumns();
        }

        private static string FormatValue(double? value)
        {
            // Форматируем числовое значение с учетом отсутствия данных
            return value.HasValue ? value.Value.ToString("0.##") : string.Empty;
        }

        private static string FormatDeviation(double? deviationPercent)
        {
            // Форматируем отклонение в процентах
            return deviationPercent.HasValue ? $"{deviationPercent.Value:+0.##;-0.##;0}%" : string.Empty;
        }

        private static double? GetMetricValue(IEnumerable<DataRow> rows, MetricDefinition metric)
        {
            // Ищем значение метрики в строке соответствующей стратегии
            foreach (var row in rows)
            {
                string strategy = Convert.ToString(row["strategy"]) ?? string.Empty;
                if (metric.Strategy != null && !string.Equals(strategy, metric.Strategy, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var value = GetNullableDouble(row[metric.ColumnName]);
                if (value.HasValue)
                {
                    return value.Value;
                }
            }

            return null;
        }

        private static List<double> GetMetricValues(IEnumerable<DataRow> rows, MetricDefinition metric)
        {
            // Собираем значения метрики по выбранным строкам для медианного расчета
            var values = new List<double>();
            foreach (var row in rows)
            {
                string strategy = Convert.ToString(row["strategy"]) ?? string.Empty;
                if (metric.Strategy != null && !string.Equals(strategy, metric.Strategy, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var value = GetNullableDouble(row[metric.ColumnName]);
                if (value.HasValue)
                {
                    values.Add(value.Value);
                }
            }

            return values;
        }

        private static double? GetNullableDouble(object value)
        {
            // Преобразуем значение БД в double, исключая пустые и заглушки
            if (value == null || value == DBNull.Value)
            {
                return null;
            }

            if (double.TryParse(Convert.ToString(value), out var parsed))
            {
                if (Math.Abs(parsed + 1) < double.Epsilon)
                {
                    return null;
                }

                return parsed;
            }

            return null;
        }

        private static double? CalculateMedian(IReadOnlyList<double> values)
        {
            // Рассчитываем медиану списка чисел
            if (values == null || values.Count == 0)
            {
                return null;
            }

            var ordered = values.OrderBy(x => x).ToList();
            int middle = ordered.Count / 2;
            if (ordered.Count % 2 == 0)
            {
                return (ordered[middle - 1] + ordered[middle]) / 2d;
            }

            return ordered[middle];
        }

        private static double? CalculateDeviationPercent(double? actualValue, double? medianValue)
        {
            // Рассчитываем процент отклонения относительно медианы
            if (!actualValue.HasValue || !medianValue.HasValue)
            {
                return null;
            }

            if (Math.Abs(medianValue.Value) < double.Epsilon)
            {
                return Math.Abs(actualValue.Value) < double.Epsilon ? 0d : (double?)null;
            }

            return (actualValue.Value - medianValue.Value) / medianValue.Value * 100d;
        }

        private string BuildRecommendation(MetricDefinition metric, double? actualValue, double? medianValue, double? deviationPercent)
        {
            // Формируем рекомендацию на основе направления метрики и порогов отклонения
            if (!actualValue.HasValue || !medianValue.HasValue || !deviationPercent.HasValue)
            {
                return "Недостаточно данных для рекомендации.";
            }

            decimal warning = _warningThreshold.Value;
            decimal critical = _criticalThreshold.Value;
            decimal deviationAbs = (decimal)Math.Abs(deviationPercent.Value);

            if (deviationAbs < warning)
            {
                return "Отклонение в пределах нормы.";
            }

            bool isCritical = deviationAbs >= critical;
            string urgency = isCritical ? "Срочно" : "Рекомендуется";

            if (metric.Direction == MetricDirection.HigherIsBetter)
            {
                if (actualValue.Value < medianValue.Value)
                {
                    return $"{urgency} увеличить значение.";
                }

                return "Значение выше медианы, можно сохранить текущий уровень.";
            }

            if (actualValue.Value > medianValue.Value)
            {
                return $"{urgency} уменьшить значение.";
            }

            return "Значение лучше медианы, можно сохранить текущий уровень.";
        }

        private static async Task<double?> CountTechnicalIssuesSafeAsync(string url, List<string> warnings)
        {
            // Выполняем технический чек и считаем количество проблем в отчете
            try
            {
                using (var checker = new TechnicalSeoChecker())
                {
                    string report = await checker.CheckAsync(url).ConfigureAwait(true);
                    return CountIssuesInReport(report);
                }
            }
            catch (Exception ex)
            {
                string message = $"Не удалось выполнить техническую проверку для {url}: {ex.Message}";
                warnings.Add(message);
                return null;
            }
        }

        private static double CountIssuesInReport(string report)
        {
            // Подсчитываем строки проблем в отчете технического чекера
            if (string.IsNullOrWhiteSpace(report))
            {
                return 0d;
            }

            var lines = report.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            bool issuesSection = false;
            int count = 0;

            foreach (var line in lines)
            {
                if (line.StartsWith("Обнаруженные проблемы", StringComparison.OrdinalIgnoreCase))
                {
                    issuesSection = true;
                    continue;
                }

                if (!issuesSection)
                {
                    continue;
                }

                if (line.TrimStart().StartsWith("-"))
                {
                    count++;
                }
            }

            return count;
        }

        private static IReadOnlyList<MetricDefinition> BuildMetricDefinitions()
        {
            // Определяем перечень метрик анализа с учетом направлений улучшения
            var metrics = new List<MetricDefinition>();
            var psiMetrics = new List<MetricDefinition>
            {
                new MetricDefinition("psi_perf_score", "Производительность", MetricDirection.HigherIsBetter, null),
                new MetricDefinition("psi_seo_score", "SEO", MetricDirection.HigherIsBetter, null),
                new MetricDefinition("psi_bp_score", "Best Practices", MetricDirection.HigherIsBetter, null),
                new MetricDefinition("psi_a11y_score", "Доступность", MetricDirection.HigherIsBetter, null),
                new MetricDefinition("psi_lcp_ms", "LCP (мс)", MetricDirection.LowerIsBetter, null),
                new MetricDefinition("psi_cls", "CLS", MetricDirection.LowerIsBetter, null),
                new MetricDefinition("psi_inp_ms", "INP (мс)", MetricDirection.LowerIsBetter, null),
                new MetricDefinition("psi_tbt_ms", "TBT (мс)", MetricDirection.LowerIsBetter, null),
                new MetricDefinition("psi_ttfb_ms", "TTFB (мс)", MetricDirection.LowerIsBetter, null),
                new MetricDefinition("psi_fcp_ms", "FCP (мс)", MetricDirection.LowerIsBetter, null),
                new MetricDefinition("psi_si_ms", "Speed Index (мс)", MetricDirection.LowerIsBetter, null),
                new MetricDefinition("psi_bytes", "Вес страницы (байты)", MetricDirection.LowerIsBetter, null),
                new MetricDefinition("psi_req_cnt", "Количество запросов", MetricDirection.LowerIsBetter, null),
                new MetricDefinition("psi_unused_js_b", "Неиспользуемый JS (байты)", MetricDirection.LowerIsBetter, null),
                new MetricDefinition("psi_unused_css_b", "Неиспользуемый CSS (байты)", MetricDirection.LowerIsBetter, null),
                new MetricDefinition("psi_offscr_img_b", "Offscreen изображения (байты)", MetricDirection.LowerIsBetter, null),
                new MetricDefinition("psi_modern_img_b", "Modern изображения (байты)", MetricDirection.LowerIsBetter, null),
                new MetricDefinition("psi_opt_img_b", "Оптимизация изображений (байты)", MetricDirection.LowerIsBetter, null)
            };

            var wordMetrics = new List<MetricDefinition>
            {
                new MetricDefinition("word_total_words", "Количество слов", MetricDirection.HigherIsBetter, null),
                new MetricDefinition("word_total_sentences", "Количество предложений", MetricDirection.HigherIsBetter, null),
                new MetricDefinition("word_total_paragraphs", "Количество абзацев", MetricDirection.HigherIsBetter, null),
                new MetricDefinition("word_total_words_in_paragraphs", "Слов в абзацах", MetricDirection.HigherIsBetter, null),
                new MetricDefinition("word_h1_count", "Количество H1", MetricDirection.HigherIsBetter, null),
                new MetricDefinition("word_h2_count", "Количество H2", MetricDirection.HigherIsBetter, null),
                new MetricDefinition("word_h3_count", "Количество H3", MetricDirection.HigherIsBetter, null),
                new MetricDefinition("word_h4_count", "Количество H4", MetricDirection.HigherIsBetter, null),
                new MetricDefinition("word_h5_count", "Количество H5", MetricDirection.HigherIsBetter, null),
                new MetricDefinition("word_total_words_in_headers", "Слов в заголовках", MetricDirection.HigherIsBetter, null),
                new MetricDefinition("word_total_words_in_title", "Слов в Title", MetricDirection.HigherIsBetter, null),
                new MetricDefinition("word_total_words_in_description", "Слов в Description", MetricDirection.HigherIsBetter, null),
                new MetricDefinition("word_image_count", "Количество изображений", MetricDirection.HigherIsBetter, null),
                new MetricDefinition("word_inner_links", "Внутренние ссылки", MetricDirection.HigherIsBetter, null),
                new MetricDefinition("word_outer_links", "Внешние ссылки", MetricDirection.HigherIsBetter, null),
                new MetricDefinition("word_total_words_in_links", "Слов в ссылках", MetricDirection.HigherIsBetter, null),
                new MetricDefinition("word_kw_words_count", "Количество ключевых слов", MetricDirection.HigherIsBetter, null),
                new MetricDefinition("word_kw_words_in_title", "Ключевые слова в Title", MetricDirection.HigherIsBetter, null),
                new MetricDefinition("word_kw_words_in_description", "Ключевые слова в Description", MetricDirection.HigherIsBetter, null),
                new MetricDefinition("word_kw_words_in_headers", "Ключевые слова в заголовках", MetricDirection.HigherIsBetter, null),
                new MetricDefinition("word_kw_words_in_alt", "Ключевые слова в Alt", MetricDirection.HigherIsBetter, null),
                new MetricDefinition("word_kw_words_in_text", "Ключевые слова в тексте", MetricDirection.HigherIsBetter, null),
                new MetricDefinition("word_tokens_ratio", "Доля ключевых слов", MetricDirection.HigherIsBetter, null),
                new MetricDefinition("word_kincaid_score", "Kincaid", MetricDirection.LowerIsBetter, null),
                new MetricDefinition("word_flesch_reading_ease", "Flesch Reading Ease", MetricDirection.HigherIsBetter, null),
                new MetricDefinition("word_gunning_fog", "Gunning Fog", MetricDirection.LowerIsBetter, null),
                new MetricDefinition("word_smog_index", "SMOG", MetricDirection.LowerIsBetter, null),
                new MetricDefinition("word_ari", "ARI", MetricDirection.LowerIsBetter, null),
                new MetricDefinition("word_main_keyword_density", "Плотность ключевого слова", MetricDirection.HigherIsBetter, null)
            };

            var strategies = new[] { "Mobile", "Desktop" };
            foreach (var metric in psiMetrics.Concat(wordMetrics))
            {
                foreach (var strategy in strategies)
                {
                    metrics.Add(metric.WithStrategy(strategy));
                }
            }

            return metrics;
        }

        private sealed class MetricDefinition
        {
            public MetricDefinition(string columnName, string displayName, MetricDirection direction, string strategy)
            {
                // Сохраняем основные свойства метрики, включая стратегию и направление улучшения
                ColumnName = columnName ?? throw new ArgumentNullException(nameof(columnName));
                DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
                Direction = direction;
                Strategy = strategy;
            }

            public string ColumnName { get; }
            public string DisplayName { get; }
            public MetricDirection Direction { get; }
            public string Strategy { get; }

            public MetricDefinition WithStrategy(string strategy)
            {
                // Создаем копию метрики для конкретной стратегии
                string prefix = string.IsNullOrWhiteSpace(strategy) ? string.Empty : strategy + ": ";
                return new MetricDefinition(ColumnName, prefix + DisplayName, Direction, strategy);
            }
        }

        private enum MetricDirection
        {
            HigherIsBetter,
            LowerIsBetter
        }
    }
}
