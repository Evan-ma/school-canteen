using Emgu.CV;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace My_Menu
{
    public partial class brushFace : Form
    {
        public brushFace()
        {
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            InitializeComponent();
        }
        FaceCamera _faceCamera;
        private void startUp_Click(object sender, EventArgs e)
        {
            if(this.startUp.Text != "停止")
            {          
            //初始化人脸识别库
            _faceCamera = new FaceCamera();
            int ret = _faceCamera.Init();
            if (ret == -1)
            {
                MessageBox.Show("面部识别库初始化失败！" + Environment.NewLine + "点击确定关闭程序。", "人脸库失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                System.Environment.Exit(0);
            }
            else if (ret == -2)
            {
                MessageBox.Show("相机打开失败！" + Environment.NewLine + "点击确定关闭程序。", "相机失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                System.Environment.Exit(0);
            }
            this.startUp.Text = "停止";
            //设置人脸相机参数
            _faceCamera.PicBoxRealTime = realTime;//指定实时图像的图片框
            _faceCamera.Start();
            _faceCamera.PicBoxShotFace = realFace;//实时人脸
            _faceCamera.FaceCmd = FaceCamera.FaceCommand.ShotOneFindSimiler;
            } else {
                try
                {
                    _faceCamera.Stop();
                    _faceCamera.Dispose();
                }
                catch
                {
					
                }
                finally
                {
                    this.startUp.Text = "启动";
                }
            }
        }

        private void brushFace_FormClosed(object sender, FormClosedEventArgs e)
        {
              MainMenu.f.Show();
        }
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (((int)e.KeyChar < 48 || (int)e.KeyChar > 57) && (int)e.KeyChar != 8 && (int)e.KeyChar != 46)
                e.Handled = true;
            try
            {
                string[] a = textBox1.Text.Split('.');
                if (a[1] != "")
                {
                    int s = int.Parse(a[1]);
                    if (s > 9&& e.KeyChar != '\b')
                    {
                        e.Handled = true;
                    }
                }
            }catch{}
            finally
            {  //小数点的处理。
                if ((int)e.KeyChar == 46)//小数点
                {
                    if (textBox1.Text.Length <= 0)
                        e.Handled = true;//小数点不能在第一位
                    else
                    {
                        float f;
                        float oldf;
                        bool b1 = false, b2 = false;
                        b1 = float.TryParse(textBox1.Text, out oldf);
                        b2 = float.TryParse(textBox1.Text + e.KeyChar.ToString(), out f);
                        if (b2 == false)
                        {
                            if (b1 == true)
                                e.Handled = true;
                            else
                                e.Handled = false;
                        }
                    }
                }
            }
        }

        private void but_close_Click(object sender, EventArgs e)
        {
            try
            {
                _faceCamera.Stop();
                _faceCamera.Dispose();
            }
            catch
            {
            }
            finally
            {
                this.Close();
            }

        }
    }
}
