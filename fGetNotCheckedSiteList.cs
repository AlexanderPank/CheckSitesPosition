using System;
using System.Windows.Forms;

namespace CheckPosition
{
    public partial class fGetNotCheckedSiteList : Form
    {
        private DataBaseSqlite database;
        public fGetNotCheckedSiteList(DataBaseSqlite _database)
        {
            InitializeComponent();
            database = _database;
        }

        private void bClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void bLoad_Click(object sender, EventArgs e)
        {
            this.tbSiteList.Lines = this.database.getNotCheckedSiteList().ToArray();
        }
    }
}
