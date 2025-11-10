using EO.WebBrowser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using System.Xml.Linq;

namespace CheckPosition
{
    public partial class DomainList : Form
    {
        //const string SeverRegRuApiUrl = "https://api.reg.ru/api/regru2/service/get_list?input_data=%7B%22servtype%22%3A%22domain%22%7D&input_format=json&output_content_type=plain&password=apiUSer1980&username=platonn";
        const string SeverRegRuApiUrl = "https://alexnet.ru/api/index.php?query=regru";

        public DataBaseSqlite database;
        private bool isStop = false;
        private Dictionary<int, string> columnName = new Dictionary<int, string>();
        private string ipCountInfo = string.Empty;
        public DomainList(DataBaseSqlite db)
        {
            InitializeComponent();
            this.database = db;
            columnName.Add(colID.Index, "id");
            columnName.Add(colName.Index, "name");
            columnName.Add(colRusName.Index, "rus_name");
            columnName.Add(colExpireDate.Index, "expire_date");
            columnName.Add(colIP.Index, "ip");
            columnName.Add(colHasSite.Index, "has_site");
            columnName.Add(colVisits.Index, "visits");
            columnName.Add(colSails.Index, "sales");
            columnName.Add(colComment.Index, "comments");
            columnName.Add(colShow.Index, "show");
        }

        private void bRegRuLoad_Click(object sender, EventArgs e)
        {
            bRegRuLoad.Enabled = false;
            string json_string = BrowserEO.loadJsonUrl(SeverRegRuApiUrl);
            if (json_string == "")
            {
                MessageBox.Show("Данные не загружены");
                bRegRuLoad.Enabled = true;
                return;
            }
            try
            {
                var json = new JavaScriptSerializer();
                RegRuAnswer data = json.Deserialize<RegRuAnswer>(json_string);

                foreach (RegRuDomainItemStruct domain in data.answer.services)
                {
 
                    if (domain.state == "S")
                    {
                        int id_s = this.database.getIdDomainByName(domain.dname);
                        if (id_s != -1) 
                            this.database.updateDomain(id_s, domain.expiration_date);
                        continue;
                    }
                    if (domain.state != "A") continue;
                    int id = this.database.getIdDomainByName(domain.dname);
                    if (id == -1)
                        this.database.appendDomain(domain.service_id, domain.dname, domain.expiration_date);
                    else
                        this.database.updateDomain(id, domain.expiration_date);

                }
            } catch (Exception ex) {
                try
                {
                    RegRuError data = (new JavaScriptSerializer()).Deserialize<RegRuError>(json_string);
                    MessageBox.Show("Ошибка " + data.error_code);
                    return;
                }
                catch (Exception )
                {

                }
                MessageBox.Show("Ошибка " + ex.Message);
            }
            bRegRuLoad.Enabled = true;
            this.database.getTableData(dg, "domains");
            this.UpdateInfoText();
        }

        private void DomainList_Shown(object sender, EventArgs e)
        {
            this.database.getTableData(dg, "domains");
            this.hideHiddenRows();
            this.UpdateInfoText();
            int rowCount = dg.Rows.Count;
            if (rowCount > 0) dg.FirstDisplayedScrollingRowIndex = rowCount - 1;
        }
        private void UpdateInfoText()
        {
            int cntSites = 0;
            int cntNotHide = 0;
            Dictionary<string, int> ips = new Dictionary<string, int>();
            for (int i = 0; i < dg.RowCount - 1; i++)
            {
                int isActive = Helper.getIngValue(dg.Rows[i].Cells[colShow.Index].Value);
                cntNotHide += isActive;
                cntSites = Helper.getStringValue(dg.Rows[i].Cells[colHasSite.Index].Value) == "" || isActive != 1 ? cntSites : cntSites + 1;
                string ip = Helper.getStringValue(dg.Rows[i].Cells[colIP.Index].Value);
                if (ip!="")
                    ips[ip] = ips.Keys.Contains(ip) ? ips[ip] + 1 : 1;
                
            }
            ipCountInfo = "";
            foreach (string ip in ips.Keys)
            {
                ipCountInfo += $"{ip} = {ips[ip].ToString()} шт.\n";
            }
            this.labelInfo.Text = $"Доменов всего - {dg.RowCount - 1}, активных - {cntNotHide} из них сайтов: {cntSites} ";
        }

     

        private string get_ip(string name)
        {
            string ip = "";
            try
            {
                if (name.Trim() == "") return "";

                IPAddress[] addresses = Dns.GetHostAddresses(name);
                foreach (IPAddress address in addresses)
                    ip += ip == "" ? address.ToString().Trim() : ", " + address.ToString().Trim();
            } catch{}
            return ip;
        }

