namespace CheckPosition
{
    partial class SiteHistoryForm
    {
        private System.ComponentModel.IContainer components = null;
        // Диаграмма для отображения динамики позиции сайта
        private System.Windows.Forms.DataVisualization.Charting.Chart historyChart;

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
            this.historyChart.Location = new System.Drawing.Point(12, 12);
            this.historyChart.Name = "historyChart";
            this.historyChart.Size = new System.Drawing.Size(776, 426);
            this.historyChart.TabIndex = 0;
            this.historyChart.Text = "chart1";
            //
            // SiteHistoryForm
            //
            // Настраиваем базовые параметры формы истории позиций
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.historyChart);
            this.Name = "SiteHistoryForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "История позиций";
            // Задаем отступы формы, чтобы график не прилипал к краям окна
            this.Padding = new System.Windows.Forms.Padding(12);
            ((System.ComponentModel.ISupportInitialize)(this.historyChart)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
