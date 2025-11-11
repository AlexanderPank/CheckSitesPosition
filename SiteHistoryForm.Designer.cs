namespace CheckPosition
{
    partial class SiteHistoryForm
    {
        private System.ComponentModel.IContainer components = null;
        // Диаграмма для отображения динамики позиции сайта
        private System.Windows.Forms.DataVisualization.Charting.Chart historyChart;
        // Панель с чекбоксами для управления отображением серий
        private System.Windows.Forms.FlowLayoutPanel seriesFilterPanel;
        // Флажок для отображения основной позиции
        private System.Windows.Forms.CheckBox showPositionCheckBox;
        // Флажок для отображения средней позиции
        private System.Windows.Forms.CheckBox showMiddleCheckBox;

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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SiteHistoryForm));
            this.historyChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.seriesFilterPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.showPositionCheckBox = new System.Windows.Forms.CheckBox();
            this.showMiddleCheckBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.historyChart)).BeginInit();
            this.seriesFilterPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // historyChart
            // 
            chartArea1.Name = "ChartArea1";
            this.historyChart.ChartAreas.Add(chartArea1);
            this.historyChart.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Name = "Legend1";
            this.historyChart.Legends.Add(legend1);
            this.historyChart.Location = new System.Drawing.Point(16, 40);
            this.historyChart.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.historyChart.Name = "historyChart";
            this.historyChart.Size = new System.Drawing.Size(1035, 432);
            this.historyChart.TabIndex = 1;
            this.historyChart.Text = "chart1";
            this.historyChart.Click += new System.EventHandler(this.historyChart_Click);
            // 
            // seriesFilterPanel
            // 
            this.seriesFilterPanel.AutoSize = true;
            this.seriesFilterPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.seriesFilterPanel.Controls.Add(this.showMiddleCheckBox);
            this.seriesFilterPanel.Controls.Add(this.showPositionCheckBox);
            this.seriesFilterPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.seriesFilterPanel.Location = new System.Drawing.Point(16, 13);
            this.seriesFilterPanel.Margin = new System.Windows.Forms.Padding(0);
            this.seriesFilterPanel.Name = "seriesFilterPanel";
            this.seriesFilterPanel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 9);
            this.seriesFilterPanel.Size = new System.Drawing.Size(1035, 27);
            this.seriesFilterPanel.TabIndex = 0;
            this.seriesFilterPanel.WrapContents = false;
            // 
            // showPositionCheckBox
            // 
            this.showPositionCheckBox.AutoSize = true;
            this.showPositionCheckBox.Checked = true;
            this.showPositionCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showPositionCheckBox.Location = new System.Drawing.Point(244, 0);
            this.showPositionCheckBox.Margin = new System.Windows.Forms.Padding(0, 0, 16, 0);
            this.showPositionCheckBox.Name = "showPositionCheckBox";
            this.showPositionCheckBox.Size = new System.Drawing.Size(164, 18);
            this.showPositionCheckBox.TabIndex = 0;
            this.showPositionCheckBox.Text = "Показывать позицию";
            this.showPositionCheckBox.UseVisualStyleBackColor = true;
            this.showPositionCheckBox.CheckedChanged += new System.EventHandler(this.OnSeriesVisibilityChanged);
            // 
            // showMiddleCheckBox
            // 
            this.showMiddleCheckBox.AutoSize = true;
            this.showMiddleCheckBox.Checked = true;
            this.showMiddleCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showMiddleCheckBox.Location = new System.Drawing.Point(0, 0);
            this.showMiddleCheckBox.Margin = new System.Windows.Forms.Padding(0, 0, 16, 0);
            this.showMiddleCheckBox.Name = "showMiddleCheckBox";
            this.showMiddleCheckBox.Size = new System.Drawing.Size(228, 18);
            this.showMiddleCheckBox.TabIndex = 1;
            this.showMiddleCheckBox.Text = "Показывать среднюю позицию";
            this.showMiddleCheckBox.UseVisualStyleBackColor = true;
            this.showMiddleCheckBox.CheckedChanged += new System.EventHandler(this.OnSeriesVisibilityChanged);
            // 
            // SiteHistoryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1067, 485);
            this.Controls.Add(this.historyChart);
            this.Controls.Add(this.seriesFilterPanel);
            this.Font = new System.Drawing.Font("Verdana", 9F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "SiteHistoryForm";
            this.Padding = new System.Windows.Forms.Padding(16, 13, 16, 13);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "История позиций";
            ((System.ComponentModel.ISupportInitialize)(this.historyChart)).EndInit();
            this.seriesFilterPanel.ResumeLayout(false);
            this.seriesFilterPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
