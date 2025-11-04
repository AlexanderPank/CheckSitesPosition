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


            var dt = Properties.Settings.Default.TimeToCheck;
            if (dt != "")
            {
                dtTimer.Value = DateTime.Parse(Properties.Settings.Default.TimeToCheck);
                cbTimer.Checked = true;
            }
        }

        private void bOk_Click(object sender, EventArgs e)
        {
            if (rbUse1.Checked)
                Properties.Settings.Default.XMLURL = tbYandexUrl.Text;
            else
                Properties.Settings.Default.XMLURL = tbYandexUrl2.Text;
            if (cbTimer.Checked)
                Properties.Settings.Default.TimeToCheck = dtTimer.Text;
            else
                Properties.Settings.Default.TimeToCheck = "";
            Close();
        }

        private void cbTimer_CheckedChanged(object sender, EventArgs e)
        {
            dtTimer.Enabled = cbTimer.Checked;
        }
    }
}
