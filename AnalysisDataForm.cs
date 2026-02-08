using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using PsiMetricsNet48;
using WordStatisticParserClient;

namespace CheckPosition
{
    public partial class AnalysisDataForm : Form
    {
        private const int SearchMinLength = 4;
        private const int ParserTimeoutSeconds = 90;
        private const string HiddenColumnsFileName = "analysis_hidden_columns.txt";
        private const string StrategyMobile = "Mobile";
        private const string StrategyDesktop = "Desktop";

        private readonly DataBaseSqlite _database;
        private DataTable _analysisTable;
        private CancellationTokenSource _analysisCts;
        private bool _isRunning;
        private readonly HashSet<string> _hiddenColumns;
        private bool _isUpdatingColumnVisibility;
        private String psiApiKey;

        public AnalysisDataForm(DataBaseSqlite database)
        {
            InitializeComponent();
            psiApiKey = Environment.GetEnvironmentVariable("PSI_API_KEY") ?? "AIzaSyCwiCkltlDMBBzNlW5R_mKnfROT8LBPWoI";
            // Сохраняем ссылку на базу данных и настраиваем таблицу
            _database = database ?? throw new ArgumentNullException(nameof(database));
            // Загружаем сохраненные настройки скрытых столбцов, чтобы восстановить вид таблицы
            _hiddenColumns = LoadHiddenColumns();
            analysisGrid.AutoGenerateColumns = true;
            analysisGrid.DataBindingComplete += AnalysisGrid_DataBindingComplete;
            analysisGrid.SelectionChanged += AnalysisGrid_SelectionChanged;
            analysisGrid.CellFormatting += AnalysisGrid_CellFormatting;
            // Настраиваем фильтр стратегии, чтобы отображать только mobile или desktop
            ConfigureStrategyFilter();
        }

        private void AnalysisDataForm_Shown(object sender, EventArgs e)
        {
            // При первом показе загружаем актуальные данные аналитики
            LoadAnalysisData();
        }

        private void AnalysisGrid_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            // После привязки данных скрываем служебные поля и включаем сортировку
            UpdateGridColumns();
        }

        private void AnalysisGrid_SelectionChanged(object sender, EventArgs e)
        {
            // Обновляем состояние кнопки "Проверить выбранный" при смене строки
            checkSelectedButton.Enabled = analysisGrid.CurrentRow != null && !analysisGrid.CurrentRow.IsNewRow;
        }

        private void AnalysisGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Заменяем служебное значение -1 на пустое отображение, чтобы таблица была читаемой
            if (e.Value == null)
            {
                return;
            }

