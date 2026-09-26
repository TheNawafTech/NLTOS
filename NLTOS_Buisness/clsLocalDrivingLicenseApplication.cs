using System;
using System.Data;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Xml.Linq;
using NLTOS_DataAccess;
using static System.Net.Mime.MediaTypeNames;
using static NLTOS_Buisness.clsTestType;


namespace NLTOS_Buisness
{
    public   class clsLocalDrivingLicenseApplication : clsApplication

    {

        public int LocalDrivingLicenseApplicationID { set; get; }
        public int LicenseClassID { set; get; }
        public clsLicenseClass LicenseClassInfo;
        public string PersonFullName   
        {
            get { 
                return clsPerson.Find(ApplicantPersonID).FullName; 
            }   
            
        }

        public clsLocalDrivingLicenseApplication()

        {
            this.LocalDrivingLicenseApplicationID = -1;
            this.LicenseClassID = -1;
            
           
            Mode = enMode.AddNew;

        }

        private clsLocalDrivingLicenseApplication(int LocalDrivingLicenseApplicationID, int ApplicationID, int ApplicantPersonID, 
            DateTime ApplicationDate, int ApplicationTypeID,
             enApplicationStatus ApplicationStatus, DateTime LastStatusDate,
             float PaidFees, int CreatedByUserID, int LicenseClassID)

        {
            this.LocalDrivingLicenseApplicationID= LocalDrivingLicenseApplicationID; ;
            this.ApplicationID = ApplicationID;
            this.ApplicantPersonID = ApplicantPersonID;
            this.ApplicationDate = ApplicationDate;
            this.ApplicationTypeID = (int) ApplicationTypeID;
            this.ApplicationStatus = ApplicationStatus;
            this.LastStatusDate = LastStatusDate;
            this.PaidFees = PaidFees;
            this.CreatedByUserID = CreatedByUserID;
            this.LicenseClassID = LicenseClassID;
            this.LicenseClassInfo = clsLicenseClass.Find(LicenseClassID);
            Mode = enMode.Update;
        }

        public static clsLocalDrivingLicenseApplication  FindByLocalDrivingAppLicenseID(int LocalDrivingLicenseApplicationID)
        {
            // 
            int ApplicationID=-1, LicenseClassID=-1;

            bool IsFound = clsLocalDrivingLicenseApplicationData.GetLocalDrivingLicenseApplicationInfoByID
                (LocalDrivingLicenseApplicationID, ref ApplicationID, ref LicenseClassID);


            if (IsFound)
            { 
               //now we find the base application
                clsApplication Application = clsApplication.FindBaseApplication(ApplicationID);

                //we return new object of that person with the right data
                return new clsLocalDrivingLicenseApplication(
                    LocalDrivingLicenseApplicationID, Application.ApplicationID, 
                    Application.ApplicantPersonID,
                                     Application.ApplicationDate, Application.ApplicationTypeID,
                                    (enApplicationStatus)Application.ApplicationStatus, Application.LastStatusDate,
                                     Application.PaidFees, Application.CreatedByUserID,LicenseClassID);
            }
            else
                return null;
          

        }


        public bool  Save()
        {
          
            switch (Mode)
            {
                case enMode.AddNew:

                    //the application and the record naming its licence class are created
                    //together, so they go to the data access layer as one call and share a
                    //transaction there. Nothing here is assigned until it returns, so a
                    //failure cannot leave this object holding the id of a row that was
                    //rolled back, or believing it has already been saved.
                    clsLocalDrivingLicenseApplicationData.CreateLocalDrivingLicenseApplication(
                        this.ApplicantPersonID, this.ApplicationDate, this.ApplicationTypeID,
                        (byte)this.ApplicationStatus, this.LastStatusDate, this.PaidFees,
                        this.CreatedByUserID, this.LicenseClassID,
                        out int newApplicationID, out int newLocalApplicationID);

                    if (newApplicationID == -1 || newLocalApplicationID == -1)
                        return false;

                    this.ApplicationID = newApplicationID;
                    this.LocalDrivingLicenseApplicationID = newLocalApplicationID;

                    Mode = enMode.Update;

                    return true;

                case enMode.Update:

                    //the application row and the row naming its licence class describe one
                    //application between them, so they are updated in one call that shares
                    //a transaction in the data access layer, rather than saving the base
                    //application first and the licence class after it. Updating only the
                    //base application would leave its fees, status or applicant no longer
                    //agreeing with the licence class being applied for.
clsLocalDrivingLicenseApplicationData.UpdateLocalDrivingLicenseApplicationAndApplication(
                        this.LocalDrivingLicenseApplicationID, this.LicenseClassID,
                        this.ApplicationID, this.ApplicantPersonID, this.ApplicationDate,
                        this.ApplicationTypeID, (byte)this.ApplicationStatus,
                        this.LastStatusDate, this.PaidFees, this.CreatedByUserID);

                    return true;

            }

            return false;
        }

        public static DataTable GetAllLocalDrivingLicenseApplications()
        {
            return clsLocalDrivingLicenseApplicationData.GetAllLocalDrivingLicenseApplications();
        }

