using DBLayer;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FaceSDK;

namespace My_Menu
{
    public partial class BrushFace : Form
    {
        FaceCamera _faceCamera;

        List<FaceInfoBase> facelist_m = new List<FaceInfoBase>();
        List<FaceInfoBase> facelist_f = new List<FaceInfoBase>();
        SpeechSynthesizer speak = new SpeechSynthesizer();
        private int _lastFaceID;

        public BrushFace()
        {
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            InitializeComponent();
            _faceCamera = FaceCamera.Instance;
            if (!_faceCamera.Open("0", 640, 480))
            {
                MessageBox.Show("相机打开失败！\n" + "请确认相机硬件连接。",
                    "相机启动失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            //设置人脸相机参数
            _faceCamera.PicBoxRealTime = realTime;//指定实时图像的图片框
            _faceCamera.PicBoxShotFace = null;//实时人脸
            _faceCamera.PicBoxFoundPic = pictureBox_dicFace;

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
                    pictureBox_dicFace.Image = null;
                    return;
                }
                if (_lastFaceID == e.faceinfo.faceid) return;
                
                if (f.userid != 0)
                {
                    _faceCamera.FaceCmd = FaceCamera.FaceCommand.NodShakeDetect;     //转换为检测角度模式

                    UserInfo uinfo = UserInfo.Get(f.userid);
                    label_userinfo.Text = 
                    "账号：" + uinfo.usernumber +
                    "\n姓名：" + uinfo.username +
                    "\n性别：" + (uinfo.gender==0?"男":"女") ;
                    textBox_balance.Text = string.Format("{0}", uinfo.money);
                    textBox_payment.Text = string.Format("{0}",10);
                    textBox_balanceRemain.Text = "";
                    _faceCamera.SetSpeakOrderedAsync("确认付款请点头，取消付款请摇头。");
                }
                else
                {
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
                    label_DetectResultHint.Text = "支付成功，祝您用餐愉快！";
                    _faceCamera.SetSpeakAsync("支付成功，祝您用餐愉快！");
                }
                else if (e.type == FaceEvent.EventType.HeadShakeDetected)
                {
                    label_DetectResultHint.Text = "付款取消，欢迎下次光临！";
                    _faceCamera.SetSpeakAsync("付款取消，欢迎下次光临！");
                }

                _faceCamera.FaceCmd = FaceCamera.FaceCommand.ShotOneAndFind;
            };
            this.BeginInvoke(d);
        }
  
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (((int)e.KeyChar < 48 || (int)e.KeyChar > 57) && (int)e.KeyChar != 8 && (int)e.KeyChar != 46)
                e.Handled = true;
            try
            {
                string[] a = textBox_payment.Text.Split('.');
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
                    if (textBox_payment.Text.Length <= 0)
                        e.Handled = true;//小数点不能在第一位
                    else
                    {
                        float f;
                        float oldf;
                        bool b1 = false, b2 = false;
                        b1 = float.TryParse(textBox_payment.Text, out oldf);
                        b2 = float.TryParse(textBox_payment.Text + e.KeyChar.ToString(), out f);
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
            this.Close();
        }
        private void brushFace_FormClosing(object sender, FormClosingEventArgs e)
        {
            _faceCamera.CancelSpeak();
            _faceCamera.Close();
            _faceCamera.FaceHandler -= onFaceHandler;
            MainMenu.Instance.Show();
        }
    }
  
}
