using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.InteropServices.WindowsRuntime;

namespace NLTOS_DataAccess
{
    public class clsLocalDrivingLicenseApplicationData
    {
      
        public static bool GetLocalDrivingLicenseApplicationInfoByID(
            int LocalDrivingLicenseApplicationID, ref int ApplicationID, 
            ref int LicenseClassID)
            {
                bool isFound = false;

                SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);


                string query = "SELECT * FROM LocalDrivingLicenseApplications WHERE LocalDrivingLicenseApplicationID = @LocalDrivingLicenseApplicationID";

                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@LocalDrivingLicenseApplicationID", LocalDrivingLicenseApplicationID);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {

                        // The record was found
                        isFound = true;

                    ApplicationID = (int)reader["ApplicationID"];
                    LicenseClassID = (int)reader["LicenseClassID"];



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

        public static bool GetLocalDrivingLicenseApplicationInfoByApplicationID(
         int ApplicationID, ref int LocalDrivingLicenseApplicationID, 
         ref int LicenseClassID)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);

            string query = "SELECT * FROM LocalDrivingLicenseApplications WHERE ApplicationID = @ApplicationID";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@ApplicationID", ApplicationID);

            try
            {
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {

                    // The record was found
                    isFound = true;

                    LocalDrivingLicenseApplicationID = (int)reader["LocalDrivingLicenseApplicationID"];
                    LicenseClassID = (int)reader["LicenseClassID"];

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

        public static DataTable GetAllLocalDrivingLicenseApplications()
            {

                DataTable dt = new DataTable();
                SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);

            string query = @"SELECT *
                              FROM LocalDrivingLicenseApplications_View
                              order by ApplicationDate Desc";


          

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

        /// <summary>
        /// Inserts a local driving licence application on a connection the caller
        /// already owns, returning the new identity or -1.
        ///
        /// Opens nothing, closes nothing, commits nothing and handles no exception.
        /// A null transaction runs it outside one. Internal deliberately, since
        /// connections and transactions do not leave this layer.
        /// </summary>
        internal static int InsertLocalDrivingLicenseApplication(
            SqlConnection connection, SqlTransaction transaction,
            int ApplicationID, int LicenseClassID)
        {
            string query = @"INSERT INTO LocalDrivingLicenseApplications (
                            ApplicationID,LicenseClassID)
                             VALUES (@ApplicationID,@LicenseClassID);
                             SELECT SCOPE_IDENTITY();";

            SqlCommand command = new SqlCommand(query, connection);
            command.Transaction = transaction;

            command.Parameters.AddWithValue("ApplicationID", ApplicationID);
            command.Parameters.AddWithValue("LicenseClassID", LicenseClassID);

            object result = command.ExecuteScalar();

            if (result != null && int.TryParse(result.ToString(), out int insertedID))
                return insertedID;

            return -1;
        }

        /// <summary>
        /// Creates a local driving licence application: the application itself and the
        /// record that says which licence class it is for, inside a single transaction
        /// on a single connection.
        ///
        /// An application with no class attached to it is not something the rest of the
        /// system can work with. Leaving the block without reaching Commit disposes the
        /// transaction, which rolls the work back, so a failure needs no handling here
        /// and travels on unchanged.
        ///
        /// The identities are handed back only after the commit, so a caller can never
        /// hold the id of a row that was rolled back.
        /// </summary>
        public static void CreateLocalDrivingLicenseApplication(
            int ApplicantPersonID, DateTime ApplicationDate, int ApplicationTypeID,
            byte ApplicationStatus, DateTime LastStatusDate, float PaidFees,
            int CreatedByUserID, int LicenseClassID,
            out int NewApplicationID, out int NewLocalDrivingLicenseApplicationID)
        {
            int applicationID;
            int localApplicationID;

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    applicationID = clsApplicationData.InsertApplication(connection, transaction,
                        ApplicantPersonID, ApplicationDate, ApplicationTypeID,
                        ApplicationStatus, LastStatusDate, PaidFees, CreatedByUserID);

                    localApplicationID = InsertLocalDrivingLicenseApplication(
                        connection, transaction, applicationID, LicenseClassID);

                    transaction.Commit();
                }
            }

