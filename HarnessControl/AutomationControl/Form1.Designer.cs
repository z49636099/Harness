namespace AutomationControl
{
    partial class Form1
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btnChange = new System.Windows.Forms.Button();
            this.btnControl = new System.Windows.Forms.Button();
            this.btnFrontend = new System.Windows.Forms.Button();
            this.btnBackend = new System.Windows.Forms.Button();
            this.btnConfig = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnBackend02 = new System.Windows.Forms.Button();
            this.btnBackend03 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnChange
            // 
            this.btnChange.Location = new System.Drawing.Point(31, 263);
            this.btnChange.Name = "btnChange";
            this.btnChange.Size = new System.Drawing.Size(75, 25);
            this.btnChange.TabIndex = 5;
            this.btnChange.Text = "PollChange";
            this.btnChange.UseVisualStyleBackColor = true;
            this.btnChange.Click += new System.EventHandler(this.btnChange_Click);
            // 
            // btnControl
            // 
            this.btnControl.Location = new System.Drawing.Point(31, 231);
            this.btnControl.Name = "btnControl";
            this.btnControl.Size = new System.Drawing.Size(75, 25);
            this.btnControl.TabIndex = 6;
            this.btnControl.Text = "PollControl";
            this.btnControl.UseVisualStyleBackColor = true;
            this.btnControl.Click += new System.EventHandler(this.btnControl_Click);
            // 
            // btnFrontend
            // 
            this.btnFrontend.Location = new System.Drawing.Point(31, 200);
            this.btnFrontend.Name = "btnFrontend";
            this.btnFrontend.Size = new System.Drawing.Size(75, 25);
            this.btnFrontend.TabIndex = 7;
            this.btnFrontend.Text = "Frontend";
            this.btnFrontend.UseVisualStyleBackColor = true;
            this.btnFrontend.Click += new System.EventHandler(this.btnFrontend_Click);
            // 
            // btnBackend
            // 
            this.btnBackend.Location = new System.Drawing.Point(31, 107);
            this.btnBackend.Name = "btnBackend";
            this.btnBackend.Size = new System.Drawing.Size(75, 25);
            this.btnBackend.TabIndex = 4;
            this.btnBackend.Text = "Backend01";
            this.btnBackend.UseVisualStyleBackColor = true;
            this.btnBackend.Click += new System.EventHandler(this.btnBackend_Click);
            // 
            // btnConfig
            // 
            this.btnConfig.Location = new System.Drawing.Point(31, 76);
            this.btnConfig.Name = "btnConfig";
            this.btnConfig.Size = new System.Drawing.Size(75, 25);
            this.btnConfig.TabIndex = 3;
            this.btnConfig.Text = "Config";
            this.btnConfig.UseVisualStyleBackColor = true;
            this.btnConfig.Click += new System.EventHandler(this.btnConfig_Click);
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Right;
            this.textBox1.Location = new System.Drawing.Point(188, 0);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(655, 598);
            this.textBox1.TabIndex = 8;
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(31, 47);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 9;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(31, 294);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 25);
            this.button1.TabIndex = 5;
            this.button1.Text = "RELIABILITY";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnBackend02
            // 
            this.btnBackend02.Location = new System.Drawing.Point(31, 138);
            this.btnBackend02.Name = "btnBackend02";
            this.btnBackend02.Size = new System.Drawing.Size(75, 25);
            this.btnBackend02.TabIndex = 10;
            this.btnBackend02.Text = "Backend02";
            this.btnBackend02.UseVisualStyleBackColor = true;
            this.btnBackend02.Click += new System.EventHandler(this.btnBackend02_Click);
            // 
            // btnBackend03
            // 
            this.btnBackend03.Location = new System.Drawing.Point(31, 169);
            this.btnBackend03.Name = "btnBackend03";
            this.btnBackend03.Size = new System.Drawing.Size(75, 25);
            this.btnBackend03.TabIndex = 11;
            this.btnBackend03.Text = "Backend03";
            this.btnBackend03.UseVisualStyleBackColor = true;
            this.btnBackend03.Click += new System.EventHandler(this.btnBackend03_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(843, 598);
            this.Controls.Add(this.btnBackend03);
            this.Controls.Add(this.btnBackend02);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnChange);
            this.Controls.Add(this.btnControl);
            this.Controls.Add(this.btnFrontend);
            this.Controls.Add(this.btnBackend);
            this.Controls.Add(this.btnConfig);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnChange;
        private System.Windows.Forms.Button btnControl;
        private System.Windows.Forms.Button btnFrontend;
        private System.Windows.Forms.Button btnBackend;
        private System.Windows.Forms.Button btnConfig;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnBackend02;
        private System.Windows.Forms.Button btnBackend03;
    }
}

