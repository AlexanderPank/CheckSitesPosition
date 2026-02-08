using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using PsiMetricsNet48;
using WordStatisticParserClient;
using System.Net.Security;
using TechnicalSeoCheckerNet48;

namespace CheckPosition
{
    public partial class Form1 : Form
    {
 
        private String appName = "CheckPosition";
        private String _CurrentProjectFileName = "";
        private fSettings _FormSettings = new fSettings();
        private String appPath = Application.ExecutablePath;
        private String _ProjectPath ;
        private String _CurrentPath ;
        private DataBaseSqlite database;
        private Func<object, int, int> getIngValue;
        private Func<object, string> getStringValue;
        private fSearchReplace searchForm;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private ContextMenuStrip notifyContextMenu; // Контекстное меню для иконки в системном трее
        private DateTime LastDateCheck = DateTime.Now.AddDays(-3);
        private String formTitle = "Проверка позиций сайта в поисковой выдаче Яндекс";
        private bool isProcessCheckin = false;
        private CancellationTokenSource domainCheckCancellation;
        private System.Threading.Timer periodicCheckTimer;
        // Добавляем формы справочников для управления хостингами и CPA сетями
        private HostingList hostingListForm;
        private CpaList cpaListForm;
        // Форма аналитики для результатов парсеров
        private AnalysisDataForm analysisDataForm;
        private TechnicalSeoChecker checker;
        // Храним интервалы периодического обновления и срок устаревания записей
        private static readonly TimeSpan periodicCheckInterval = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan staleCheckThreshold = TimeSpan.FromDays(7);
        private const string StatusCheckingText = "идет проверка доменов";
        private static readonly TimeSpan[] domainCheckRetryDelays = new[] // График повторов проверки доменов с нарастающими задержками
        {
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(3),
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(5)
        };
        // Определяем разделители IP-адресов для поиска в списке хостингов
        private static readonly char[] HostingIpSeparators = new[] { ' ', ',', ';', '\t', '\r', '\n' };
        // Создаем элементы управления для выбора хостинга из таблицы
        private ComboBox hostingSelector;
        private int hostingSelectorRowIndex = -1;
        // Создаем элементы управления для выбора CPA сети из таблицы
        private ComboBox cpaSelector;
        private int cpaSelectorRowIndex = -1;
        // Создаем HTTP-клиент для загрузки страниц при поиске CPA
        private static readonly HttpClient cpaHttpClient = CreateCpaHttpClient();

        public Form1(string[] args, string title)
        {
            InitializeComponent();

            this.Text = title;
            _CurrentPath = Path.GetDirectoryName(Application.ExecutablePath).Replace("CheckPosition.exe", "");
            database = new DataBaseSqlite(_CurrentPath);
            _ProjectPath = Path.Combine(_CurrentPath, "Projects");

            Directory.SetCurrentDirectory(_CurrentPath);
            od.InitialDirectory = _ProjectPath;
            sd.InitialDirectory = _ProjectPath;
            dg.ClipboardCopyMode = DataGridViewClipboardCopyMode.Disable;
            dg.MultiSelect = false;
            this.getIngValue = Helper.getIngValue;
            this.getStringValue = Helper.getStringValue;
            string[] deletedMessage = new string[] { "Данное объявление было удалено, перемещено или временно приостановлено" };
            IdnMapping idn = new IdnMapping();

            // Инициализируем выпадающий список для назначения хостинга по двойному щелчку
            hostingSelector = new ComboBox
            {
                Visible = false,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FormattingEnabled = true
            };
            hostingSelector.SelectionChangeCommitted += HostingSelector_SelectionChangeCommitted;
            hostingSelector.LostFocus += HostingSelector_LostFocus;
            hostingSelector.KeyDown += HostingSelector_KeyDown;
            dg.Controls.Add(hostingSelector);
            // Инициализируем выпадающий список для назначения CPA по двойному щелчку
            cpaSelector = new ComboBox
            {
                Visible = false,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FormattingEnabled = true
            };
            cpaSelector.SelectionChangeCommitted += CpaSelector_SelectionChangeCommitted;
            cpaSelector.LostFocus += CpaSelector_LostFocus;
            cpaSelector.KeyDown += CpaSelector_KeyDown;
            dg.Controls.Add(cpaSelector);
            dg.Scroll += dg_Scroll;
            dg.ColumnWidthChanged += dg_ColumnWidthChanged;
            dg.RowHeightChanged += dg_RowHeightChanged;
            dg.CellLeave += dg_CellLeave;
        
            searchForm = new fSearchReplace();
         

            if (Properties.Settings.Default.LastDateCheck != null && Properties.Settings.Default.LastDateCheck is DateTime)
                LastDateCheck = Properties.Settings.Default.LastDateCheck;

           

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            var resourceNames = assembly.GetManifestResourceNames();
            foreach (var resourceName in resourceNames)
            {
                Console.WriteLine(resourceName);
            }

            notifyIcon = new System.Windows.Forms.NotifyIcon();
            //CheckPosition.Properties.Resources.resources

            using (System.IO.Stream stream = assembly.GetManifestResourceStream("CheckPosition.Resources.favicon.ico"))
            {
                notifyIcon.Icon = new System.Drawing.Icon(stream);
            }
            notifyIcon.Visible = true;
            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
            notifyContextMenu = new ContextMenuStrip(); // Создаем контекстное меню для управления приложением из трея
            
            var showMenuItem = new ToolStripMenuItem("Открыть"); // Добавляем пункт выхода
            showMenuItem.Click += (sender, e) => mOpen_Click(sender, e); // Используем уже существующую логику завершения приложения
            notifyContextMenu.Items.Add(showMenuItem); // Регистрируем пункт в меню трея

            var separator = new ToolStripSeparator();
            notifyContextMenu.Items.Add(separator);

            var exitMenuItem = new ToolStripMenuItem("Выход"); // Добавляем пункт выхода
            exitMenuItem.Click += (sender, e) => mExit_Click(sender, e); // Используем уже существующую логику завершения приложения
            notifyContextMenu.Items.Add(exitMenuItem); // Регистрируем пункт в меню трея

            notifyIcon.ContextMenuStrip = notifyContextMenu; // Привязываем меню к иконке в трее

            foreach (string arg in args)
                if (arg == "--intray"){
                    var t = new System.Timers.Timer(100);
                    t.Elapsed += (sender, e) =>
                    {
                        this.WindowState = FormWindowState.Minimized;
                        this.Hide();
                        t.Stop();
                    };
                    t.Start();
                    break;
                }
            AddToStartup();
            //  test();
            checker = new TechnicalSeoChecker();

            InitializePeriodicCheckTimer();

            //var apiKey = Environment.GetEnvironmentVariable("PSI_API_KEY") ?? "AIzaSyCwiCkltlDMBBzNlW5R_mKnfROT8LBPWoI";
            //var client = new PageSpeedInsightsClient(apiKey);
            //var url = "https://hotel-olginka.ru";

            //var m = client.GetMetricsAsync(url, Strategy.Mobile, locale: "ru").GetAwaiter().GetResult();
            //Console.WriteLine("Performance score: " + m.PerformanceScore);
            //Console.WriteLine("LCP ms: " + m.LargestContentfulPaintMs);
            //Console.WriteLine("CLS: " + m.CumulativeLayoutShift);
            //Console.WriteLine("Requests: " + m.NetworkRequestsCount);
            //Console.WriteLine("Total bytes: " + m.TotalByteWeight);
            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            //var client = new WordParserClient("https://txtxt.ru/lemmatizer/api/v1/text/parse_by_url");

            //var url = "https://agat123.ru/";
            //var keyword = "Гостевой дом Агат Кабардинка";

            //// синхронный вызов без async/await:
            //var m = client.ParseAsync(url, keyword, includeRawJson: false)
            //              .GetAwaiter()
            //              .GetResult();

            //Console.WriteLine("TotalWords: " + m.TotalWords);
            //Console.WriteLine("H1: " + m.H1Count);
            //Console.WriteLine("KW in title: " + m.KeywordWordsInTitle);
            //Console.WriteLine("Flesch: " + m.FleschReadingEase);
 
        }

        // Создаем и настраиваем HTTP-клиент для скачивания страниц при определении CPA
        private static HttpClient CreateCpaHttpClient()
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            var client = new HttpClient(handler, disposeHandler: true)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            client.DefaultRequestHeaders.UserAgent.ParseAdd("CheckPosition/1.0 (+https://example.local)");
            client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate");
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

