using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

namespace TimeManager.Models
{
    public partial class PortalUser
    {
        #region Constructors
        /// <summary>
        /// Blank constructor
        /// </summary>
        public PortalUser()
        {
            Initialize();
        }

        /// <summary>
        /// Load a specific user
        /// </summary>
        /// <param name="userid">User ID</param>
        public PortalUser(string userid)
        {
            Initialize();

            DataSet ds = new DataSet();
            SqlDataAdapter da;
            using (SqlConnection conn = new SqlConnection(PortalUtility.GetConnectionString("default")))
            {
                //Just using the UserSearch stored proc at this time to make managing SQL queries easier
                SqlCommand cmd = new SqlCommand("UserSearch_Sel", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserID", userid);
                da = new SqlDataAdapter(cmd);
                da.Fill(ds, "Users");

                if (ds.Tables["Users"].Rows.Count > 0)
                { LoadDataRow(ds.Tables["Users"].Rows[0]); }
            }
        }

        /// <summary>
        /// Load from a DataRow
        /// </summary>
        /// <param name="dr">DataRow</param>
        public PortalUser(DataRow dr)
        {
            Initialize();
            LoadDataRow(dr);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="username">UserName</param>
        /// <param name="password">Password</param>
        public static void CreateNewUser(string username, string password)
        {
            string result = String.Empty;

            string encryptionseed = Guid.NewGuid().ToString();
            byte[] passwordhash = PortalUtility.HashString(encryptionseed, password);

            using (SqlConnection conn = new SqlConnection(PortalUtility.GetConnectionString("default")))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("User_Ins", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserName", username);
                cmd.Parameters.AddWithValue("@Password", passwordhash);
                cmd.Parameters.AddWithValue("@EncryptionSeed", encryptionseed);
                result = Convert.ToString(cmd.ExecuteScalar());

                conn.Close();
            }

            if (result == "EXISTING")
            { throw new Exception("Existing User"); }
        }

        /// <summary>
        /// Get the daily hours total for a user
        /// </summary>
        /// <param name="userid">UserID</param>
        /// <param name="taskdate">Task Date</param>
        /// <returns>hours total for specified date</returns>
        public static int GetDailyHours(string userid, DateTime taskdate)
        {
            int returnval = 0;

            using (SqlConnection conn = new SqlConnection(PortalUtility.GetConnectionString("default")))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("DailyHours_Sel", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserID", userid);
                cmd.Parameters.AddWithValue("@TaskDate", taskdate.Date);
                returnval = Convert.ToInt32(cmd.ExecuteScalar());

                conn.Close();
            }

            return returnval;
        }

        /// <summary>
        /// Get a list of all users
        /// </summary>
        /// <returns></returns>
        public static List<PortalUser> GetAllUsers()
        {
            List<PortalUser> returnval = new List<PortalUser>();

            DataSet ds = new DataSet();
            SqlDataAdapter da;
            using (SqlConnection conn = new SqlConnection(PortalUtility.GetConnectionString("default")))
            {
                SqlCommand cmd = new SqlCommand("UserSearch_Sel", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                da = new SqlDataAdapter(cmd);
                da.Fill(ds, "Users");

                foreach (DataRow dr in ds.Tables["Users"].Rows)
                { returnval.Add(new PortalUser(dr)); }
            }

            return returnval;
        }

        /// <summary>
        /// Update user data.  This method is only for existing users.  New users must be registered with CreateNewUser first.
        /// </summary>
        public void Save()
        {
            if (!String.IsNullOrEmpty(this.UserID))
            {
                string result = String.Empty;

                using (SqlConnection conn = new SqlConnection(PortalUtility.GetConnectionString("default")))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("User_Upd", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", this.UserID);
                    cmd.Parameters.AddWithValue("@UserName", this.UserName);
                    cmd.Parameters.AddWithValue("@IsActive", this.IsActive);
                    cmd.Parameters.AddWithValue("@IsManager", this.IsManager);
                    cmd.Parameters.AddWithValue("@IsAdmin", this.IsAdmin);
                    result = Convert.ToString(cmd.ExecuteScalar());

                    conn.Close();
                }

                if (result == "EXISTING")
                { throw new Exception("Existing User"); }
            }
            else
            {
                throw new Exception("UserID is required");
            }
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        public void Delete()
        {
            using (SqlConnection conn = new SqlConnection(PortalUtility.GetConnectionString("default")))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("User_Del", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserID", this.UserID);
                cmd.ExecuteNonQuery();

                conn.Close();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initialize class properties
        /// </summary>
        private void Initialize()
        {
            this.UserID = String.Empty;
            this.UserName = String.Empty;
            this.Password = new byte[64];
            this.EncryptionSeed = String.Empty;
            this.IsActive = false;
            this.IsManager = false;
            this.IsAdmin = false;
            this.Added = null;
            this.LastModified = null;
            this.UserSettings = new List<UserSetting>();
            //this.Meals = new List<Meal>();
        }

        /// <summary>
        /// Load class from DataRow
        /// </summary>
        /// <param name="dr">DataRow</param>
        private void LoadDataRow(DataRow dr)
        {
            this.UserID = dr["UserID"].ToString();
            this.UserName = dr["UserName"].ToString();
            this.IsActive = Convert.ToBoolean(dr["IsActive"]);
            this.IsManager = Convert.ToBoolean(dr["IsManager"]);
            this.IsAdmin = Convert.ToBoolean(dr["IsAdmin"]);
        }

        #endregion
    }
}