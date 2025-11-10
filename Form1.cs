using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        // Храним интервалы периодического обновления и срок устаревания записей
        private static readonly TimeSpan periodicCheckInterval = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan staleCheckThreshold = TimeSpan.FromDays(7);
        private const string StatusCheckingText = "идет проверка доменов";
        private static readonly TimeSpan[] domainCheckRetryDelays = new[] // График повторов проверки доменов с нарастающими задержками
        {
            TimeSpan.Zero,
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(2),
            TimeSpan.FromMinutes(3),
            TimeSpan.FromMinutes(4),
            TimeSpan.FromMinutes(5)
        };

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
            InitializePeriodicCheckTimer();
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
        private DomainCheckResult ExecuteDomainCheck(DomainCheckRequest request, CancellationToken token)
        {
            var attempts = new List<int>();
            int bestPosition = int.MaxValue;
            string foundPageUrl = string.Empty;

            for (int attempt = 0; attempt < 4; attempt++)
            {
                token.ThrowIfCancellationRequested();

                int position = getPosition(request.Keyword, request.Url, out var foundUrl, "2", token);
                if (position < 0) throw new InvalidOperationException("Ошибка получения позиции домена");
                bestPosition = Math.Min(bestPosition, position);
                attempts.Add(position);
                foundPageUrl = foundUrl;
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

     
        private int getPosition(String keyword, String findUrl, out String url, String Region = "2", CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            url = "";
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
            int pos = FindUrl(urls, findUrl);
            if (pos > 0) url = "OK";
            else
            {
                pos = FindUrl(urls, findUrl, false);
                if (pos > 0) url = urls[pos-1];
            }

            return pos;
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
            string url          = getStringValue(row.Cells[colPageUrl.Index].Value);
            string keyword      = getStringValue(row.Cells[colKeyword.Index].Value);
            string dateNow      = getStringValue(row.Cells[colDateCheck.Index].Value);
            string coment       = getStringValue(row.Cells[colAction.Index].Value) ;
            string status       = getStringValue(row.Cells[colStatus.Index].Value) ;

            if (id != -1)
                this.database.updateSite(id, dateNow, url, keyword, position, middle_position,  prev_pos, middle_prev_pos, foundPageUrl, coment, status);
            else
            {
               long id_row =  this.database.appendSite(dateNow, url, keyword, position, prev_pos, foundPageUrl, coment, status);
                if (id_row >= 0)
                    row.Cells[colID.Index].Value = id_row;
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
                if (row == null || row.IsNewRow || !ShouldCheckRowAutomatically(row.Index)) continue;
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
            using (DomainList f = new DomainList(this.database)) {
                f.ShowDialog();
            }
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

        private void mStop_Click(object sender, EventArgs e)
        {
            // Отменяем текущую проверку доменов по требованию пользователя
            domainCheckCancellation?.Cancel();
        }
    }
}
