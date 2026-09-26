using Microsoft.Win32;
using NLTOS_DataAccess;
using System;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace NLTOS_Buisness
{
    public class clsUser
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

        private clsUser(int UserID, int PersonID, string Username, string Password,
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

            this.UserID = clsUserData.AddNewUser(this.PersonID, this.UserName,
                this.Password, this.IsActive);

            return (this.UserID != -1);
        }
        private bool _UpdateUser()
        {
            //call DataAccess Layer 

            return clsUserData.UpdateUser(this.UserID, this.PersonID, this.UserName,
                this.Password, this.IsActive);
        }
        public static clsUser FindByUserID(int UserID)
        {
            int PersonID = -1;
            string UserName = "", Password = "";
            bool IsActive = false;

            bool IsFound = clsUserData.GetUserInfoByUserID
                                (UserID, ref PersonID, ref UserName, ref Password, ref IsActive);

            if (IsFound)
                //we return new object of that User with the right data
                return new clsUser(UserID, PersonID, UserName, Password, IsActive);
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
        public static clsUser FindByUsernameAndPassword(string UserName, string Password)
        {
            int UserID = -1;
            int PersonID = -1;

            bool IsActive = false;

            bool IsFound = clsUserData.GetUserInfoByUsernameAndPassword
                                (UserName, Password, ref UserID, ref PersonID, ref IsActive);

            if (IsFound)
                //we return new object of that User with the right data
                return new clsUser(UserID, PersonID, UserName, Password, IsActive);
            else
                return null;
        }

        // Used by the login screen: the account is fetched by username, then the supplied
        // password is verified against the stored hash by VerifyPassword.
        public static clsUser FindByUserName(string UserName)
        {
            int UserID = -1;
            int PersonID = -1;
            string Password = "";
            bool IsActive = false;

            bool IsFound = clsUserData.GetUserInfoByUserName
                                (UserName, ref UserID, ref PersonID, ref Password, ref IsActive);

            if (IsFound)
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

        // ------------------------------------------------------------------
        // Credential storage for "Remember Me".
        //
        // The password is encrypted with DPAPI before it is written to the registry,
        // so the stored value is only readable by the Windows account that wrote it.
        // ------------------------------------------------------------------

        private const string RegistryKeyPath = @"HKEY_CURRENT_USER\Software\NLTOS";
        private const string RegistrySubKey = @"Software\NLTOS";

        private static readonly byte[] CredentialEntropy =
            Encoding.UTF8.GetBytes("NLTOS.Credentials.v1");

        private static string Protect(string plainText)
        {
            byte[] protectedBytes = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(plainText ?? ""),
                CredentialEntropy,
                DataProtectionScope.CurrentUser);

            return Convert.ToBase64String(protectedBytes);
        }

        private static string Unprotect(string protectedText)
        {
            byte[] plainBytes = ProtectedData.Unprotect(
                Convert.FromBase64String(protectedText),
                CredentialEntropy,
                DataProtectionScope.CurrentUser);

            return Encoding.UTF8.GetString(plainBytes);
        }

        private static void ClearStoredPassword()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistrySubKey, true))
                {
                    if (key != null)
                        key.DeleteValue("Password", false);
                }
            }
            catch (Exception)
            {
                // Nothing further to do; the stale value simply stays unused.
            }
        }

        /// <summary>
        /// Discards the password kept for this machine, leaving the remembered username
        /// in place. Called when the password it refers to is no longer the right one.
        /// </summary>
        public static void ForgetRememberedPassword()
        {
            ClearStoredPassword();
        }

        public static bool SaveCredentials(string userName, string password, bool isRemembered, ref string errorMessage)
        {
            try
            {
                if (isRemembered)
                {
                    Registry.SetValue(RegistryKeyPath, "Username", userName);
                    Registry.SetValue(RegistryKeyPath, "Password", Protect(password));
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
                string savedUser = (string)Registry.GetValue(RegistryKeyPath, "Username", null);
                string savedPass = (string)Registry.GetValue(RegistryKeyPath, "Password", null);

                if (!string.IsNullOrEmpty(savedUser))
                {
                    userName = savedUser;
                    isRemembered = true;

                    if (!string.IsNullOrEmpty(savedPass))
                    {
                        try
                        {
                            password = Unprotect(savedPass);
                        }
                        catch (Exception)
                        {
                            // Written by an earlier build in clear text, or encrypted under a
                            // different Windows account. Discard it rather than trust it.
                            password = "";
                            ClearStoredPassword();
                        }
                    }

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


        // ------------------------------------------------------------------
        // Password hashing.
        //
        // Passwords are hashed with PBKDF2 using a random per-user salt, so two users
        // with the same password do not end up with the same stored value.
        //
        // Stored format: PBKDF2$<iterations>$<base64 salt>$<base64 hash>
        //
        // Accounts created before salted hashing was introduced hold a plain SHA-256
        // hex digest. VerifyPassword still accepts those, so they keep working.
        // ------------------------------------------------------------------

        private const string HashPrefix = "PBKDF2";
        private const int HashIterations = 100000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public static string HashPassword(string password)
        {
            byte[] salt = new byte[SaltSize];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password ?? "", salt, HashIterations))
            {
                string hash = Convert.ToBase64String(pbkdf2.GetBytes(HashSize));

                return string.Join("$", HashPrefix, HashIterations.ToString(),
                                   Convert.ToBase64String(salt), hash);
            }
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash))
                return false;

            // Legacy accounts: unsalted SHA-256 hex written by earlier builds.
            if (!storedHash.StartsWith(HashPrefix + "$"))
                return AreEqual(Encoding.UTF8.GetBytes(ComputeHash(password ?? "")),
                                Encoding.UTF8.GetBytes(storedHash));

            string[] parts = storedHash.Split('$');

            if (parts.Length != 4)
                return false;

            int iterations;

            if (!int.TryParse(parts[1], out iterations) || iterations <= 0)
                return false;

            try
            {
                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] expected = Convert.FromBase64String(parts[3]);

                using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password ?? "", salt, iterations))
                {
                    return AreEqual(pbkdf2.GetBytes(expected.Length), expected);
                }
            }
            catch (FormatException)
            {
                // Stored value is not in the expected format.
                return false;
            }
        }

        // Comparison that does not return early, so the time taken does not reveal
        // how much of the hash matched.
        private static bool AreEqual(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;

            int difference = 0;

            for (int i = 0; i < a.Length; i++)
                difference |= a[i] ^ b[i];

            return difference == 0;
        }

        // Kept only so accounts created before salted hashing can still sign in.
        // Do not use it for new or changed passwords; use HashPassword instead.
        public static string ComputeHash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

    }
}
