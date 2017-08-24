using Leap;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System;

namespace AuthenticationSystem
{
    public partial class LoginForm : Form
    {
        
        public LoginForm()
        {
            InitializeComponent();
        }

        private void Form1_Load (object sender, EventArgs e)
        {

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = "CBAdmin";
            string password = "1240CBAdmin";
            if (txtUsername.Text != username && txtPassword.Text != password) {
                MessageBox.Show("Please enter valid login credentials.", "ERROR");
            }else
            {
                Form2 frm2 = new Form2();
                frm2.Show();
                this.Hide();
            }
            
        }
    }
}
