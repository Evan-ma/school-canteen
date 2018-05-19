using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FaceSDK;
using DBLayer;

namespace My_Menu
{
   
    public partial class MainMenu : Form
    {
        public static Form Instance;
        public MainMenu(Login form2)
        {
            Instance = this;
            InitializeComponent();

            FaceCamera camera = new FaceCamera();
            //初始化相机
            if (!camera.Init())
            {
                MessageBox.Show("面部识别库初始化失败！\n" + "点击确定关闭程序。",
                    "人脸库失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                System.Environment.Exit(0);
            }
            if (!SQLModel.InitDB())
            {
                MessageBox.Show("数据库初始化失败！\n" + "点击确定关闭程序。",
                    "人脸库失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                System.Environment.Exit(0);
            }
            camera.Data.DataSource = DataAccess.DataSrcType.DataBase;
            camera.Data.InitDB(SQLEngine.Instance.Connection);
            camera.Data.LoadFaceData(null);
            camera.SpeechEnable = true;
        }

        private void button_BrushFace_Click(object sender, EventArgs e)
        {
            new BrushFace().Show();
            this.Visible = false;
        }
        private void button_UserManagement_Click(object sender, EventArgs e)
        {
            new UserManagement().Show();
            this.Visible = false;
        }
        private void button_Recharge_Click(object sender, EventArgs e)
        {
            new Recharge(this).Show();
            this.Hide();
        }

        private void button_AccountManagement_Click(object sender, EventArgs e)
        {
            new AccountManagement().Show();
            this.Visible = false;
        }

        private void button_DataManagement_Click(object sender, EventArgs e)
        {
            new DataManagement().Show();
            this.Visible = false;
        }

        

        private void MainMenu_FormClosing(object sender, FormClosingEventArgs e)
        {
            FaceCamera.Instance.Dispose();
            System.Environment.Exit(0);
        }

        
    }

   
}
