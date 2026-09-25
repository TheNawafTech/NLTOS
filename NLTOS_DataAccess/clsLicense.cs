using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NLTOS_DataAccess.clsCountryData;
using System.Net;
using System.Security.Policy;
using System.ComponentModel;

namespace NLTOS_DataAccess
{
    public class clsLicenseData
    {

        public static bool GetLicenseInfoByID(int LicenseID,ref int ApplicationID, ref int DriverID, ref int LicenseClass,
            ref DateTime IssueDate, ref DateTime ExpirationDate,ref string Notes,
            ref float PaidFees,ref bool IsActive, ref byte IssueReason, ref int CreatedByUserID)
            {
                bool isFound = false;

                SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);

                string query = "SELECT * FROM Licenses WHERE LicenseID = @LicenseID";

                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@LicenseID", LicenseID);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {

                        // The record was found
                        isFound = true;
                        ApplicationID= (int)reader["ApplicationID"];
                        DriverID  = (int)reader["DriverID"];
                        LicenseClass = (int)reader["LicenseClass"];
                        IssueDate=(DateTime)reader["IssueDate"];
                        ExpirationDate = (DateTime)reader["ExpirationDate"];

                        if (reader["Notes"]==DBNull.Value)
                            Notes = "";
                        else
                            Notes = (string)reader["Notes"];

                        PaidFees = Convert.ToSingle(reader["PaidFees"]);
                        IsActive = (bool)reader["IsActive"];
                        IssueReason = (byte)reader["IssueReason"];
                        CreatedByUserID = (int)reader["CreatedByUserID"];


                }
                    else
                    {
                        // The record was not found
                        isFound = false;
                    }