            if (IsPlaceholderValue(e.Value))
            {
                e.Value = string.Empty;
                e.FormattingApplied = true;
            }
        }

        private void searchTextBox_TextChanged(object sender, EventArgs e)
        {
            // Запускаем поиск только при достаточной длине запроса, чтобы избежать лишней нагрузки
            string query = searchTextBox.Text?.Trim() ?? string.Empty;
            if (query.Length < SearchMinLength)
            {
                return;
            }

            FindAndSelectRow(query);
        }

        private async void checkAllButton_Click(object sender, EventArgs e)
        {
            // Проверяем все сайты из таблицы sites
            var targets = _database.LoadSitesForAnalysis(null);
            await RunAnalysisAsync(targets).ConfigureAwait(true);
        }

        private async void checkSelectedButton_Click(object sender, EventArgs e)
        {
            // Проверяем только выбранную строку с учетом скрытого идентификатора сайта
            List<long> siteIds = GetSelectedSiteIds();
            if (siteIds.Count <= 0)
            {
                MessageBox.Show("Выберите строку для проверки.");
                return;
            }

            var targets = _database.LoadSitesForAnalysis(siteIds);
            await RunAnalysisAsync(targets).ConfigureAwait(true);
        }

        private void addMissingButton_Click(object sender, EventArgs e)
        {
            // Добавляем пустые записи аналитики для сайтов, у которых еще нет данных по обеим стратегиям
            try
            {
                int addedCount = _database.InsertMissingAnalysisRecords();
                LoadAnalysisData();
                MessageBox.Show(addedCount > 0 ? $"Добавлено записей: {addedCount}." : "Новых записей не найдено.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления записей: " + ex.Message);
            }
        }

        private void strategyComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Перезагружаем данные при смене стратегии, чтобы пользователь видел корректную выборку
            LoadAnalysisData();
        }

        private void exportCsvButton_Click(object sender, EventArgs e)
        {
            // Выгружаем текущую таблицу в CSV с учетом фильтра и видимых столбцов
            try
            {
                ExportCurrentViewToCsv();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка выгрузки CSV: " + ex.Message);
            }
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            // Отправляем запрос на остановку всех активных проверок
            if (_analysisCts == null)
            {
                return;
            }

            _analysisCts.Cancel();
        }

        private void LoadAnalysisData()
        {
            // Загружаем и показываем данные аналитики из базы
            try
            {
                string strategyFilter = GetSelectedStrategyFilter();
                _analysisTable = _database.LoadAnalysisData(strategyFilter);
                analysisGrid.DataSource = _analysisTable;
                UpdateGridColumns();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки аналитики: " + ex.Message);
            }
        }

        private void UpdateGridColumns()
        {
            // Включаем сортировку и скрываем системный идентификатор
            foreach (DataGridViewColumn column in analysisGrid.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.Automatic;
            }
            // Применяем заголовки и порядок столбцов перед восстановлением видимости
            ApplyColumnHeaders();
            ApplyColumnOrder();
            ApplySavedColumnVisibility();
            BuildColumnVisibilityControls();
        }

        private void ApplyColumnHeaders()
        {
            // Проставляем русские заголовки по смыслу метрик из комментариев парсеров
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["id"] = "ID",
                ["site_id"] = "ID сайта",
                ["page_url"] = "URL страницы",
                ["position_current"] = "позиция",
                ["check_date"] = "Дата проверки",
                ["strategy"] = "Стратегия",
                ["fetch_time"] = "Время Lighthouse",
                ["psi_perf_score"] = "Производительность",
                ["psi_seo_score"] = "SEO",
                ["psi_bp_score"] = "Best Practices",
                ["psi_a11y_score"] = "Доступность",
                ["psi_lcp_ms"] = "LCP, мс",
                ["psi_cls"] = "CLS",
                ["psi_inp_ms"] = "INP, мс",
                ["psi_tbt_ms"] = "TBT, мс",
                ["psi_ttfb_ms"] = "TTFB, мс",
                ["psi_fcp_ms"] = "FCP, мс",
                ["psi_si_ms"] = "Speed Index, мс",
                ["psi_bytes"] = "Вес, байт",
                ["psi_req_cnt"] = "Запросы",
                ["psi_unused_js_b"] = "Неисп. JS, байт",
                ["psi_unused_css_b"] = "Неисп. CSS, байт",
                ["psi_offscr_img_b"] = "Offscreen img, байт",
                ["psi_modern_img_b"] = "Modern img, байт",
                ["psi_opt_img_b"] = "Опт. img, байт",
                ["word_keyword"] = "Ключевая фраза",
                ["word_total_words"] = "Всего слов",
                ["word_total_sentences"] = "Предложений",
                ["word_total_paragraphs"] = "Абзацев",
                ["word_total_words_in_paragraphs"] = "Слов в абзацах",
                ["word_h1_count"] = "H1",
                ["word_h2_count"] = "H2",
                ["word_h3_count"] = "H3",
                ["word_h4_count"] = "H4",
                ["word_h5_count"] = "H5",
                ["word_total_words_in_headers"] = "Слов в заголовках",
                ["word_total_words_in_title"] = "Слов в Title",
                ["word_total_words_in_description"] = "Слов в Description",
                ["word_image_count"] = "Изображений",
                ["word_inner_links"] = "Внутр. ссылок",
                ["word_outer_links"] = "Внешн. ссылок",
                ["word_total_words_in_links"] = "Слов в ссылках",
                ["word_kw_words_count"] = "Слов в ключе",
                ["word_kw_words_in_title"] = "Ключ в Title",
                ["word_kw_words_in_description"] = "Ключ в Desc",
                ["word_kw_words_in_headers"] = "Ключ в H1-Hx",
                ["word_kw_words_in_alt"] = "Ключ в Alt",
                ["word_kw_words_in_text"] = "Ключ в тексте",
                ["word_tokens_ratio"] = "Tokens ratio",
                ["word_kincaid_score"] = "Kincaid",
                ["word_flesch_reading_ease"] = "Flesch",
                ["word_gunning_fog"] = "Gunning Fog",
                ["word_smog_index"] = "SMOG",
                ["word_ari"] = "ARI",
                ["word_main_keyword_density"] = "Плотность ключа"
            };

            foreach (DataGridViewColumn column in analysisGrid.Columns)
            {
                if (headers.TryGetValue(column.Name, out string headerText))
                {
                    column.HeaderText = headerText;
                }
            }
        }

        private void ApplyColumnOrder()
        {
            // Проставляем порядок ключевых столбцов, чтобы пользователю было проще ориентироваться
            var order = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["id"] = 0,
                ["site_id"] = 1,
                ["page_url"] = 2,
                ["position_current"] = 3,
                ["check_date"] = 4
            };

            foreach (DataGridViewColumn column in analysisGrid.Columns)
            {
                if (order.TryGetValue(column.Name, out int index))
                {
                    column.DisplayIndex = index;
                }
            }
        }

        private void ApplySavedColumnVisibility()
        {
            // Восстанавливаем видимость столбцов из сохраненного списка и применяем дефолты при первом запуске
            EnsureDefaultHiddenColumns();
            foreach (DataGridViewColumn column in analysisGrid.Columns)
            {
                column.Visible = !_hiddenColumns.Contains(column.Name);
            }
        }

        private void EnsureDefaultHiddenColumns()
        {
            // Добавляем дефолтные скрытые столбцы, если пользователь еще не настраивал видимость
            if (_hiddenColumns.Count > 0)
            {
                return;
            }

            _hiddenColumns.Add("site_id");
            _hiddenColumns.Add("raw_json");
        }

        private void BuildColumnVisibilityControls()
        {
            // Пересоздаем список чекбоксов только после загрузки колонок, чтобы он отражал текущее состояние
            if (columnsPanel == null || analysisGrid.Columns.Count == 0)
            {
                return;
            }

            _isUpdatingColumnVisibility = true;
            columnsPanel.SuspendLayout();
            try
            {
                columnsPanel.Controls.Clear();
                foreach (DataGridViewColumn column in analysisGrid.Columns)
                {
                    var checkBox = new CheckBox
                    {
                        AutoSize = true,
                        Text = column.HeaderText,
                        Checked = column.Visible,
                        Tag = column.Name
                    };
                    checkBox.CheckedChanged += ColumnVisibility_CheckedChanged;
                    columnsPanel.Controls.Add(checkBox);
                }
            }
            finally
            {
                columnsPanel.ResumeLayout();
                _isUpdatingColumnVisibility = false;
            }
        }

        private void ColumnVisibility_CheckedChanged(object sender, EventArgs e)
        {
            // Применяем выбор пользователя и сохраняем список скрытых колонок в профиле
            if (_isUpdatingColumnVisibility)
            {
                return;
            }

            if (!(sender is CheckBox checkBox))
            {
                return;
            }

            string columnName = Convert.ToString(checkBox.Tag);
            if (string.IsNullOrWhiteSpace(columnName) || !analysisGrid.Columns.Contains(columnName))
            {
                return;
            }

            analysisGrid.Columns[columnName].Visible = checkBox.Checked;
            UpdateHiddenColumnsState(columnName, !checkBox.Checked);
        }

        private void UpdateHiddenColumnsState(string columnName, bool isHidden)
        {
            // Обновляем внутренний список и сохраняем его на диск, чтобы настройка переживала закрытие формы
            if (isHidden)
            {
                _hiddenColumns.Add(columnName);
            }
            else
            {
                _hiddenColumns.Remove(columnName);
            }

            SaveHiddenColumns();
        }

        private HashSet<string> LoadHiddenColumns()
        {
            // Читаем сохраненные настройки из файла профиля, чтобы не зависеть от системных настроек проекта
            var hiddenColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                string filePath = GetHiddenColumnsFilePath();
                if (!File.Exists(filePath))
                {
                    return hiddenColumns;
                }

                foreach (string line in File.ReadAllLines(filePath))
                {
                    string trimmed = line?.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmed))
                    {
                        hiddenColumns.Add(trimmed);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки настроек столбцов: " + ex.Message);
            }

            return hiddenColumns;
        }

        private void SaveHiddenColumns()
        {
            // Сохраняем список скрытых колонок атомарно, чтобы избежать повреждения настроек
            try
            {
                string filePath = GetHiddenColumnsFilePath();
                string folderPath = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrWhiteSpace(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string tempPath = filePath + ".tmp";
                File.WriteAllLines(tempPath, _hiddenColumns.OrderBy(name => name));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                File.Move(tempPath, filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения настроек столбцов: " + ex.Message);
            }
        }

        private string GetHiddenColumnsFilePath()
        {
            // Формируем путь к файлу в профиле пользователя, чтобы настройки не зависели от каталога приложения
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appDataPath, "CheckPosition", HiddenColumnsFileName);
        }

        private static bool IsPlaceholderValue(object value)
        {
            // Определяем служебное значение -1 в разных представлениях
            if (value is int intValue)
            {
                return intValue == -1;
            }

            if (value is long longValue)
            {
                return longValue == -1;
            }

            if (value is short shortValue)
            {
                return shortValue == -1;
            }

            if (value is double doubleValue)
            {
                return Math.Abs(doubleValue + 1d) < 0.000001d;
            }

            if (value is float floatValue)
            {
                return Math.Abs(floatValue + 1f) < 0.000001f;
            }

            string stringValue = Convert.ToString(value);
            return string.Equals(stringValue?.Trim(), "-1", StringComparison.Ordinal);
        }

        private List<long> GetSelectedSiteIds()
        {
            List<long> siteIds = new List<long>();
            // Получаем идентификатор сайта из скрытого столбца
            if (analysisGrid.CurrentRow == null)
            {
                return siteIds;
            }

            if (!analysisGrid.Columns.Contains("site_id"))
            {
                return siteIds;
            }
          

            foreach (DataGridViewRow row in analysisGrid.SelectedRows)
            {
                object rawValue = row.Cells["site_id"].Value;
                siteIds.Add(Helper.getIngValue(rawValue, 0));
            }
            return siteIds;
        }

        private void FindAndSelectRow(string query)
        {
            // Ищем адрес страницы, начиная с текущей строки, и циклически продолжаем поиск
            if (analysisGrid.Rows.Count == 0)
            {
                return;
            }

            int startIndex = analysisGrid.CurrentCell?.RowIndex ?? 0;
            int foundIndex = FindRowIndex(query, startIndex, analysisGrid.Rows.Count - 1);
            if (foundIndex < 0 && startIndex > 0)
            {
                foundIndex = FindRowIndex(query, 0, startIndex - 1);
            }

            if (foundIndex >= 0)
            {
                SelectRow(foundIndex);
            }
        }

        private int FindRowIndex(string query, int startIndex, int endIndex)
        {
            // Пробегаем по диапазону строк и сравниваем page_url без учета регистра
            if (startIndex < 0 || endIndex < 0 || startIndex > endIndex)
            {
                return -1;
            }

            if (!analysisGrid.Columns.Contains("page_url"))
            {
                return -1;
            }

            for (int i = startIndex; i <= endIndex; i++)
            {
                DataGridViewRow row = analysisGrid.Rows[i];
                string value = Convert.ToString(row.Cells["page_url"].Value) ?? string.Empty;
                if (value.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return i;
                }
            }

            return -1;
        }

        private void SelectRow(int rowIndex)
        {
            // Выделяем строку и прокручиваем таблицу к найденной записи
            if (rowIndex < 0 || rowIndex >= analysisGrid.Rows.Count)
            {
                return;
            }

            analysisGrid.ClearSelection();
            analysisGrid.Rows[rowIndex].Selected = true;
            analysisGrid.CurrentCell = analysisGrid.Rows[rowIndex].Cells[0];
            analysisGrid.FirstDisplayedScrollingRowIndex = rowIndex;
        }

        private async Task RunAnalysisAsync(IReadOnlyList<SiteParserTarget> targets)
        {
            // Запускаем цепочку проверок, строго обрабатывая ошибки и отмену
            if (_isRunning)
            {
                MessageBox.Show("Проверка уже выполняется.");
                return;
            }

            if (targets == null || targets.Count == 0)
            {
                MessageBox.Show("Нет сайтов для проверки.");
                return;
            }

             
            if (string.IsNullOrWhiteSpace(psiApiKey))
            {
                MessageBox.Show("Не найден ключ PSI_API_KEY в переменных окружения.");
                return;
            }

            string wordParserEndpoint = Environment.GetEnvironmentVariable("WORD_PARSER_ENDPOINT");
            if (string.IsNullOrWhiteSpace(wordParserEndpoint))
            {
                wordParserEndpoint = "https://txtxt.ru/lemmatizer/api/v1/text/parse_by_url";
            }

            _isRunning = true;
            _analysisCts = new CancellationTokenSource();
            ToggleButtons(false);

            var errors = new List<string>();
            try
            {
                using (var psiClient = new PageSpeedInsightsClient(psiApiKey))
                using (var wordClient = new WordParserClient(wordParserEndpoint))
                {
                    foreach (SiteParserTarget target in targets)
                    {
                        if (_analysisCts.IsCancellationRequested)
                        {
                            break;
                        }

                        if (string.IsNullOrWhiteSpace(target.PageAddress))
                        {
                            errors.Add($"Пустой адрес для сайта id={target.Id}.");
                            continue;
                        }

                        ParsedPageMetrics wordMetrics = null;
                        try
                        {
                            // Запускаем парсер слов, только если есть ключевая фраза
                            if (string.IsNullOrWhiteSpace(target.Query))
                            {
                                errors.Add($"Пустой запрос для WordParser: сайт id={target.Id}.");
                            }
                            else
                            {
                                wordMetrics = await wordClient.ParseAsync(
                                        target.PageAddress,
                                        target.Query,
                                        timeout: TimeSpan.FromSeconds(ParserTimeoutSeconds),
                                        cancellationToken: _analysisCts.Token,
                                        includeRawJson: true)
                                    .ConfigureAwait(true);
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"Ошибка WordParser для {target.PageAddress}: {ex.Message}");
                        }

                        try
                        {
                            // Получаем PSI метрики для mobile и desktop, сохраняя обе стратегии
                            var mobileMetrics = await psiClient.GetMetricsAsync(
                                    target.PageAddress,
                                    Strategy.Mobile,
                                    timeout: TimeSpan.FromSeconds(ParserTimeoutSeconds),
                                    cancellationToken: _analysisCts.Token)
                                .ConfigureAwait(true);

                            var desktopMetrics = await psiClient.GetMetricsAsync(
                                    target.PageAddress,
                                    Strategy.Desktop,
                                    timeout: TimeSpan.FromSeconds(ParserTimeoutSeconds),
                                    cancellationToken: _analysisCts.Token)
                                .ConfigureAwait(true);

                            DateTimeOffset checkDate = DateTimeOffset.Now;
                            bool mobileUpdated = _database.UpdateSiteAnalysisRecord(
                                target.Id,
                                target.PageAddress,
                                target.Query,
                                checkDate,
                                mobileMetrics,
                                wordMetrics);

                            bool desktopUpdated = _database.UpdateSiteAnalysisRecord(
                                target.Id,
                                target.PageAddress,
                                target.Query,
                                checkDate,
                                desktopMetrics,
                                wordMetrics);

                            if (!mobileUpdated)
                            {
                                errors.Add($"Не найдена запись для mobile: сайт id={target.Id}. Добавьте запись через кнопку 'Добавить новые'.");
                            }

                            if (!desktopUpdated)
                            {
                                errors.Add($"Не найдена запись для desktop: сайт id={target.Id}. Добавьте запись через кнопку 'Добавить новые'.");
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"Ошибка PSI для {target.PageAddress}: {ex.Message}");
                        }
                    }
                }
            }
            finally
            {
                _analysisCts?.Dispose();
                _analysisCts = null;
                _isRunning = false;
                ToggleButtons(true);
            }

            // Обновляем таблицу после завершения и показываем ошибки при наличии
            LoadAnalysisData();
            if (errors.Count > 0)
            {
                MessageBox.Show("При проверке возникли ошибки:\n" + string.Join("\n", errors.Take(10)));
            } else
                MessageBox.Show("Проверка окончена без ошибок");
        }

        private void ToggleButtons(bool enabled)
        {
            // Переключаем доступность кнопок в зависимости от состояния проверки
            checkAllButton.Enabled = enabled;
            checkSelectedButton.Enabled = enabled && analysisGrid.CurrentRow != null;
            addMissingButton.Enabled = enabled;
            stopButton.Enabled = !enabled;
            exportCsvButton.Enabled = enabled;
            strategyComboBox.Enabled = enabled;
        }

        private void ConfigureStrategyFilter()
        {
            // Заполняем комбобокс фиксированными значениями стратегий без возможности ввода
            var items = new List<StrategyFilterItem>
            {
                new StrategyFilterItem("Desktop", StrategyDesktop),
                new StrategyFilterItem("Mobile", StrategyMobile)
            };

            strategyComboBox.DisplayMember = nameof(StrategyFilterItem.DisplayName);
            strategyComboBox.ValueMember = nameof(StrategyFilterItem.Value);
            strategyComboBox.DataSource = items;
            strategyComboBox.SelectedIndex = 0;
        }

        private string GetSelectedStrategyFilter()
        {
            // Возвращаем выбранную стратегию для фильтрации или пустую строку, если выбор недоступен
            if (strategyComboBox?.SelectedItem is StrategyFilterItem item)
            {
                return item.Value;
            }

            return string.Empty;
        }

        private void ExportCurrentViewToCsv()
        {
            // Формируем CSV по текущей таблице и сохраняем файл в выбранное место
            if (analysisGrid.DataSource == null || analysisGrid.Rows.Count == 0)
            {
                MessageBox.Show("Нет данных для выгрузки.");
                return;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                dialog.FileName = $"analysis_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                var lines = BuildCsvLines();
                File.WriteAllLines(dialog.FileName, lines);
                MessageBox.Show("Выгрузка завершена: " + dialog.FileName);
            }
        }

        private List<string> BuildCsvLines()
        {
            // Собираем строки CSV по видимым колонкам и текущим данным таблицы
            var visibleColumns = analysisGrid.Columns.Cast<DataGridViewColumn>()
                .Where(column => column.Visible)
                .OrderBy(column => column.DisplayIndex)
                .ToList();

            var lines = new List<string>(analysisGrid.Rows.Count + 1);
            string headerLine = string.Join(",", visibleColumns.Select(column => EscapeCsvValue(column.HeaderText)));
            lines.Add(headerLine);

            foreach (DataGridViewRow row in analysisGrid.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                var cells = visibleColumns.Select(column => EscapeCsvValue(Convert.ToString(row.Cells[column.Name].Value)));
                lines.Add(string.Join(",", cells));
            }

            return lines;
        }

        private static string EscapeCsvValue(string value)
        {
            // Экранируем значения CSV, чтобы корректно сохранять запятые, кавычки и переносы строк
            string safeValue = value ?? string.Empty;
            bool shouldQuote = safeValue.Contains(",") || safeValue.Contains("\"") || safeValue.Contains("\n") || safeValue.Contains("\r");
            if (!shouldQuote)
            {
                return safeValue;
            }

            string escaped = safeValue.Replace("\"", "\"\"");
            return $"\"{escaped}\"";
        }

        private sealed class StrategyFilterItem
        {
            // Описываем элемент фильтра стратегии для комбобокса
            public StrategyFilterItem(string displayName, string value)
            {
                DisplayName = displayName;
                Value = value;
            }

            public string DisplayName { get; }
            public string Value { get; }
        }
    }
}
