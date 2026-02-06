namespace CheckPosition
{
    partial class AnalysisDataForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView analysisGrid;
        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.Button checkAllButton;
        private System.Windows.Forms.Button checkSelectedButton;
        private System.Windows.Forms.Button addMissingButton;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.Label searchLabel;
        private System.Windows.Forms.TextBox searchTextBox;
        private System.Windows.Forms.FlowLayoutPanel columnsPanel;
        private System.Windows.Forms.ComboBox strategyComboBox;
        private System.Windows.Forms.Label strategyLabel;
        private System.Windows.Forms.Button exportCsvButton;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.analysisGrid = new System.Windows.Forms.DataGridView();
            this.topPanel = new System.Windows.Forms.Panel();
            this.checkAllButton = new System.Windows.Forms.Button();
            this.checkSelectedButton = new System.Windows.Forms.Button();
            this.addMissingButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.searchLabel = new System.Windows.Forms.Label();
            this.searchTextBox = new System.Windows.Forms.TextBox();
            this.columnsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.strategyComboBox = new System.Windows.Forms.ComboBox();
            this.strategyLabel = new System.Windows.Forms.Label();
            this.exportCsvButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.analysisGrid)).BeginInit();
            this.topPanel.SuspendLayout();
            this.columnsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // analysisGrid
            // 
            this.analysisGrid.AllowUserToAddRows = false;
            this.analysisGrid.AllowUserToDeleteRows = false;
            this.analysisGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.analysisGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.analysisGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.analysisGrid.Location = new System.Drawing.Point(0, 128);
            this.analysisGrid.MultiSelect = false;
            this.analysisGrid.Name = "analysisGrid";
            this.analysisGrid.ReadOnly = true;
            this.analysisGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.analysisGrid.Size = new System.Drawing.Size(1200, 522);
            this.analysisGrid.TabIndex = 0;
            // 
            // topPanel
            // 
            this.topPanel.Controls.Add(this.exportCsvButton);
            this.topPanel.Controls.Add(this.strategyComboBox);
            this.topPanel.Controls.Add(this.strategyLabel);
            this.topPanel.Controls.Add(this.searchTextBox);
            this.topPanel.Controls.Add(this.searchLabel);
            this.topPanel.Controls.Add(this.stopButton);
            this.topPanel.Controls.Add(this.addMissingButton);
            this.topPanel.Controls.Add(this.checkSelectedButton);
            this.topPanel.Controls.Add(this.checkAllButton);
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topPanel.Location = new System.Drawing.Point(0, 0);
            this.topPanel.Name = "topPanel";
            this.topPanel.Size = new System.Drawing.Size(1200, 48);
            this.topPanel.TabIndex = 1;
            // 
            // checkAllButton
            // 
            this.checkAllButton.Location = new System.Drawing.Point(12, 12);
            this.checkAllButton.Name = "checkAllButton";
            this.checkAllButton.Size = new System.Drawing.Size(150, 23);
            this.checkAllButton.TabIndex = 0;
            this.checkAllButton.Text = "Проверить все";
            this.checkAllButton.UseVisualStyleBackColor = true;
            this.checkAllButton.Click += new System.EventHandler(this.checkAllButton_Click);
            // 
            // checkSelectedButton
            // 
            this.checkSelectedButton.Location = new System.Drawing.Point(168, 12);
            this.checkSelectedButton.Name = "checkSelectedButton";
            this.checkSelectedButton.Size = new System.Drawing.Size(170, 23);
            this.checkSelectedButton.TabIndex = 1;
            this.checkSelectedButton.Text = "Проверить выбранный";
            this.checkSelectedButton.UseVisualStyleBackColor = true;
            this.checkSelectedButton.Click += new System.EventHandler(this.checkSelectedButton_Click);
            // 
            // stopButton
            // 
            this.stopButton.Enabled = false;
            this.stopButton.Location = new System.Drawing.Point(470, 12);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(100, 23);
            this.stopButton.TabIndex = 2;
            this.stopButton.Text = "Стоп";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // addMissingButton
            // 
            this.addMissingButton.Location = new System.Drawing.Point(344, 12);
            this.addMissingButton.Name = "addMissingButton";
            this.addMissingButton.Size = new System.Drawing.Size(120, 23);
            this.addMissingButton.TabIndex = 5;
            this.addMissingButton.Text = "Добавить новые";
            this.addMissingButton.UseVisualStyleBackColor = true;
            this.addMissingButton.Click += new System.EventHandler(this.addMissingButton_Click);
            // 
            // searchLabel
            // 
            this.searchLabel.AutoSize = true;
            this.searchLabel.Location = new System.Drawing.Point(758, 16);
            this.searchLabel.Name = "searchLabel";
            this.searchLabel.Size = new System.Drawing.Size(49, 13);
            this.searchLabel.TabIndex = 3;
            this.searchLabel.Text = "Поиск:";
            // 
            // searchTextBox
            // 
            this.searchTextBox.Location = new System.Drawing.Point(813, 13);
            this.searchTextBox.Name = "searchTextBox";
            this.searchTextBox.Size = new System.Drawing.Size(300, 20);
            this.searchTextBox.TabIndex = 4;
            this.searchTextBox.TextChanged += new System.EventHandler(this.searchTextBox_TextChanged);
            // 
            // strategyComboBox
            // 
            this.strategyComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.strategyComboBox.FormattingEnabled = true;
            this.strategyComboBox.Location = new System.Drawing.Point(639, 13);
            this.strategyComboBox.Name = "strategyComboBox";
            this.strategyComboBox.Size = new System.Drawing.Size(110, 21);
            this.strategyComboBox.TabIndex = 6;
            this.strategyComboBox.SelectedIndexChanged += new System.EventHandler(this.strategyComboBox_SelectedIndexChanged);
            // 
            // strategyLabel
            // 
            this.strategyLabel.AutoSize = true;
            this.strategyLabel.Location = new System.Drawing.Point(586, 16);
            this.strategyLabel.Name = "strategyLabel";
            this.strategyLabel.Size = new System.Drawing.Size(47, 13);
            this.strategyLabel.TabIndex = 7;
            this.strategyLabel.Text = "Версия:";
            // 
            // exportCsvButton
            // 
            this.exportCsvButton.Location = new System.Drawing.Point(1120, 12);
            this.exportCsvButton.Name = "exportCsvButton";
            this.exportCsvButton.Size = new System.Drawing.Size(68, 23);
            this.exportCsvButton.TabIndex = 8;
            this.exportCsvButton.Text = "CSV";
            this.exportCsvButton.UseVisualStyleBackColor = true;
            this.exportCsvButton.Click += new System.EventHandler(this.exportCsvButton_Click);
            // 
            // columnsPanel
            // 
            this.columnsPanel.AutoScroll = true;
            this.columnsPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.columnsPanel.Location = new System.Drawing.Point(0, 48);
            this.columnsPanel.Name = "columnsPanel";
            this.columnsPanel.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.columnsPanel.Size = new System.Drawing.Size(1200, 80);
            this.columnsPanel.TabIndex = 2;
            // 
            // AnalysisDataForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 650);
            this.Controls.Add(this.analysisGrid);
            this.Controls.Add(this.columnsPanel);
            this.Controls.Add(this.topPanel);
            this.Name = "AnalysisDataForm";
            this.Text = "Анализ данных";
            this.Shown += new System.EventHandler(this.AnalysisDataForm_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.analysisGrid)).EndInit();
            this.topPanel.ResumeLayout(false);
            this.topPanel.PerformLayout();
            this.columnsPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
