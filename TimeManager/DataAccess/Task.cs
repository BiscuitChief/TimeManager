using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

namespace TimeManager.Models
{
    public partial class Task
    {
        #region Constructors

        /// <summary>
        /// Blank Task constructor
        /// </summary>
        public Task()
        {
            Initialize();
        }

        /// <summary>
        /// Existing Task
        /// </summary>
        /// <param name="recordid">Record ID</param>
        public Task(int recordid)
        {
            Initialize();

            DataSet ds = new DataSet();
            SqlDataAdapter da;
            using (SqlConnection conn = new SqlConnection(PortalUtility.GetConnectionString("default")))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("Task_Sel", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@RecordID", recordid);

                da = new SqlDataAdapter(cmd);
                da.Fill(ds, "Tasks");

                if (ds.Tables["Tasks"].Rows.Count > 0)
                { LoadDataRow(ds.Tables["Tasks"].Rows[0]); }
            }
        }

        /// <summary>
        /// Constructor for if we have a data row, mostly used in loops
        /// </summary>
        /// <param name="dr"></param>
        public Task(DataRow dr)
        {
            Initialize();
            LoadDataRow(dr);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Add/Update a Task
        /// </summary>
        public void Save()
        {
            using (SqlConnection conn = new SqlConnection(PortalUtility.GetConnectionString("default")))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("Task_Upd", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@RecordID", this.RecordID);
                cmd.Parameters.AddWithValue("@UserID", this.UserID);
                cmd.Parameters.AddWithValue("@Notes", this.Notes);
                cmd.Parameters.AddWithValue("@Hours", this.Hours);
                cmd.Parameters.AddWithValue("@TaskDate", this.TaskDate);
                this.RecordID = Convert.ToInt32(cmd.ExecuteScalar());

                conn.Close();
            }
        }

        /// <summary>
        /// Delete a Task
        /// </summary>
        public void Delete()
        {
            Delete(this.RecordID);
        }

        /// <summary>
        /// Search Tasks for a user
        /// </summary>
        /// <param name="userid">UserID</param>
        /// <param name="startdate">Start Date</param>
        /// <param name="enddate">End Date</param>
        /// <returns></returns>
        public static List<Task> SearchTasks(string userid, string startdate, string enddate)
        {
            List<Task> returnval = new List<Task>();

            //validate that the date/time parameters are valid date and time values.
            //Format them correctly to ensure that the SQL query will recognize them
            if (!String.IsNullOrEmpty(startdate))
            { startdate = String.Format("{0:MM/dd/yyyy}", DateTime.Parse(startdate)); }
            if (!String.IsNullOrEmpty(enddate))
            { enddate = String.Format("{0:MM/dd/yyyy}", DateTime.Parse(enddate)); }

            DataSet ds = new DataSet();
            SqlDataAdapter da;
            using (SqlConnection conn = new SqlConnection(PortalUtility.GetConnectionString("default")))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("TaskSearch_Sel", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserID", userid);
                //Only add date/time parameters if they have values
                if (!String.IsNullOrEmpty(startdate))
                { cmd.Parameters.AddWithValue("@StartDate", startdate); }
                if (!String.IsNullOrEmpty(enddate))
                { cmd.Parameters.AddWithValue("@EndDate", enddate); }

                da = new SqlDataAdapter(cmd);
                da.Fill(ds, "Tasks");

                foreach (DataRow dr in ds.Tables["Tasks"].Rows)
                { returnval.Add(new Task(dr)); }
            }

            return returnval;
        }

        /// <summary>
        /// Static delete method
        /// </summary>
        /// <param name="recordid">Record ID</param>
        public static void Delete(int recordid)
        {
            using (SqlConnection conn = new SqlConnection(PortalUtility.GetConnectionString("default")))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("Task_Del", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@RecordID", recordid);
                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initialize class properties
        /// </summary>
        private void Initialize()
        {
            this.RecordID = 0;
            this.UserID = String.Empty;
            this.Notes = String.Empty;
            this.Hours = 0;
            this.TaskDate = PortalUtility.DefaultDate;
        }

        /// <summary>
        /// Load current class from a DataRow
        /// </summary>
        /// <param name="dr">DataRow</param>
        private void LoadDataRow(DataRow dr)
        {
            this.RecordID = Convert.ToInt32(dr["RecordID"]);
            this.UserID = dr["UserID"].ToString();
            this.Notes = dr["Notes"].ToString();
            this.Hours = Convert.ToInt32(dr["Hours"]);
            this.DailyHours = Convert.ToInt32(dr["DailyHours"]);
            this.TaskDate = Convert.ToDateTime(dr["TaskDate"]);
        }

        #endregion
    }
}