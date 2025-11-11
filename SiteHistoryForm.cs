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
        // Храним агрегированную по месяцам коллекцию точек истории
        private readonly IReadOnlyList<MonthlyHistoryPoint> _monthlyHistory;

        public SiteHistoryForm(string siteCaption, IReadOnlyList<CheckHistoryPoint> history)
        {
            // Запускаем конструктор формы и сохраняем данные истории
            InitializeComponent();
            // Сохраняем помесячно агрегированные точки истории, заранее сортируя исходные данные по датам
            var orderedHistory = (history ?? throw new ArgumentNullException(nameof(history)))
                .OrderBy(point => point.Date)
                .ToArray();
            _monthlyHistory = PrepareMonthlyHistory(orderedHistory);
            Text = string.IsNullOrWhiteSpace(siteCaption) ? "История позиций" : $"История позиций: {siteCaption}";
            ConfigureChart();
            InitializeSeriesFilters();
            BindHistory();
        }

        private void ConfigureChart()
        {
            // Настраиваем визуальные параметры области диаграммы
            var chartArea = historyChart.ChartAreas[0];
            chartArea.AxisX.LabelStyle.Format = "MMM yyyy";
            chartArea.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            chartArea.AxisX.Interval = 1;
            chartArea.AxisX.IntervalType = DateTimeIntervalType.Months;
            chartArea.AxisX.MajorGrid.Interval = 1;
            chartArea.AxisX.MajorGrid.IntervalType = DateTimeIntervalType.Months;
            chartArea.AxisX.MajorGrid.LineColor = Color.LightGray;
            chartArea.AxisX.MajorTickMark.Interval = 1;
            chartArea.AxisX.MajorTickMark.IntervalOffset = 0;
            chartArea.AxisX.LabelStyle.Angle = -45;
            chartArea.AxisY.MajorGrid.LineColor = Color.LightGray;
            chartArea.AxisY.Title = "Позиция";
            chartArea.AxisX.Title = "Дата";
            chartArea.AxisY.IsReversed = true;
            chartArea.AxisX.Crossing = double.NaN;
            // Размещаем легенду сверху для экономии вертикального места
            historyChart.Legends[0].Docking = Docking.Top;
            historyChart.Legends[0].Alignment = StringAlignment.Near;
        }

        private void BindHistory()
        {
            // Пересоздаем серии, чтобы обновить график под актуальные данные
            historyChart.Series.Clear();

            if (_monthlyHistory.Count == 0)
            {
                // Показываем информационный заголовок при отсутствии данных
                historyChart.Titles.Clear();
                historyChart.Titles.Add("Нет данных для отображения");
                historyChart.ChartAreas[0].AxisY.Minimum = double.NaN;
                historyChart.ChartAreas[0].AxisY.Maximum = double.NaN;
                historyChart.ChartAreas[0].AxisX.Crossing = double.NaN;
                return;
            }

            historyChart.Titles.Clear();

            var chartArea = historyChart.ChartAreas[0];
            var yValuesBuffer = new List<double>(_monthlyHistory.Count * 2);

            if (showPositionCheckBox.Checked)
            {
                // Создаем серию позиций и заполняем усредненными значениями по месяцам
                var positionSeries = new Series("Позиция")
                {
                    ChartType = SeriesChartType.Line,
                    XValueType = ChartValueType.DateTime,
                    YValueType = ChartValueType.Double,
                    BorderWidth = 3,
                    MarkerStyle = MarkerStyle.Circle,
                    MarkerSize = 7,
                    Color = Color.FromArgb(33, 150, 243)
                };

                foreach (var point in _monthlyHistory)
                {
                    positionSeries.Points.AddXY(point.Month, point.PositionAverage);
                    yValuesBuffer.Add(point.PositionAverage);
                }

                historyChart.Series.Add(positionSeries);
            }

            if (showMiddleCheckBox.Checked)
            {
                // Добавляем серию средних позиций только по тем месяцам, где есть данные
                var averageSeries = new Series("Средняя позиция")
                {
                    ChartType = SeriesChartType.Line,
                    XValueType = ChartValueType.DateTime,
                    YValueType = ChartValueType.Double,
                    BorderDashStyle = ChartDashStyle.Dash,
                    BorderWidth = 2,
                    MarkerStyle = MarkerStyle.Diamond,
                    MarkerSize = 6,
                    Color = Color.FromArgb(76, 175, 80)
                };

                foreach (var point in _monthlyHistory)
                {
                    if (!point.MiddlePositionAverage.HasValue)
                    {
                        continue;
                    }

                    averageSeries.Points.AddXY(point.Month, point.MiddlePositionAverage.Value);
                    yValuesBuffer.Add(point.MiddlePositionAverage.Value);
                }

                if (averageSeries.Points.Count > 0)
                {
                    historyChart.Series.Add(averageSeries);
                }
            }

            if (historyChart.Series.Count == 0)
            {
                // Сообщаем пользователю, что необходимо выбрать хотя бы одну серию
                historyChart.Titles.Clear();
                historyChart.Titles.Add("Выберите хотя бы одну серию для отображения");
                chartArea.AxisY.Minimum = double.NaN;
                chartArea.AxisY.Maximum = double.NaN;
                chartArea.AxisX.Crossing = double.NaN;
                return;
            }

            if (yValuesBuffer.Count > 0)
            {
                // Фиксируем границы оси Y, чтобы разместить подписи дат у нижней границы графика
                var minValue = yValuesBuffer.Min();
                var maxValue = yValuesBuffer.Max();
                if (Math.Abs(maxValue - minValue) < double.Epsilon)
                {
                    maxValue = minValue + 1;
                }

                chartArea.AxisY.Minimum = minValue;
                chartArea.AxisY.Maximum = maxValue;
                chartArea.AxisX.Crossing = maxValue;
            }
            else
            {
                chartArea.AxisY.Minimum = double.NaN;
                chartArea.AxisY.Maximum = double.NaN;
                chartArea.AxisX.Crossing = double.NaN;
            }
        }

        private void InitializeSeriesFilters()
        {
            // Инициализируем состояние чекбоксов исходя из наличия соответствующих данных
            var hasHistory = _monthlyHistory.Count > 0;
            showPositionCheckBox.Checked = hasHistory;
            showPositionCheckBox.Enabled = hasHistory;

            var hasMiddlePosition = _monthlyHistory.Any(point => point.MiddlePositionAverage.HasValue);
            showMiddleCheckBox.Enabled = hasMiddlePosition;
            showMiddleCheckBox.Checked = hasMiddlePosition;
        }

        private void OnSeriesVisibilityChanged(object sender, EventArgs e)
        {
            // При переключении видимости серий перестраиваем график
            BindHistory();
        }

        private static IReadOnlyList<MonthlyHistoryPoint> PrepareMonthlyHistory(IEnumerable<CheckHistoryPoint> history)
        {
            // Группируем точки по месяцам и вычисляем средние значения метрик
            var monthlyGroups = history
                .GroupBy(point => new DateTime(point.Date.Year, point.Date.Month, 1))
                .OrderBy(group => group.Key)
                .ToArray();

            if (monthlyGroups.Length == 0)
            {
                return Array.Empty<MonthlyHistoryPoint>();
            }

            var result = new List<MonthlyHistoryPoint>(monthlyGroups.Length);
            foreach (var group in monthlyGroups)
            {
                var positionAverage = group.Average(point => point.Position);
                var middleValues = group
                    .Where(point => point.MiddlePosition.HasValue)
                    .Select(point => (double)point.MiddlePosition.Value)
                    .ToArray();

                double middleAverage = middleValues.Length == 0 ? 0 : middleValues.Average();
                result.Add(new MonthlyHistoryPoint(group.Key, positionAverage, middleAverage));
            }

            return result;
        }

        private sealed class MonthlyHistoryPoint
        {
            public MonthlyHistoryPoint(DateTime month, double positionAverage, double? middlePositionAverage)
            {
                // Фиксируем усредненные за месяц значения для построения графика
                Month = month;
                PositionAverage = positionAverage;
                MiddlePositionAverage = middlePositionAverage;
            }

            public DateTime Month { get; }
            public double PositionAverage { get; }
            public double? MiddlePositionAverage { get; }
        }

        private void historyChart_Click(object sender, EventArgs e)
        {

        }
    }
}
