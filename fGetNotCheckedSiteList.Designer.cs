namespace CheckPosition
{
    partial class fGetNotCheckedSiteList
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tbSiteList = new System.Windows.Forms.TextBox();
            this.bLoad = new System.Windows.Forms.Button();
            this.bClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tbSiteList
            // 
            this.tbSiteList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSiteList.Location = new System.Drawing.Point(5, 3);
            this.tbSiteList.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.tbSiteList.Multiline = true;
            this.tbSiteList.Name = "tbSiteList";
            this.tbSiteList.Size = new System.Drawing.Size(694, 377);
            this.tbSiteList.TabIndex = 0;
            // 
            // bLoad
            // 
            this.bLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bLoad.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.bLoad.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.bLoad.Location = new System.Drawing.Point(5, 390);
            this.bLoad.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.bLoad.Name = "bLoad";
            this.bLoad.Size = new System.Drawing.Size(190, 31);
            this.bLoad.TabIndex = 1;
            this.bLoad.Text = "Загрузить";
            this.bLoad.UseVisualStyleBackColor = false;
            this.bLoad.Click += new System.EventHandler(this.bLoad_Click);
            // 
            // bClose
            // 
            this.bClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bClose.Location = new System.Drawing.Point(509, 390);
            this.bClose.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.bClose.Name = "bClose";
            this.bClose.Size = new System.Drawing.Size(190, 31);
            this.bClose.TabIndex = 2;
            this.bClose.Text = "Закрыть";
            this.bClose.UseVisualStyleBackColor = true;
            this.bClose.Click += new System.EventHandler(this.bClose_Click);
            // 
            // fGetNotCheckedSiteList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(702, 428);
            this.Controls.Add(this.bClose);
            this.Controls.Add(this.bLoad);
            this.Controls.Add(this.tbSiteList);
            this.Font = new System.Drawing.Font("Verdana", 9F);
            this.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.Name = "fGetNotCheckedSiteList";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Список сайтов которые не были добавлены для проверки позиций";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbSiteList;
        private System.Windows.Forms.Button bLoad;
        private System.Windows.Forms.Button bClose;
    }
}