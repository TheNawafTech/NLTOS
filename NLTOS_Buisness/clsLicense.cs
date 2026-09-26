using System;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Xml.Linq;
using NLTOS_DataAccess;
using static System.Net.Mime.MediaTypeNames;

namespace NLTOS_Buisness
{
    public class clsLicense
    {

        public enum enMode { AddNew = 0, Update = 1 };
        public enMode Mode = enMode.AddNew;

        public enum enIssueReason { FirstTime = 1, Renew = 2, DamagedReplacement = 3, LostReplacement = 4 };

        public clsDriver DriverInfo;
        public int LicenseID { set; get; }
        public int ApplicationID { set; get; }
        public int DriverID { set; get; }
        public int LicenseClass { set; get; }
        public clsLicenseClass LicenseClassIfo;
        public DateTime IssueDate { set; get; }
        public DateTime ExpirationDate { set; get; }
        public string Notes { set; get; }
        public float PaidFees { set; get; }
        public bool IsActive { set; get; }
        public enIssueReason IssueReason { set; get; }
        public string IssueReasonText
        { get 
            { 
                return GetIssueReasonText(this.IssueReason); 
            } 
        }
        public clsDetainedLicense DetainedInfo { set; get; }
        public int CreatedByUserID { set; get; }
        public bool IsDetained
        {
            get { return clsDetainedLicense.IsLicenseDetained(this.LicenseID); }
        }

        public clsLicense()

        {
            this.LicenseID = -1;
            this.ApplicationID= -1;
            this.DriverID = -1;
            this.LicenseClass = -1;
            this.IssueDate = DateTime.Now;
            this.ExpirationDate = DateTime.Now;
            this.Notes = "";
            this.PaidFees = 0;
            this.IsActive = true;
            this.IssueReason = enIssueReason.FirstTime;
            this.CreatedByUserID = -1;

            Mode = enMode.AddNew;

        }

        public clsLicense(int LicenseID,int ApplicationID, int DriverID, int LicenseClass,
            DateTime IssueDate, DateTime ExpirationDate, string Notes,
            float PaidFees, bool IsActive,enIssueReason IssueReason, int CreatedByUserID)

        {
            this.LicenseID = LicenseID;
            this.ApplicationID = ApplicationID;
            this.DriverID = DriverID;
            this.LicenseClass = LicenseClass;
            this.IssueDate = IssueDate;
            this.ExpirationDate = ExpirationDate;
            this.Notes = Notes;
            this.PaidFees = PaidFees;
            this.IsActive = IsActive;
            this.IssueReason = IssueReason;
            this.CreatedByUserID = CreatedByUserID;

            this.DriverInfo = clsDriver.FindByDriverID(this.DriverID);
            this.LicenseClassIfo = clsLicenseClass.Find(this.LicenseClass);
            this.DetainedInfo=clsDetainedLicense.FindByLicenseID(this.LicenseID);   

            Mode = enMode.Update;
        }

        private bool _AddNewLicense()
        {
            //call DataAccess Layer 

            this.LicenseID = clsLicenseData.AddNewLicense(this.ApplicationID, this.DriverID, this.LicenseClass,
               this.IssueDate, this.ExpirationDate, this.Notes, this.PaidFees,
               this.IsActive,(byte) this.IssueReason, this.CreatedByUserID);


            return (this.LicenseID != -1);
        }

        private bool _UpdateLicense()
        {
            //call DataAccess Layer 

            return clsLicenseData.UpdateLicense(this.ApplicationID, this.LicenseID, this.DriverID, this.LicenseClass,
               this.IssueDate, this.ExpirationDate, this.Notes, this.PaidFees,
               this.IsActive,(byte) this.IssueReason, this.CreatedByUserID);
        }

        public static clsLicense Find(int LicenseID)
        {
            int ApplicationID = -1; int DriverID = -1; int LicenseClass = -1;
            DateTime IssueDate = DateTime.Now; DateTime ExpirationDate = DateTime.Now;
            string Notes = "";
            float PaidFees = 0; bool IsActive = true; int CreatedByUserID = 1;
            byte IssueReason = 1;
            if (clsLicenseData.GetLicenseInfoByID(LicenseID,ref ApplicationID, ref DriverID, ref LicenseClass,
            ref IssueDate, ref ExpirationDate, ref Notes,
            ref PaidFees, ref IsActive,ref IssueReason, ref CreatedByUserID))

                return new clsLicense(LicenseID,ApplicationID, DriverID, LicenseClass,
                                     IssueDate, ExpirationDate, Notes,
                                     PaidFees, IsActive,(enIssueReason) IssueReason, CreatedByUserID);
            else
                return null;

        }

        public static DataTable GetAllLicenses()
        {
            return clsLicenseData.GetAllLicenses();

        }

        public bool Save()
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    if (_AddNewLicense())
                    {

                        Mode = enMode.Update;
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case enMode.Update:

                    return _UpdateLicense();

            }

            return false;
        }

        public static bool IsLicenseExistByPersonID(int PersonID, int LicenseClassID)
        {
            return (GetActiveLicenseIDByPersonID(PersonID, LicenseClassID) != -1);
        }