            NewApplicationID = applicationID;
            NewLocalDrivingLicenseApplicationID = localApplicationID;
        }

        /// <summary>
        /// Updates a local driving licence application row on a connection the caller
        /// already owns, and returns how many rows that changed, so a caller inside a
        /// transaction can tell a row that was updated from one that was not there to
        /// update.
        ///
        /// Opens nothing, closes nothing, commits nothing and handles no exception.
        /// A null transaction runs it outside one. Internal deliberately.
        /// </summary>
        internal static int UpdateLocalDrivingLicenseApplication(
            SqlConnection connection, SqlTransaction transaction,
            int LocalDrivingLicenseApplicationID, int ApplicationID, int LicenseClassID)
        {
            string query = @"Update  LocalDrivingLicenseApplications  
                            set ApplicationID = @ApplicationID,
                                LicenseClassID = @LicenseClassID
                            where LocalDrivingLicenseApplicationID=@LocalDrivingLicenseApplicationID";

            SqlCommand command = new SqlCommand(query, connection);
            command.Transaction = transaction;

            command.Parameters.AddWithValue("@LocalDrivingLicenseApplicationID", LocalDrivingLicenseApplicationID);
            command.Parameters.AddWithValue("@ApplicationID", ApplicationID);
            command.Parameters.AddWithValue("@LicenseClassID", LicenseClassID);

            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Updates a local driving licence application together with the application it
        /// belongs to, inside a single transaction on a single connection.
        ///
        /// The two rows describe one application between them, so changing one without
        /// the other leaves an application whose fees, status or applicant no longer
        /// agree with the licence class being applied for. Leaving the block without
        /// reaching Commit disposes the transaction, which rolls both updates back, so a
        /// failure needs no handling here and travels on unchanged.
        ///
        /// An update that matches no row changes nothing and raises nothing, so each
        /// step is checked by the number of rows it changed: if either row is not there,
        /// neither is updated.
        /// </summary>
        public static void UpdateLocalDrivingLicenseApplicationAndApplication(
            int LocalDrivingLicenseApplicationID, int LicenseClassID,
            int ApplicationID, int ApplicantPersonID, DateTime ApplicationDate, int ApplicationTypeID,
            byte ApplicationStatus, DateTime LastStatusDate, float PaidFees, int CreatedByUserID)
        {
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    int applicationRowsUpdated = clsApplicationData.UpdateApplication(connection, transaction,
                        ApplicationID, ApplicantPersonID, ApplicationDate, ApplicationTypeID,
                        ApplicationStatus, LastStatusDate, PaidFees, CreatedByUserID);

                    if (applicationRowsUpdated != 1)
                        throw new InvalidOperationException(
                            "The application behind the local driving licence application could not be updated.");

                    int localRowsUpdated = UpdateLocalDrivingLicenseApplication(connection, transaction,
                        LocalDrivingLicenseApplicationID, ApplicationID, LicenseClassID);

                    if (localRowsUpdated != 1)
                        throw new InvalidOperationException(
                            "The local driving licence application could not be updated.");

                    transaction.Commit();
                }
            }
        }


        /// <summary>
        /// Deletes a local driving licence application row on a connection the caller
        /// already owns, and returns how many rows that removed.
        ///
        /// Opens nothing, closes nothing, commits nothing and handles no exception.
        /// Internal deliberately.
        /// </summary>
        internal static int DeleteLocalDrivingLicenseApplication(
            SqlConnection connection, SqlTransaction transaction,
            int LocalDrivingLicenseApplicationID)
        {
            string query = @"Delete LocalDrivingLicenseApplications
                                where LocalDrivingLicenseApplicationID = @LocalDrivingLicenseApplicationID";

            SqlCommand command = new SqlCommand(query, connection);
            command.Transaction = transaction;

            command.Parameters.AddWithValue("@LocalDrivingLicenseApplicationID", LocalDrivingLicenseApplicationID);

            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Deletes a local driving licence application together with the application it
        /// belongs to, inside a single transaction on a single connection.
        ///
        /// The two rows describe one thing between them, so removing one without the
        /// other leaves a record the rest of the system cannot make sense of. The child
        /// row goes first because it refers to the application. Leaving the block
        /// without reaching Commit disposes the transaction, which rolls both deletes
        /// back, so a failure needs no handling here and travels on unchanged.
        ///
        /// A delete that matches no row removes nothing and raises nothing, so each
        /// step is checked by the number of rows it removed: if either row is not
        /// there, neither is deleted.
        /// </summary>
        public static void DeleteLocalDrivingLicenseApplicationAndApplication(
            int LocalDrivingLicenseApplicationID, int ApplicationID)
        {
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    int localRowsDeleted = DeleteLocalDrivingLicenseApplication(
                        connection, transaction, LocalDrivingLicenseApplicationID);

                    if (localRowsDeleted != 1)
                        throw new InvalidOperationException(
                            "The local driving licence application could not be deleted.");

                    int applicationRowsDeleted = clsApplicationData.DeleteApplication(
                        connection, transaction, ApplicationID);

                    if (applicationRowsDeleted != 1)
                        throw new InvalidOperationException(
                            "The application behind the local driving licence application could not be deleted.");

                    transaction.Commit();
                }
            }
        }

        public static bool DoesPassTestType( int LocalDrivingLicenseApplicationID, int TestTypeID)

        {
           
             
            bool Result = false;

            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);

            string query = @" SELECT top 1 TestResult
                            FROM LocalDrivingLicenseApplications INNER JOIN
                                 TestAppointments ON LocalDrivingLicenseApplications.LocalDrivingLicenseApplicationID = TestAppointments.LocalDrivingLicenseApplicationID INNER JOIN
                                 Tests ON TestAppointments.TestAppointmentID = Tests.TestAppointmentID
                            WHERE
                            (LocalDrivingLicenseApplications.LocalDrivingLicenseApplicationID = @LocalDrivingLicenseApplicationID) 
                            AND(TestAppointments.TestTypeID = @TestTypeID)
                            ORDER BY TestAppointments.TestAppointmentID desc";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@LocalDrivingLicenseApplicationID", LocalDrivingLicenseApplicationID);
            command.Parameters.AddWithValue("@TestTypeID", TestTypeID);

            try
            {
                connection.Open();

                object result = command.ExecuteScalar();

                if (result != null && bool.TryParse(result.ToString(), out bool returnedResult))
                {
                    Result = returnedResult;
                }
            }

            finally
            {
                connection.Close();
            }

            return Result;

        }

        public static bool DoesAttendTestType(int LocalDrivingLicenseApplicationID, int TestTypeID)

        {


            bool IsFound = false;

            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);

            string query = @" SELECT top 1 Found=1
                            FROM LocalDrivingLicenseApplications INNER JOIN
                                 TestAppointments ON LocalDrivingLicenseApplications.LocalDrivingLicenseApplicationID = TestAppointments.LocalDrivingLicenseApplicationID INNER JOIN
                                 Tests ON TestAppointments.TestAppointmentID = Tests.TestAppointmentID
                            WHERE
                            (LocalDrivingLicenseApplications.LocalDrivingLicenseApplicationID = @LocalDrivingLicenseApplicationID) 
                            AND(TestAppointments.TestTypeID = @TestTypeID)
                            ORDER BY TestAppointments.TestAppointmentID desc";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@LocalDrivingLicenseApplicationID", LocalDrivingLicenseApplicationID);
            command.Parameters.AddWithValue("@TestTypeID", TestTypeID);

            try
            {
                connection.Open();

                object result = command.ExecuteScalar();

                if (result != null )
                {
                    IsFound = true;
                }
            }

            finally
            {
                connection.Close();
            }

            return IsFound;

        }

        public static byte TotalTrialsPerTest(int LocalDrivingLicenseApplicationID, int TestTypeID)

        {


            byte TotalTrialsPerTest = 0;

            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);

            string query = @" SELECT TotalTrialsPerTest = count(TestID)
                            FROM LocalDrivingLicenseApplications INNER JOIN
                                 TestAppointments ON LocalDrivingLicenseApplications.LocalDrivingLicenseApplicationID = TestAppointments.LocalDrivingLicenseApplicationID INNER JOIN
                                 Tests ON TestAppointments.TestAppointmentID = Tests.TestAppointmentID
                            WHERE
                            (LocalDrivingLicenseApplications.LocalDrivingLicenseApplicationID = @LocalDrivingLicenseApplicationID) 
                            AND(TestAppointments.TestTypeID = @TestTypeID)
                       ";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@LocalDrivingLicenseApplicationID", LocalDrivingLicenseApplicationID);
            command.Parameters.AddWithValue("@TestTypeID", TestTypeID);

            try
            {
                connection.Open();

                object result = command.ExecuteScalar();

                if (result != null && byte.TryParse(result.ToString(), out byte Trials))
                {
                    TotalTrialsPerTest = Trials;
                }
            }

            finally
            {
                connection.Close();
            }

            return TotalTrialsPerTest;

        }

        public static bool IsThereAnActiveScheduledTest(int LocalDrivingLicenseApplicationID, int TestTypeID)

        {

            bool Result = false;

            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);

            string query = @" SELECT top 1 Found=1
                            FROM LocalDrivingLicenseApplications INNER JOIN
                                 TestAppointments ON LocalDrivingLicenseApplications.LocalDrivingLicenseApplicationID = TestAppointments.LocalDrivingLicenseApplicationID 
                            WHERE
                            (LocalDrivingLicenseApplications.LocalDrivingLicenseApplicationID = @LocalDrivingLicenseApplicationID)  
                            AND(TestAppointments.TestTypeID = @TestTypeID) and isLocked=0
                            ORDER BY TestAppointments.TestAppointmentID desc";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@LocalDrivingLicenseApplicationID", LocalDrivingLicenseApplicationID);
            command.Parameters.AddWithValue("@TestTypeID", TestTypeID);

            try
            {
                connection.Open();

                object result = command.ExecuteScalar();
             

               if (result != null )
                {
                    Result = true;
                }

            }

            finally
            {
                connection.Close();
            }

            return Result;

        }

    }
}
