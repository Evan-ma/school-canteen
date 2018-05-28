using DBLayer;
using FaceSDK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace My_Menu
{
    public partial class Recharge : Form
    {
        FaceCamera _faceCamera;
        private int _lastFaceID;
        public Recharge()
        {
            InitializeComponent();
            _faceCamera = FaceCamera.Instance;
            if (!_faceCamera.Open("0", 640, 480))
            {
                MessageBox.Show("相机打开失败！\n" + "请确认相机硬件连接。",
                    "相机启动失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            //设置人脸相机参数
            _faceCamera.PicBoxRealTime = pictureBox1;//指定实时图像的图片框
            _faceCamera.PicBoxShotFace = pictureBox2;//实时人脸
            _faceCamera.PicBoxFoundPic = pictureBox3;

            _faceCamera.FaceHandler += onFaceHandler;
            _faceCamera.Start();
            _faceCamera.FaceCmd = FaceCamera.FaceCommand.ShotOneAndFind;
        }
        protected void onFaceHandler(FaceEvent e)
        {
            switch (e.type)
            {
                case FaceEvent.EventType.ShotOne:
                    onOne2NHandler(e);
                    break;
                case FaceEvent.EventType.HeadNodDetected:
                    onHeadNodShakeHandler(e);
                    break;
                case FaceEvent.EventType.HeadShakeDetected:
                    onHeadNodShakeHandler(e);
                    break;
                default:
                    break;
            }
        }

        protected void onOne2NHandler(FaceEvent e)
        {
            Action d = () =>
            {
                FaceInfo f = e.faceinfo;
                if (f == null)
                {
                    pictureBox1.Image = null;
                    return;
                }
                if (_lastFaceID == e.faceinfo.faceid) return;

                if (f.userid != 0)
                {
                    _faceCamera.FaceCmd = FaceCamera.FaceCommand.NodShakeDetect;     //转换为检测角度模式

                    UserInfo uinfo = UserInfo.Get(f.userid);
                    xingming.Text = uinfo.usernumber;
                    shoujihao.Text = uinfo.phonenumber;
                    yue.Text = uinfo.money.ToString();
                    _faceCamera.SetSpeakOrderedAsync("确认充值请点头，取消充值请摇头。");
                }
                else
                {
                    xingming.Text = "";
                    shoujihao.Text = "";
                    yue.Text ="";
                    //输出语音提示，您还没有注册账号
                    _faceCamera.SetSpeakAsync("您尚未注册账号！");
                }
                if (_lastFaceID != e.faceinfo.faceid)
                {
                    _lastFaceID = e.faceinfo.faceid;
                }

            };
            this.BeginInvoke(d);
        }
        protected void onHeadNodShakeHandler(FaceEvent e)
        {
            Action d = () =>
            {
                if (e.type == FaceEvent.EventType.HeadNodDetected)
                {
                    MoneyRecord.Create(_lastFaceID, int.Parse(textBox1.Text), DateTime.Now);
                    yue.Text = (int.Parse(yue.Text) + int.Parse(textBox1.Text)).ToString();
                    _faceCamera.SetSpeakAsync("充值成功，感谢使用");
                   
                }
                else if (e.type == FaceEvent.EventType.HeadShakeDetected)
                {
                    _faceCamera.SetSpeakAsync("充值取消，欢迎下次光临！");
                }

                _faceCamera.FaceCmd = FaceCamera.FaceCommand.ShotOneAndFind;
            };
            this.BeginInvoke(d);
        }


        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Recharge_FormClosing(object sender, FormClosingEventArgs e)
        {
            _faceCamera.CancelSpeak();
            _faceCamera.Close();
            _faceCamera.FaceHandler -= onFaceHandler;
            MainMenu.Instance.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //MoneyRecord.Create(_lastFaceID, int.Parse(textBox1.Text), DateTime.Now);
            //yue.Text = (int.Parse(yue.Text) + int.Parse(textBox1.Text)).ToString();
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
                    if (s > 9 && e.KeyChar != '\b')
                    {
                        e.Handled = true;
                    }
                }
            }
            catch { }
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
    }
}
