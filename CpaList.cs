using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace CheckPosition
{
    public partial class CpaList : Form
    {
        private readonly DataBaseSqlite _database;
        private bool _isLoading;
        private bool _descriptionDirty;
        private DataGridViewRow _currentRow;
        private bool _suppressDescriptionChange;

        public CpaList(DataBaseSqlite database)
        {
            InitializeComponent();
            // Сохраняем ссылку на базу для дальнейшей работы с данными
            _database = database ?? throw new ArgumentNullException(nameof(database));
            // Настраиваем грид для одиночного выбора строк и явного определения колонок
            dgvCpa.AutoGenerateColumns = false;
            dgvCpa.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void CpaList_Shown(object sender, EventArgs e)
        {
            // Подгружаем текущий список CPA-партнеров при открытии окна
            LoadData();
        }

        private void LoadData()
        {
            // Обновляем содержимое таблицы из базы, не реагируя на события редактирования
            _isLoading = true;
            try
            {
                dgvCpa.Rows.Clear();
                foreach (CpaRecord record in _database.LoadCpaList())
                {
                    dgvCpa.Rows.Add(record.Id, record.Name, record.Login, record.Url, record.Script, record.Description);
                }

                if (dgvCpa.Rows.Count > 0)
                {
                    dgvCpa.Rows[0].Selected = true;
                    UpdateDescriptionBinding(dgvCpa.Rows[0]);
                }
                else
                {
                    descriptionText.Clear();
                    _currentRow = null;
                    _descriptionDirty = false;
                }
            }
            finally
            {
                _isLoading = false;
            }
        }

        private static long GetRowId(DataGridViewRow row)
        {
            // Возвращаем значение идентификатора строки через вспомогательный парсер
            if (row == null)
            {
                return 0;
            }

            object rawValue = row.Cells[0].Value;
            return Helper.getIngValue(rawValue, 0);
        }

        private static string NormalizeUrl(string url)
        {
            // Удаляем пробелы и приводим URL к аккуратному виду
            if (string.IsNullOrWhiteSpace(url))
            {
                return string.Empty;
            }

            return url.Trim();
        }

        private void dgvCpa_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // При завершении редактирования ячейки сохраняем изменения в базе
            if (_isLoading)
            {
                return;
            }

            DataGridViewRow row = dgvCpa.Rows[e.RowIndex];
            if (row.IsNewRow)
            {
                return;
            }

            long id = GetRowId(row);
            if (id <= 0)
            {
                return;
            }

            string fieldName = GetFieldName(e.ColumnIndex);
            if (string.IsNullOrEmpty(fieldName))
            {
                return;
            }

            string newValue = Convert.ToString(row.Cells[e.ColumnIndex].Value)?.Trim() ?? string.Empty;
            if (fieldName == "url")
            {
                newValue = NormalizeUrl(newValue);
                row.Cells[e.ColumnIndex].Value = newValue;
            }

            _database.UpdateCpaField(id, fieldName, newValue);
        }

        private string GetFieldName(int columnIndex)
        {
            // Привязываем индекс колонки к имени поля таблицы
            if (columnIndex == colName.Index)
            {
                return "name";
            }

            if (columnIndex == colLogin.Index)
            {
                return "login";
            }

            if (columnIndex == colUrl.Index)
            {
                return "url";
            }

            if (columnIndex == colScript.Index)
            {
                return "script";
            }

            if (columnIndex == colDescription.Index)
            {
                return "description";
            }

            return string.Empty;
        }

        private void dgvCpa_RowValidated(object sender, DataGridViewCellEventArgs e)
        {
            // Если строка новая и валидная, фиксируем ее в таблице базы данных
            if (_isLoading)
            {
                return;
            }

            DataGridViewRow row = dgvCpa.Rows[e.RowIndex];
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
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            string login = Convert.ToString(row.Cells[colLogin.Index].Value)?.Trim() ?? string.Empty;
            string url = NormalizeUrl(Convert.ToString(row.Cells[colUrl.Index].Value));
            string script = Convert.ToString(row.Cells[colScript.Index].Value)?.Trim() ?? string.Empty;
            string description = Convert.ToString(row.Cells[colDescription.Index].Value) ?? string.Empty;
            long newId = _database.InsertCpaRecord(name, login, url, script, description);
            row.Cells[colId.Index].Value = newId;
        }

        private void dgvCpa_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            // Перед сменой строки сохраняем описание и выводим данные новой записи
            if (_isLoading)
            {
                return;
            }

            SaveDescriptionIfNeeded();
            if (e.RowIndex < 0 || e.RowIndex >= dgvCpa.Rows.Count)
            {
                return;
            }

            DataGridViewRow row = dgvCpa.Rows[e.RowIndex];
            UpdateDescriptionBinding(row);
        }

        private void UpdateDescriptionBinding(DataGridViewRow row)
        {
            // Обновляем текст описания и сбрасываем флаг изменения
            _currentRow = row;
            string description = Convert.ToString(row?.Cells[colDescription.Index].Value) ?? string.Empty;
            _suppressDescriptionChange = true;
            descriptionText.Text = description;
            _suppressDescriptionChange = false;
            _descriptionDirty = false;
        }

        private void descriptionText_TextChanged(object sender, EventArgs e)
        {
            // Отмечаем, что описание требует сохранения
            if (_isLoading || _suppressDescriptionChange)
            {
                return;
            }

            _descriptionDirty = true;
        }

        private void SaveDescriptionIfNeeded()
        {
            // Сохраняем описание только если оно было изменено пользователем
            if (!_descriptionDirty || _currentRow == null)
            {
                return;
            }

            long id = GetRowId(_currentRow);
            string newValue = descriptionText.Text ?? string.Empty;
            _currentRow.Cells[colDescription.Index].Value = newValue;

            if (id > 0)
            {
                _database.UpdateCpaField(id, "description", newValue);
            }

            _descriptionDirty = false;
        }

        private void CpaList_FormClosing(object sender, FormClosingEventArgs e)
        {
            // При закрытии окна сохраняем последние изменения описания
            SaveDescriptionIfNeeded();
        }

        private void dgvCpa_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // При двойном клике с нажатым Ctrl открываем ссылку в браузере
            if ((ModifierKeys & Keys.Control) != Keys.Control)
            {
                return;
            }

            if (e.RowIndex < 0 || e.RowIndex >= dgvCpa.Rows.Count)
            {
                return;
            }

            string url = Convert.ToString(dgvCpa.Rows[e.RowIndex].Cells[colUrl.Index].Value);
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть ссылку: {ex.Message}");
            }
        }

        private void dgvCpa_KeyDown(object sender, KeyEventArgs e)
        {
            // При нажатии Ctrl+C копируем логин партнера в буфер обмена
            if (!e.Control || e.KeyCode != Keys.C)
            {
                return;
            }

            if (dgvCpa.CurrentRow == null)
            {
                return;
            }

            string login = Convert.ToString(dgvCpa.CurrentRow.Cells[colLogin.Index].Value);
            if (!string.IsNullOrEmpty(login))
            {
                Clipboard.SetText(login);
            }

            e.Handled = true;
        }
    }
}
