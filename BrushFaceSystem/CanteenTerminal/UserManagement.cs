using DBLayer;
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
    public partial class UserManagement : Form
    {
        public UserManagement()
        {
            InitializeComponent();

            
        }
        public void UpdateGridView()
        {
            DataSet ds = UserInfo.GetAll();
            if (ds.Tables.Count > 0)
                dataGridView.DataSource = ds.Tables[0];
        }
        private void button_Add_Click(object sender, EventArgs e)
        {
            new UserAdd(this).ShowDialog();
        }

        private void UserManagement_FormClosing(object sender, FormClosingEventArgs e)
        {
            MainMenu.Instance.Show();
        }
    }
}
