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
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox1.Text=="123")
            {
                //MessageBox.Show("登陆成功！", "登陆成功");
                new Form1(this).Show();
                this.Hide();
            }
            else MessageBox.Show("密码错误，请重新输入！", "登陆出错");
        }
    }
}






















