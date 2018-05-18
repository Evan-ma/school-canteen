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
    public partial class brushFace : Form
    {
        FaceCamera _faceCamera;

        List<FaceInfoBase> facelist_m = new List<FaceInfoBase>();
        List<FaceInfoBase> facelist_f = new List<FaceInfoBase>();
        FaceEvent fevent = null;
        SpeechSynthesizer speak = new SpeechSynthesizer();


        public brushFace()
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
            _faceCamera.PicBoxShotFace = pictureBox_shotface;//实时人脸
            _faceCamera.PicBoxFoundPic = pictureBox_dicFace;

            _faceCamera.FaceHandler += onFaceHandler;
            _faceCamera.FaceCmd = FaceCamera.FaceCommand.ShotOneAndFind;
            _faceCamera.Start();
        }

        private void Sqlcreate() //数据库初始化
        {
            FaceInfoBase.MakeTable();
            UserInfo.MakeTable();
            MoneyRecord.MakeTable();
            facelist_m = FaceInfoBase.GetList(1);
            facelist_f = FaceInfoBase.GetList(0);
        }

        private void startUp_Click(object sender, EventArgs e)
        {
            //if(this.startUp.Text != "停止")
            //{          
 
            //this.startUp.Text = "停止";
            ////设置人脸相机参数
            //_faceCamera.PicBoxRealTime = realTime;//指定实时图像的图片框
            //_faceCamera.FaceHandler += onFaceHandler;
            //_faceCamera.Start();
            //_faceCamera.PicBoxShotFace = realFace;//实时人脸

            //_faceCamera.FaceCmd = FaceCamera.FaceCommand.ShotOneFindSimiler;
            
            //} else {
            //    try
            //    {
            //        _faceCamera.Stop();
            //        _faceCamera.Dispose();
            //    }
            //    catch
            //    {
            //        Console.WriteLine("catch");
            //    }
            //    finally
            //    {
            //        this.startUp.Text = "启动";
            //    }
            //}
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
                    this.pictureBox_shotface.Image = null;
                    return;
                }
                if(f.userid != 0)
                {
                    _faceCamera.FaceCmd = FaceCamera.FaceCommand.NodShakeDetect;     //转换为检测角度模式
                    Thread t = new Thread(() => speaker("您好，   付款请点头。    取消付款请摇头"));
                    t.Start();
                    UserInfo uinfo = UserInfo.Get(f.userid);
                    stuinfo.Text = "姓名：" + uinfo.username +
                    "\r学号：" + uinfo.usernumber +
                    "\r性别：" + uinfo.gender +
                    "\r账户余额：" + uinfo.money;
                }
                else
                {
                    //输出语音提示，您还没有注册账号
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
                    label_NodShakeDetectResult.Text = "点头";
                }
                else if (e.type == FaceEvent.EventType.HeadShakeDetected)
                {
                    label_NodShakeDetectResult.Text = "摇头";
                }
                
                    Thread t = new Thread(() => speaker("付款取消，欢迎下次光临"));
                    t.Start();
                    _faceCamera.FaceCmd = FaceCamera.FaceCommand.ShotOneAndFind;
                    t = new Thread(() => speaker("支付成功，欢迎下次光临"));
                    t.Start();
                    _faceCamera.FaceCmd = FaceCamera.FaceCommand.ShotOneAndFind;
                System.Windows.Forms.Timer tim = new System.Windows.Forms.Timer(); 
            };
            this.BeginInvoke(d);
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

        private void realFace_Paint(object sender, PaintEventArgs e) //抓住人脸
        {
            if(startUp.Text == "启动")
            {
                return;
            }
            Getuid(fevent);
            _faceCamera.FaceCmd = FaceCamera.FaceCommand.NodShakeDetect;     //转换为检测角度模式
            Thread t = new Thread(() => speaker("您好，   付款请点头。    取消付款请摇头"));
            t.Start();
        }
        FaceRecgnize fr = new FaceRecgnize();
        private void Getuid(FaceEvent e)
        {
            
            FaceInfo f = e.faceinfo;
            if (f.gender.ToString()=="男")
            {
                foreach(FaceInfoBase s in facelist_m)
                {
                    if (fr.FaceFeatureMatch(s.faceInfo, f)>=650)
                    {
                        Getinfo(s.uid);
                    }
                }
            }
            else
            {
                foreach (FaceInfoBase s in facelist_f)
                {
                    if (fr.FaceFeatureMatch(s.faceInfo, f) >= 650)
                    {
                        Getinfo(s.uid);
                    }
                }
            }

        }
        UserInfo uinfo = new UserInfo();//获取数据库中信息
        private void Getinfo(int? uid)
        {
            UserInfo.Get(uid.Value);
            stuinfo.Text = "姓名："+ uinfo.username+
                "\r学号：" +uinfo.usernumber+
                "\r性别：" +uinfo.gender+
                "\r账户余额："+uinfo.money;
   
        }


        private void speaker(string yuyin)
        {
            speak.Speak(yuyin);
        }
        private void but_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void brushFace_FormClosing(object sender, FormClosingEventArgs e)
        {
            _faceCamera.Close();
            _faceCamera.FaceHandler -= onFaceHandler;
            speak.Dispose();
            MainMenu.Instance.Show();
        }
    }
  
}
