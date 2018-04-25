using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

namespace TimeManager.Models
{
    public partial class UserSetting
    {
        #region Constructors

        /// <summary>
        /// Blank constructor
        /// </summary>
        public UserSetting()
        {
            Initialize();
        }

        /// <summary>
        /// Create a user setting object
        /// </summary>
        /// <param name="userid">UserID</param>
        /// <param name="code">Setting Code</param>
        /// <param name="value">Setting Value</param>
        public UserSetting(string userid, string code, string value)
        {
            Initialize();

            this.UserID = userid;
            this.SettingCode = code;
            this.SettingValue = value;
        }

        public UserSetting(DataRow dr)
        {
            Initialize();
            LoadDataRow(dr);
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Add/Update user setting
        /// </summary>
        public void Save()
        {
            Save(this.UserID, this.SettingCode, this.SettingValue);
        }

        /// <summary>
        /// Static save method to add/update a user setting
        /// </summary>
        /// <param name="userid">UserID</param>
        /// <param name="code">Setting Code</param>
        /// <param name="value">Setting Value</param>
        public static void Save(string userid, string code, string value)
        {
            string result = String.Empty;

            try
            {
                using (SqlConnection conn = new SqlConnection(PortalUtility.GetConnectionString("default")))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("UserSetting_Upd", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", userid);
                    cmd.Parameters.AddWithValue("@SettingCode", code);
                    cmd.Parameters.AddWithValue("@SettingValue", value);
                    cmd.ExecuteNonQuery();

                    conn.Close();
                }
            }
            catch (Exception ex)
            { throw ex; }
        }

        /// <summary>
        /// Get a setting value for a user
        /// </summary>
        /// <param name="userid">UserID</param>
        /// <param name="code">Setting Code</param>
        /// <returns></returns>
        public static string GetSettingValue(string userid, string code)
        {
            string returnval = String.Empty;

            try
            {
                using (SqlConnection conn = new SqlConnection(PortalUtility.GetConnectionString("default")))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("UserSetting_Sel", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", userid);
                    cmd.Parameters.AddWithValue("@SettingCode", code);
                    returnval = Convert.ToString(cmd.ExecuteScalar());

                    conn.Close();
                }
            }
            catch (Exception ex)
            { throw ex; }

            return returnval;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initialize class properties
        /// </summary>
        private void Initialize()
        {
            this.UserID = String.Empty;
            this.SettingCode = String.Empty;
            this.SettingValue = String.Empty;
        }

        /// <summary>
        /// Load class from data row
        /// </summary>
        /// <param name="dr">DataRow</param>
        private void LoadDataRow(DataRow dr)
        {
            this.UserID = dr["UserID"].ToString();
            this.SettingCode = dr["SettingCode"].ToString();
            this.SettingValue = dr["SettingValue"].ToString();
        }

        #endregion
    }
}