using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;

namespace My_Menu
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }
        private String password;
        private void button1_Click(object sender, EventArgs e)
        {
            password= ConfigurationManager.AppSettings["password"];
            if (textBox1.Text==password)
            {
                //MessageBox.Show("登陆成功！", "登陆成功");
                new MainMenu(this).Show();
                this.Hide();
            }
            else MessageBox.Show("密码错误，请重新输入！", "登陆出错");
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
                button1_Click(null,null);
        }
        /*更改配置文件
        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        config.AppSettings.Settings["password"].Value ="10000";
        config.Save(ConfigurationSaveMode.Modified);
        //强制重新载入配置文件的ConnectionStrings配置节   
        ConfigurationManager.RefreshSection("appSettings");
        MessageBox.Show("保存成功！");
         */
    }
}






