        public static int GetActiveLicenseIDByPersonID(int PersonID, int LicenseClassID)
        {

            return clsLicenseData.GetActiveLicenseIDByPersonID(PersonID, LicenseClassID);

        }

        public static DataTable GetDriverLicenses(int DriverID)
        {
            return clsLicenseData.GetDriverLicenses(DriverID);
        }

        public Boolean IsLicenseExpired()
        {

            return ( this.ExpirationDate < DateTime.Now );

        }

        public static string GetIssueReasonText(enIssueReason IssueReason)
        {

            switch (IssueReason)
            {
                case enIssueReason.FirstTime:
                    return "First Time";
                case enIssueReason.Renew:
                    return "Renew";
                case enIssueReason.DamagedReplacement:
                    return "Replacement for Damaged";
                case enIssueReason.LostReplacement:
                    return "Replacement for Lost";
                default:
                    return "First Time";
            }
        }

        public int Detain(float FineFees,int CreatedByUserID)
        {
            clsDetainedLicense detainedLicense = new clsDetainedLicense();
            detainedLicense.LicenseID = this.LicenseID;
            detainedLicense.DetainDate = DateTime.Now;
            detainedLicense.FineFees = Convert.ToSingle(FineFees);
            detainedLicense.CreatedByUserID = CreatedByUserID;

            if (!detainedLicense.Save())
            {
               
                return -1;
            }

            return detainedLicense.DetainID;

        }

        public bool ReleaseDetainedLicense(int ReleasedByUserID,ref int ApplicationID)
        {

            //charging the fine and lifting the detention are one action: a fine paid
            //against a licence still recorded as detained is money taken for nothing.
            //Both writes go to the data access layer as a single call and share a
            //transaction there, and nothing here is assigned until it returns.
            clsDetainedLicenseData.CreateReleaseForDetainedLicense(
                this.DetainedInfo.DetainID,
                this.DriverInfo.PersonID, DateTime.Now,
                (int)clsApplication.enApplicationType.ReleaseDetainedDrivingLicsense,
                (byte)clsApplication.enApplicationStatus.Completed, DateTime.Now,
                clsApplicationType.Find((int)clsApplication.enApplicationType.ReleaseDetainedDrivingLicsense).Fees,
                ReleasedByUserID,
                out int NewApplicationID);

            if (NewApplicationID == -1)
            {
                ApplicationID = -1;
                return false;
            }

            ApplicationID = NewApplicationID;

            return true;

        }

        public clsLicense RenewLicense(string Notes, int CreatedByUserID)
        {

            //raising the application, issuing the renewed licence and retiring this one
            //are a single action: this driver must not be left holding two active
            //licences, nor charged for one that was never issued. The data access layer
            //does all three in one transaction and returns the ids only once it commits.
            int DefaultValidityLength = this.LicenseClassIfo.DefaultValidityLength;

            clsLicenseData.ReissueLicense(
                this.LicenseID,
                this.DriverInfo.PersonID, DateTime.Now,
                (int)clsApplication.enApplicationType.RenewDrivingLicense,
                (byte)clsApplication.enApplicationStatus.Completed, DateTime.Now,
                clsApplicationType.Find((int)clsApplication.enApplicationType.RenewDrivingLicense).Fees,
                this.DriverID, this.LicenseClass,
                DateTime.Now, DateTime.Now.AddYears(DefaultValidityLength), Notes,
                this.LicenseClassIfo.ClassFees, (byte)clsLicense.enIssueReason.Renew,
                CreatedByUserID,
                out int NewApplicationID, out int NewLicenseID);

            if (NewApplicationID == -1 || NewLicenseID == -1)
                return null;

            //this licence is the one that was just retired.
            this.IsActive = false;

            return clsLicense.Find(NewLicenseID);
        }

        public clsLicense Replace(enIssueReason IssueReason, int CreatedByUserID)
        {


            int ApplicationTypeID = (IssueReason == enIssueReason.DamagedReplacement) ?
                (int)clsApplication.enApplicationType.ReplaceDamagedDrivingLicense :
                (int)clsApplication.enApplicationType.ReplaceLostDrivingLicense;

            //a replacement follows the same shape as a renewal: raise the application,
            //issue the licence, retire the one it replaces, as one action. It differs
            //only in what is passed - the replacement carries no licence fee and keeps
            //the expiry date of the licence it stands in for.
            clsLicenseData.ReissueLicense(
                this.LicenseID,
                this.DriverInfo.PersonID, DateTime.Now, ApplicationTypeID,
                (byte)clsApplication.enApplicationStatus.Completed, DateTime.Now,
                clsApplicationType.Find(ApplicationTypeID).Fees,
                this.DriverID, this.LicenseClass,
                DateTime.Now, this.ExpirationDate, this.Notes,
                0, (byte)IssueReason,
                CreatedByUserID,
                out int NewApplicationID, out int NewLicenseID);

            if (NewApplicationID == -1 || NewLicenseID == -1)
                return null;

            //this licence is the one that was just retired.
            this.IsActive = false;

            return clsLicense.Find(NewLicenseID);
        }

    }
}
