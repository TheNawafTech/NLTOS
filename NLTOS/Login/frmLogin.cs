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

        private void frmLogin_Load(object sender, EventArgs e)
        {
            this.ActiveControl = btnLogin;

            LoadCredentials();

            chkRememberMe.Checked = true;
           
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public void SaveCredentials()
        {
            if (chkRememberMe.Checked)
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\NLTOS", "Username", txtUserName.Text.Trim());
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\NLTOS", "Password", txtPassword.Text.Trim());
            }
            else
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\NLTOS", true))
                {
                    if (key != null)
                    {
                        key.DeleteValue("Username", false);
                        key.DeleteValue("Password", false);
                    }
                }
            }
        }

        public void LoadCredentials()
        {
            try
            {
                string savedUser = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\NLTOS", "Username", "");
                string savedPass = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\NLTOS", "Password", "");

                if (!string.IsNullOrEmpty(savedUser))
                {
                    txtUserName.Text = savedUser;
                    txtPassword.Text = savedPass;
                    
                    chkRememberMe.Checked = true;

                }
                else
                {
                    chkRememberMe.Checked = false;
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Sorry, you don't have permission to access the registry.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            clsUser user = clsUser.FindByUsernameAndPassword(txtUserName.Text.Trim(), txtPassword.Text.Trim());

            if (user != null)
            {

                SaveCredentials();

                //if (chkRememberMe.Checked)
                //{
                //    //store username and password
                //    clsGlobal.RememberUsernameAndPassword(txtUserName.Text.Trim(), txtPassword.Text.Trim());

                //}
                //else
                //{
                //    //store empty username and password
                //    clsGlobal.RememberUsernameAndPassword("", "");

                //}

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
