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
            // Инициализируем визуальные элементы формы истории
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            this.historyChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.seriesFilterPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.showPositionCheckBox = new System.Windows.Forms.CheckBox();
            this.showMiddleCheckBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.historyChart)).BeginInit();
            this.SuspendLayout();
            //
            // historyChart
            //
            // Создаем область диаграммы для отображения серий
            chartArea1.Name = "ChartArea1";
            this.historyChart.ChartAreas.Add(chartArea1);
            // Добавляем легенду, чтобы подписывать серии данных
            legend1.Name = "Legend1";
            this.historyChart.Legends.Add(legend1);
            // Растягиваем диаграмму, чтобы занимала все окно формы
            this.historyChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.historyChart.Location = new System.Drawing.Point(12, 59);
            this.historyChart.Name = "historyChart";
            this.historyChart.Size = new System.Drawing.Size(776, 379);
            this.historyChart.TabIndex = 1;
            this.historyChart.Text = "chart1";
            //
            // seriesFilterPanel
            //
            // Создаем панель с флажками для управления сериями
            this.seriesFilterPanel.AutoSize = true;
            this.seriesFilterPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.seriesFilterPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.seriesFilterPanel.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.seriesFilterPanel.Location = new System.Drawing.Point(12, 12);
            this.seriesFilterPanel.Margin = new System.Windows.Forms.Padding(0);
            this.seriesFilterPanel.Name = "seriesFilterPanel";
            this.seriesFilterPanel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.seriesFilterPanel.Size = new System.Drawing.Size(776, 47);
            this.seriesFilterPanel.TabIndex = 0;
            this.seriesFilterPanel.WrapContents = false;
            //
            // showPositionCheckBox
            //
            // Добавляем флажок управления основной позицией
            this.showPositionCheckBox.AutoSize = true;
            this.showPositionCheckBox.Checked = true;
            this.showPositionCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showPositionCheckBox.Location = new System.Drawing.Point(0, 0);
            this.showPositionCheckBox.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.showPositionCheckBox.Name = "showPositionCheckBox";
            this.showPositionCheckBox.Size = new System.Drawing.Size(131, 17);
            this.showPositionCheckBox.TabIndex = 0;
            this.showPositionCheckBox.Text = "Показывать позицию";
            this.showPositionCheckBox.UseVisualStyleBackColor = true;
            this.showPositionCheckBox.CheckedChanged += new System.EventHandler(this.OnSeriesVisibilityChanged);
            //
            // showMiddleCheckBox
            //
            // Добавляем флажок управления средней позицией
            this.showMiddleCheckBox.AutoSize = true;
            this.showMiddleCheckBox.Checked = true;
            this.showMiddleCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showMiddleCheckBox.Location = new System.Drawing.Point(143, 0);
            this.showMiddleCheckBox.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.showMiddleCheckBox.Name = "showMiddleCheckBox";
            this.showMiddleCheckBox.Size = new System.Drawing.Size(168, 17);
            this.showMiddleCheckBox.TabIndex = 1;
            this.showMiddleCheckBox.Text = "Показывать среднюю позицию";
            this.showMiddleCheckBox.UseVisualStyleBackColor = true;
            this.showMiddleCheckBox.CheckedChanged += new System.EventHandler(this.OnSeriesVisibilityChanged);
            //
            // SiteHistoryForm
            //
            // Настраиваем базовые параметры формы истории позиций
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.historyChart);
            this.Controls.Add(this.seriesFilterPanel);
            this.Name = "SiteHistoryForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "История позиций";
            // Задаем отступы формы, чтобы график не прилипал к краям окна
            this.Padding = new System.Windows.Forms.Padding(12);
            this.seriesFilterPanel.Controls.Add(this.showPositionCheckBox);
            this.seriesFilterPanel.Controls.Add(this.showMiddleCheckBox);
            ((System.ComponentModel.ISupportInitialize)(this.historyChart)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
