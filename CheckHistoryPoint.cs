using System;

namespace CheckPosition
{
    // Класс описывает точку истории изменения позиции сайта
    public sealed class CheckHistoryPoint
    {
        public DateTime Date { get; }
        public int Position { get; }
        public int? MiddlePosition { get; }

        public CheckHistoryPoint(DateTime date, int position, int? middlePosition)
        {
            // Сохраняем значения измерения для дальнейшей визуализации
            Date = date;
            Position = position;
            MiddlePosition = middlePosition;
        }
    }
}
