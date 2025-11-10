using System.ComponentModel;
using System.Windows.Forms;

namespace CheckPosition
{
    partial class CpaList
    {
        private IContainer components = null;
        private DataGridView dgvCpa;
        private Panel panelDescription;
        private Label labelDescription;
        private TextBox descriptionText;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CpaList));
            this.dgvCpa = new System.Windows.Forms.DataGridView();
            this.panelDescription = new System.Windows.Forms.Panel();
            this.descriptionText = new System.Windows.Forms.TextBox();
            this.labelDescription = new System.Windows.Forms.Label();
            this.colId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLogin = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUrl = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colScript = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCpa)).BeginInit();
            this.panelDescription.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvCpa
            // 
            this.dgvCpa.AllowUserToDeleteRows = false;
            this.dgvCpa.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCpa.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colId,
            this.colName,
            this.colLogin,
            this.colUrl,
            this.colScript,
            this.colDescription});
            this.dgvCpa.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgvCpa.Location = new System.Drawing.Point(0, 0);
            this.dgvCpa.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.dgvCpa.MultiSelect = false;
            this.dgvCpa.Name = "dgvCpa";
            this.dgvCpa.RowHeadersVisible = false;
            this.dgvCpa.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvCpa.Size = new System.Drawing.Size(1045, 280);
            this.dgvCpa.TabIndex = 0;
            this.dgvCpa.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCpa_CellDoubleClick);
            this.dgvCpa.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCpa_CellEndEdit);
            this.dgvCpa.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCpa_RowEnter);
            this.dgvCpa.RowValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCpa_RowValidated);
            this.dgvCpa.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dgvCpa_KeyDown);
            // 
            // panelDescription
            // 
            this.panelDescription.Controls.Add(this.descriptionText);
            this.panelDescription.Controls.Add(this.labelDescription);
            this.panelDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDescription.Location = new System.Drawing.Point(0, 280);
            this.panelDescription.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panelDescription.Name = "panelDescription";
            this.panelDescription.Padding = new System.Windows.Forms.Padding(11, 9, 11, 9);
            this.panelDescription.Size = new System.Drawing.Size(1045, 216);
            this.panelDescription.TabIndex = 1;
            // 
            // descriptionText
            // 
            this.descriptionText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.descriptionText.Location = new System.Drawing.Point(11, 31);
            this.descriptionText.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.descriptionText.Multiline = true;
            this.descriptionText.Name = "descriptionText";
            this.descriptionText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.descriptionText.Size = new System.Drawing.Size(1023, 176);
            this.descriptionText.TabIndex = 1;
            this.descriptionText.TextChanged += new System.EventHandler(this.descriptionText_TextChanged);
            // 
            // labelDescription
            // 
            this.labelDescription.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelDescription.Location = new System.Drawing.Point(11, 9);
            this.labelDescription.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(1023, 22);
            this.labelDescription.TabIndex = 0;
            this.labelDescription.Text = "Описание:";
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
            this.colName.FillWeight = 200F;
            this.colName.HeaderText = "Название";
            this.colName.Name = "colName";
            this.colName.Width = 200;
            // 
            // colLogin
            // 
            this.colLogin.HeaderText = "Логин";
            this.colLogin.Name = "colLogin";
            this.colLogin.Width = 150;
            // 
            // colUrl
            // 
            this.colUrl.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colUrl.HeaderText = "URL";
            this.colUrl.Name = "colUrl";
            // 
            // colScript
            // 
            this.colScript.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colScript.HeaderText = "Скрипт";
            this.colScript.Name = "colScript";
            // 
            // colDescription
            // 
            this.colDescription.HeaderText = "Описание";
            this.colDescription.Name = "colDescription";
            this.colDescription.Visible = false;
            // 
            // CpaList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1045, 496);
            this.Controls.Add(this.panelDescription);
            this.Controls.Add(this.dgvCpa);
            this.Font = new System.Drawing.Font("Verdana", 9F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "CpaList";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Справочник CPA";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CpaList_FormClosing);
            this.Shown += new System.EventHandler(this.CpaList_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCpa)).EndInit();
            this.panelDescription.ResumeLayout(false);
            this.panelDescription.PerformLayout();
            this.ResumeLayout(false);

        }

        private DataGridViewTextBoxColumn colId;
        private DataGridViewTextBoxColumn colName;
        private DataGridViewTextBoxColumn colLogin;
        private DataGridViewTextBoxColumn colUrl;
        private DataGridViewTextBoxColumn colScript;
        private DataGridViewTextBoxColumn colDescription;
    }
}
