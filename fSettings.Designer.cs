namespace CheckPosition
{
    partial class fSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(fSettings));
            this.tbYandexUrl = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.bOk = new System.Windows.Forms.Button();
            this.bCancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.tbYandexUrl2 = new System.Windows.Forms.TextBox();
            this.rbUse1 = new System.Windows.Forms.RadioButton();
            this.rbUse2 = new System.Windows.Forms.RadioButton();
            this.cbTimer = new System.Windows.Forms.CheckBox();
            this.dtTimer = new System.Windows.Forms.DateTimePicker();
            this.SuspendLayout();
            // 
            // tbYandexUrl
            // 
            this.tbYandexUrl.Location = new System.Drawing.Point(38, 38);
            this.tbYandexUrl.Name = "tbYandexUrl";
            this.tbYandexUrl.Size = new System.Drawing.Size(511, 20);
            this.tbYandexUrl.TabIndex = 0;
            this.tbYandexUrl.Text = "https://yandex.ru/search/xml?user=pr0grammer&key=03.10886406:65f689bae5cc0f74fd08" +
    "e2b0749fe409";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(35, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(234, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Введите URL для запросов в формате XML :";
            // 
            // bOk
            // 
            this.bOk.Location = new System.Drawing.Point(106, 169);
            this.bOk.Name = "bOk";
            this.bOk.Size = new System.Drawing.Size(139, 28);
            this.bOk.TabIndex = 2;
            this.bOk.Text = "OK";
            this.bOk.UseVisualStyleBackColor = true;
            this.bOk.Click += new System.EventHandler(this.bOk_Click);
            // 
            // bCancel
            // 
            this.bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bCancel.Location = new System.Drawing.Point(275, 169);
            this.bCancel.Name = "bCancel";
            this.bCancel.Size = new System.Drawing.Size(139, 28);
            this.bCancel.TabIndex = 3;
            this.bCancel.Text = "Cancel";
            this.bCancel.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(35, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(284, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Введите покупной URL для запросов в формате XML :";
            // 
            // tbYandexUrl2
            // 
            this.tbYandexUrl2.Location = new System.Drawing.Point(38, 82);
            this.tbYandexUrl2.Name = "tbYandexUrl2";
            this.tbYandexUrl2.Size = new System.Drawing.Size(511, 20);
            this.tbYandexUrl2.TabIndex = 4;
            this.tbYandexUrl2.Text = "https://xmlproxy.ru/search/xml?user=alex-linux%40yandex.ru&key=d2f7ff88296a06a238" +
    "0a3f16fb6705f6";
            // 
            // rbUse1
            // 
            this.rbUse1.AutoSize = true;
            this.rbUse1.Checked = true;
            this.rbUse1.Location = new System.Drawing.Point(12, 39);
            this.rbUse1.Name = "rbUse1";
            this.rbUse1.Size = new System.Drawing.Size(14, 13);
            this.rbUse1.TabIndex = 6;
            this.rbUse1.TabStop = true;
            this.rbUse1.UseVisualStyleBackColor = true;
            // 
            // rbUse2
            // 
            this.rbUse2.AutoSize = true;
            this.rbUse2.Location = new System.Drawing.Point(12, 82);
            this.rbUse2.Name = "rbUse2";
            this.rbUse2.Size = new System.Drawing.Size(14, 13);
            this.rbUse2.TabIndex = 7;
            this.rbUse2.UseVisualStyleBackColor = true;
            // 
            // cbTimer
            // 
            this.cbTimer.AutoSize = true;
            this.cbTimer.Location = new System.Drawing.Point(12, 119);
            this.cbTimer.Name = "cbTimer";
            this.cbTimer.Size = new System.Drawing.Size(223, 17);
            this.cbTimer.TabIndex = 8;
            this.cbTimer.Text = "Проверять автоматически по времени";
            this.cbTimer.UseVisualStyleBackColor = true;
            this.cbTimer.CheckedChanged += new System.EventHandler(this.cbTimer_CheckedChanged);
            // 
            // dtTimer
            // 
            this.dtTimer.Enabled = false;
            this.dtTimer.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtTimer.Location = new System.Drawing.Point(241, 119);
            this.dtTimer.Name = "dtTimer";
            this.dtTimer.Size = new System.Drawing.Size(108, 20);
            this.dtTimer.TabIndex = 9;
            this.dtTimer.Value = new System.DateTime(2024, 8, 31, 23, 30, 0, 0);
            // 
            // fSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.bCancel;
            this.ClientSize = new System.Drawing.Size(561, 220);
            this.Controls.Add(this.dtTimer);
            this.Controls.Add(this.cbTimer);
            this.Controls.Add(this.rbUse2);
            this.Controls.Add(this.rbUse1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbYandexUrl2);
            this.Controls.Add(this.bCancel);
            this.Controls.Add(this.bOk);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbYandexUrl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "fSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Настройки программы";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbYandexUrl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button bOk;
        private System.Windows.Forms.Button bCancel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbYandexUrl2;
        private System.Windows.Forms.RadioButton rbUse1;
        private System.Windows.Forms.RadioButton rbUse2;
        private System.Windows.Forms.CheckBox cbTimer;
        private System.Windows.Forms.DateTimePicker dtTimer;
    }
}