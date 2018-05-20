namespace My_Menu
{
    partial class BrushFace
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
            this.realTime = new System.Windows.Forms.PictureBox();
            this.pictureBox_dicFace = new System.Windows.Forms.PictureBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_payment = new System.Windows.Forms.TextBox();
            this.but_close = new System.Windows.Forms.Button();
            this.label_DetectResultHint = new System.Windows.Forms.Label();
            this.label_userinfo = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_balance = new System.Windows.Forms.TextBox();
            this.textBox_balanceRemain = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.realTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_dicFace)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // realTime
            // 
            this.realTime.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.realTime.Location = new System.Drawing.Point(6, 49);
            this.realTime.Name = "realTime";
            this.realTime.Size = new System.Drawing.Size(532, 433);
            this.realTime.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.realTime.TabIndex = 0;
            this.realTime.TabStop = false;
            // 
            // pictureBox_dicFace
            // 
            this.pictureBox_dicFace.Location = new System.Drawing.Point(18, 68);
            this.pictureBox_dicFace.Name = "pictureBox_dicFace";
            this.pictureBox_dicFace.Size = new System.Drawing.Size(241, 206);
            this.pictureBox_dicFace.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox_dicFace.TabIndex = 2;
            this.pictureBox_dicFace.TabStop = false;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(139, 3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(178, 42);
            this.label4.TabIndex = 6;
            this.label4.Text = "账户余额：";
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(139, 78);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(178, 42);
            this.label5.TabIndex = 8;
            this.label5.Text = "扣款金额：";
            // 
            // textBox_payment
            // 
            this.textBox_payment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_payment.Font = new System.Drawing.Font("宋体", 24F, System.Drawing.FontStyle.Bold);
            this.textBox_payment.Location = new System.Drawing.Point(323, 81);
            this.textBox_payment.MaxLength = 6;
            this.textBox_payment.Name = "textBox_payment";
            this.textBox_payment.ReadOnly = true;
            this.textBox_payment.Size = new System.Drawing.Size(118, 44);
            this.textBox_payment.TabIndex = 9;
            this.textBox_payment.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            // 
            // but_close
            // 
            this.but_close.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.but_close.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.but_close.Location = new System.Drawing.Point(223, 549);
            this.but_close.MaximumSize = new System.Drawing.Size(120, 40);
            this.but_close.MinimumSize = new System.Drawing.Size(80, 40);
            this.but_close.Name = "but_close";
            this.but_close.Size = new System.Drawing.Size(120, 40);
            this.but_close.TabIndex = 10;
            this.but_close.Text = "返回";
            this.but_close.UseVisualStyleBackColor = true;
            this.but_close.Click += new System.EventHandler(this.but_close_Click);
            // 
            // label_DetectResultHint
            // 
            this.label_DetectResultHint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_DetectResultHint.AutoSize = true;
            this.label_DetectResultHint.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_DetectResultHint.Location = new System.Drawing.Point(122, 542);
            this.label_DetectResultHint.Name = "label_DetectResultHint";
            this.label_DetectResultHint.Size = new System.Drawing.Size(146, 42);
            this.label_DetectResultHint.TabIndex = 11;
            this.label_DetectResultHint.Text = "提示信息";
            // 
            // label_userinfo
            // 
            this.label_userinfo.AutoSize = true;
            this.label_userinfo.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_userinfo.Location = new System.Drawing.Point(275, 68);
            this.label_userinfo.Name = "label_userinfo";
            this.label_userinfo.Size = new System.Drawing.Size(114, 126);
            this.label_userinfo.TabIndex = 12;
            this.label_userinfo.Text = "账号：\r\n姓名：\r\n性别：";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.groupBox1.Controls.Add(this.pictureBox_dicFace);
            this.groupBox1.Controls.Add(this.label_userinfo);
            this.groupBox1.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(561, 323);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "客户信息";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.AutoSize = true;
            this.groupBox2.Controls.Add(this.realTime);
            this.groupBox2.Font = new System.Drawing.Font("微软雅黑", 24F);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(545, 500);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "实时画面";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(139, 149);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(178, 42);
            this.label1.TabIndex = 15;
            this.label1.Text = "剩余金额：";
            // 
            // textBox_balance
            // 
            this.textBox_balance.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_balance.Font = new System.Drawing.Font("宋体", 24F, System.Drawing.FontStyle.Bold);
            this.textBox_balance.Location = new System.Drawing.Point(323, 6);
            this.textBox_balance.MaxLength = 6;
            this.textBox_balance.Name = "textBox_balance";
            this.textBox_balance.ReadOnly = true;
            this.textBox_balance.Size = new System.Drawing.Size(118, 44);
            this.textBox_balance.TabIndex = 16;
            // 
            // textBox_balanceRemain
            // 
            this.textBox_balanceRemain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_balanceRemain.Font = new System.Drawing.Font("宋体", 24F, System.Drawing.FontStyle.Bold);
            this.textBox_balanceRemain.Location = new System.Drawing.Point(323, 152);
            this.textBox_balanceRemain.MaxLength = 6;
            this.textBox_balanceRemain.Name = "textBox_balanceRemain";
            this.textBox_balanceRemain.ReadOnly = true;
            this.textBox_balanceRemain.Size = new System.Drawing.Size(118, 44);
            this.textBox_balanceRemain.TabIndex = 17;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(447, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 42);
            this.label2.TabIndex = 18;
            this.label2.Text = "元";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(447, 78);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 42);
            this.label3.TabIndex = 18;
            this.label3.Text = "元";
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(447, 149);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(50, 42);
            this.label6.TabIndex = 18;
            this.label6.Text = "元";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.textBox_balance);
            this.panel1.Controls.Add(this.textBox_balanceRemain);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.textBox_payment);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Location = new System.Drawing.Point(3, 332);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(561, 211);
            this.panel1.TabIndex = 19;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.Controls.Add(this.groupBox1);
            this.flowLayoutPanel1.Controls.Add(this.panel1);
            this.flowLayoutPanel1.Controls.Add(this.but_close);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(583, 24);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(564, 651);
            this.flowLayoutPanel1.TabIndex = 20;
            // 
            // BrushFace
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1159, 687);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.label_DetectResultHint);
            this.Name = "BrushFace";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Text = "食堂刷脸机";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.brushFace_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.realTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_dicFace)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox realTime;
        private System.Windows.Forms.PictureBox pictureBox_dicFace;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_payment;
        private System.Windows.Forms.Button but_close;
        private System.Windows.Forms.Label label_DetectResultHint;
        private System.Windows.Forms.Label label_userinfo;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_balance;
        private System.Windows.Forms.TextBox textBox_balanceRemain;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}