            return client;
        }

        // Настраиваем периодический таймер, запускающий проверку каждые 10 минут
        private void InitializePeriodicCheckTimer()
        {
            periodicCheckTimer = new System.Threading.Timer(_ =>
            {
                try
                {
                    if (!IsHandleCreated) return;
                    BeginInvoke(new Action(RunAutomaticDomainCheck));
                }
                catch (ObjectDisposedException)
                {
                    // Игнорируем, если форма уже уничтожена
                }
            }, null, periodicCheckInterval, periodicCheckInterval);
        }

        // Описываем данные, необходимые для фоновой проверки конкретной строки
        private sealed class DomainCheckRequest
        {
            public int RowIndex { get; set; }
            public string Url { get; set; }
            public string Keyword { get; set; }

            public DomainCheckRequest(int rowIndex, string url, string keyword)
            {
                RowIndex = rowIndex;
                Url = url;
                Keyword = keyword;
            }
        }

        // Результат проверки домена, требующий применения в UI-потоке
        private sealed class DomainCheckResult
        {
            public DomainCheckRequest Request { get; set; }
            public int Position { get; set; }
            public int AveragePosition { get; set; }
            public string FoundPageUrl { get; set; }
            public string Competitors { get; set; }
            public DateTime CheckDate { get; set; }
        }

        private sealed class DomainCheckExecutionException : Exception // Исключение с дополнительной диагностикой по прерванной проверке
        {
            public int SuccessfulCount { get; }
            public int LastAttemptedIndex { get; }

            public DomainCheckExecutionException(int successfulCount, int lastAttemptedIndex, Exception inner) : base(inner?.Message, inner)
            {
                SuccessfulCount = successfulCount;
                LastAttemptedIndex = lastAttemptedIndex;
            }
        }

        // Запускаем автоматическую проверку строк, устаревших более чем на заданный срок
        private void RunAutomaticDomainCheck()
        {
            if (isProcessCheckin) return;
            if (dg.RowCount == 0) return;

            var indexes = new List<int>();
            for (int i = 0; i < dg.RowCount; i++)
            {
                if (dg.Rows[i].IsNewRow) continue;
                if (ShouldCheckRowAutomatically(i)) indexes.Add(i);
            }

            StartDomainCheckForIndexes(indexes, true);
        }

        // Готовим и запускаем асинхронную проверку для выбранных строк
        private void StartDomainCheckForIndexes(List<int> indexes, bool triggeredAutomatically)
        {
            if (indexes == null || indexes.Count == 0) return;
            if (isProcessCheckin) return;

            var requests = CollectRequests(indexes);
            if (requests.Count == 0)
            {
                UpdateStatusTextSafe(triggeredAutomatically ? string.Empty : "Нет подходящих записей для проверки");
                statusProgressBar.Visible = false;
                return;
            }

            _ = RunDomainChecksAsync(requests, triggeredAutomatically);
        }

        // Считываем данные строк и готовим заявки на проверку доменов
        private List<DomainCheckRequest> CollectRequests(IEnumerable<int> indexes)
        {
            var requests = new List<DomainCheckRequest>();
            foreach (var index in indexes)
            {
                if (index < 0 || index >= dg.Rows.Count) continue;
                var row = dg.Rows[index];
                if (row.IsNewRow) continue;

                string url = getStringValue(row.Cells[colPageUrl.Index].Value);
                string keyword = getStringValue(row.Cells[colKeyword.Index].Value);
                if (string.IsNullOrWhiteSpace(url) || !url.Trim().StartsWith("http", StringComparison.OrdinalIgnoreCase)) continue;
                if (string.IsNullOrWhiteSpace(keyword)) continue;

                requests.Add(new DomainCheckRequest(index, url.Trim(), keyword.Trim()) );
            }
            return requests;
        }