                    reader.Close();


                }
                finally
                {
                    connection.Close();
                }

                return isFound;
            }

        public static DataTable GetAllLicenses()
            {

                DataTable dt = new DataTable();
                SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);

                string query = "SELECT * FROM Licenses";

                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)

                    {
                        dt.Load(reader);
                    }

                    reader.Close();


                }

                finally
                {
                    connection.Close();
                }

                return dt;

            }

        public static DataTable GetDriverLicenses(int DriverID)
        {

            DataTable dt = new DataTable();
            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);

            string query = @"SELECT     
                           Licenses.LicenseID,
                           ApplicationID,
		                   LicenseClasses.ClassName, Licenses.IssueDate, 
		                   Licenses.ExpirationDate, Licenses.IsActive
                           FROM Licenses INNER JOIN
                                LicenseClasses ON Licenses.LicenseClass = LicenseClasses.LicenseClassID
                            where DriverID=@DriverID
                            Order By IsActive Desc, ExpirationDate Desc";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@DriverID", DriverID);

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)

                {
                    dt.Load(reader);
                }

                reader.Close();


            }

            finally
            {
                connection.Close();
            }

            return dt;

        }

        public static int AddNewLicense(  int ApplicationID, int DriverID,  int LicenseClass,
             DateTime IssueDate,  DateTime ExpirationDate,  string Notes,
             float PaidFees,  bool IsActive,byte IssueReason,  int CreatedByUserID)
        {
            int LicenseID = -1;

            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);

            try
            {
                connection.Open();

                LicenseID = InsertLicense(connection, null,
                    ApplicationID, DriverID, LicenseClass, IssueDate, ExpirationDate,
                    Notes, PaidFees, IsActive, IssueReason, CreatedByUserID);
            }

            finally
            {
                connection.Close();
            }


            return LicenseID;

        }

        /// <summary>
        /// Inserts a licence on a connection the caller already owns, returning the new
        /// identity or -1. Opens nothing, closes nothing, commits nothing and handles no
        /// exception. A null transaction runs it outside one. Internal deliberately.
        /// </summary>
        internal static int InsertLicense(
            SqlConnection connection, SqlTransaction transaction,
            int ApplicationID, int DriverID, int LicenseClass,
            DateTime IssueDate, DateTime ExpirationDate, string Notes,
            float PaidFees, bool IsActive, byte IssueReason, int CreatedByUserID)
        {
            string query = @"
                              INSERT INTO Licenses
                               (ApplicationID,
                                DriverID,
                                LicenseClass,
                                IssueDate,
                                ExpirationDate,
                                Notes,
                                PaidFees,
                                IsActive,IssueReason,
                                CreatedByUserID)
                         VALUES
                               (
                               @ApplicationID,
                               @DriverID,
                               @LicenseClass,
                               @IssueDate,
                               @ExpirationDate,
                               @Notes,
                               @PaidFees,
                               @IsActive,@IssueReason,
                               @CreatedByUserID);
                            SELECT SCOPE_IDENTITY();";

            SqlCommand command = new SqlCommand(query, connection);
            command.Transaction = transaction;

            command.Parameters.AddWithValue("@ApplicationID", ApplicationID);
            command.Parameters.AddWithValue("@DriverID", DriverID);
            command.Parameters.AddWithValue("@LicenseClass", LicenseClass);
            command.Parameters.AddWithValue("@IssueDate", IssueDate);
            command.Parameters.AddWithValue("@ExpirationDate", ExpirationDate);

            if (Notes == "")
                command.Parameters.AddWithValue("@Notes", DBNull.Value);
            else
                command.Parameters.AddWithValue("@Notes", Notes);

            command.Parameters.AddWithValue("@PaidFees", PaidFees);
            command.Parameters.AddWithValue("@IsActive", IsActive);
            command.Parameters.AddWithValue("@IssueReason", IssueReason);
            command.Parameters.AddWithValue("@CreatedByUserID", CreatedByUserID);

            object result = command.ExecuteScalar();

            if (result != null && int.TryParse(result.ToString(), out int insertedID))
                return insertedID;

            return -1;
        }

        /// <summary>
        /// Issues a driver's first licence as one action: the driver record if they do
        /// not have one yet, the licence itself, and the completion of the application
        /// it was issued against, inside a single transaction on a single connection.
        ///
        /// A licence issued against an application still marked incomplete is a licence
        /// the system does not know it granted. Leaving the block without reaching
        /// Commit disposes the transaction, which rolls the work back, so a failure
        /// needs no handling here and travels on unchanged.
        ///
        /// Pass -1 for <paramref name="ExistingDriverID"/> when the person has no driver
        /// record yet and one should be created for them; otherwise the licence is
        /// issued to the driver given.
        ///
        /// The identities are handed back only after the commit, so a caller can never
        /// hold the id of a row that was rolled back.
        /// </summary>
        public static void IssueFirstLicense(
            int ExistingDriverID, int PersonID,
            int ApplicationID, int LicenseClass,
            DateTime IssueDate, DateTime ExpirationDate, string Notes,
            float PaidFees, bool IsActive, byte IssueReason, int CreatedByUserID,
            out int DriverID, out int NewLicenseID)
        {
            int driverID;
            int licenseID;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    driverID = ExistingDriverID != -1
                        ? ExistingDriverID
                        : clsDriverData.InsertDriver(connection, transaction, PersonID, CreatedByUserID);

                    licenseID = InsertLicense(connection, transaction,
                        ApplicationID, driverID, LicenseClass, IssueDate, ExpirationDate,
                        Notes, PaidFees, IsActive, IssueReason, CreatedByUserID);

                    // Completing the application is part of issuing the licence, and an
                    // update that changes nothing is a silent way of not doing it. The
                    // row is expected to be there, so if it is not, the licence is not
                    // granted either.
                    int rowsCompleted = clsApplicationData.UpdateApplicationStatus(
                        connection, transaction, ApplicationID, 3);

                    if (rowsCompleted != 1)
                        throw new InvalidOperationException(
                            "The application the licence was issued against could not be completed.");

                    transaction.Commit();
                }
            }

            DriverID = driverID;
            NewLicenseID = licenseID;
        }

        /// <summary>
        /// Marks a licence inactive on a connection the caller already owns, and returns
        /// how many rows that changed, so a caller inside a transaction can tell a
        /// licence that was retired from one that was not there to retire.
        ///
        /// Opens nothing, closes nothing, commits nothing and handles no exception.
        /// Internal deliberately.
        /// </summary>
        internal static int DeactivateLicense(
            SqlConnection connection, SqlTransaction transaction, int LicenseID)
        {
            string query = @"UPDATE Licenses
                           SET
                              IsActive = 0
                         WHERE LicenseID=@LicenseID";

            SqlCommand command = new SqlCommand(query, connection);
            command.Transaction = transaction;

            command.Parameters.AddWithValue("@LicenseID", LicenseID);

            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Replaces a licence with a new one: the application that authorises it, the
        /// new licence, and the retirement of the one it supersedes, inside a single
        /// transaction on a single connection.
        ///
        /// Both renewing and replacing a licence follow this shape, and differ only in
        /// the application type, the fee and the dates the caller passes. Leaving two
        /// licences active for the same driver, or taking a fee for one that was never
        /// issued, are states the workflow should never reach. Leaving the block without
        /// reaching Commit disposes the transaction, which rolls the work back, so a
        /// failure needs no handling here and travels on unchanged.
        ///
        /// Retiring the old licence matches no row if it is not there, which raises no
        /// error of its own, so the row count is checked rather than assumed.
        ///
        /// The identities are handed back only after the commit.
        /// </summary>
        public static void ReissueLicense(
            int OldLicenseID,
            int ApplicantPersonID, DateTime ApplicationDate, int ApplicationTypeID,
            byte ApplicationStatus, DateTime LastStatusDate, float ApplicationFees,
            int DriverID, int LicenseClass,
            DateTime IssueDate, DateTime ExpirationDate, string Notes,
            float LicenseFees, byte IssueReason, int CreatedByUserID,
            out int NewApplicationID, out int NewLicenseID)
        {
            int applicationID;
            int licenseID;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    applicationID = clsApplicationData.InsertApplication(connection, transaction,
                        ApplicantPersonID, ApplicationDate, ApplicationTypeID,
                        ApplicationStatus, LastStatusDate, ApplicationFees, CreatedByUserID);

                    licenseID = InsertLicense(connection, transaction,
                        applicationID, DriverID, LicenseClass, IssueDate, ExpirationDate,
                        Notes, LicenseFees, true, IssueReason, CreatedByUserID);

                    int rowsRetired = DeactivateLicense(connection, transaction, OldLicenseID);

                    if (rowsRetired != 1)
                        throw new InvalidOperationException(
                            "The licence being replaced could not be retired, so the new one was not issued.");

                    transaction.Commit();
                }
            }

            NewApplicationID = applicationID;
            NewLicenseID = licenseID;
        }

        public static bool UpdateLicense(int LicenseID ,int ApplicationID, int DriverID, int LicenseClass,
             DateTime IssueDate, DateTime ExpirationDate, string Notes,
             float PaidFees, bool IsActive,byte IssueReason, int CreatedByUserID)
        {

            int rowsAffected = 0;
            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);

            string query = @"UPDATE Licenses
                           SET ApplicationID=@ApplicationID, DriverID = @DriverID,
                              LicenseClass = @LicenseClass,
                              IssueDate = @IssueDate,
                              ExpirationDate = @ExpirationDate,
                              Notes = @Notes,
                              PaidFees = @PaidFees,
                              IsActive = @IsActive,IssueReason=@IssueReason,
                              CreatedByUserID = @CreatedByUserID
                         WHERE LicenseID=@LicenseID";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@LicenseID", LicenseID);
            command.Parameters.AddWithValue("@ApplicationID", ApplicationID);
            command.Parameters.AddWithValue("@DriverID", DriverID);
            command.Parameters.AddWithValue("@LicenseClass", LicenseClass);
            command.Parameters.AddWithValue("@IssueDate", IssueDate);
            command.Parameters.AddWithValue("@ExpirationDate", ExpirationDate);
            
            if (Notes=="")
                command.Parameters.AddWithValue("@Notes", DBNull.Value );
            else
                command.Parameters.AddWithValue("@Notes", Notes);

            command.Parameters.AddWithValue("@PaidFees", PaidFees);
            command.Parameters.AddWithValue("@IsActive", IsActive);
            command.Parameters.AddWithValue("@IssueReason", IssueReason);
            command.Parameters.AddWithValue("@CreatedByUserID", CreatedByUserID);

            try
            {
                connection.Open();
                rowsAffected = command.ExecuteNonQuery();

            }
            finally
            {
                connection.Close();
            }

            return (rowsAffected > 0);
        }

        public static int GetActiveLicenseIDByPersonID(int PersonID,int LicenseClassID)
        {
            int LicenseID = -1;

            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);

            string query = @"SELECT        Licenses.LicenseID
                            FROM Licenses INNER JOIN
                                                     Drivers ON Licenses.DriverID = Drivers.DriverID
                            WHERE  
                             
                             Licenses.LicenseClass = @LicenseClass 
                              AND Drivers.PersonID = @PersonID
                              And IsActive=1;";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@PersonID", PersonID);
            command.Parameters.AddWithValue("@LicenseClass", LicenseClassID);

            try
            {
                connection.Open();

                object result = command.ExecuteScalar();

                if (result != null && int.TryParse(result.ToString(), out int insertedID))
                {
                    LicenseID = insertedID;
                }
            }

            finally
            {
                connection.Close();
            }


            return LicenseID;
        }

        public static bool DeactivateLicense(int LicenseID)
        {

            int rowsAffected = 0;
            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);

            try
            {
                connection.Open();

                rowsAffected = DeactivateLicense(connection, null, LicenseID);
            }
            finally
            {
                connection.Close();
            }

            return (rowsAffected > 0);
        }

    }
}
