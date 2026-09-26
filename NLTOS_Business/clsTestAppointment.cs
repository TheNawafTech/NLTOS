using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Xml.Linq;
using NLTOS_DataAccess;

namespace NLTOS_Business
{
    public class clsTestAppointment
    {

        public enum enMode { AddNew = 0, Update = 1 };
        public enMode Mode = enMode.AddNew;

        public int TestAppointmentID { set; get; }
        public clsTestType.enTestType TestTypeID { set; get; }
        public int LocalDrivingLicenseApplicationID { set; get; }
        public DateTime AppointmentDate { set; get; }
        public float PaidFees { set; get; }
        public int CreatedByUserID { set; get; }
        public bool IsLocked { set; get; }
        public int RetakeTestApplicationID { set; get; }
        public clsApplication RetakeTestAppInfo { set; get; }

        public int  TestID   
        {
            get { return _GetTestID(); }   
          
        }

        public clsTestAppointment()

        {
            this.TestAppointmentID = -1;
            this.TestTypeID = clsTestType.enTestType.VisionTest;
            this.AppointmentDate = DateTime.Now;
            this.PaidFees = 0;
            this.CreatedByUserID = -1;
            this.RetakeTestApplicationID = -1;  
            Mode = enMode.AddNew;

        }

        public clsTestAppointment(int TestAppointmentID, clsTestType.enTestType TestTypeID,
           int LocalDrivingLicenseApplicationID, DateTime AppointmentDate, float PaidFees, 
           int CreatedByUserID ,bool IsLocked, int RetakeTestApplicationID)

        {
            this.TestAppointmentID = TestAppointmentID;
            this.TestTypeID = TestTypeID;
            this.LocalDrivingLicenseApplicationID = LocalDrivingLicenseApplicationID;
            this.AppointmentDate = AppointmentDate;
            this.PaidFees = PaidFees;
            this.CreatedByUserID = CreatedByUserID;
            this.IsLocked = IsLocked;
            this.RetakeTestApplicationID= RetakeTestApplicationID;
            this.RetakeTestAppInfo = clsApplication.FindBaseApplication(RetakeTestApplicationID);
            Mode = enMode.Update;
        }

        private bool _AddNewTestAppointment()
        {
            //call DataAccess Layer 

            this.TestAppointmentID = clsTestAppointmentData.AddNewTestAppointment((int) this.TestTypeID,this.LocalDrivingLicenseApplicationID,
                this.AppointmentDate,this.PaidFees,this.CreatedByUserID,this.RetakeTestApplicationID);

            return (this.TestAppointmentID != -1);
        }

        private bool _UpdateTestAppointment()
        {
            //call DataAccess Layer 

            return clsTestAppointmentData.UpdateTestAppointment(this.TestAppointmentID, (int) this.TestTypeID, this.LocalDrivingLicenseApplicationID,
                this.AppointmentDate, this.PaidFees, this.CreatedByUserID,this.IsLocked,this.RetakeTestApplicationID);
        }

        public static clsTestAppointment Find(int TestAppointmentID)
        {
            int TestTypeID = 1;  int LocalDrivingLicenseApplicationID = -1;
            DateTime AppointmentDate = DateTime.Now;  float PaidFees = 0;  
            int CreatedByUserID = -1; bool IsLocked=false;int RetakeTestApplicationID = -1;

            if (clsTestAppointmentData.GetTestAppointmentInfoByID(TestAppointmentID, ref  TestTypeID, ref  LocalDrivingLicenseApplicationID,
            ref   AppointmentDate, ref  PaidFees, ref  CreatedByUserID, ref IsLocked, ref RetakeTestApplicationID))

                return new clsTestAppointment(TestAppointmentID,  (clsTestType.enTestType) TestTypeID,  LocalDrivingLicenseApplicationID,
             AppointmentDate,  PaidFees,  CreatedByUserID, IsLocked, RetakeTestApplicationID);
            else
                return null;

        }

        public static clsTestAppointment GetLastTestAppointment(int LocalDrivingLicenseApplicationID, clsTestType.enTestType TestTypeID )
        {
             int TestAppointmentID=-1;
            DateTime AppointmentDate = DateTime.Now; float PaidFees = 0;
            int CreatedByUserID = -1;bool IsLocked=false;int RetakeTestApplicationID=-1;

            if (clsTestAppointmentData.GetLastTestAppointment( LocalDrivingLicenseApplicationID, (int) TestTypeID,
                ref TestAppointmentID,ref AppointmentDate, ref PaidFees, ref CreatedByUserID,ref IsLocked,ref RetakeTestApplicationID))

                return new clsTestAppointment(TestAppointmentID, TestTypeID, LocalDrivingLicenseApplicationID,
             AppointmentDate, PaidFees, CreatedByUserID, IsLocked, RetakeTestApplicationID);
            else
                return null;

        }

        public static DataTable GetAllTestAppointments()
        {
            return clsTestAppointmentData.GetAllTestAppointments();

        }

        public  DataTable GetApplicationTestAppointmentsPerTestType(clsTestType.enTestType TestTypeID)
        {
            return clsTestAppointmentData.GetApplicationTestAppointmentsPerTestType(this.LocalDrivingLicenseApplicationID,(int) TestTypeID);

        }

        public static DataTable GetApplicationTestAppointmentsPerTestType(int LocalDrivingLicenseApplicationID,clsTestType.enTestType TestTypeID)
        {
            return clsTestAppointmentData.GetApplicationTestAppointmentsPerTestType(LocalDrivingLicenseApplicationID, (int) TestTypeID);

        }

        /// <summary>
        /// Schedules this appointment as a retake, together with the application that
        /// charges for it.
        ///
        /// A retake is two records that only make sense as a pair, so they are created
        /// by one call to the data access layer and share a transaction there. Nothing
        /// on this object is set until that call returns, so a failure cannot leave it
        /// pointing at an application that was rolled back.
        ///
        /// <paramref name="ApplicantPersonID"/> is the person the retake application is
        /// raised for, and <paramref name="ApplicationFees"/> the fee it carries, which
        /// is charged separately from the fee for the appointment itself.
        /// </summary>
        public bool SaveAsRetake(int ApplicantPersonID, float ApplicationFees)
        {
            if (Mode != enMode.AddNew)
                return false;

            clsTestAppointmentData.CreateRetakeAppointment(
                ApplicantPersonID, DateTime.Now,
                (int)clsApplication.enApplicationType.RetakeTest,
                (byte)clsApplication.enApplicationStatus.Completed, DateTime.Now,
                ApplicationFees,
                (int)this.TestTypeID, this.LocalDrivingLicenseApplicationID,
                this.AppointmentDate, this.PaidFees, this.CreatedByUserID,
                out int newRetakeApplicationID, out int newTestAppointmentID);

            if (newRetakeApplicationID == -1 || newTestAppointmentID == -1)
                return false;

            this.RetakeTestApplicationID = newRetakeApplicationID;
            this.TestAppointmentID = newTestAppointmentID;

            Mode = enMode.Update;

            return true;
        }

        public bool Save()
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    if (_AddNewTestAppointment())
                    {

                        Mode = enMode.Update;
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case enMode.Update:

                    return _UpdateTestAppointment();

            }

            return false;
        }

        private int  _GetTestID()
        {
            return clsTestAppointmentData.GetTestID(TestAppointmentID);
        }

    }
}
