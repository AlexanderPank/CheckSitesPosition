using System.ComponentModel;
using System.Windows.Forms;

namespace CheckPosition
{
    partial class HostingList
    {
        private IContainer components = null;
        private DataGridView hostingGrid;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HostingList));
            this.hostingGrid = new System.Windows.Forms.DataGridView();
            this.colId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIp = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.hostingGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // hostingGrid
            // 
            this.hostingGrid.AllowUserToDeleteRows = false;
            this.hostingGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.hostingGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colId,
            this.colName,
            this.colIp});
            this.hostingGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hostingGrid.Location = new System.Drawing.Point(0, 0);
            this.hostingGrid.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.hostingGrid.MultiSelect = false;
            this.hostingGrid.Name = "hostingGrid";
            this.hostingGrid.RowHeadersVisible = false;
            this.hostingGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.hostingGrid.Size = new System.Drawing.Size(779, 389);
            this.hostingGrid.TabIndex = 0;
            this.hostingGrid.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.hostingGrid_CellEndEdit);
            this.hostingGrid.RowValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.hostingGrid_RowValidated);
            // 
            // colId
            // 
            this.colId.HeaderText = "ID";
            this.colId.Name = "colId";
            this.colId.ReadOnly = true;
            this.colId.Width = 50;
            // 
            // colName
            // 
            this.colName.FillWeight = 180F;
            this.colName.HeaderText = "Название";
            this.colName.Name = "colName";
            this.colName.Width = 180;
            // 
            // colIp
            // 
            this.colIp.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colIp.HeaderText = "IP";
            this.colIp.Name = "colIp";
            // 
            // HostingList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(779, 389);
            this.Controls.Add(this.hostingGrid);
            this.Font = new System.Drawing.Font("Verdana", 9F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "HostingList";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Справочник хостингов";
            this.Shown += new System.EventHandler(this.HostingList_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.hostingGrid)).EndInit();
            this.ResumeLayout(false);

        }

        private DataGridViewTextBoxColumn colId;
        private DataGridViewTextBoxColumn colName;
        private DataGridViewTextBoxColumn colIp;
    }
}
