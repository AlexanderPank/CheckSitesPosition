using System;
using System.Linq;
using System.Windows.Forms;

namespace CheckPosition
{
    public partial class HostingList : Form
    {
        private readonly DataBaseSqlite _database;
        private bool _isLoading;

        public HostingList(DataBaseSqlite database)
        {
            InitializeComponent();
            // Сохраняем ссылку на базу и блокируем пустой запуск формы
            _database = database ?? throw new ArgumentNullException(nameof(database));
            // Отключаем автогенерацию столбцов, чтобы контролировать порядок и имена колонок
            hostingGrid.AutoGenerateColumns = false;
        }

        private void HostingList_Shown(object sender, EventArgs e)
        {
            // При первом показе формы загружаем все записи из таблицы
            LoadData();
        }

        private void LoadData()
        {
            // Загружаем данные из БД и наполняем таблицу вручную
            _isLoading = true;
            try
            {
                hostingGrid.Rows.Clear();
                foreach (HostingRecord record in _database.LoadHostingList())
                {
                    hostingGrid.Rows.Add(record.Id, record.Name, record.Ip);
                }
            }
            finally
            {
                _isLoading = false;
            }
        }

        private static string NormalizeIp(string value)
        {
            // Нормализуем список IP-адресов, разделяя их запятыми и удаляя дубликаты
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string[] separators = { " ", ",", ";", "\t", "\r", "\n" };
            string[] parts = value.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(", ", parts.Select(p => p.Trim()).Where(p => p.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase));
        }

        private static long GetRowId(DataGridViewRow row)
        {
            // Получаем числовой идентификатор из первой колонки строки
            if (row == null || row.Cells.Count == 0)
            {
                return 0;
            }

            object rawValue = row.Cells[0].Value;
            return Helper.getIngValue(rawValue, 0);
        }

        private void hostingGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // После редактирования ячейки обновляем соответствующее поле в базе
            if (_isLoading)
            {
                return;
            }

            DataGridViewRow row = hostingGrid.Rows[e.RowIndex];
            if (row.IsNewRow)
            {
                return;
            }

            long id = GetRowId(row);
            if (id <= 0)
            {
                return;
            }

            string fieldName = e.ColumnIndex == colName.Index ? "name" : e.ColumnIndex == colIp.Index ? "ip" : string.Empty;
            if (string.IsNullOrEmpty(fieldName))
            {
                return;
            }

            string newValue = Convert.ToString(row.Cells[e.ColumnIndex].Value)?.Trim() ?? string.Empty;
            if (fieldName == "ip")
            {
                newValue = NormalizeIp(newValue);
                row.Cells[colIp.Index].Value = newValue;
            }

            _database.UpdateHostingField(id, fieldName, newValue);
        }

        private void hostingGrid_RowValidated(object sender, DataGridViewCellEventArgs e)
        {
            // При завершении редактирования строки без идентификатора создаем новую запись в таблице
            if (_isLoading)
            {
                return;
            }

            DataGridViewRow row = hostingGrid.Rows[e.RowIndex];
            if (row == null || row.IsNewRow)
            {
                return;
            }

            long id = GetRowId(row);
            if (id > 0)
            {
                return;
            }

            string name = Convert.ToString(row.Cells[colName.Index].Value)?.Trim() ?? string.Empty;
            string ip = Convert.ToString(row.Cells[colIp.Index].Value)?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            ip = NormalizeIp(ip);
            row.Cells[colIp.Index].Value = ip;
            long newId = _database.InsertHostingRecord(name, ip);
            row.Cells[colId.Index].Value = newId;
        }
    }
}
