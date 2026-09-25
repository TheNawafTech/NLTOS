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

namespace NLTOS_DataAccess
{
    public class clsDriverData
    {

        public static bool GetDriverInfoByDriverID(int DriverID, 
            ref int PersonID,ref int CreatedByUserID,ref DateTime CreatedDate )
            {
                bool isFound = false;

                SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);

                string query = "SELECT * FROM Drivers WHERE DriverID = @DriverID";

                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@DriverID", DriverID);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {

                        // The record was found
                        isFound = true;

                    PersonID = (int)reader["PersonID"];
                    CreatedByUserID = (int)reader["CreatedByUserID"];
                    CreatedDate = (DateTime)reader["CreatedDate"];


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

        public static bool GetDriverInfoByPersonID(int PersonID,ref int DriverID,
            ref int CreatedByUserID,ref DateTime CreatedDate)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);

            string query = "SELECT * FROM Drivers WHERE PersonID = @PersonID";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@PersonID", PersonID);

            try
            {
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {

                    // The record was found
                    isFound = true;

                    DriverID = (int)reader["DriverID"];
                    CreatedByUserID = (int)reader["CreatedByUserID"];
                    CreatedDate = (DateTime)reader["CreatedDate"];

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

        public static DataTable GetAllDrivers()
            {

                DataTable dt = new DataTable();
                SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);

                string query = "SELECT * FROM Drivers_View order by FullName";
           
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

        public static int AddNewDriver( int PersonID, int CreatedByUserID)
        {
            int DriverID = -1;

            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);

            try
            {
                connection.Open();

                DriverID = InsertDriver(connection, null, PersonID, CreatedByUserID);
            }

            finally
            {
                connection.Close();
            }


            return DriverID;

        }

        /// <summary>
        /// Inserts a driver on a connection the caller already owns, returning the new
        /// identity or -1. Opens nothing, closes nothing, commits nothing and handles no
        /// exception. A null transaction runs it outside one. Internal deliberately.
        /// </summary>
        internal static int InsertDriver(
            SqlConnection connection, SqlTransaction transaction,
            int PersonID, int CreatedByUserID)
        {
            string query = @"Insert Into Drivers (PersonID,CreatedByUserID,CreatedDate)
                            Values (@PersonID,@CreatedByUserID,@CreatedDate);
                            SELECT SCOPE_IDENTITY();";

            SqlCommand command = new SqlCommand(query, connection);
            command.Transaction = transaction;

            command.Parameters.AddWithValue("@PersonID", PersonID);
            command.Parameters.AddWithValue("@CreatedByUserID", CreatedByUserID);
            command.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

            object result = command.ExecuteScalar();

            if (result != null && int.TryParse(result.ToString(), out int insertedID))
                return insertedID;

            return -1;
        }

        public static bool UpdateDriver(int DriverID, int PersonID, int CreatedByUserID)
        {

            int rowsAffected = 0;
            SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
            //we dont update the createddate for the driver.
            string query = @"Update  Drivers  
                            set PersonID = @PersonID,
                                CreatedByUserID = @CreatedByUserID
                                where DriverID = @DriverID";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@DriverID", DriverID);
            command.Parameters.AddWithValue("@PersonID", PersonID);
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

    }
}
