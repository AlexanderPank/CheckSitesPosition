using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CheckPosition
{
    public partial class fSearchReplace : Form
    {   
        private Func<string, int> func = null;
        public fSearchReplace()
        {
            InitializeComponent();
        }

        private void bSearch_Click(object sender, EventArgs e)
        {
            if (func != null) { this.func(tbSearch.Text); }
        }

        public void showWindow(Func<string, int> func) {
            this.func = func;
            this.Show();
            
            
        }

        private void tbSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar ==  13) { bSearch_Click(sender, null); e.Handled = true; }
        }

        private void fSearchReplace_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
