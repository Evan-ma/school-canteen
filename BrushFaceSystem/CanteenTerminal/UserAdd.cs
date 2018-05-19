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
    public partial class UserAdd : Form
    {
        private UserManagement _parent;
        private FaceCamera _fcamera;
        private FaceInfo[] _faceData = new FaceInfo[5];
        public UserAdd(UserManagement f)
        {
            InitializeComponent();
            _parent = f;
            _fcamera = FaceCamera.Instance;
            if (!_fcamera.Open("0", 640, 480))
            {
                MessageBox.Show("相机打开失败！\n" + "请确认相机硬件连接。",
                    "相机启动失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _fcamera.PicBoxRealTime = pictureBox_rt;
            _fcamera.PicBoxFoundPic = null;
            _fcamera.PicBoxShotFace = null;
            _fcamera.FaceHandler += onFaceHandler;
            _fcamera.Start();
            _fcamera.FaceCmd = FaceCamera.FaceCommand.None;
        }

        protected void onFaceHandler(FaceEvent e)
        {
            switch (e.type)
            {
                case FaceEvent.EventType.FaceCollected:
                    onFaceCollectedHandler(e);
                    break;
                default:
                    break;
            }
        }
        protected void onFaceCollectedHandler(FaceEvent e)
        {
            Action d = () =>
            {
                FaceInfo f = e.faceinfo;
                switch (f.angleType)
                {
                    case FaceData.FaceAngleType.Middle:
                        pictureBox_face1.Image = f.FaceShotBmp;
                        _fcamera.RequestAngleType = FaceData.FaceAngleType.Up;
                        _faceData[0] = new FaceInfo();
                        _faceData[0].copyContent(f);
                        break;
                    case FaceData.FaceAngleType.Up:
                        pictureBox_face2.Image = f.FaceShotBmp;
                        _fcamera.RequestAngleType = FaceData.FaceAngleType.Down;
                        _faceData[1] = new FaceInfo();
                        _faceData[1].copyContent(f);
                        break;
                    case FaceData.FaceAngleType.Down:
                        pictureBox_face3.Image = f.FaceShotBmp;
                        _fcamera.RequestAngleType = FaceData.FaceAngleType.Left;
                        _faceData[2] = new FaceInfo();
                        _faceData[2].copyContent(f);
                        break;
                    case FaceData.FaceAngleType.Left:
                        pictureBox_face4.Image = f.FaceShotBmp;
                        _fcamera.RequestAngleType = FaceData.FaceAngleType.Right;
                        _faceData[3] = new FaceInfo();
                        _faceData[3].copyContent(f);
                        break;
                    case FaceData.FaceAngleType.Right:
                        pictureBox_face5.Image = f.FaceShotBmp;
                        _fcamera.FaceCmd = FaceCamera.FaceCommand.None;
                        MessageBox.Show("谢谢您的配合，照片采集完成！");
                        _faceData[4] = new FaceInfo();
                        _faceData[4].copyContent(f);
                        break;
                    default:
                        break;
                }
            };
            this.BeginInvoke(d);
        }

        private void button_save_Click(object sender, EventArgs e)
        {
            UserInfo ui = new UserInfo(textBox_no.Text, textBox_name.Text,
                textBox_gender.Text == "男" ? 0 : 1, textBox_phone.Text,
                textBox_addr.Text, textBox_node.Text, 0);
            _fcamera.Data.AddUserFaces(_faceData, ui);
            this.Close();
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void UserAdd_FormClosing(object sender, FormClosingEventArgs e)
        {
            _fcamera.Close();
            _fcamera.FaceHandler -= onFaceHandler;
            _parent.UpdateGridView();
        }

        private void button_collect_Click(object sender, EventArgs e)
        {
            if(button_collect.Text == "开始采集人脸信息")
            {
                button_collect.Text = "停止采集";
                _fcamera.FaceCmd = FaceCamera.FaceCommand.FaceCollect;
            }
            else
            {
                button_collect.Text = "开始采集人脸信息";
                _fcamera.FaceCmd = FaceCamera.FaceCommand.None;
            }

        }

    }
}
