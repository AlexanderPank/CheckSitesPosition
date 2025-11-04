namespace CheckPosition
{
    partial class DomainList
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DomainList));
            this.panel1 = new System.Windows.Forms.Panel();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.buttonStop = new System.Windows.Forms.Button();
            this.bCheckSite = new System.Windows.Forms.Button();
            this.bRegRuLoad = new System.Windows.Forms.Button();
            this.labelInfo = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.dg = new System.Windows.Forms.DataGridView();
            this.colID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRusName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colExpireDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIP = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colHasSite = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colVisits = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSails = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colComment = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMetrica = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colShow = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.contextCheckSiteAndIp = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.contextCheckMetricaCounter = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.bHide = new System.Windows.Forms.ToolStripMenuItem();
            this.bShowHidden = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.метрикаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.searchCounterYandexMetrica = new System.Windows.Forms.ToolStripMenuItem();
            this.загрузкаСтатистикаЗа30ДнейToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.iPАдресаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bShowCountIp = new System.Windows.Forms.ToolStripMenuItem();
            this.доменыToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dg)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.textBox1);
            this.panel1.Controls.Add(this.buttonStop);
            this.panel1.Controls.Add(this.bCheckSite);
            this.panel1.Controls.Add(this.bRegRuLoad);
            this.panel1.Controls.Add(this.labelInfo);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1251, 84);
            this.panel1.TabIndex = 2;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 45);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(411, 22);
            this.textBox1.TabIndex = 6;
            // 
            // buttonStop
            // 
            this.buttonStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(222)))), ((int)(((byte)(222)))));
            this.buttonStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonStop.ForeColor = System.Drawing.Color.Red;
            this.buttonStop.Location = new System.Drawing.Point(547, 12);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(130, 23);
            this.buttonStop.TabIndex = 3;
            this.buttonStop.Text = "STOP ALL";
            this.buttonStop.UseVisualStyleBackColor = false;
            // 
            // bCheckSite
            // 
            this.bCheckSite.BackColor = System.Drawing.Color.Bisque;
            this.bCheckSite.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.bCheckSite.Location = new System.Drawing.Point(217, 12);
            this.bCheckSite.Name = "bCheckSite";
            this.bCheckSite.Size = new System.Drawing.Size(206, 23);
            this.bCheckSite.TabIndex = 2;
            this.bCheckSite.Text = "Проверить наличие сайта";
            this.bCheckSite.UseVisualStyleBackColor = false;
            this.bCheckSite.Click += new System.EventHandler(this.bCheckSite_Click);
            // 
            // bRegRuLoad
            // 
            this.bRegRuLoad.Location = new System.Drawing.Point(12, 12);
            this.bRegRuLoad.Name = "bRegRuLoad";
            this.bRegRuLoad.Size = new System.Drawing.Size(188, 23);
            this.bRegRuLoad.TabIndex = 1;
            this.bRegRuLoad.Text = "RegRu Load";
            this.bRegRuLoad.UseVisualStyleBackColor = true;
            this.bRegRuLoad.Click += new System.EventHandler(this.bRegRuLoad_Click);
            // 
            // labelInfo
            // 
            this.labelInfo.Location = new System.Drawing.Point(724, 6);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(521, 43);
            this.labelInfo.TabIndex = 0;
            this.labelInfo.Text = "Статистика";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.dg);
            this.panel2.Controls.Add(this.menuStrip1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 84);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1251, 438);
            this.panel2.TabIndex = 3;
            // 
            // dg
            // 
            this.dg.AllowUserToOrderColumns = true;
            this.dg.AllowUserToResizeRows = false;
            this.dg.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dg.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dg.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colID,
            this.colName,
            this.colRusName,
            this.colExpireDate,
            this.colIP,
            this.colHasSite,
            this.colVisits,
            this.colSails,
            this.colComment,
            this.colMetrica,
            this.colShow});
            this.dg.ContextMenuStrip = this.contextMenuStrip1;
            this.dg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dg.Location = new System.Drawing.Point(0, 24);
            this.dg.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.dg.Name = "dg";
            this.dg.RowHeadersVisible = false;
            this.dg.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dg.Size = new System.Drawing.Size(1251, 414);
            this.dg.TabIndex = 2;
            this.dg.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dg_CellEndEdit);
            // 
            // colID
            // 
            this.colID.HeaderText = "id";
            this.colID.Name = "colID";
            this.colID.Visible = false;
            // 
            // colName
            // 
            this.colName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colName.HeaderText = "Имя";
            this.colName.Name = "colName";
            this.colName.ReadOnly = true;
            this.colName.Width = 200;
            // 
            // colRusName
            // 
            this.colRusName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colRusName.HeaderText = "на RU";
            this.colRusName.Name = "colRusName";
            this.colRusName.Visible = false;
            this.colRusName.Width = 150;
            // 
            // colExpireDate
            // 
            this.colExpireDate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colExpireDate.HeaderText = "Exp.Date";
            this.colExpireDate.Name = "colExpireDate";
            this.colExpireDate.ReadOnly = true;
            // 
            // colIP
            // 
            this.colIP.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colIP.HeaderText = "IP";
            this.colIP.Name = "colIP";
            // 
            // colHasSite
            // 
            this.colHasSite.HeaderText = "Сайт";
            this.colHasSite.Name = "colHasSite";
            this.colHasSite.ToolTipText = "Наличие сайта";
            // 
            // colVisits
            // 
            this.colVisits.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colVisits.HeaderText = "Пос.";
            this.colVisits.Name = "colVisits";
            this.colVisits.ToolTipText = "Количество посетителей на сайте за месяц";
            this.colVisits.Width = 50;
            // 
            // colSails
            // 
            this.colSails.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colSails.HeaderText = "Прод.";
            this.colSails.Name = "colSails";
            this.colSails.ToolTipText = "Количество продаж с сайта";
            this.colSails.Width = 50;
            // 
            // colComment
            // 
            this.colComment.HeaderText = "Примечание";
            this.colComment.Name = "colComment";
            // 
            // colMetrica
            // 
            this.colMetrica.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colMetrica.HeaderText = "Метрик ID";
            this.colMetrica.Name = "colMetrica";
            // 
            // colShow
            // 
            this.colShow.HeaderText = "is_Show";
            this.colShow.Name = "colShow";
            this.colShow.Visible = false;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contextCheckSiteAndIp,
            this.toolStripMenuItem1,
            this.contextCheckMetricaCounter,
            this.toolStripMenuItem2,
            this.bHide,
            this.bShowHidden});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(241, 104);
            // 
            // contextCheckSiteAndIp
            // 
            this.contextCheckSiteAndIp.Name = "contextCheckSiteAndIp";
            this.contextCheckSiteAndIp.Size = new System.Drawing.Size(240, 22);
            this.contextCheckSiteAndIp.Text = "Проверить наличие сайта и IP";
            this.contextCheckSiteAndIp.Click += new System.EventHandler(this.bCheckSite_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(237, 6);
            // 
            // contextCheckMetricaCounter
            // 
            this.contextCheckMetricaCounter.Name = "contextCheckMetricaCounter";
            this.contextCheckMetricaCounter.Size = new System.Drawing.Size(240, 22);
            this.contextCheckMetricaCounter.Text = "Проверить счетчик метрики";
            this.contextCheckMetricaCounter.Click += new System.EventHandler(this.searchCounterYandexMetrica_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(237, 6);
            // 
            // bHide
            // 
            this.bHide.Name = "bHide";
            this.bHide.Size = new System.Drawing.Size(240, 22);
            this.bHide.Text = "Скрыть домен";
            this.bHide.Click += new System.EventHandler(this.bHide_Click);
            // 
            // bShowHidden
            // 
            this.bShowHidden.Name = "bShowHidden";
            this.bShowHidden.Size = new System.Drawing.Size(240, 22);
            this.bShowHidden.Text = "Показать скрытые домены";
            this.bShowHidden.Click += new System.EventHandler(this.bShowHidden_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.метрикаToolStripMenuItem,
            this.iPАдресаToolStripMenuItem,
            this.доменыToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1251, 24);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // метрикаToolStripMenuItem
            // 
            this.метрикаToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.searchCounterYandexMetrica,
            this.загрузкаСтатистикаЗа30ДнейToolStripMenuItem});
            this.метрикаToolStripMenuItem.Name = "метрикаToolStripMenuItem";
            this.метрикаToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
            this.метрикаToolStripMenuItem.Text = "Метрика";
            // 
            // searchCounterYandexMetrica
            // 
            this.searchCounterYandexMetrica.Name = "searchCounterYandexMetrica";
            this.searchCounterYandexMetrica.Size = new System.Drawing.Size(242, 22);
            this.searchCounterYandexMetrica.Text = "Поиск счетчиков на сайте";
            this.searchCounterYandexMetrica.Click += new System.EventHandler(this.searchCounterYandexMetrica_Click);
            // 
            // загрузкаСтатистикаЗа30ДнейToolStripMenuItem
            // 
            this.загрузкаСтатистикаЗа30ДнейToolStripMenuItem.Name = "загрузкаСтатистикаЗа30ДнейToolStripMenuItem";
            this.загрузкаСтатистикаЗа30ДнейToolStripMenuItem.Size = new System.Drawing.Size(242, 22);
            this.загрузкаСтатистикаЗа30ДнейToolStripMenuItem.Text = "Загрузка статистика за 30 дней";
            // 
            // iPАдресаToolStripMenuItem
            // 
            this.iPАдресаToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bShowCountIp});
            this.iPАдресаToolStripMenuItem.Name = "iPАдресаToolStripMenuItem";
            this.iPАдресаToolStripMenuItem.Size = new System.Drawing.Size(71, 20);
            this.iPАдресаToolStripMenuItem.Text = "IP Адреса";
            // 
            // bShowCountIp
            // 
            this.bShowCountIp.Name = "bShowCountIp";
            this.bShowCountIp.Size = new System.Drawing.Size(188, 22);
            this.bShowCountIp.Text = "Распределение по IP";
            this.bShowCountIp.Click += new System.EventHandler(this.bShowIpInfo_Click);
            // 
            // доменыToolStripMenuItem
            // 
            this.доменыToolStripMenuItem.Name = "доменыToolStripMenuItem";
            this.доменыToolStripMenuItem.Size = new System.Drawing.Size(65, 20);
            this.доменыToolStripMenuItem.Text = "Домены";
            // 
            // DomainList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1251, 522);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Verdana", 9F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.Name = "DomainList";
            this.Text = "Список зарегистрированных доменных имен";
            this.Shown += new System.EventHandler(this.DomainList_Shown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dg)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.DataGridView dg;
        private System.Windows.Forms.Button bRegRuLoad;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Button bCheckSite;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem contextCheckSiteAndIp;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem метрикаToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem searchCounterYandexMetrica;
        private System.Windows.Forms.ToolStripMenuItem загрузкаСтатистикаЗа30ДнейToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem iPАдресаToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bShowCountIp;
        private System.Windows.Forms.ToolStripMenuItem доменыToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contextCheckMetricaCounter;
        private System.Windows.Forms.DataGridViewTextBoxColumn colID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRusName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colExpireDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn colIP;
        private System.Windows.Forms.DataGridViewTextBoxColumn colHasSite;
        private System.Windows.Forms.DataGridViewTextBoxColumn colVisits;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSails;
        private System.Windows.Forms.DataGridViewTextBoxColumn colComment;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMetrica;
        private System.Windows.Forms.DataGridViewTextBoxColumn colShow;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem bHide;
        private System.Windows.Forms.ToolStripMenuItem bShowHidden;
    }
}