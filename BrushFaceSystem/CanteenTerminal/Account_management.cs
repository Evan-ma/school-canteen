using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBLayer;

namespace My_Menu
{
    public partial class AccountManagement : Form
    {
       
        public AccountManagement()
        {
            InitializeComponent();
            freshdata("11");
        }
        private int money = 0;

        private void data_add(List<MoneyRecord> records)
        {
            money = 0;
            while (dataGridView1.Rows.Count != 0)
            {
                dataGridView1.Rows.RemoveAt(0);
            }
            if(records ==  null)
            {
                return ;
            }
            if (radioButton1.Checked)
            {
                foreach (MoneyRecord mr in records)
                {
                    if (mr.changedmoney <= 0)
                    {
                        adddr(mr);
                    }
                }
            }
            if (radioButton2.Checked)
            {
                foreach (MoneyRecord mr in records)
                {
                    if (mr.changedmoney >= 0)
                    {
                        adddr(mr);
                    }
                }
            }
            if (radioButton3.Checked)
            {
                foreach (MoneyRecord mr in records)
                {
                    adddr(mr);
                }
            }
            textBox2.Text = "合计：" + money + "元";
            records.Clear();
        }

        private void adddr(MoneyRecord mr)
        {
            DataGridViewRow dr = new DataGridViewRow();
            dr.CreateCells(dataGridView1);
            dr.Cells[0].Value = mr.usernumber;
            dr.Cells[1].Value = mr.username;
            dr.Cells[2].Value = mr.changedmoney;
            dr.Cells[3].Value = mr.changetime.ToString();
            dataGridView1.Rows.Insert(0, dr);
            money += mr.changedmoney;
        }

        List<MoneyRecord> records;
        private void freshdata(string uid)
        {
            if(textBox1.Text!= "输入学号")
            {
                records = MoneyRecord.FindByUid(UserInfo.Getuid(uid));
                data_add(records);
            }
            else
            {
               records = MoneyRecord.Getall();
                data_add(records);
            }

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {
            
        }
        
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void Account_management_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            freshdata(textBox1.Text);
        }

        private void textBox1_MouseClick(object sender, MouseEventArgs e)
        {
            textBox1.Text = "";
            this.textBox1.ForeColor = Color.Black;
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if(textBox1.Text=="")
            {
                this.textBox1.ForeColor = Color.DarkGray;
                textBox1.Text = "输入账号";
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MainMenu.Instance.Show();
            this.Hide();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            freshdata(textBox1.Text);
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            freshdata(textBox1.Text);
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            freshdata(textBox1.Text);
        }
    }
}