        // Асинхронно выполняем проверку доменов и обновляем интерфейс
        private async Task RunDomainChecksAsync(List<DomainCheckRequest> requests, bool triggeredAutomatically)
        {
            if (requests.Count == 0) return;

            isProcessCheckin = true;
            domainCheckCancellation?.Cancel();
            domainCheckCancellation?.Dispose();
            domainCheckCancellation = new CancellationTokenSource();
            var token = domainCheckCancellation.Token;

            PrepareUiForCheck(requests.Count);

            int successfulCount = 0;
            bool completedSuccessfully = false;
            Exception lastError = null;

            try
            {
                for (int attempt = 1; attempt <= domainCheckRetryDelays.Length; attempt++)
                {
                    try
                    {
                        // Запускаем проверку доменов в отдельном потоке, чтобы не блокировать интерфейс
                        var summary = await Task.Run(() => ProcessDomainChecks(requests, triggeredAutomatically, token), token);
                        successfulCount = summary.SuccessfulCount;
                        completedSuccessfully = true;
                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (DomainCheckExecutionException ex)
                    {
                        successfulCount = ex.SuccessfulCount; // Сохраняем количество успешных строк к моменту ошибки
                        lastError = ex.InnerException ?? ex; // Запоминаем исходную причину сбоя для сообщения пользователю
                    }
                    catch (Exception ex)
                    {
                        lastError = ex; // Храним последнюю ошибку для последующей обработки
                    }

                    if (attempt == domainCheckRetryDelays.Length)
                    {
                        break; // Завершаем попытки, если достигли лимита
                    }

                    var delay = domainCheckRetryDelays[attempt];
                    if (delay > TimeSpan.Zero)
                    {
                        // Уведомляем пользователя о повторной попытке с задержкой и ждем указанное время
                        var delayMinutes = (int)Math.Round(delay.TotalMinutes);
                        var attemptLabel = $"{attempt + 1}/{domainCheckRetryDelays.Length}"; // Формируем человекочитаемое отображение номера попытки
                        var retryMessage = $"Ошибка проверки доменов: {lastError?.Message}."; // Готовим сообщение об ошибке для статуса
                        retryMessage += $" Повторная попытка {attemptLabel} через {delayMinutes} мин."; // Дополняем сообщение сведениями о следующем запуске
                        UpdateStatusTextSafe(retryMessage);
                        await Task.Delay(delay, token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                UpdateStatusTextSafe("Проверка остановлена пользователем");
            }

           
            finally
            {
                if (!completedSuccessfully && lastError != null)
                {
                    // Сообщаем пользователю о неудавшейся проверке после всех повторов
                    HandleDomainCheckError(triggeredAutomatically, lastError);
                }
                FinalizeDomainCheck(triggeredAutomatically, successfulCount, requests.Count);
                domainCheckCancellation?.Dispose();
                domainCheckCancellation = null;
                isProcessCheckin = false;
            }
        }

        // Выполняем сетевые вызовы и формируем результаты проверок доменов
        private (int SuccessfulCount, int LastAttemptedIndex) ProcessDomainChecks(List<DomainCheckRequest> requests, bool triggeredAutomatically, CancellationToken token)
        {
            int successfulCount = 0;
            int lastAttemptedIndex = -1;

            for (int i = 0; i < requests.Count; i++)
            {
                token.ThrowIfCancellationRequested();
                var request = requests[i];
                lastAttemptedIndex = request.RowIndex;

                try
                {
                    var result = ExecuteDomainCheck(request, token);
                    ApplyDomainCheckResultSafe(result);
                    UpdateProgressSafe(i + 1, requests.Count);
                    successfulCount++;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new DomainCheckExecutionException(successfulCount, lastAttemptedIndex, ex); // Прерываем обработку, сохранив статистику по выполненным строкам
                }
            }

            return (successfulCount, lastAttemptedIndex);
        }

        // Выполняем запросы к XML API и высчитываем позицию домена
        // Выполняем проверку позиции и собираем информацию по конкурентам
        private DomainCheckResult ExecuteDomainCheck(DomainCheckRequest request, CancellationToken token)
        {
            var attempts = new List<int>();
            int bestPosition = int.MaxValue;
            string foundPageUrl = string.Empty;
            string competitors = string.Empty;

            for (int attempt = 0; attempt < 3; attempt++)
            {
                token.ThrowIfCancellationRequested();

                int position = getPosition(request.Keyword, request.Url, out var foundUrl, out var competitorList, "2", token);
                if (position < 0) throw new InvalidOperationException("Ошибка получения позиции домена");
                bestPosition = Math.Min(bestPosition, position);
                attempts.Add(position);
                foundPageUrl = foundUrl;
                competitors = competitorList;
                Task.Delay(TimeSpan.FromSeconds(1), token).Wait(token);
            }

            if (bestPosition == -1) throw new InvalidOperationException("Ошибка работы XML сервиса");

            int averagePosition = attempts.Count > 0 ? (int)attempts.Average() : 0;
            return new DomainCheckResult
            {
                Request = request,
                Position = bestPosition,
                AveragePosition = averagePosition,
                FoundPageUrl = foundPageUrl,
                Competitors = competitors,
                CheckDate = DateTime.Now
            };
        }

        // Применяем результаты проверки в потоке интерфейса
        private void ApplyDomainCheckResultSafe(DomainCheckResult result)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<DomainCheckResult>(ApplyDomainCheckResultSafe), result);
                return;
            }
            ApplyDomainCheckResult(result);
        }

        // Обновляем данные строки, сохраняем результат и пишем историю проверок
        private void ApplyDomainCheckResult(DomainCheckResult result)
        {
            var index = result.Request.RowIndex;
            if (index < 0 || index >= dg.Rows.Count) return;
            var row = dg.Rows[index];
            if (row.IsNewRow) return;

            int prevPosition = getIngValue(row.Cells[colCurrentPosition.Index].Value, 0);
            int prevMidPosition = getIngValue(row.Cells[colMidCurrent.Index].Value, 0);

            row.Cells[colCurrentPosition.Index].Value = result.Position;
            row.Cells[colMidCurrent.Index].Value = result.AveragePosition;
            row.Cells[colLastPosition.Index].Value = prevPosition;
            row.Cells[colMidPrev.Index].Value = prevMidPosition;
            row.Cells[colFoundPageUrl.Index].Value = result.FoundPageUrl;
            row.Cells[colCompetitor.Index].Value = result.Competitors;
            row.Cells[colDateCheck.Index].Value = result.CheckDate.ToString("dd.MM.yy");

            saveRow(index);

            int id = getIngValue(row.Cells[colID.Index].Value, -1);
            if (id >= 0)
                this.database.insertChecks(id, result.CheckDate.ToString("dd.MM.yy"), result.Position, result.AveragePosition);
        }

        // Обновляем прогресс проверки в статус-баре
        private void UpdateProgressSafe(int current, int total)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<int, int>(UpdateProgressSafe), current, total);
                return;
            }

            int safeTotal = Math.Max(1, total);
            statusProgressBar.Maximum = safeTotal;
            statusProgressBar.Value = Math.Min(current, safeTotal);
            statusProgressBar.Visible = true;
            statusLabel.Visible = true;
            statusLabel.Text = $"{StatusCheckingText}: {current}/{total}";
        }

        // Подготавливаем интерфейс перед запуском фоновой проверки
        private void PrepareUiForCheck(int total)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<int>(PrepareUiForCheck), total);
                return;
            }
             
            statusProgressBar.Value = 0;
            statusProgressBar.Maximum = Math.Max(1, total);
            statusProgressBar.Visible = true;
            statusLabel.Text = StatusCheckingText;
            statusLabel.Visible = true;
        }

        // Завершаем проверку и возвращаем интерфейс в исходное состояние
        private void FinalizeDomainCheck(bool triggeredAutomatically, int successfulCount, int totalCount)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<bool, int, int>(FinalizeDomainCheck), triggeredAutomatically, successfulCount, totalCount);
                return;
            }
             
            statusProgressBar.Visible = false;
            statusLabel.Visible = false;
            statusLabel.Text = string.Empty;
            colorize();

            bool needUpdateLastDate = totalCount > 0 && successfulCount > 0 && (triggeredAutomatically || successfulCount == totalCount);
            if (needUpdateLastDate)
            {
                LastDateCheck = DateTime.Now;
                Properties.Settings.Default.LastDateCheck = LastDateCheck;
                Properties.Settings.Default.Save();
                this.Text = this.formTitle + " - последняя проверка от " + LastDateCheck.ToString("dd.MM.yy");
            }
        }

        // Безопасно обновляем текст статуса в строке состояния
        private void UpdateStatusTextSafe(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(UpdateStatusTextSafe), text);
                return;
            }
            statusLabel.Text = text;
            statusLabel.Visible = !string.IsNullOrWhiteSpace(text);
            if (!statusLabel.Visible) statusProgressBar.Visible = false;
        }

        // Отвечаем за обработку ошибок проверки и уведомление пользователя
        private void HandleDomainCheckError(bool triggeredAutomatically, Exception ex)
        {
            string message = ex?.Message ?? "Неизвестная ошибка";
            if (triggeredAutomatically)
            {
                UpdateStatusTextSafe($"Ошибка проверки доменов: {message}"); // Обновляем строку состояния при автоматическом запуске
            }

            void ShowErrorMessage()
            {
                MessageBox.Show($"Ошибка проверки домена: {message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (InvokeRequired)
            {
                BeginInvoke(new Action(ShowErrorMessage)); // Показываем всплывающее сообщение о неудачной проверке
            }
            else
            {
                ShowErrorMessage(); // Немедленно уведомляем пользователя о повторяющейся проблеме
            }
        }
        private async void test()
        {
            var visitorsWithoutBots = await YandexMetricChecker.GetVisitorsWithoutBots("96666372");
            Console.WriteLine($"Количество посетителей без роботов: {visitorsWithoutBots}");
        }

        public void AddToStartup()
        {
            // Используем using, чтобы гарантировать закрытие ключа реестра
            using (RegistryKey reg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if (reg.GetValue(appName) == null)
                {
                    reg.SetValue(appName, $"\"{appPath}\"");
                }
            }
        }

        private bool ShouldCheckRowAutomatically(int rowIndex)
        {
            // Определяем, устарела ли запись и требует ли она автоматической перепроверки
            var cellValue = dg.Rows[rowIndex].Cells[colDateCheck.Index].Value;
            if (cellValue == null) return true;
            var raw = cellValue.ToString();
            if (string.IsNullOrWhiteSpace(raw)) return true;

            string[] formats = { "dd.MM.yy", "dd.MM.yyyy", "dd.MM.yy HH:mm", "dd.MM.yyyy HH:mm", "dd.MM.yy H:mm", "dd.MM.yyyy H:mm" };
            if (DateTime.TryParseExact(raw, formats, CultureInfo.GetCultureInfo("ru-RU"), DateTimeStyles.None, out var lastCheck))
            {
                var nowDate = DateTime.Now.Date;
                var checkDate = lastCheck.Date;
                return (nowDate - checkDate) >= staleCheckThreshold;
            }

            if (DateTime.TryParse(raw, out lastCheck))
            {
                var nowDate = DateTime.Now.Date;
                var checkDate = lastCheck.Date;
                return (nowDate - checkDate) >= staleCheckThreshold;
            }

            return true;
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon.Visible = false;
        }
        private void Form1_Shown(object sender, EventArgs e)
        {

            this.database.getTableData(this.dg, "sites");
            colorize();

            // Обновляем заголовок, если известна дата последней проверки
            if (LastDateCheck > DateTime.MinValue)
                this.Text = this.formTitle + " - последняя проверка от " + LastDateCheck.ToString("dd.MM.yy");

        }

        private void colorize()
        {
            for (int i = 0; i < this.dg.RowCount; i++)
            {
                if (this.dg.Rows[i].IsNewRow) continue;
                try
                {
                    int cur = int.Parse(this.dg.Rows[i].Cells[colCurrentPosition.Index].Value.ToString());
                    int prv = int.Parse(this.dg.Rows[i].Cells[colLastPosition.Index].Value.ToString());

                    this.dg.Rows[i].Cells[colCurrentPosition.Index].Style.BackColor =  cur < prv || (cur > 0 && prv ==0) ? Color.LightGreen : cur == 0 || cur>prv ? Color.LightPink : Color.White;
                }
                catch { continue; }

            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.LastProjectFileName = _CurrentProjectFileName;
            Properties.Settings.Default.Save();
        }

     
        // Определяем позицию домена и список конкурентов по выдаче
        private int getPosition(String keyword, String findUrl, out String url, out String competitors, String Region = "2", CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            url = "";
            competitors = string.Empty;
            String urlXML = Properties.Settings.Default.XMLURL;
            //urlXML += "&query=" + keyword + "&lr=" + Region + "&lr=2&l10n=ru&sortby=rlv&filter=none&maxpassages=1&groupby=attr%3Dd.mode%3Ddeep.groups-on-page%3D100.docs-in-group%3D1&page=0";
            urlXML += keyword;

            String text = "";
            using (WebClient webClient = new WebClient())
            {
                // Некоторые сайты требуют наличия User-Agent
                webClient.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US; rv:1.9.2.6) Gecko/20100625 Firefox/3.6.6 (.NET CLR 3.5.30729)");
                webClient.Headers.Add("Auth-my", "CheckerPosition");
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                ServicePointManager.ServerCertificateValidationCallback += (send, certificate, chain, sslPolicyErrors) =>
                {
                    return true;
                };

                try
                {
                    token.ThrowIfCancellationRequested();
                    //https://довериевсети.рф/site/ifish2.ru
                    var data = webClient.DownloadData(new Uri(urlXML));
                    token.ThrowIfCancellationRequested();
                    text = Encoding.UTF8.GetString(data);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Ошибка загрузки данных: {ex.Message}", ex);
                }
            }

            if (string.IsNullOrWhiteSpace(text)) throw new InvalidOperationException("Ошибка получения данных поиска");


            String regExp = @"error code=(.*?)>(.*?)<";
            String mText = System.Xml.Linq.XDocument.Parse(text).ToString();
            Regex regex = new Regex(regExp, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            MatchCollection matches = regex.Matches(mText);
            if (matches != null && matches.Count > 0 && matches[0].Groups.Count == 3)
            {
                String errorCode = matches[0].Groups[1].Value;
                String errorText =  matches[0].Groups[2].Value ;
                throw new InvalidOperationException($"Ошибка {errorCode} {errorText}");
            }

            List<string> urls = listUrl(text);
            competitors = BuildCompetitorList(urls, findUrl);
            int pos = FindUrl(urls, findUrl);
            if (pos > 0) url = "OK";
            else
            {
                pos = FindUrl(urls, findUrl, false);
                if (pos > 0) url = urls[pos-1];
            }

            return pos;
        }

        // Формируем список конкурентов из первых 10 результатов, ограничиваясь главными страницами других доменов
        private string BuildCompetitorList(IReadOnlyList<string> urls, string ownUrl)
        {
            if (urls == null || urls.Count == 0) return string.Empty;

            string ownDomain = TryGetDomain(ownUrl);
            var competitors = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int limit = Math.Min(10, urls.Count);

            for (int i = 0; i < limit; i++)
            {
                string candidate = urls[i];
                if (!TryCreateUri(candidate, out var uri)) continue;
                if (!IsMainPage(uri)) continue;

                string candidateDomain = uri.Host;
                if (string.IsNullOrWhiteSpace(candidateDomain)) continue;
                if (!string.IsNullOrWhiteSpace(ownDomain) && string.Equals(candidateDomain, ownDomain, StringComparison.OrdinalIgnoreCase)) continue;

                competitors.Add(candidateDomain);
            }

            return string.Join(",", competitors);
        }

        // Пытаемся получить домен из входного URL, возвращаем пустую строку при ошибке
        private string TryGetDomain(string url)
        {
            if (!TryCreateUri(url, out var uri)) return string.Empty;
            return uri.Host ?? string.Empty;
        }

        // Безопасно создаем Uri, чтобы не падать на мусорных данных из выдачи
        private bool TryCreateUri(string url, out Uri uri)
        {
            uri = null;
            if (string.IsNullOrWhiteSpace(url)) return false;
            return Uri.TryCreate(url.Trim(), UriKind.Absolute, out uri);
        }


        private  bool IsMainPage(Uri uri)
        {
            if (uri == null) return false;

            // без параметров и якоря
            if (!string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
                return false;

            var path = uri.AbsolutePath ?? string.Empty;

            // домен или /
            if (string.IsNullOrEmpty(path) || path == "/")
                return true;

            // нормализуем хвост: /index.html -> index.html, / -> ""
            var fileName = Path.GetFileName(path.TrimEnd('/'));
            if (string.IsNullOrEmpty(fileName))
                return true; // путь заканчивается на /

            // index.* (любой регистр)
            // index, index.html, index.php, index.aspx, index.htm и т.д.
            var name = Path.GetFileNameWithoutExtension(fileName);
            if (!string.IsNullOrEmpty(name) && name.Equals("index", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        // Нормализуем URL главной страницы в компактный вид для сохранения в базе
        private string NormalizeMainPageUrl(Uri uri)
        {
            if (uri == null) return string.Empty;
            string baseUrl = uri.GetLeftPart(UriPartial.Authority);
            if (string.IsNullOrWhiteSpace(baseUrl)) return string.Empty;
            return baseUrl.TrimEnd('/') + "/";
        }

        private int FindUrl(List<string> lStr, String url, bool isStrong = true)
        {
            url = url.Replace("\t", "").Trim();
            Uri uri = new Uri(url);
            // Получение домена
            string domain = uri.Host.ToLower();
            for (int i = 0; i < lStr.Count; i++)
                if ( (lStr[i].Trim().ToLower() == url.Trim().ToLower() && isStrong) ||
                      (lStr[i].Trim().ToLower().IndexOf(domain) > 0 && !isStrong))
                     return i+1;

            return 0;
        }

        private List<string> listUrl(String text)
        {
            List<string> str = text.Split(new string[] { "<url>" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            for (int i = 0; i < str.Count; i++)
            {
                if (str[i].IndexOf("</url>")<=0) { str.RemoveAt(i);i--; continue; }

                str[i] = str[i].Remove(str[i].IndexOf("</url>")).Replace("\r", "").Replace("\n", "");
            }
            return str;
        }
        

        private void mSettings_Click(object sender, EventArgs e)
        {
            // Просто открываем окно настроек без дополнительной логики расписания
            _FormSettings.ShowDialog();
        }



        private void dg_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            e.Cancel = MessageBox.Show("Really delete ?", "", MessageBoxButtons.OKCancel) == DialogResult.Cancel;
            if (e.Cancel) return;
            int id = int.Parse(this.dg.Rows[e.Row.Index].Cells[colID.Index].Value.ToString());
            this.database.removeRecord(id, "sites");
        }

        private void saveRow(int index)
        {


            DataGridViewRow row = dg.Rows[index];   
            int id = getIngValue(row.Cells[colID.Index].Value, -1);
              
            int position = getIngValue(row.Cells[colCurrentPosition.Index].Value, 0); 
            int middle_position = getIngValue(row.Cells[colMidCurrent.Index].Value, 0); 
            int prev_pos = getIngValue(row.Cells[colLastPosition.Index].Value, 0);  
            int middle_prev_pos = getIngValue(row.Cells[colMidPrev.Index].Value, 0);  
            
            string foundPageUrl = getStringValue(row.Cells[colFoundPageUrl.Index].Value);
            string competitors   = getStringValue(row.Cells[colCompetitor.Index].Value);
            string url          = getStringValue(row.Cells[colPageUrl.Index].Value);
            string keyword      = getStringValue(row.Cells[colKeyword.Index].Value);
            string dateNow      = getStringValue(row.Cells[colDateCheck.Index].Value);
            string coment       = getStringValue(row.Cells[colAction.Index].Value) ;
            string status       = getStringValue(row.Cells[colStatus.Index].Value) ;
            // Извлекаем выбранный идентификатор хостинга из скрытого столбца
            long hostingId = ParseHostingId(row.Cells[colHostingId.Index].Value);
            long effectiveSiteId = id;

            if (id != -1)
                this.database.updateSite(id, dateNow, url, keyword, position, middle_position, prev_pos, middle_prev_pos, foundPageUrl, competitors, coment, status);
            else
            {
               long id_row =  this.database.appendSite(dateNow, url, keyword, position, prev_pos, foundPageUrl, competitors, coment, status);
                if (id_row >= 0)
                {
                    row.Cells[colID.Index].Value = id_row;
                    effectiveSiteId = id_row;
                }
            }

            if (effectiveSiteId > 0)
            {
                try
                {
                    // Обновляем поле hosting_id сразу после сохранения основной части строки
                    database.UpdateSiteHosting(effectiveSiteId, hostingId > 0 ? (long?)hostingId : null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось сохранить выбранный хостинг: {ex.Message}", "Назначение хостинга", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void mStart_Click(object sender, EventArgs e)
        {
            // Запускаем полную проверку доменов от первой строки
            checkDomains(0);
        }


        private void mContinue_Click(object sender, EventArgs e)
        {
            int startFrom = 0;
            if (dg.SelectedRows.Count > 0)
                startFrom = dg.SelectedRows[0].Cells[0].RowIndex;
            checkDomains(startFrom);
        }

        private void checkDomains(int startFrom = 0, bool skipRecent = false, bool triggeredAutomatically = false)
        {
            // Запускаем проверку доменов в отдельном потоке, формируя список индексов для обработки
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.XMLURL))
            {
                MessageBox.Show("Не задан XML Url для запросов!");
                return;
            }

            int safeStart = Math.Max(0, startFrom);
            var indexes = new List<int>();
            for (int i = safeStart; i < dg.RowCount; i++)
            {
                if (dg.Rows[i].IsNewRow) continue;
                bool sh = ShouldCheckRowAutomatically(i);
                if (skipRecent || !sh) continue;
                indexes.Add(i);
            }

            StartDomainCheckForIndexes(indexes, triggeredAutomatically);
        }


        private void checkRow_Click(object sender, EventArgs e)
        {
            if (dg.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите строку для проверки!");
                return;
            }

            // Собираем индексы выбранных строк для точечной проверки
            var indexes = new List<int>();
            foreach (DataGridViewRow row in dg.SelectedRows)
            {
                if (row == null || row.IsNewRow || !ShouldCheckRowAutomatically(row.Index))
                {
                  if (sender != checkRow)  continue;
                }
                indexes.Add(row.Index);
            }

            if (indexes.Count == 0)
            {
                MessageBox.Show("Выберите строку для проверки!");
                return;
            }

            // Запускаем асинхронную проверку только для выделенных позиций
            StartDomainCheckForIndexes(indexes, false);
        }

        // Открывает форму графика истории для выбранной строки
        private void showHistoryMenuItem_Click(object sender, EventArgs e)
        {
            // Проверяем наличие выделенной строки перед открытием истории
            if (dg.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите строку для просмотра истории.");
                return;
            }

            var selectedRow = dg.SelectedRows[0];
            // Извлекаем идентификатор сайта из скрытого столбца
            string rawId = Convert.ToString(selectedRow.Cells[colID.Index].Value);
            if (!int.TryParse(rawId, out int siteId) || siteId <= 0)
            {
                MessageBox.Show("Не удалось определить сайт для отображения истории.");
                return;
            }

            try
            {
                // Загружаем исторические данные из базы
                var history = database.GetCheckHistory(siteId);
                string pageUrl = Convert.ToString(selectedRow.Cells[colPageUrl.Index].Value) ?? string.Empty;
                string keyword = Convert.ToString(selectedRow.Cells[colKeyword.Index].Value) ?? string.Empty;
                string caption = string.Join(" | ", new[] { pageUrl.Trim(), keyword.Trim() }.Where(text => !string.IsNullOrEmpty(text)));

                using (var historyForm = new SiteHistoryForm(caption, history))
                {
                    // Показываем форму истории как модальное окно
                    historyForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                // Сообщаем пользователю об ошибке загрузки истории
                MessageBox.Show($"Не удалось загрузить историю позиций: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void rowUp_Click(object sender, EventArgs e)
        {
            int increment = 1;
            if (sender == rowUp) increment = -1;
            int index = dg.SelectedRows[0].Index;
            if ((index + increment) < 0 || (index + increment) >= dg.Rows.Count)
                return;

            DataGridViewRow row1 = (DataGridViewRow)dg.Rows[index];
            DataGridViewRow row2 = (DataGridViewRow)dg.Rows[index + increment];
            // Удаляем строки из таблицы
            dg.Rows.Remove(row1);
            dg.Rows.Remove(row2);
            // Вставляем строки на новые позиции
            if (index>= dg.Rows.Count) dg.Rows.Add(row2);
            else dg.Rows.Insert(index, row2);
            if ((index + increment) >= dg.Rows.Count) dg.Rows.Add(row1);
            else dg.Rows.Insert(index + increment, row1);
            dg.Rows[index + increment].Selected = true;
            

        }

        private void dg_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            this.saveRow(e.RowIndex);
        }

        private void dg_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (e.ColumnIndex == colHostingName.Index)
            {
                // Открываем выпадающий список выбора хостинга для текущей строки
                ShowHostingSelector(e.RowIndex);
                return;
            }

            if (e.ColumnIndex == colCpaName.Index)
            {
                // Открываем выпадающий список выбора CPA сети для текущей строки
                ShowCpaSelector(e.RowIndex);
                return;
            }

            if ((Control.ModifierKeys & Keys.Control) == 0) return;
            if (e.ColumnIndex != colPageUrl.Index) return;
            string url =  getStringValue(dg.Rows[e.RowIndex].Cells[colPageUrl.Index].Value);
            if (url == null) return;
            Process.Start(url);

        }

        private void dg_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            e.Cancel = (Control.ModifierKeys & Keys.Control) != 0;
        }

        private void showDomainListForm_Click(object sender, EventArgs e)
        {
            using (DomainList f = new DomainList(this.database))
            {
                f.ShowDialog();
            }
        }

        private void showHostingListMenuItem_Click(object sender, EventArgs e)
        {
            // Открываем форму хостингов и делаем ее активной для пользователя
            if (hostingListForm == null || hostingListForm.IsDisposed)
            {
                hostingListForm = new HostingList(database);
            }

            hostingListForm.Show(this);
            hostingListForm.WindowState = FormWindowState.Normal;
            hostingListForm.BringToFront();
            hostingListForm.Activate();
        }

        private void showCpaListMenuItem_Click(object sender, EventArgs e)
        {
            // Аналогично открываем справочник CPA сетей и передаем управление пользователю
            if (cpaListForm == null || cpaListForm.IsDisposed)
            {
                cpaListForm = new CpaList(database);
            }

            cpaListForm.Show(this);
            cpaListForm.WindowState = FormWindowState.Normal;
            cpaListForm.BringToFront();
            cpaListForm.Activate();
        }

        private void analysisDataMenuItem_Click(object sender, EventArgs e)
        {
            // Открываем форму анализа данных и активируем ее поверх других окон
            if (analysisDataForm == null || analysisDataForm.IsDisposed)
            {
                analysisDataForm = new AnalysisDataForm(database);
            }

            analysisDataForm.Show(this);
            analysisDataForm.WindowState = FormWindowState.Normal;
            analysisDataForm.BringToFront();
            analysisDataForm.Activate();
        }

        private void bYandexMetrica_Click(object sender, EventArgs e)
        {
           using(YandexForm form = new YandexForm())
            {
                form.ShowDialog();
                String acc = form.access_key;

                form.getStatisticById(94131720);
            }
        }

        private void bSearch_Click(object sender, EventArgs e)
        {
            int lastSearchIndex = -1;
            string lastSearchText = string.Empty;

            int searchText(string text)
            {
                if (string.IsNullOrEmpty(text)) return 0;
                if (text != lastSearchText) lastSearchIndex = -1;
                lastSearchText = text;

                bool bFound = false;
                for (int i = lastSearchIndex+1; i < dg.Rows.Count - 1; i++)
                    if (dg.Rows[i].Cells[colPageUrl.Index].Value != null)
                    {
                        string tmp = dg.Rows[i].Cells[colPageUrl.Index].Value.ToString().ToLower();
                        string tmp2 = dg.Rows[i].Cells[colKeyword.Index].Value != null ? dg.Rows[i].Cells[colKeyword.Index].Value.ToString().ToLower() : "";
                        if (tmp.IndexOf(text.ToLower()) != -1 || tmp2.IndexOf(text.ToLower()) != -1)
                        {
                            bFound = true;
                            dg.ClearSelection();
                            dg.Rows[i].Selected = true;
                            dg.FirstDisplayedScrollingRowIndex = i - 2 > 0 ? i - 2 : i;
                            lastSearchIndex = i;
                            break;
                        }
                    }
                if (!bFound)
                {
                    MessageBox.Show("Искомый текст не найден");
                    lastSearchIndex = -1;
                }
                return 0;
            }

            searchForm.showWindow(searchText);
           
        }

        private void bGetSiteList_Click(object sender, EventArgs e)
        {
            using(var form = new fGetNotCheckedSiteList(database))
            {
                form.ShowDialog();
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                notifyIcon.Visible = true;
            }
        }

        bool trueClose = false;
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!trueClose)
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
                this.Hide();
                // Гарантируем видимость иконки в трее при сворачивании
                notifyIcon.Visible = true;
            }
            else
            {
                // Освобождаем ресурсы при реальном закрытии приложения
                periodicCheckTimer?.Dispose();
                domainCheckCancellation?.Cancel();
                domainCheckCancellation?.Dispose();
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
                notifyContextMenu?.Dispose(); // Закрываем контекстное меню, чтобы не оставлять подвешенные элементы UI
            }
        }

        private void mExit_Click(object sender, EventArgs e)
        {
            trueClose = true;
            Application.Exit();
        }
        private void mOpen_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal; // если было свернуто — разворачиваем
            this.Activate();

        }

        private void mStop_Click(object sender, EventArgs e)
        {
            // Отменяем текущую проверку доменов по требованию пользователя
            domainCheckCancellation?.Cancel();
        }

        private void determineHostingMenuItem_Click(object sender, EventArgs e)
        {
            // Запускаем определение хостинга для всех сайтов
            DetermineHostingForSites(null);
        }

        private void determineHostingContextMenuItem_Click(object sender, EventArgs e)
        {
            // Получаем набор идентификаторов выделенных строк и запускаем выборочное определение хостинга
            var selectedSiteIds = new HashSet<long>();
            foreach (DataGridViewRow row in dg.SelectedRows)
            {
                if (row.IsNewRow) continue;
                long siteId = getIngValue(row.Cells[colID.Index].Value, -1);
                if (siteId > 0) selectedSiteIds.Add(siteId);
            }

            if (selectedSiteIds.Count == 0)
            {
                MessageBox.Show("Выберите строку для определения хостинга.", "Определение хостинга", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DetermineHostingForSites(selectedSiteIds);
        }

        private async void determineCpaMenuItem_Click(object sender, EventArgs e)
        {
            // Запускаем определение CPA для всех сайтов
            await DetermineCpaForSitesAsync(null);
        }

        private async void determineCpaContextMenuItem_Click(object sender, EventArgs e)
        {
            // Получаем набор идентификаторов выделенных строк и запускаем выборочное определение CPA
            var selectedSiteIds = new HashSet<long>();
            foreach (DataGridViewRow row in dg.SelectedRows)
            {
                if (row.IsNewRow) continue;
                long siteId = getIngValue(row.Cells[colID.Index].Value, -1);
                if (siteId > 0) selectedSiteIds.Add(siteId);
            }

            if (selectedSiteIds.Count == 0)
            {
                MessageBox.Show("Выберите строку для определения CPA.", "Определение CPA", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            await DetermineCpaForSitesAsync(selectedSiteIds);
        }

        private void DetermineHostingForSites(IReadOnlyCollection<long> targetSiteIds)
        {
            // Выполняем определение хостинга с подготовкой данных и обновлением интерфейса
            Cursor previousCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                var domainRecords = database.LoadDomainsWithIp();
                if (domainRecords.Count == 0)
                {
                    MessageBox.Show("В справочнике доменов отсутствуют записи.", "Определение хостинга", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var hostingRecords = database.LoadHostingList();
                if (hostingRecords.Count == 0)
                {
                    MessageBox.Show("В справочнике хостингов отсутствуют записи.", "Определение хостинга", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var siteRecords = database.LoadSitesForHostingDetection(targetSiteIds);
                if (siteRecords.Count == 0)
                {
                    MessageBox.Show("Не найдены сайты для обработки.", "Определение хостинга", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var idnMapping = new IdnMapping();
                var domainLookup = BuildDomainLookup(domainRecords, idnMapping);
                if (domainLookup.Count == 0)
                {
                    MessageBox.Show("Не удалось подготовить список доменов для сравнения.", "Определение хостинга", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var hostingLookup = BuildHostingLookup(hostingRecords);
                if (hostingLookup.Count == 0)
                {
                    MessageBox.Show("Не найдено ни одного IP-адреса в справочнике хостингов.", "Определение хостинга", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var hostingNameById = hostingRecords.ToDictionary(h => h.Id, h => h.Name);
                var updates = new Dictionary<long, long>();
                var resolvedNames = new Dictionary<long, string>();

                foreach (var site in siteRecords)
                {
                    // Пытаемся извлечь доменное имя из адреса страницы
                    if (!TryExtractHost(site.PageAddress, out var host)) continue;

                    var hostVariants = BuildNormalizedVariants(host, idnMapping);
                    var matchedDomain = FindDomainForHost(hostVariants, domainLookup);
                    if (matchedDomain == null) continue;

                    if (!TryFindHostingId(matchedDomain.Ip, hostingLookup, out var hostingId)) continue;

                    if (site.CurrentHostingId == hostingId) continue;

                    updates[site.Id] = hostingId;
                    if (hostingNameById.TryGetValue(hostingId, out var hostingName))
                        resolvedNames[site.Id] = hostingName;
                }

                if (updates.Count == 0)
                {
                    string message = targetSiteIds == null ? "Не удалось определить новые хостинги." : "Для выбранных записей хостинг не определен.";
                    MessageBox.Show(message, "Определение хостинга", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                database.UpdateSiteHostingBulk(updates);

                if (targetSiteIds == null)
                {
                    database.getTableData(this.dg, "sites");
                    colorize();
                }
                else
                {
                    ApplyResolvedHostingToGrid(updates, resolvedNames);
                }

                MessageBox.Show($"Определено хостингов: {updates.Count}.", "Определение хостинга", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка определения хостинга: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = previousCursor;
            }
        }

        private async Task DetermineCpaForSitesAsync(IReadOnlyCollection<long> targetSiteIds)
        {
            // Выполняем определение CPA сетей на основе содержимого целевых страниц
            Cursor previousCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                var cpaRecords = database.LoadCpaList();
                var cpaScripts = cpaRecords
                    .Where(record => !string.IsNullOrWhiteSpace(record.Script))
                    .Select(record => new { record.Id, Script = record.Script.Trim(), Name = record.Name ?? string.Empty })
                    .Where(record => !string.IsNullOrEmpty(record.Script))
                    .ToList();

                if (cpaScripts.Count == 0)
                {
                    MessageBox.Show("В справочнике CPA отсутствуют записи со скриптами.", "Определение CPA", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var siteRecords = database.LoadSitesForCpaDetection(targetSiteIds);
                if (siteRecords.Count == 0)
                {
                    MessageBox.Show("Не найдено сайтов для обработки.", "Определение CPA", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var updates = new Dictionary<long, long>();
                var cpaNamesById = cpaRecords.ToDictionary(record => record.Id, record => record.Name ?? string.Empty);

                foreach (var site in siteRecords)
                {
                    string normalizedUrl = NormalizeSiteUrl(site.PageAddress);
                    if (string.IsNullOrEmpty(normalizedUrl)) continue;

                    string pageContent = await DownloadPageContentAsync(normalizedUrl);
                    if (string.IsNullOrEmpty(pageContent)) continue;

                    foreach (var cpa in cpaScripts)
                    {
                        if (pageContent.IndexOf(cpa.Script, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            updates[site.Id] = cpa.Id;
                            break;
                        }
                    }
                }

                if (updates.Count == 0)
                {
                    string message = targetSiteIds == null ? "Не удалось обнаружить CPA скрипты на страницах." : "Для выбранных записей CPA не определен.";
                    MessageBox.Show(message, "Определение CPA", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                database.UpdateSiteCpaBulk(updates);

                if (targetSiteIds == null)
                {
                    database.getTableData(this.dg, "sites");
                    colorize();
                }
                else
                {
                    ApplyCpaUpdatesToGrid(updates, cpaNamesById);
                }

                MessageBox.Show($"Определено CPA: {updates.Count}.", "Определение CPA", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при определении CPA: {ex.Message}", "Определение CPA", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = previousCursor;
            }
        }

        private static string NormalizeSiteUrl(string pageAddress)
        {
            // Нормализуем адрес страницы перед загрузкой содержимого
            if (string.IsNullOrWhiteSpace(pageAddress)) return string.Empty;

            string trimmed = pageAddress.Trim();
            string[] candidates = trimmed.Contains("://")
                ? new[] { trimmed }
                : new[] { "https://" + trimmed, "http://" + trimmed };

            foreach (string candidate in candidates)
            {
                if (Uri.TryCreate(candidate, UriKind.Absolute, out var uri) && !string.IsNullOrEmpty(uri.Host))
                {
                    return uri.ToString();
                }
            }

            return string.Empty;
        }

        private static async Task<string> DownloadPageContentAsync(string url)
        {
            // Загружаем содержимое страницы с учетом ошибок сети
            if (string.IsNullOrEmpty(url)) return string.Empty;

            try
            {
                using (var response = await cpaHttpClient.GetAsync(url).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode) return string.Empty;
                    string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return content ?? string.Empty;
                }
            }
            catch (HttpRequestException)
            {
                return string.Empty;
            }
            catch (TaskCanceledException)
            {
                return string.Empty;
            }
        }

        private void ApplyCpaUpdatesToGrid(IReadOnlyDictionary<long, long> updates, IReadOnlyDictionary<long, string> cpaNamesById)
        {
            // Применяем найденные CPA сети к строкам таблицы без полной перезагрузки
            if (updates == null || updates.Count == 0) return;

            foreach (DataGridViewRow row in dg.Rows)
            {
                if (row.IsNewRow) continue;
                long siteId = getIngValue(row.Cells[colID.Index].Value, -1);
                if (!updates.TryGetValue(siteId, out long cpaId)) continue;

                row.Cells[colCpaId.Index].Value = cpaId > 0 ? cpaId.ToString() : string.Empty;
                string cpaName = string.Empty;
                if (cpaId > 0 && cpaNamesById != null)
                {
                    cpaNamesById.TryGetValue(cpaId, out cpaName);
                }
                row.Cells[colCpaName.Index].Value = cpaName ?? string.Empty;
            }
        }

        private static bool TryExtractHost(string pageAddress, out string host)
        {
            // Извлекаем доменное имя из URL с учетом отсутствующей схемы
            host = string.Empty;
            if (string.IsNullOrWhiteSpace(pageAddress)) return false;

            string candidate = pageAddress.Trim();
            if (!candidate.Contains("://")) candidate = "http://" + candidate;

            if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri)) return false;
            if (string.IsNullOrWhiteSpace(uri.Host)) return false;

            host = uri.Host;
            return true;
        }

        private static Dictionary<string, DomainRecord> BuildDomainLookup(IEnumerable<DomainRecord> domains, IdnMapping idnMapping)
        {
            // Строим словарь доменных имен и их вариантов для быстрого сопоставления
            var lookup = new Dictionary<string, DomainRecord>(StringComparer.OrdinalIgnoreCase);
            foreach (var domain in domains)
            {
                if (domain == null) continue;
                foreach (var variant in BuildNormalizedVariants(domain.Name, idnMapping))
                {
                    lookup[variant] = domain;
                }
                foreach (var variant in BuildNormalizedVariants(domain.RusName, idnMapping))
                {
                    lookup[variant] = domain;
                }
            }
            return lookup;
        }

        private static HashSet<string> BuildNormalizedVariants(string value, IdnMapping idnMapping)
        {
            // Формируем множество вариантов доменного имени в разных представлениях
            var variants = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(value)) return variants;

            void AddVariant(string candidate)
            {
                string normalized = NormalizeHostName(candidate);
                if (!string.IsNullOrEmpty(normalized)) variants.Add(normalized);
            }

            AddVariant(value);
            if (idnMapping != null)
            {
                try { AddVariant(idnMapping.GetAscii(value)); } catch { /* Игнорируем ошибки преобразования в punycode */ }
                try { AddVariant(idnMapping.GetUnicode(value)); } catch { /* Игнорируем ошибки преобразования из punycode */ }
            }

            return variants;
        }

        private static string NormalizeHostName(string host)
        {
            // Приводим доменное имя к каноническому нижнему регистру
            if (string.IsNullOrWhiteSpace(host)) return string.Empty;
            return host.Trim().TrimEnd('.').ToLowerInvariant();
        }

        private static DomainRecord FindDomainForHost(IEnumerable<string> hostVariants, Dictionary<string, DomainRecord> domainLookup)
        {
            // Ищем домен для указанного хоста, учитывая поддомены
            foreach (var variant in hostVariants)
            {
                if (domainLookup.TryGetValue(variant, out var record)) return record;

                int dotIndex = variant.IndexOf('.');
                while (dotIndex > 0 && dotIndex < variant.Length - 1)
                {
                    string parent = variant.Substring(dotIndex + 1);
                    if (domainLookup.TryGetValue(parent, out record)) return record;
                    dotIndex = variant.IndexOf('.', dotIndex + 1);
                }
            }
            return null;
        }

        private static Dictionary<string, long> BuildHostingLookup(IEnumerable<HostingRecord> hostingRecords)
        {
            // Подготавливаем словарь IP-адресов и связанных с ними хостингов
            var lookup = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            foreach (var hosting in hostingRecords)
            {
                if (hosting == null || string.IsNullOrWhiteSpace(hosting.Ip)) continue;
                var parts = hosting.Ip.Split(HostingIpSeparators, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    string ip = part.Trim();
                    if (ip.Length == 0) continue;
                    if (!lookup.ContainsKey(ip)) lookup[ip] = hosting.Id;
                }
            }
            return lookup;
        }

        private static bool TryFindHostingId(string domainIp, Dictionary<string, long> hostingLookup, out long hostingId)
        {
            // Подбираем идентификатор хостинга по IP-адресу домена
            hostingId = 0;
            if (string.IsNullOrWhiteSpace(domainIp)) return false;

            var parts = domainIp.Split(HostingIpSeparators, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                string ip = part.Trim();
                if (ip.Length == 0) continue;
                if (hostingLookup.TryGetValue(ip, out hostingId)) return true;
            }
            return false;
        }

        // Показываем комбобокс со списком хостингов поверх выбранной ячейки
        private void ShowHostingSelector(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dg.Rows.Count) return;
            var row = dg.Rows[rowIndex];
            if (row.IsNewRow) return;

            List<HostingRecord> hostings;
            try
            {
                hostings = database.LoadHostingList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить список хостингов: {ex.Message}", "Назначение хостинга", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (hostings == null || hostings.Count == 0)
            {
                MessageBox.Show("Справочник хостингов пуст. Добавьте записи перед назначением.", "Назначение хостинга", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            hostings.Insert(0, new HostingRecord(0, "— Не выбран —", string.Empty));

            hostingSelector.ValueMember = nameof(HostingRecord.Id);
            hostingSelector.DisplayMember = nameof(HostingRecord.Name);
            hostingSelector.DataSource = hostings;
            hostingSelectorRowIndex = rowIndex;

            long currentHostingId = ParseHostingId(row.Cells[colHostingId.Index].Value);
            if (currentHostingId > 0 && hostings.Any(h => h.Id == currentHostingId))
                hostingSelector.SelectedValue = currentHostingId;
            else
                hostingSelector.SelectedIndex = 0;

            Rectangle cellRect = dg.GetCellDisplayRectangle(colHostingName.Index, rowIndex, true);
            hostingSelector.Bounds = cellRect;
            hostingSelector.Visible = true;
            hostingSelector.BringToFront();
            hostingSelector.Focus();
            hostingSelector.DroppedDown = true;
        }

        // Скрываем комбобокс и очищаем привязанные данные
        private void HideHostingSelector()
        {
            if (hostingSelector == null) return;
            hostingSelector.Visible = false;
            hostingSelector.DroppedDown = false;
            hostingSelector.DataSource = null;
            hostingSelectorRowIndex = -1;
        }

        // Отображаем выпадающий список CPA и наполняем его данными справочника
        private void ShowCpaSelector(int rowIndex)
        {
            HideCpaSelector(); // Сначала скрываем предыдущий комбобокс CPA
            HideHostingSelector(); // Также убираем список хостингов, чтобы элементы не перекрывались

            if (rowIndex < 0 || rowIndex >= dg.Rows.Count) return;
            var row = dg.Rows[rowIndex];
            if (row.IsNewRow) return;

            List<CpaRecord> cpaRecords;
            try
            {
                cpaRecords = database.LoadCpaList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить список CPA: {ex.Message}", "Назначение CPA", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (cpaRecords == null || cpaRecords.Count == 0)
            {
                MessageBox.Show("Справочник CPA пуст. Добавьте записи перед назначением.", "Назначение CPA", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectorItems = new List<CpaRecord>(cpaRecords.Count + 1);
            selectorItems.Add(new CpaRecord(0, "— Не выбрана —", string.Empty, string.Empty, string.Empty, string.Empty)); // Добавляем технический элемент для сброса CPA
            selectorItems.AddRange(cpaRecords);

            cpaSelector.ValueMember = nameof(CpaRecord.Id);
            cpaSelector.DisplayMember = nameof(CpaRecord.Name);
            cpaSelector.DataSource = selectorItems;
            cpaSelectorRowIndex = rowIndex;

            long currentCpaId = ParseCpaId(row.Cells[colCpaId.Index].Value);
            if (currentCpaId > 0 && selectorItems.Any(item => item.Id == currentCpaId))
                cpaSelector.SelectedValue = currentCpaId;
            else
                cpaSelector.SelectedIndex = 0;

            Rectangle cellRect = dg.GetCellDisplayRectangle(colCpaName.Index, rowIndex, true);
            cpaSelector.Bounds = cellRect;
            cpaSelector.Visible = true;
            cpaSelector.BringToFront();
            cpaSelector.Focus();
            cpaSelector.DroppedDown = true;
        }

        // Скрываем комбобокс CPA и очищаем привязку данных
        private void HideCpaSelector()
        {
            if (cpaSelector == null) return;
            cpaSelector.Visible = false;
            cpaSelector.DroppedDown = false;
            cpaSelector.DataSource = null;
            cpaSelectorRowIndex = -1;
        }

        // Сохраняем выбранную CPA сеть и обновляем ячейки строки
        private void CpaSelector_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (cpaSelectorRowIndex < 0 || cpaSelectorRowIndex >= dg.Rows.Count)
            {
                HideCpaSelector();
                return;
            }

            var row = dg.Rows[cpaSelectorRowIndex];
            long siteId = getIngValue(row.Cells[colID.Index].Value, -1);
            long selectedCpaId = ParseCpaId(cpaSelector.SelectedValue);
            string cpaName = selectedCpaId > 0 ? cpaSelector.Text ?? string.Empty : string.Empty; // Прячем подсказку при отсутствии CPA

            row.Cells[colCpaId.Index].Value = selectedCpaId > 0 ? selectedCpaId.ToString() : string.Empty;
            row.Cells[colCpaName.Index].Value = cpaName;

            if (siteId > 0)
            {
                try
                {
                    database.UpdateSiteCpa(siteId, selectedCpaId > 0 ? (long?)selectedCpaId : null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось сохранить выбранный CPA: {ex.Message}", "Назначение CPA", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            HideCpaSelector();
        }

        // Скрываем список CPA при потере фокуса
        private void CpaSelector_LostFocus(object sender, EventArgs e)
        {
            if (!cpaSelector.DroppedDown)
                HideCpaSelector();
        }

        // Обрабатываем клавишу Escape для отмены выбора CPA
        private void CpaSelector_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                HideCpaSelector();
                e.Handled = true;
            }
        }

        // Обрабатываем выбор элемента списка и сохраняем его в базе
        private void HostingSelector_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (hostingSelectorRowIndex < 0 || hostingSelectorRowIndex >= dg.Rows.Count)
            {
                HideHostingSelector();
                return;
            }

            var row = dg.Rows[hostingSelectorRowIndex];
            long siteId = getIngValue(row.Cells[colID.Index].Value, -1);
            long selectedHostingId = ParseHostingId(hostingSelector.SelectedValue);
            // Прячем служебную подпись "Не выбран" и оставляем поле пустым
            string hostingName = selectedHostingId > 0 ? hostingSelector.Text ?? string.Empty : string.Empty;

            row.Cells[colHostingId.Index].Value = selectedHostingId > 0 ? selectedHostingId.ToString() : string.Empty;
            row.Cells[colHostingName.Index].Value = hostingName;

            if (siteId > 0)
            {
                try
                {
                    database.UpdateSiteHosting(siteId, selectedHostingId > 0 ? (long?)selectedHostingId : null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось сохранить выбранный хостинг: {ex.Message}", "Назначение хостинга", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            HideHostingSelector();
            HideCpaSelector(); // Дополнительно закрываем список CPA, чтобы не было зависших элементов
        }

        // Прячем список при потере фокуса, чтобы он не зависал на форме
        private void HostingSelector_LostFocus(object sender, EventArgs e)
        {
            if (!hostingSelector.DroppedDown)
                HideHostingSelector();
        }

        // Реагируем на клавишу Escape для отмены выбора
        private void HostingSelector_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                HideHostingSelector();
                HideCpaSelector(); // Одновременно скрываем список CPA для единообразия поведения
                e.Handled = true;
            }
        }

        // Скрываем комбобокс при прокрутке таблицы
        private void dg_Scroll(object sender, ScrollEventArgs e)
        {
            HideHostingSelector();
            HideCpaSelector(); // Одновременно скрываем список выбора CPA
        }

        // Скрываем комбобокс при изменении ширины столбцов
        private void dg_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            HideHostingSelector();
            HideCpaSelector(); // Одновременно скрываем список выбора CPA
        }

        // Скрываем комбобокс при изменении высоты строк
        private void dg_RowHeightChanged(object sender, DataGridViewRowEventArgs e)
        {
            HideHostingSelector();
            HideCpaSelector(); // Одновременно скрываем список выбора CPA
        }

        // Сбрасываем комбобокс при переходе на другую ячейку
        private void dg_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            HideHostingSelector();
            HideCpaSelector(); // Одновременно скрываем список выбора CPA
        }

        // Унифицируем преобразование значения столбца в числовой идентификатор хостинга
        private static long ParseHostingId(object value)
        {
            return ParseIdentifier(value); // Используем общее преобразование идентификаторов
        }

        // Унифицируем преобразование значения столбца CPA в числовой идентификатор
        private static long ParseCpaId(object value)
        {
            return ParseIdentifier(value); // Используем общее преобразование идентификаторов
        }

        // Обобщаем преобразование произвольного значения к целочисленному идентификатору
        private static long ParseIdentifier(object value)
        {
            if (value == null) return 0;
            if (value is long longValue) return longValue;
            if (value is int intValue) return intValue;
            if (value is short shortValue) return shortValue;
            if (value is string text && long.TryParse(text, out var parsed)) return parsed;
            if (long.TryParse(Convert.ToString(value), out var converted)) return converted;
            return 0;
        }

        private void ApplyResolvedHostingToGrid(IReadOnlyDictionary<long, long> updates, IReadOnlyDictionary<long, string> resolvedNames)
        {
            // Обновляем значения столбца хостинга в таблице без полной перезагрузки данных
            foreach (DataGridViewRow row in dg.Rows)
            {
                if (row.IsNewRow) continue;
                long siteId = getIngValue(row.Cells[colID.Index].Value, -1);
                if (siteId <= 0) continue;
                if (!updates.ContainsKey(siteId)) continue;

                // Обновляем скрытый идентификатор выбранного хостинга
                if (updates.TryGetValue(siteId, out var hostingId))
                    row.Cells[colHostingId.Index].Value = hostingId > 0 ? hostingId.ToString() : string.Empty;

                if (resolvedNames.TryGetValue(siteId, out var hostingName))
                    row.Cells[colHostingName.Index].Value = hostingName;
                else
                    row.Cells[colHostingName.Index].Value = string.Empty;
            }
        }

        private void tecknicalCheck_Click(object sender, EventArgs e)
        {
            if (dg.SelectedRows.Count == 0) return; 
            var url = dg.SelectedRows[0].Cells[colPageUrl.Index].Value.ToString();
            var report = checker.Check(url);
            Clipboard.SetText(report);
            MessageBox.Show(report, "Инфа", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
        }

        private void fullAnalysisMenuItem_Click(object sender, EventArgs e)
        {
            // Открываем форму полного анализа для выбранного сайта
            if (dg.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите сайт для анализа.");
                return;
            }

            long siteId = getIngValue(dg.SelectedRows[0].Cells[colID.Index].Value, -1);
            if (siteId <= 0)
            {
                MessageBox.Show("Не удалось определить ID сайта.");
                return;
            }

            using (var form = new AnalysisDataResult(database, siteId))
            {
                form.ShowDialog(this);
            }
        }
    }
}
