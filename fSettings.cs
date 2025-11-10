using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CheckPosition
{
    public partial class fSettings : Form
    {
        public fSettings()
        {
            InitializeComponent();
            String xml = Properties.Settings.Default.XMLURL;
            if (xml != null  && xml != tbYandexUrl2.Text) { 
                tbYandexUrl.Text = xml;
            } else if (xml != null)
                rbUse2.Checked = true;


            // Переносим время с учётом пользовательских настроек и старого формата хранения
            var dt = Properties.Settings.Default.TimeToCheck;
            if (!string.IsNullOrWhiteSpace(dt))
            {
                if (TimeSpan.TryParse(dt, out var parsedTime))
                {
                    dtTimer.Value = DateTime.Today.Add(parsedTime);
                    cbTimer.Checked = true;
                }
                else if (DateTime.TryParse(dt, out var parsedDateTime))
                {
                    // Поддерживаем прежний формат, когда сохранялась полная дата
                    dtTimer.Value = DateTime.Today.Add(parsedDateTime.TimeOfDay);
                    cbTimer.Checked = true;
                }
            }
        }

        private void bOk_Click(object sender, EventArgs e)
        {
            if (rbUse1.Checked)
                Properties.Settings.Default.XMLURL = tbYandexUrl.Text;
            else
                Properties.Settings.Default.XMLURL = tbYandexUrl2.Text;
            if (cbTimer.Checked)
            {
                // Сохраняем только время, чтобы исключить проблемы с парсингом строки
                Properties.Settings.Default.TimeToCheck = dtTimer.Value.TimeOfDay.ToString();
            }
            else
            {
                // При отключении таймера очищаем настройку
                Properties.Settings.Default.TimeToCheck = string.Empty;
            }
            Close();
        }

        private void cbTimer_CheckedChanged(object sender, EventArgs e)
        {
            dtTimer.Enabled = cbTimer.Checked;
        }
    }
}
