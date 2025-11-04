using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Globalization;
using Microsoft.Win32;

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
        private System.Timers.Timer timer = null;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private DateTime LastDateCheck = DateTime.Now.AddDays(-3);
        private String formTitle = "Проверка позиций сайта в поисковой выдаче Яндекс";
        private bool isProcessCheckin = false;
        private bool bStopChecking = false;

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

            bool startInTray = false;
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
        }
        private async void test()
        {
            var visitorsWithoutBots = await YandexMetricChecker.GetVisitorsWithoutBots("96666372");
            Console.WriteLine($"Количество посетителей без роботов: {visitorsWithoutBots}");
        }

        public void AddToStartup()
        {
            RegistryKey reg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (reg.GetValue(appName) == null)
            {
                reg.SetValue(appName, $"\"{appPath}\"");
            }
            reg.Close();
        }

        private void SetDailyTimer(TimeSpan scheduledTime)
        {
            return;

            DateTime now = DateTime.Now;
            DateTime firstRun = new DateTime(LastDateCheck.Year, LastDateCheck.Month, LastDateCheck.Day, scheduledTime.Hours, scheduledTime.Minutes, scheduledTime.Seconds);
            double dailyInterval = 7 * 24 * 60 * 60 * 1000; // 3 * 24 часа в миллисекундах

            // Если firstRun уже прошел, добавляем 3 дня
            if (now > firstRun) firstRun = firstRun.AddDays(7); 

            TimeSpan timeDifference = firstRun - now;
            double initialInterval = timeDifference.TotalMilliseconds;
            if (initialInterval < 7) initialInterval = 7;

            // Устанавливаем таймер
            this.timer = new System.Timers.Timer();

            this.timer.Interval = initialInterval;

            this.timer.Elapsed += (sender, e) =>
            {
                timer.Interval = dailyInterval;
                if (!isProcessCheckin) mStart_Click(sender, e);
            };
            this.timer.Start();
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

            if (Properties.Settings.Default.TimeToCheck != "")
            {
                this.Text = this.formTitle + " - последняя проверка от " + LastDateCheck.ToString("dd.MM.yy");
                SetDailyTimer(TimeSpan.Parse(Properties.Settings.Default.TimeToCheck));

            }

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

     
        private bool wasStop = false;
 

        private int getPosition(String keyword, String findUrl, out String url, String Region="2")
        {
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
                    //https://довериевсети.рф/site/ifish2.ru
                    var d = webClient.DownloadData(new Uri(urlXML));
                    text = Encoding.UTF8.GetString(d);
                }
                catch (Exception ex)
                {
                    wasStop = true;
                    Debug.WriteLine("Error " + ex.Message + " -> " + urlXML); return 0;
                }
            }

            if (wasStop) return -1;

            if (text=="") { MessageBox.Show("Ошибка получения данных"); wasStop = true; return 0; }
            

            String regExp = @"error code=(.*?)>(.*?)<";
            String mText = System.Xml.Linq.XDocument.Parse(text).ToString();
            Regex regex = new Regex(regExp, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            MatchCollection matches = regex.Matches(mText);
            if (matches != null && matches.Count > 0 && matches[0].Groups.Count == 3)
            {
                String errorCode = matches[0].Groups[1].Value;
                String errorText =  matches[0].Groups[2].Value ;
                MessageBox.Show("Ошибка " + errorCode + " " + errorText);
                return -2;
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
            _FormSettings.ShowDialog();
            if (timer != null)
                timer.Stop();

            if (Properties.Settings.Default.TimeToCheck != "")
                SetDailyTimer(TimeSpan.Parse(Properties.Settings.Default.TimeToCheck));

        }



        private void dg_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            e.Cancel = MessageBox.Show("Really delete ?", "", MessageBoxButtons.OKCancel) == DialogResult.Cancel;
            if (e.Cancel) return;
            int id = int.Parse(this.dg.Rows[e.Row.Index].Cells[colID.Index].Value.ToString());
            this.database.removeRecord(id, "sites");
        }

        private bool checkRowInTable(int index)
        {

            if (dg.Rows[index].Cells[colPageUrl.Index].Value == null || dg.Rows[index].Cells[colKeyword.Index].Value == null
                    || dg.Rows[index].Cells[colKeyword.Index].Value.ToString() == "") return false;
            String url = dg.Rows[index].Cells[colPageUrl.Index].Value.ToString();
            if (url == "" || url.IndexOf("http") != 0) return false;
            String keyword = dg.Rows[index].Cells[colKeyword.Index].Value.ToString();


            String foundPageUrl = "";
            String dateNow = DateTime.Now.ToString("dd.MM.yy");
            
            List<int> ints = new List<int>();
            int position = 10000;
            for (int i = 0; i < 4; i++)
            {
                if (wasStop) return false;
                Application.DoEvents();
                var pos = getPosition(keyword, url, out foundPageUrl);
                if (pos == -2)
                {
                    i--;

                    Application.DoEvents();
                    Thread.Sleep(5000);
                    continue;

                }
                if (pos < 0)
                {
                    throw new Exception("Ошибка получения позиции");
                    return false;
                }
                if (pos < position) position = pos;
                ints.Add(pos);
                Application.DoEvents();
                Thread.Sleep(1000);
                
            }
            int middle_position = ints.Count > 0 ? (int)ints.Average() : 0;

            int prev_pos = 0;
            int prev_mid_pos = 0;
            if (position == -1) throw new Exception("Ошибка работы XML сервиса");

            try
            {
                prev_pos = int.Parse(dg.Rows[index].Cells[colCurrentPosition.Index].Value.ToString());
                prev_mid_pos = int.Parse(dg.Rows[index].Cells[colMidCurrent.Index].Value.ToString());
            }catch (Exception) { 
            
            }
             
            dg.Rows[index].Cells[colCurrentPosition.Index].Value = position;
            dg.Rows[index].Cells[colMidCurrent.Index].Value = middle_position;
            dg.Rows[index].Cells[colLastPosition.Index].Value = prev_pos;
            dg.Rows[index].Cells[colMidPrev.Index].Value = prev_mid_pos;
            dg.Rows[index].Cells[colFoundPageUrl.Index].Value = foundPageUrl;
            dg.Rows[index].Cells[colDateCheck.Index].Value = dateNow;
            int id = int.Parse(dg.Rows[index].Cells[colID.Index].Value.ToString());
            saveRow(index);
            this.database.insertChecks(id, dateNow, position, middle_position);
            return true;
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
            if (pProgress.InvokeRequired) pProgress.Invoke(new Action(() =>
                {
                    checkDomains(0);
                
                }
            ));
            else { checkDomains(0); }

        
        }


        private void mContinue_Click(object sender, EventArgs e)
        {
            int startFrom = 0;
            if (dg.SelectedRows.Count > 0)
                startFrom = dg.SelectedRows[0].Cells[0].RowIndex;
            checkDomains(startFrom);
        }

        private void checkDomains(int startFrom = 0)
        {
            isProcessCheckin = true;
            wasStop = false;
            if (Properties.Settings.Default.XMLURL == "") { MessageBox.Show("Не задан XML Url для запросов!"); return; }
            pProgress.Visible = true;
            dg.Enabled = false;
            pb.Value = 0;
            pb.Maximum = dg.RowCount;
            int count_checked = 0;

            for (int i = startFrom; i < dg.RowCount - 1; i++)
            {
                pb.Value = i + 1;
                if (wasStop) break;
                try
                {
                    checkRowInTable(i);
                }
                catch (Exception ex)
                {
                    break;

                }
                Application.DoEvents();
                Thread.Sleep(1500);
                count_checked++;
            }
          
            pProgress.Visible = false;
            dg.Enabled = true;
            colorize();
            isProcessCheckin = false;
            if (count_checked > dg.RowCount - 20 - startFrom)
            {
                LastDateCheck = DateTime.Now;
                Properties.Settings.Default.LastDateCheck = LastDateCheck;
                Properties.Settings.Default.Save();
                this.Text = this.formTitle + " - последняя проверка от " + LastDateCheck.ToString("dd.MM.yy");
            }
             
         }


        private void checkRow_Click(object sender, EventArgs e)
        {
            if (dg.SelectedRows.Count > 0 && dg.SelectedRows[0].Index >= dg.RowCount - 1)
            {
                MessageBox.Show("Выберите строку для проверки!");

                return;
            }
            pb.Value = 0;
            pb.Maximum = 1;
            pProgress.Visible = true;
            dg.Enabled = false;
            for (int i = 0; i < dg.SelectedRows.Count; i++)
            {
                pb.Value = i + 1;
                if (wasStop) break;
                checkRowInTable(dg.SelectedRows[i].Index);
                Application.DoEvents();
            }
            pb.Value = 1;
            pProgress.Visible = false;
            dg.Enabled = true;
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
            }
        }

        private void mExit_Click(object sender, EventArgs e)
        {
            trueClose = true;
            Application.Exit();
        }

        private void mStop_Click(object sender, EventArgs e)
        {
            wasStop = true;
        }
    }
}