        public  bool Delete()
        {
            //the two rows describe one application between them, so they are removed
            //together inside one transaction rather than one after the other. Deleting
            //the child row alone would leave an application nothing refers to.
            clsLocalDrivingLicenseApplicationData.DeleteLocalDrivingLicenseApplicationAndApplication(
                this.LocalDrivingLicenseApplicationID, this.ApplicationID);

            return true;

        }

        public bool DoesPassTestType(clsTestType.enTestType  TestTypeID)

        {
            return clsLocalDrivingLicenseApplicationData.DoesPassTestType( this.LocalDrivingLicenseApplicationID,(int) TestTypeID);
        }


        public static bool DoesPassTestType(int LocalDrivingLicenseApplicationID, clsTestType.enTestType TestTypeID)

        {
            return clsLocalDrivingLicenseApplicationData.DoesPassTestType(LocalDrivingLicenseApplicationID,(int) TestTypeID);
        }

        public  bool DoesAttendTestType( clsTestType.enTestType TestTypeID)

        {
            return clsLocalDrivingLicenseApplicationData.DoesAttendTestType(this.LocalDrivingLicenseApplicationID, (int)TestTypeID);
        }

        public  byte TotalTrialsPerTest( clsTestType.enTestType TestTypeID)
        {
            return clsLocalDrivingLicenseApplicationData.TotalTrialsPerTest(this.LocalDrivingLicenseApplicationID, (int)TestTypeID);
        }

        public static byte TotalTrialsPerTest(int LocalDrivingLicenseApplicationID, clsTestType.enTestType TestTypeID)

        {
            return clsLocalDrivingLicenseApplicationData.TotalTrialsPerTest(LocalDrivingLicenseApplicationID, (int)TestTypeID);
        }



        public static bool IsThereAnActiveScheduledTest(int LocalDrivingLicenseApplicationID, clsTestType.enTestType TestTypeID)

        {
            
            return clsLocalDrivingLicenseApplicationData.IsThereAnActiveScheduledTest(LocalDrivingLicenseApplicationID, (int)TestTypeID);
        }

        public  bool IsThereAnActiveScheduledTest( clsTestType.enTestType TestTypeID)

        {

            return clsLocalDrivingLicenseApplicationData.IsThereAnActiveScheduledTest(this.LocalDrivingLicenseApplicationID, (int)TestTypeID);
        }

        public clsTest GetLastTestPerTestType(clsTestType.enTestType TestTypeID)
        {
            return clsTest.FindLastTestPerPersonAndLicenseClass(this.ApplicantPersonID, this.LicenseClassID, TestTypeID);
        }

        public byte GetPassedTestCount()
        {
            return clsTest.GetPassedTestCount(this.LocalDrivingLicenseApplicationID);
        }

        public static byte GetPassedTestCount(int LocalDrivingLicenseApplicationID)
        {
            return clsTest.GetPassedTestCount(LocalDrivingLicenseApplicationID);
        }

        public  bool PassedAllTests()
        {
             return clsTest.PassedAllTests(this.LocalDrivingLicenseApplicationID);
        }

        public static bool PassedAllTests(int LocalDrivingLicenseApplicationID)
        {
            //if total passed test less than 3 it will return false otherwise will return true
            return clsTest.PassedAllTests (LocalDrivingLicenseApplicationID) ;
        }
        
        public int IssueLicenseForTheFirtTime(string Notes, int CreatedByUserID)
        {
            //a person has one driver record however many licences they go on to hold,
            //so an existing one is reused and a new one only created when there is none.
            clsDriver Driver = clsDriver.FindByPersonID(this.ApplicantPersonID);
            int ExistingDriverID = (Driver == null) ? -1 : Driver.DriverID;

            //creating the driver, issuing the licence and completing the application are
            //one action, so they go to the data access layer as a single call and share a
            //transaction there. Nothing here is assigned until it returns, so a failure
            //cannot leave this object describing a licence that was rolled back.
            clsLicenseData.IssueFirstLicense(
                ExistingDriverID, this.ApplicantPersonID,
                this.ApplicationID, this.LicenseClassID,
                DateTime.Now, DateTime.Now.AddYears(this.LicenseClassInfo.DefaultValidityLength),
                Notes, this.LicenseClassInfo.ClassFees, true,
                (byte)clsLicense.enIssueReason.FirstTime, CreatedByUserID,
                out int NewDriverID, out int NewLicenseID);

            if (NewDriverID == -1 || NewLicenseID == -1)
                return -1;

            this.ApplicationStatus = enApplicationStatus.Completed;
            this.LastStatusDate = DateTime.Now;

            return NewLicenseID;
        }

        public bool IsLicenseIssued()
        {
            return (GetActiveLicenseID() !=-1);
        }

        public int GetActiveLicenseID()
        {//this will get the license id that belongs to this application
            return  clsLicense.GetActiveLicenseIDByPersonID(this.ApplicantPersonID, this.LicenseClassID);
        }
    }
}
