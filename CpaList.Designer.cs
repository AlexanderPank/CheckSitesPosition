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
        private DataGridViewTextBoxColumn colId;
        private DataGridViewTextBoxColumn colName;
        private DataGridViewTextBoxColumn colLogin;
        private DataGridViewTextBoxColumn colUrl;
        private DataGridViewTextBoxColumn colScript;
        private DataGridViewTextBoxColumn colDescription;

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
            this.dgvCpa = new System.Windows.Forms.DataGridView();
            this.colId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLogin = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUrl = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colScript = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panelDescription = new System.Windows.Forms.Panel();
            this.labelDescription = new System.Windows.Forms.Label();
            this.descriptionText = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCpa)).BeginInit();
            this.panelDescription.SuspendLayout();
            this.SuspendLayout();
            // Настраиваем таблицу CPA сетей
            this.dgvCpa.AllowUserToAddRows = true;
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
            this.dgvCpa.MultiSelect = false;
            this.dgvCpa.Name = "dgvCpa";
            this.dgvCpa.RowHeadersVisible = false;
            this.dgvCpa.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvCpa.Size = new System.Drawing.Size(784, 260);
            this.dgvCpa.TabIndex = 0;
            this.dgvCpa.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCpa_CellDoubleClick);
            this.dgvCpa.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCpa_CellEndEdit);
            this.dgvCpa.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dgvCpa_KeyDown);
            this.dgvCpa.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCpa_RowEnter);
            this.dgvCpa.RowValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCpa_RowValidated);
            // Столбец ID
            this.colId.HeaderText = "ID";
            this.colId.Name = "colId";
            this.colId.ReadOnly = true;
            this.colId.Width = 70;
            // Столбец имени
            this.colName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colName.HeaderText = "Название";
            this.colName.Name = "colName";
            // Столбец логина
            this.colLogin.HeaderText = "Логин";
            this.colLogin.Name = "colLogin";
            this.colLogin.Width = 120;
            // Столбец URL
            this.colUrl.HeaderText = "URL";
            this.colUrl.Name = "colUrl";
            this.colUrl.Width = 200;
            // Столбец скрипта
            this.colScript.HeaderText = "Скрипт";
            this.colScript.Name = "colScript";
            this.colScript.Width = 150;
            // Столбец описания (скрыт для редактирования через текстовое поле)
            this.colDescription.HeaderText = "Описание";
            this.colDescription.Name = "colDescription";
            this.colDescription.Visible = false;
            // Панель описания
            this.panelDescription.Controls.Add(this.descriptionText);
            this.panelDescription.Controls.Add(this.labelDescription);
            this.panelDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDescription.Location = new System.Drawing.Point(0, 260);
            this.panelDescription.Name = "panelDescription";
            this.panelDescription.Padding = new System.Windows.Forms.Padding(8);
            this.panelDescription.Size = new System.Drawing.Size(784, 201);
            this.panelDescription.TabIndex = 1;
            // Подпись к описанию
            this.labelDescription.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelDescription.Location = new System.Drawing.Point(8, 8);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(768, 20);
            this.labelDescription.TabIndex = 0;
            this.labelDescription.Text = "Описание:";
            // Текстовое поле для описания
            this.descriptionText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.descriptionText.Location = new System.Drawing.Point(8, 28);
            this.descriptionText.Multiline = true;
            this.descriptionText.Name = "descriptionText";
            this.descriptionText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.descriptionText.Size = new System.Drawing.Size(768, 165);
            this.descriptionText.TabIndex = 1;
            this.descriptionText.TextChanged += new System.EventHandler(this.descriptionText_TextChanged);
            // Настройки формы
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 461);
            this.Controls.Add(this.panelDescription);
            this.Controls.Add(this.dgvCpa);
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
    }
}
