namespace CheckPosition
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.contextMenuTreeView = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.checkRow = new System.Windows.Forms.ToolStripMenuItem();
            this.determineHostingContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.determineCpaContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showHistoryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.rowUp = new System.Windows.Forms.ToolStripMenuItem();
            this.rowDown = new System.Windows.Forms.ToolStripMenuItem();
            this.panel2 = new System.Windows.Forms.Panel();
            this.dg = new System.Windows.Forms.DataGridView();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.проектToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showDomainListForm = new System.Windows.Forms.ToolStripMenuItem();
            this.showHostingListMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showCpaListMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.bYandexMetrica = new System.Windows.Forms.ToolStripMenuItem();
            this.загрузитьКоличествоСделокИзTravelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.загрузитьКоличестовСделокИзAdmintadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.mExit = new System.Windows.Forms.ToolStripMenuItem();
            this.настройкиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.сканироватьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mStart = new System.Windows.Forms.ToolStripMenuItem();
            this.mContinue = new System.Windows.Forms.ToolStripMenuItem();
            this.mStop = new System.Windows.Forms.ToolStripMenuItem();
            this.determineHostingMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.determineCpaMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.bGetSiteList = new System.Windows.Forms.ToolStripMenuItem();
            this.правкиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bSearch = new System.Windows.Forms.ToolStripMenuItem();
            this.sd = new System.Windows.Forms.SaveFileDialog();
            this.od = new System.Windows.Forms.OpenFileDialog();
            this.colID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDateCheck = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPageUrl = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colKeyword = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCurrentPosition = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMidCurrent = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLastPosition = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMidPrev = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFoundPageUrl = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAction = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCpaId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCpaName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colHostingId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colHostingName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contextMenuTreeView.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dg)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuTreeView
            // 
            this.contextMenuTreeView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkRow,
            this.determineHostingContextMenuItem,
            this.determineCpaContextMenuItem,
            this.showHistoryMenuItem,
            this.toolStripMenuItem2,
            this.rowUp,
            this.rowDown});
            this.contextMenuTreeView.Name = "contextMenuTreeView";
            this.contextMenuTreeView.Size = new System.Drawing.Size(242, 142);
            // 
            // checkRow
            // 
            this.checkRow.Image = global::CheckPosition.Properties.Resources.iconfinder_check_1055094;
            this.checkRow.Name = "checkRow";
            this.checkRow.Size = new System.Drawing.Size(241, 22);
            this.checkRow.Text = "Проверить позиции страницы";
            this.checkRow.Click += new System.EventHandler(this.checkRow_Click);
            // 
            // determineHostingContextMenuItem
            // 
            this.determineHostingContextMenuItem.Name = "determineHostingContextMenuItem";
            this.determineHostingContextMenuItem.Size = new System.Drawing.Size(241, 22);
            this.determineHostingContextMenuItem.Text = "Определить хостинг";
            this.determineHostingContextMenuItem.Click += new System.EventHandler(this.determineHostingContextMenuItem_Click);
            // 
            // determineCpaContextMenuItem
            // 
            this.determineCpaContextMenuItem.Name = "determineCpaContextMenuItem";
            this.determineCpaContextMenuItem.Size = new System.Drawing.Size(241, 22);
            this.determineCpaContextMenuItem.Text = "Определить CPA";
            this.determineCpaContextMenuItem.Click += new System.EventHandler(this.determineCpaContextMenuItem_Click);
            // 
            // showHistoryMenuItem
            // 
            this.showHistoryMenuItem.Name = "showHistoryMenuItem";
            this.showHistoryMenuItem.Size = new System.Drawing.Size(241, 22);
            this.showHistoryMenuItem.Text = "Показать историю";
            this.showHistoryMenuItem.Click += new System.EventHandler(this.showHistoryMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(238, 6);
            // 
            // rowUp
            // 
            this.rowUp.Name = "rowUp";
            this.rowUp.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Up)));
            this.rowUp.Size = new System.Drawing.Size(241, 22);
            this.rowUp.Text = "строка вверх";
            this.rowUp.Click += new System.EventHandler(this.rowUp_Click);
            // 
            // rowDown
            // 
            this.rowDown.Name = "rowDown";
            this.rowDown.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Down)));
            this.rowDown.Size = new System.Drawing.Size(241, 22);
            this.rowDown.Text = "строка вниз";
            this.rowDown.Click += new System.EventHandler(this.rowUp_Click);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.dg);
            this.panel2.Location = new System.Drawing.Point(0, 24);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.panel2.Size = new System.Drawing.Size(1635, 718);
            this.panel2.TabIndex = 1;
            // 
            // dg
            // 
            this.dg.AllowUserToOrderColumns = true;
            this.dg.AllowUserToResizeRows = false;
            this.dg.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dg.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dg.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colID,
            this.colDateCheck,
            this.colPageUrl,
            this.colKeyword,
            this.colCurrentPosition,
            this.colMidCurrent,
            this.colLastPosition,
            this.colMidPrev,
            this.colFoundPageUrl,
            this.colAction,
            this.colStatus,
            this.colCpaId,
            this.colCpaName,
            this.colHostingId,
            this.colHostingName});
            this.dg.ContextMenuStrip = this.contextMenuTreeView;
            this.dg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dg.Location = new System.Drawing.Point(3, 0);
            this.dg.Margin = new System.Windows.Forms.Padding(3, 3, 3, 123);
            this.dg.Name = "dg";
            this.dg.RowHeadersVisible = false;
            this.dg.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dg.Size = new System.Drawing.Size(1632, 718);
            this.dg.TabIndex = 0;
            this.dg.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.dg_CellBeginEdit);
            this.dg.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dg_CellDoubleClick);
            this.dg.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dg_CellEndEdit);
            this.dg.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.dg_UserDeletingRow);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel,
            this.statusProgressBar});
            this.statusStrip.Location = new System.Drawing.Point(0, 745);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            this.statusStrip.Size = new System.Drawing.Size(1635, 22);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 4;
            this.statusStrip.Text = "statusStrip";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(0, 17);
            this.statusLabel.Visible = false;
            // 
            // statusProgressBar
            // 
            this.statusProgressBar.AutoSize = false;
            this.statusProgressBar.Name = "statusProgressBar";
            this.statusProgressBar.Size = new System.Drawing.Size(200, 16);
            this.statusProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.statusProgressBar.Visible = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.проектToolStripMenuItem,
            this.настройкиToolStripMenuItem,
            this.сканироватьToolStripMenuItem,
            this.правкиToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(1635, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // проектToolStripMenuItem
            // 
            this.проектToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showDomainListForm,
            this.showHostingListMenuItem,
            this.showCpaListMenuItem,
            this.toolStripMenuItem4,
            this.bYandexMetrica,
            this.загрузитьКоличествоСделокИзTravelToolStripMenuItem,
            this.загрузитьКоличестовСделокИзAdmintadToolStripMenuItem,
            this.toolStripMenuItem1,
            this.mExit});
            this.проектToolStripMenuItem.Name = "проектToolStripMenuItem";
            this.проектToolStripMenuItem.Size = new System.Drawing.Size(59, 20);
            this.проектToolStripMenuItem.Text = "Проект";
            // 
            // showDomainListForm
            // 
            this.showDomainListForm.Name = "showDomainListForm";
            this.showDomainListForm.Size = new System.Drawing.Size(324, 22);
            this.showDomainListForm.Text = "Cписок доменов";
            this.showDomainListForm.Click += new System.EventHandler(this.showDomainListForm_Click);
            // 
            // showHostingListMenuItem
            // 
            this.showHostingListMenuItem.Name = "showHostingListMenuItem";
            this.showHostingListMenuItem.Size = new System.Drawing.Size(324, 22);
            this.showHostingListMenuItem.Text = "Хостинги";
            this.showHostingListMenuItem.Click += new System.EventHandler(this.showHostingListMenuItem_Click);
            // 
            // showCpaListMenuItem
            // 
            this.showCpaListMenuItem.Name = "showCpaListMenuItem";
            this.showCpaListMenuItem.Size = new System.Drawing.Size(324, 22);
            this.showCpaListMenuItem.Text = "Партнерки";
            this.showCpaListMenuItem.Click += new System.EventHandler(this.showCpaListMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(321, 6);
            // 
            // bYandexMetrica
            // 
            this.bYandexMetrica.Name = "bYandexMetrica";
            this.bYandexMetrica.Size = new System.Drawing.Size(324, 22);
            this.bYandexMetrica.Text = "Загрузить статистику посещений из Метрики";
            this.bYandexMetrica.Click += new System.EventHandler(this.bYandexMetrica_Click);
            // 
            // загрузитьКоличествоСделокИзTravelToolStripMenuItem
            // 
            this.загрузитьКоличествоСделокИзTravelToolStripMenuItem.Name = "загрузитьКоличествоСделокИзTravelToolStripMenuItem";
            this.загрузитьКоличествоСделокИзTravelToolStripMenuItem.Size = new System.Drawing.Size(324, 22);
            this.загрузитьКоличествоСделокИзTravelToolStripMenuItem.Text = "Загрузить количество сделок из Travel";
            // 
            // загрузитьКоличестовСделокИзAdmintadToolStripMenuItem
            // 
            this.загрузитьКоличестовСделокИзAdmintadToolStripMenuItem.Name = "загрузитьКоличестовСделокИзAdmintadToolStripMenuItem";
            this.загрузитьКоличестовСделокИзAdmintadToolStripMenuItem.Size = new System.Drawing.Size(324, 22);
            this.загрузитьКоличестовСделокИзAdmintadToolStripMenuItem.Text = "Загрузить количестов сделок из Admintad";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(321, 6);
            // 
            // mExit
            // 
            this.mExit.Image = global::CheckPosition.Properties.Resources.iconfinder__signin_in_common_door_exit_login_4247896;
            this.mExit.Name = "mExit";
            this.mExit.ShortcutKeys = System.Windows.Forms.Keys.F10;
            this.mExit.Size = new System.Drawing.Size(324, 22);
            this.mExit.Text = "Выход";
            this.mExit.Click += new System.EventHandler(this.mExit_Click);
            // 
            // настройкиToolStripMenuItem
            // 
            this.настройкиToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mSettings});
            this.настройкиToolStripMenuItem.Name = "настройкиToolStripMenuItem";
            this.настройкиToolStripMenuItem.Size = new System.Drawing.Size(79, 20);
            this.настройкиToolStripMenuItem.Text = "Настройки";
            // 
            // mSettings
            // 
            this.mSettings.Image = global::CheckPosition.Properties.Resources.iconfinder_settings_1054981;
            this.mSettings.Name = "mSettings";
            this.mSettings.Size = new System.Drawing.Size(207, 22);
            this.mSettings.Text = "Параметры программы";
            this.mSettings.Click += new System.EventHandler(this.mSettings_Click);
            // 
            // сканироватьToolStripMenuItem
            // 
            this.сканироватьToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mStart,
            this.mContinue,
            this.mStop,
            this.determineHostingMenuItem,
            this.determineCpaMenuItem,
            this.toolStripMenuItem3,
            this.bGetSiteList});
            this.сканироватьToolStripMenuItem.Name = "сканироватьToolStripMenuItem";
            this.сканироватьToolStripMenuItem.Size = new System.Drawing.Size(90, 20);
            this.сканироватьToolStripMenuItem.Text = "Сканировать";
            // 
            // mStart
            // 
            this.mStart.Image = global::CheckPosition.Properties.Resources.iconfinder_Arrow_film_movie_play_player_start_video_1886336;
            this.mStart.Name = "mStart";
            this.mStart.Size = new System.Drawing.Size(296, 22);
            this.mStart.Text = "Начать проверку";
            this.mStart.Click += new System.EventHandler(this.mStart_Click);
            // 
            // mContinue
            // 
            this.mContinue.Name = "mContinue";
            this.mContinue.Size = new System.Drawing.Size(296, 22);
            this.mContinue.Text = "Продолжить проверку";
            this.mContinue.Click += new System.EventHandler(this.mContinue_Click);
            // 
            // mStop
            // 
            this.mStop.Image = global::CheckPosition.Properties.Resources.iconfinder_player_stop_48794;
            this.mStop.Name = "mStop";
            this.mStop.Size = new System.Drawing.Size(296, 22);
            this.mStop.Text = "Остановить проверку";
            this.mStop.Click += new System.EventHandler(this.mStop_Click);
            // 
            // determineHostingMenuItem
            // 
            this.determineHostingMenuItem.Name = "determineHostingMenuItem";
            this.determineHostingMenuItem.Size = new System.Drawing.Size(296, 22);
            this.determineHostingMenuItem.Text = "Определить хостинг";
            this.determineHostingMenuItem.Click += new System.EventHandler(this.determineHostingMenuItem_Click);
            // 
            // determineCpaMenuItem
            // 
            this.determineCpaMenuItem.Name = "determineCpaMenuItem";
            this.determineCpaMenuItem.Size = new System.Drawing.Size(296, 22);
            this.determineCpaMenuItem.Text = "Определить CPA";
            this.determineCpaMenuItem.Click += new System.EventHandler(this.determineCpaMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(293, 6);
            // 
            // bGetSiteList
            // 
            this.bGetSiteList.Name = "bGetSiteList";
            this.bGetSiteList.Size = new System.Drawing.Size(296, 22);
            this.bGetSiteList.Text = "Список сайтов отсуствующих в таблице";
            this.bGetSiteList.Click += new System.EventHandler(this.bGetSiteList_Click);
            // 
            // правкиToolStripMenuItem
            // 
            this.правкиToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bSearch});
            this.правкиToolStripMenuItem.Name = "правкиToolStripMenuItem";
            this.правкиToolStripMenuItem.Size = new System.Drawing.Size(60, 20);
            this.правкиToolStripMenuItem.Text = "Правки";
            // 
            // bSearch
            // 
            this.bSearch.Name = "bSearch";
            this.bSearch.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.bSearch.Size = new System.Drawing.Size(149, 22);
            this.bSearch.Text = "Поиск";
            this.bSearch.Click += new System.EventHandler(this.bSearch_Click);
            // 
            // sd
            // 
            this.sd.Filter = "Check Position Project|*.chps|All Files|*.*";
            // 
            // od
            // 
            this.od.Filter = "Check Position Project|*.chps|All Files|*.*";
            // 
            // colID
            // 
            this.colID.HeaderText = "id";
            this.colID.Name = "colID";
            this.colID.Visible = false;
            // 
            // colDateCheck
            // 
            this.colDateCheck.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colDateCheck.HeaderText = "Дата";
            this.colDateCheck.Name = "colDateCheck";
            this.colDateCheck.ReadOnly = true;
            // 
            // colPageUrl
            // 
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Underline);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.Blue;
            this.colPageUrl.DefaultCellStyle = dataGridViewCellStyle1;
            this.colPageUrl.HeaderText = "Url";
            this.colPageUrl.Name = "colPageUrl";
            // 
            // colKeyword
            // 
            this.colKeyword.HeaderText = "Ключевая фраза";
            this.colKeyword.Name = "colKeyword";
            // 
            // colCurrentPosition
            // 
            this.colCurrentPosition.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colCurrentPosition.HeaderText = "Тек.";
            this.colCurrentPosition.Name = "colCurrentPosition";
            this.colCurrentPosition.ReadOnly = true;
            this.colCurrentPosition.Width = 50;
            // 
            // colMidCurrent
            // 
            this.colMidCurrent.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colMidCurrent.HeaderText = "Ср.";
            this.colMidCurrent.Name = "colMidCurrent";
            this.colMidCurrent.ReadOnly = true;
            this.colMidCurrent.Width = 50;
            // 
            // colLastPosition
            // 
            this.colLastPosition.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colLastPosition.HeaderText = "Пред.";
            this.colLastPosition.Name = "colLastPosition";
            this.colLastPosition.ReadOnly = true;
            this.colLastPosition.Width = 50;
            // 
            // colMidPrev
            // 
            this.colMidPrev.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colMidPrev.HeaderText = "Пр.ср.";
            this.colMidPrev.Name = "colMidPrev";
            this.colMidPrev.Width = 60;
            // 
            // colFoundPageUrl
            // 
            this.colFoundPageUrl.HeaderText = "URL в поиске";
            this.colFoundPageUrl.Name = "colFoundPageUrl";
            // 
            // colAction
            // 
            this.colAction.HeaderText = "Действия";
            this.colAction.Name = "colAction";
            // 
            // colStatus
            // 
            this.colStatus.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colStatus.HeaderText = "Статус";
            this.colStatus.Name = "colStatus";
            // 
            // colCpaId
            // 
            this.colCpaId.HeaderText = "CpaId";
            this.colCpaId.Name = "colCpaId";
            this.colCpaId.ReadOnly = true;
            this.colCpaId.Visible = false;
            // 
            // colCpaName
            // 
            this.colCpaName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colCpaName.HeaderText = "CPA";
            this.colCpaName.Name = "colCpaName";
            this.colCpaName.ReadOnly = true;
            // 
            // colHostingId
            // 
            this.colHostingId.HeaderText = "HostingId";
            this.colHostingId.Name = "colHostingId";
            this.colHostingId.ReadOnly = true;
            this.colHostingId.Visible = false;
            // 
            // colHostingName
            // 
            this.colHostingName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colHostingName.HeaderText = "Хостинг";
            this.colHostingName.Name = "colHostingName";
            this.colHostingName.ReadOnly = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1635, 767);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Проверка позиций сайта в поисковой выдаче Яндекс";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.contextMenuTreeView.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dg)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ContextMenuStrip contextMenuTreeView;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.DataGridView dg;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem проектToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mExit;
        private System.Windows.Forms.ToolStripMenuItem настройкиToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mSettings;
        private System.Windows.Forms.ToolStripMenuItem сканироватьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mStart;
        private System.Windows.Forms.ToolStripMenuItem mStop;
        private System.Windows.Forms.SaveFileDialog sd;
        private System.Windows.Forms.OpenFileDialog od;
        private System.Windows.Forms.ToolStripMenuItem checkRow;
        private System.Windows.Forms.ToolStripMenuItem determineHostingContextMenuItem;
        private System.Windows.Forms.ToolStripMenuItem determineCpaContextMenuItem;
        // Элемент контекстного меню для открытия истории позиций
        private System.Windows.Forms.ToolStripMenuItem showHistoryMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripProgressBar statusProgressBar;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem rowUp;
        private System.Windows.Forms.ToolStripMenuItem rowDown;
        private System.Windows.Forms.ToolStripMenuItem showDomainListForm;
        private System.Windows.Forms.ToolStripMenuItem showHostingListMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showCpaListMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem bYandexMetrica;
        private System.Windows.Forms.ToolStripMenuItem загрузитьКоличествоСделокИзTravelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem загрузитьКоличестовСделокИзAdmintadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem правкиToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bSearch;
        private System.Windows.Forms.ToolStripMenuItem determineHostingMenuItem;
        private System.Windows.Forms.ToolStripMenuItem determineCpaMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem bGetSiteList;
        private System.Windows.Forms.ToolStripMenuItem mContinue;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.DataGridViewTextBoxColumn colID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDateCheck;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPageUrl;
        private System.Windows.Forms.DataGridViewTextBoxColumn colKeyword;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCurrentPosition;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMidCurrent;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLastPosition;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMidPrev;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFoundPageUrl;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAction;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCpaId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCpaName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colHostingId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colHostingName;
    }
}

