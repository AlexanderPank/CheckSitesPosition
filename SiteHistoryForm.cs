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
            chartArea.AxisY.IsReversed = false; // Переводим ось Y в классическую систему координат с началом отсчета снизу
            chartArea.AxisY.LabelStyle.Format = "0"; // Принудительно задаем целочисленный формат подписей оси Y
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
                // Рассчитываем целевые границы по оси Y и приводим их к удобному шагу отображения
                var minValue = yValuesBuffer.Min();
                var maxValue = yValuesBuffer.Max();
                if (Math.Abs(maxValue - minValue) < double.Epsilon)
                {
                    maxValue = minValue + 1;
                }

                ApplyVerticalAxisLayout(chartArea, minValue, maxValue);
            }
            else
            {
                chartArea.AxisY.Minimum = double.NaN;
                chartArea.AxisY.Maximum = double.NaN;
                chartArea.AxisX.Crossing = double.NaN;
                chartArea.AxisY.CustomLabels.Clear(); // Сбрасываем пользовательские подписи при отсутствии данных
            }
        }

        private static void ApplyVerticalAxisLayout(ChartArea chartArea, double minValue, double maxValue)
        {
            // Настраиваем ось Y так, чтобы график начинался в левом нижнем углу и имел целочисленные подписи
            var axisY = chartArea.AxisY;
            var lowerBound = minValue < 0 ? Math.Floor(minValue) : 0d; // Корректируем нижнюю границу, чтобы отрицательные значения оставались видимыми
            var upperBound = DetermineAxisUpperBound(maxValue); // Подбираем верхнюю границу с округлением до десятков

            axisY.Minimum = lowerBound; // Фиксируем нижнюю границу оси Y
            axisY.Maximum = upperBound; // Фиксируем верхнюю границу оси Y
            axisY.Interval = 1d; // Обеспечиваем дискретность шкалы в один пункт для всех отметок
            axisY.MajorTickMark.Interval = 1d; // Сохраняем метки оси на каждом целочисленном значении
            axisY.MajorGrid.Interval = 5d; // Выводим вспомогательные линии каждые пять позиций для лучшей читаемости
            axisY.MajorGrid.IntervalOffset = 0d; // Обнуляем смещение сетки, чтобы отметки совпадали с целыми значениями
            axisY.CustomLabels.Clear(); // Удаляем ранее добавленные пользовательские подписи

            foreach (var value in BuildAxisLabels(lowerBound, upperBound))
            {
                axisY.CustomLabels.Add(new CustomLabel(value - 0.5, value + 0.5, value.ToString("0"), 0, LabelMarkStyle.None)); // Формируем подписи ровно для нужных значений
            }

            chartArea.AxisX.Crossing = lowerBound; // Перемещаем ось X к нижней границе для отображения дат снизу
            axisY.Crossing = double.NaN; // Сохраняем ось Y на левой границе области построения
        }

        private static IReadOnlyList<double> BuildAxisLabels(double lowerBound, double upperBound)
        {
            // Готовим набор целочисленных отметок, соответствующих требованиям заказчика
            var labels = new SortedSet<double>();
            var normalizedLower = Math.Ceiling(lowerBound);
            if (normalizedLower > upperBound)
            {
                normalizedLower = upperBound;
            }

            for (var value = normalizedLower; value <= upperBound; value += 1d)
            {
                labels.Add(value); // Добавляем все целые значения в диапазоне для поддержки отрицательных диапазонов
            }

            if (upperBound >= 1d)
            {
                labels.Add(1d); // Явно фиксируем отметку "1" как ключевую для поисковых позиций
            }

            if (upperBound >= 5d)
            {
                labels.Add(5d); // Добавляем отметку "5" согласно требуемому набору значений
            }

            for (var value = 0d; value <= upperBound; value += 10d)
            {
                labels.Add(value); // Дополняем шкалу отметками каждые десять позиций
            }

            return labels
                .Where(value => value >= lowerBound && value <= upperBound)
                .OrderBy(value => value)
                .ToArray(); // Сортируем и возвращаем итоговый набор отметок в виде неизменяемой коллекции
        }

        private static double DetermineAxisUpperBound(double maxValue)
        {
            // Округляем верхнюю границу шкалы до ближайшего десятка, сохраняя минимум в районе пятерки для компактных наборов данных
            if (maxValue <= 5d)
            {
                return 5d;
            }

            var rounded = Math.Ceiling(maxValue / 10d) * 10d; // Поднимаем верхнюю границу до ближайших десяти
            return Math.Max(rounded, 10d); // Гарантируем, что шкала не станет слишком узкой
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