        private void doActionUnderRows(bool useOneRow, Func<int, long, string, string> callback)
        {
            int startIndex = dg.SelectedRows.Count > 0 ? dg.SelectedRows[0].Index : 0;
            int endIndex = useOneRow ?  startIndex + 1 : dg.RowCount - 1;

            dg.Enabled = false;
            bCheckSite.Enabled = false;
            this.isStop = false;
            for (int i = startIndex; i < endIndex; i++)
            {
                Application.DoEvents();
                if (this.isStop) break;
                string dname = Helper.getStringValue(dg.Rows[i].Cells[colName.Index].Value);
                dname = BrowserEO.GetDomainName(dname);
                string dname_rus = Helper.getStringValue(dg.Rows[i].Cells[colRusName.Index].Value);
                long id = Helper.getIngValue(dg.Rows[i].Cells[colID.Index].Value);

                callback(i, id, dname);

                if (i - 1 >= 0) dg.Rows[i - 1].Selected = false;
                dg.Rows[i].Selected = true;
            }

            dg.Enabled = true;
            bCheckSite.Enabled = true;
        }

        private void bCheckSite_Click(object sender, EventArgs e)
        {
            doActionUnderRows(sender != bCheckSite, (int index, long id, string dname) =>
            {
                string title = BrowserEO.getTitlePage("https://" + dname);
                this.database.updateRecord("domains", id, "has_site", title);
                string ip = this.get_ip(dname);
                this.database.updateRecord("domains", id, "ip", ip);

                dg.Rows[index].Cells[colHasSite.Index].Value = title;
                dg.Rows[index].Cells[colIP.Index].Value = ip;

                return title;
            });
        }

        private void dg_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            
            long id = Helper.getIngValue(dg.Rows[e.RowIndex].Cells[colID.Index].Value);
            if (id == -1) return;

            if (e.ColumnIndex == colSails.Index || e.ColumnIndex == colVisits.Index)
                this.database.updateRecord( "domains", id, columnName[e.ColumnIndex], Helper.getIngValue(dg.Rows[e.RowIndex].Cells[e.ColumnIndex].Value));
            else
                this.database.updateRecord("domains", id, columnName[e.ColumnIndex], Helper.getStringValue(dg.Rows[e.RowIndex].Cells[e.ColumnIndex].Value));
        }

        private void bShowIpInfo_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this.ipCountInfo);
        }

        private void searchCounterYandexMetrica_Click(object sender, EventArgs e)
        {
            doActionUnderRows(sender != searchCounterYandexMetrica,
                (int index, long id, string dname) => {

                    int counter_id = BrowserEO.getCounterID("https://" + dname);
                    this.database.updateRecord("domains", id, "counter_id", counter_id);
                    dg.Rows[index].Cells[colMetrica.Index].Value = counter_id;
                    return counter_id.ToString();
                });

        }

        private void hideHiddenRows(bool isHide = true)
        {
            for (int i = 0; i < dg.RowCount - 1; i++)
            {
                int isRowVisible = Helper.getIngValue(dg.Rows[i].Cells[colShow.Index].Value);
                dg.Rows[i].Visible = true;
                if (isRowVisible == 0)
                {
                    if (isHide) dg.Rows[i].Visible = false;
                    dg.Rows[i].DefaultCellStyle.BackColor = Color.Gray;
                } else
                    dg.Rows[i].DefaultCellStyle.BackColor = Color.White;
            }
            
        }
        private void bHide_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dg.SelectedRows.Count; i++)
            {
                var rowIndex = dg.SelectedRows[i].Index;
                long id = Helper.getIngValue(dg.Rows[rowIndex].Cells[colID.Index].Value);
                if (id == -1) return;
                int intVal = Helper.getIngValue(dg.Rows[rowIndex].Cells[colShow.Index].Value);
                intVal = intVal == 1 ? 0 : 1;
                this.database.updateRecord("domains", id, columnName[colShow.Index], intVal);
                dg.Rows[rowIndex].Cells[colShow.Index].Value = intVal;
            }
            hideHiddenRows();
        }

        private void bShowHidden_Click(object sender, EventArgs e)
        {
            hideHiddenRows(false);
        }
    }

    class RegRuError
    {
        public string charset;
        public string error_code;
        public object error_params;
    }
    class RegRuAnswer
    {
        public RegRuAnswerService answer;
        public string charset;
        public string messagestore;
        public string result;

    }
    class RegRuAnswerService
    {
        public RegRuDomainItemStruct[] services;
    }
    class RegRuDomainItemStruct
    {
        public string creation_date;
        public string dname;
        public string expiration_date;
        public long service_id;
        public string servtype;
        public string state;
        public string subtype;
        public int uplink_service_id;
    }
 
}
