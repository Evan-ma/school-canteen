using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My_Menu
{
   
    public partial class MainMenu : Form
    {
        public static Form f;
        public MainMenu(Login form2)
        {
            f = this;
            InitializeComponent();
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            new Recharge(this).Show();
            this.Hide();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            new Account_management().Show();
            this.Visible = false;
        }

        private void MainMenu_Load(object sender, EventArgs e)
        {

        }
        private void button5_Click(object sender, EventArgs e)
        {
            new Data_management().Show();
            this.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new brushFace().Show();
            this.Visible = false;
        }
    }

   
}
