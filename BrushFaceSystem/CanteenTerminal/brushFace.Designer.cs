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
            this.components = new System.ComponentModel.Container();
            this.realTime = new System.Windows.Forms.PictureBox();
            this.pictureBox_shotface = new System.Windows.Forms.PictureBox();
            this.pictureBox_dicFace = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.but_close = new System.Windows.Forms.Button();
            this.label_NodShakeDetectResult = new System.Windows.Forms.Label();
            this.stuinfo = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.realTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_shotface)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_dicFace)).BeginInit();
            this.SuspendLayout();
            // 
            // realTime
            // 
            this.realTime.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.realTime.Location = new System.Drawing.Point(12, 78);
            this.realTime.Name = "realTime";
            this.realTime.Size = new System.Drawing.Size(743, 337);
            this.realTime.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.realTime.TabIndex = 0;
            this.realTime.TabStop = false;
            // 
            // pictureBox_shotface
            // 
            this.pictureBox_shotface.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox_shotface.Location = new System.Drawing.Point(800, 78);
            this.pictureBox_shotface.Name = "pictureBox_shotface";
            this.pictureBox_shotface.Size = new System.Drawing.Size(226, 206);
            this.pictureBox_shotface.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox_shotface.TabIndex = 1;
            this.pictureBox_shotface.TabStop = false;
            // 
            // pictureBox_dicFace
            // 
            this.pictureBox_dicFace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox_dicFace.Location = new System.Drawing.Point(1094, 78);
            this.pictureBox_dicFace.Name = "pictureBox_dicFace";
            this.pictureBox_dicFace.Size = new System.Drawing.Size(232, 206);
            this.pictureBox_dicFace.TabIndex = 2;
            this.pictureBox_dicFace.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(13, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(147, 33);
            this.label1.TabIndex = 3;
            this.label1.Text = "实时画面";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 24F, System.Drawing.FontStyle.Bold);
            this.label2.Location = new System.Drawing.Point(816, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(147, 33);
            this.label2.TabIndex = 4;
            this.label2.Text = "实时人脸";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(1103, 45);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(147, 33);
            this.label3.TabIndex = 5;
            this.label3.Text = "学生照片";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("宋体", 24F, System.Drawing.FontStyle.Bold);
            this.label4.Location = new System.Drawing.Point(826, 382);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(213, 33);
            this.label4.TabIndex = 6;
            this.label4.Text = "学生账户信息";
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("宋体", 24F, System.Drawing.FontStyle.Bold);
            this.label5.Location = new System.Drawing.Point(19, 485);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(279, 33);
            this.label5.TabIndex = 8;
            this.label5.Text = "请输入扣费金额：";
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox1.Font = new System.Drawing.Font("宋体", 24F, System.Drawing.FontStyle.Bold);
            this.textBox1.Location = new System.Drawing.Point(316, 485);
            this.textBox1.MaxLength = 6;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(118, 44);
            this.textBox1.TabIndex = 9;
            this.textBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            // 
            // but_close
            // 
            this.but_close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.but_close.Font = new System.Drawing.Font("宋体", 18F);
            this.but_close.Location = new System.Drawing.Point(1242, 557);
            this.but_close.MaximumSize = new System.Drawing.Size(84, 33);
            this.but_close.Name = "but_close";
            this.but_close.Size = new System.Drawing.Size(84, 33);
            this.but_close.TabIndex = 10;
            this.but_close.Text = "返回";
            this.but_close.UseVisualStyleBackColor = true;
            this.but_close.Click += new System.EventHandler(this.but_close_Click);
            // 
            // label_NodShakeDetectResult
            // 
            this.label_NodShakeDetectResult.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label_NodShakeDetectResult.AutoSize = true;
            this.label_NodShakeDetectResult.Font = new System.Drawing.Font("宋体", 24F, System.Drawing.FontStyle.Bold);
            this.label_NodShakeDetectResult.Location = new System.Drawing.Point(1030, 332);
            this.label_NodShakeDetectResult.Name = "label_NodShakeDetectResult";
            this.label_NodShakeDetectResult.Size = new System.Drawing.Size(114, 33);
            this.label_NodShakeDetectResult.TabIndex = 11;
            this.label_NodShakeDetectResult.Text = "请点头";
            // 
            // stuinfo
            // 
            this.stuinfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.stuinfo.AutoSize = true;
            this.stuinfo.Font = new System.Drawing.Font("宋体", 24F, System.Drawing.FontStyle.Bold);
            this.stuinfo.Location = new System.Drawing.Point(903, 450);
            this.stuinfo.Name = "stuinfo";
            this.stuinfo.Size = new System.Drawing.Size(100, 33);
            this.stuinfo.TabIndex = 12;
            this.stuinfo.Text = "     ";
            // 
            // brushFace
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1355, 637);
            this.Controls.Add(this.stuinfo);
            this.Controls.Add(this.label_NodShakeDetectResult);
            this.Controls.Add(this.but_close);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox_dicFace);
            this.Controls.Add(this.pictureBox_shotface);
            this.Controls.Add(this.realTime);
            this.Name = "brushFace";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Text = "brushFace";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.brushFace_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.realTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_shotface)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_dicFace)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox realTime;
        private System.Windows.Forms.PictureBox pictureBox_shotface;
        private System.Windows.Forms.PictureBox pictureBox_dicFace;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button but_close;
        private System.Windows.Forms.Label label_NodShakeDetectResult;
        private System.Windows.Forms.Label stuinfo;
        private System.Windows.Forms.Timer timer1;
    }
}