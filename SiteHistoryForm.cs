using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace CheckPosition
{
    // Форма визуализирует историю изменения позиций выбранного сайта
    public partial class SiteHistoryForm : Form
    {
        // Храним подготовленную коллекцию точек истории
        private readonly IReadOnlyList<CheckHistoryPoint> _history;

        public SiteHistoryForm(string siteCaption, IReadOnlyList<CheckHistoryPoint> history)
        {
            // Запускаем конструктор формы и сохраняем данные истории
            InitializeComponent();
            _history = history ?? throw new ArgumentNullException(nameof(history));
            Text = string.IsNullOrWhiteSpace(siteCaption) ? "История позиций" : $"История позиций: {siteCaption}";
            ConfigureChart();
            BindHistory();
        }

        private void ConfigureChart()
        {
            // Настраиваем визуальные параметры области диаграммы
            var chartArea = historyChart.ChartAreas[0];
            chartArea.AxisX.LabelStyle.Format = "dd.MM.yyyy";
            chartArea.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            chartArea.AxisX.MajorGrid.LineColor = Color.LightGray;
            chartArea.AxisY.MajorGrid.LineColor = Color.LightGray;
            chartArea.AxisY.Title = "Позиция";
            chartArea.AxisX.Title = "Дата";
            chartArea.AxisY.IsReversed = true;
            // Размещаем легенду сверху для экономии вертикального места
            historyChart.Legends[0].Docking = Docking.Top;
        }

        private void BindHistory()
        {
            // Пересоздаем серии, чтобы обновить график под актуальные данные
            historyChart.Series.Clear();

            if (_history.Count == 0)
            {
                // Показываем информационный заголовок при отсутствии данных
                historyChart.Titles.Clear();
                historyChart.Titles.Add("Нет данных для отображения");
                return;
            }

            historyChart.Titles.Clear();

            var positionSeries = new Series("Позиция")
            {
                ChartType = SeriesChartType.Line,
                XValueType = ChartValueType.DateTime,
                YValueType = ChartValueType.Int32,
                BorderWidth = 3,
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 7,
                Color = Color.FromArgb(33, 150, 243)
            };

            foreach (var point in _history)
            {
                // Добавляем точку позиции в основную серию
                positionSeries.Points.AddXY(point.Date, point.Position);
            }

            historyChart.Series.Add(positionSeries);

            if (_history.Any(p => p.MiddlePosition.HasValue))
            {
                var averageSeries = new Series("Средняя позиция")
                {
                    ChartType = SeriesChartType.Line,
                    XValueType = ChartValueType.DateTime,
                    YValueType = ChartValueType.Int32,
                    BorderDashStyle = ChartDashStyle.Dash,
                    BorderWidth = 2,
                    MarkerStyle = MarkerStyle.Diamond,
                    MarkerSize = 6,
                    Color = Color.FromArgb(76, 175, 80)
                };

                foreach (var point in _history)
                {
                    if (point.MiddlePosition.HasValue)
                    {
                        // Добавляем точку средней позиции в дополнительную серию
                        averageSeries.Points.AddXY(point.Date, point.MiddlePosition.Value);
                    }
                }

                if (averageSeries.Points.Count > 0)
                {
                    // Выводим среднюю линию, только если данные присутствуют
                    historyChart.Series.Add(averageSeries);
                }
            }
        }
    }
}
