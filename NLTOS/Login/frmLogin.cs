using NLTOS.Classes;
using NLTOS_Buisness;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace NLTOS.Login
{
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
        }

        void LoadCredentials_UI()
        {
            string error = "";

            if (clsUser.LoadCredentials(out string u, out string p, out bool rem, ref error))
            {
                txtUserName.Text = u;
                txtPassword.Text = p;
                chkRememberMe.Checked = rem;
            }
            else if (!string.IsNullOrEmpty(error))
            {
                MessageBox.Show(error, "Registry Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        void SaveCredentials_UI()
        {
            string error = "";
            bool success = clsUser.SaveCredentials(txtUserName.Text, txtPassword.Text, chkRememberMe.Checked, ref error);

            if (!success && !string.IsNullOrEmpty(error))
            {
                MessageBox.Show(error, "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void frmLogin_Load(object sender, EventArgs e)
        {
            this.ActiveControl = btnLogin;

            LoadCredentials_UI();

            chkRememberMe.Checked = true;
           
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            clsUser user = clsUser.FindByUsernameAndPassword(txtUserName.Text.Trim(), txtPassword.Text.Trim());

            if (user != null)
            {
                SaveCredentials_UI();

                //incase the user is not active
                if (!user.IsActive)
                {

                    txtUserName.Focus();
                    MessageBox.Show("Your accound is not Active, Contact Admin.", "In Active Account", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                clsGlobal.CurrentUser = user;
                this.Hide();
                frmMain frm = new frmMain(this);
                frm.ShowDialog();


            }
            else
            {
                txtUserName.Focus();
                MessageBox.Show("Invalid Username/Password.", "Wrong Credintials", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
