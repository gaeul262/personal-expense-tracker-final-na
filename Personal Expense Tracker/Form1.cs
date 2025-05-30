using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Personal_Expense_Tracker
{
    public partial class Form1 : Form
    {
        //DATABASE CONNECTION TO, WAG NIYONG GAGALAWIN - ZEJI
        string sqlConnection = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""C:\Users\agaca\OneDrive - STI College Ortigas-Cainta\Documents\UsersExpense.mdf"";Integrated Security=True;Connect Timeout=30";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void label2_Click(object sender, EventArgs e)
        {
        }

        private void label3_Click(object sender, EventArgs e)
        {
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
        }

        private void LoginPass1_TextChanged(object sender, EventArgs e)
        {
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            LoginPass1.PasswordChar = checkBox1.Checked ? '\0' : '*';
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        // Login button (static) - ZEJI
        private void button2_Click(object sender, EventArgs e)
        {
            /*
            string email = LoginText1.Text.Trim();
            string password = LoginPass1.Text.Trim();
            //DEFAULT EMAIL AND PASSWORD
            if (email == "admin" && password == "12345678")
            {
                ExpenseTracker mf = new ExpenseTracker();
                mf.Show();
                LoginText1.Clear();
                LoginPass1.Clear();
              
            }
            else
            {
                MessageBox.Show("Email or Password is incorrect.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
           */






            //DATABASE CONNECTION TO, WAG NIYONG GAGALAWIN - ZEJI
           using (SqlConnection dbZ = new SqlConnection(sqlConnection))
              {
                  dbZ.Open();

                  string selectData = "SELECT * FROM UsersExpense WHERE username = @usern AND password = @pass";

                  using (SqlCommand cmd = new SqlCommand(selectData, dbZ))
                  {
                      cmd.Parameters.AddWithValue("@usern", LoginText1.Text.Trim());
                      cmd.Parameters.AddWithValue("@pass", LoginPass1.Text.Trim());

                      SqlDataAdapter adt = new SqlDataAdapter(cmd);
                      DataTable dataTable = new DataTable();
                      adt.Fill(dataTable);

                      if (dataTable.Rows.Count > 0)
                      {
                          MessageBox.Show("Login Successfully", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                          ExpenseTracker main = new ExpenseTracker();
                          main.Show();
                          this.Hide();
                      }
                      else
                      {
                          MessageBox.Show("Incorrect Login Credentials", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                      }

                  }
              }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
        }

        private void linkLabel1_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            RegisterForm regform = new RegisterForm();
            regform.Show();
            this.Hide();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
