using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace TimeManager.api
{
    public class TasksController : ApiController
    {
        /// <summary>
        /// Add/Update a Task
        /// </summary>
        /// <param name="newTask">Task data to add/update</param>
        /// <returns></returns>
        [Route("Tasks/{recordid}")]
        [HttpPost]
        [Authorize]
        public IHttpActionResult ModifyTask(int recordid, string userid, string notes, int hours, DateTime taskdate)
        {
            //Security: only admins can alter records for other users
            if (User.Identity.Name != userid && !User.IsInRole("ADMIN"))
            { return new PortalUtility.PlainTextResult("Unauthorized access", HttpStatusCode.Unauthorized); }

            try
            {
                Models.Task newTask = new Models.Task();
                newTask.RecordID = recordid;
                newTask.UserID = userid;
                newTask.Notes = notes;
                newTask.Hours = hours;
                newTask.TaskDate = taskdate;

                newTask.Save();
                return Ok(newTask);
            }
            catch (Exception ex)
            { return new PortalUtility.PlainTextResult(ex.Message, HttpStatusCode.InternalServerError); }
        }

        /// <summary>
        /// Search for Tasks
        /// </summary>
        /// <param name="userid">UserID we want Tasks for</param>
        /// <param name="startdate">Start Date</param>
        /// <param name="enddate">End Date</param>
        /// <returns>List of Task objects for the user</returns>
        [Route("Tasks/search")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult TaskSearch(string userid, string startdate, string enddate)
        {
            //Security: need the manager role to view data for other users
            if (User.Identity.Name != userid && !User.IsInRole("ADMIN"))
            { return new PortalUtility.PlainTextResult("Unauthorized access", HttpStatusCode.Unauthorized); }

            List<Models.Task> returnval = new List<Models.Task>();

            try
            {
                returnval = Models.Task.SearchTasks(userid, startdate, enddate);
                return Ok(returnval);
            }
            catch (Exception ex)
            { return new PortalUtility.PlainTextResult(ex.Message, HttpStatusCode.InternalServerError); }
        }

        /// <summary>
        /// Get a single Task
        /// </summary>
        /// <param name="recordid">Task Record ID</param>
        /// <returns>Task Data</returns>
        [Route("Tasks/{recordid}")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult GetTask(int recordid)
        {
            Models.Task returnval = new Models.Task();

            try
            {
                returnval = new Models.Task(recordid);

                //Security: need the manager role to view data for other users, compare against UserID on the Task since we aren't passing the UserID as a parameter
                if (User.Identity.Name != returnval.UserID && !User.IsInRole("ADMIN"))
                { return new PortalUtility.PlainTextResult("Unauthorized access", HttpStatusCode.Unauthorized); }

                return Ok(returnval);
            }
            catch (Exception ex)
            { return new PortalUtility.PlainTextResult(ex.Message, HttpStatusCode.InternalServerError); }
        }

        /// <summary>
        /// Delete a Task
        /// </summary>
        /// <param name="recordid">Task Record ID</param>
        /// <returns></returns>
        [Route("Tasks/{recordid}")]
        [HttpDelete]
        [Authorize]
        public IHttpActionResult DeleteTask(int recordid)
        {
            try
            {
                Models.Task tasktodel = new Models.Task(recordid);
                if (tasktodel.RecordID > 0)
                {
                    //Security: need the manager role to view data for other users, compare against UserID on the Task since we aren't passing the UserID as a parameter
                    if (User.Identity.Name != tasktodel.UserID && !User.IsInRole("ADMIN"))
                    { return new PortalUtility.PlainTextResult("Unauthorized access", HttpStatusCode.Unauthorized); }

                    tasktodel.Delete();
                }

                return Ok();
            }
            catch (Exception ex)
            { return new PortalUtility.PlainTextResult(ex.Message, HttpStatusCode.InternalServerError); }
        }
    }
}
