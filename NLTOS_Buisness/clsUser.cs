using Microsoft.Win32;
using NLTOS_DataAccess;
using System;
using System.Data;
using System.Runtime.InteropServices;

namespace NLTOS_Buisness
{
    public  class clsUser
    {
        public enum enMode { AddNew = 0, Update = 1 };
        public enMode Mode = enMode.AddNew;

        public int UserID { set; get; }
        public int PersonID { set; get; }
        public clsPerson PersonInfo;
        public string UserName { set; get; }
        public string Password { set; get; }
        public bool IsActive { set; get; }
     
        public clsUser()

        {     
            this.UserID = -1;
            this.UserName = "";
            this.Password = "";
            this.IsActive = true;
            Mode = enMode.AddNew;
        }

        private clsUser(int UserID, int PersonID, string Username,string Password,
            bool IsActive)

        {
            this.UserID = UserID; 
            this.PersonID = PersonID;
            this.PersonInfo = clsPerson.Find(PersonID);
            this.UserName = Username;
            this.Password = Password;
            this.IsActive = IsActive;

            Mode = enMode.Update;
        }

        private bool _AddNewUser()
        {
            //call DataAccess Layer 

            this.UserID = clsUserData.AddNewUser(this.PersonID,this.UserName,
                this.Password,this.IsActive);

            return (this.UserID != -1);
        }
        private bool _UpdateUser()
        {
            //call DataAccess Layer 

            return clsUserData.UpdateUser(this.UserID,this.PersonID,this.UserName,
                this.Password,this.IsActive);
        }
        public static clsUser FindByUserID(int UserID)
        {
            int PersonID = -1;
            string UserName = "", Password = "";
            bool IsActive = false;

            bool IsFound = clsUserData.GetUserInfoByUserID
                                ( UserID,ref PersonID, ref UserName,ref Password,ref IsActive);

            if (IsFound)
                //we return new object of that User with the right data
                return new clsUser(UserID,PersonID,UserName,Password,IsActive);
            else
                return null;
        }
        public static clsUser FindByPersonID(int PersonID)
        {
            int UserID = -1;
            string UserName = "", Password = "";
            bool IsActive = false;

            bool IsFound = clsUserData.GetUserInfoByPersonID
                                (PersonID, ref UserID, ref UserName, ref Password, ref IsActive);

            if (IsFound)
                //we return new object of that User with the right data
                return new clsUser(UserID, UserID, UserName, Password, IsActive);
            else
                return null;
        }
        public static clsUser FindByUsernameAndPassword(string UserName,string Password)
        {
            int UserID = -1;
            int PersonID=-1;

            bool IsActive = false;

            bool IsFound = clsUserData.GetUserInfoByUsernameAndPassword
                                (UserName , Password,ref UserID,ref PersonID, ref IsActive);

            if (IsFound)
                //we return new object of that User with the right data
                return new clsUser(UserID, PersonID, UserName, Password, IsActive);
            else
                return null;
        }

        public bool Save()
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    if (_AddNewUser())
                    {

                        Mode = enMode.Update;
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case enMode.Update:

                    return _UpdateUser();

            }

            return false;
        }

        public static DataTable GetAllUsers()
        {
            return clsUserData.GetAllUsers();
        }

        public static bool DeleteUser(int UserID)
        {
            return clsUserData.DeleteUser(UserID); 
        }

        public static bool isUserExist(int UserID)
        {
           return clsUserData.IsUserExist(UserID);
        }

        public static bool isUserExist(string UserName)
        {
            return clsUserData.IsUserExist(UserName);
        }

        public static bool isUserExistForPersonID(int PersonID)
        {
            return clsUserData.IsUserExistForPersonID(PersonID);
        }

        public static bool SaveCredentials(string userName, string password, bool isRemembered, ref string errorMessage)
        {
            try
            {
                if (isRemembered)
                {
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\NLTOS", "Username", userName);
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\NLTOS", "Password", password);
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
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                errorMessage = "Access Denied: You don't have permission to write to the Registry.";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = "An unexpected error occurred while saving: " + ex.Message;
                return false;
            }
        }
        public static bool LoadCredentials(out string userName, out string password, out bool isRemembered, ref string errorMessage)
        {
            userName = "";
            password = "";
            isRemembered = false;

            try
            {
                string savedUser = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\NLTOS", "Username", null);
                string savedPass = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\NLTOS", "Password", null);

                if (!string.IsNullOrEmpty(savedUser))
                {
                    userName = savedUser;
                    password = savedPass;
                    isRemembered = true;
                    return true;
                }
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                errorMessage = "Access Denied: No permission to read from the Registry.";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = "Error loading credentials: " + ex.Message;
                return false;
            }
        }
    }
}
