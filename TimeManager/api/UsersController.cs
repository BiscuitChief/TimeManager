using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TimeManager.Models;


namespace TimeManager.Controllers
{
    public class UsersController : ApiController
    {
        /// <summary>
        /// Return a list of all users
        /// </summary>
        /// <returns></returns>
        [Route("users/all")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult GetAllUsers()
        {
            //Security: can't view other user data without manager role
            if (!User.IsInRole("MANAGER"))
            { return new PortalUtility.PlainTextResult("Unauthorized access", HttpStatusCode.Unauthorized); }

            try
            { 
                List<PortalUser> returnval = PortalUser.GetAllUsers();
                return Ok(returnval);
            }
            catch(Exception ex)
            { return new PortalUtility.PlainTextResult(ex.Message, HttpStatusCode.InternalServerError); }
        }

        /// <summary>
        /// Get a specific user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User data</returns>
        [Route("users/{id}")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult GetUser(string id)
        {
            //Security: can't view other user data without manager role
            if (User.Identity.Name != id && !User.IsInRole("MANAGER"))
            { return new PortalUtility.PlainTextResult("Unauthorized access", HttpStatusCode.Unauthorized); }

            try
            {
                PortalUser returnuser = new PortalUser(id);
                return Ok(returnuser);
            }
            catch (Exception ex)
            { return new PortalUtility.PlainTextResult(ex.Message, HttpStatusCode.InternalServerError); }
        }

        /// <summary>
        /// Get the daily hours for a user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="mealdate">Date we want hours total for</param>
        /// <returns>Hours total for specified date</returns>
        [Route("users/{id}/dailyhours")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult GetDailyHours(string id, DateTime taskdate)
        {
            //Security: can't view other user data without manager role
            if (User.Identity.Name != id && !User.IsInRole("MANAGER"))
            { return new PortalUtility.PlainTextResult("Unauthorized access", HttpStatusCode.Unauthorized); }

            try
            {
                int returnval = PortalUser.GetDailyHours(id, taskdate);
                return Ok(returnval);
            }
            catch (Exception ex)
            { return new PortalUtility.PlainTextResult(ex.Message, HttpStatusCode.InternalServerError); }
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="username">UserName</param>
        /// <param name="password">Password</param>
        /// <returns>blank if success, otherwise an error message</returns>
        [Route("users")]
        [HttpPost]
        public IHttpActionResult RegisterUser(PortalLogin logininfo)
        {
            string responsemessage = String.Empty;

            try
            { PortalUser.CreateNewUser(logininfo.UserName, logininfo.Password); }
            catch (Exception ex)
            { responsemessage = ex.Message; }

            if (String.IsNullOrEmpty(responsemessage))
            { return Ok(); }
            else if (responsemessage == "Existing User")
            { return new PortalUtility.PlainTextResult("UserName is not available.  Please enter a different UserName.", HttpStatusCode.Conflict); }
            else
            { return new PortalUtility.PlainTextResult(responsemessage, HttpStatusCode.InternalServerError); }
        }

        /// <summary>
        /// Update user information
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="username">UserName</param>
        /// <param name="isactive">IsActive</param>
        /// <param name="ismanager">IsManager</param>
        /// <param name="isadmin">IsAdmin</param>
        /// <returns></returns>
        [Route("users")]
        [HttpPut]
        [Authorize]
        public IHttpActionResult UpdateUser(PortalUser newuserdata)
        {
            string responsemessage = String.Empty;

            try
            {
                PortalUser userUpd = new PortalUser(newuserdata.UserID);
                if (User.IsInRole("MANAGER") //Need the manager role to update any users
                    && (User.IsInRole("ADMIN") || !userUpd.IsAdmin)) //Only admins can edit other admins
                {
                    userUpd.UserName = newuserdata.UserName;
                    userUpd.IsActive = newuserdata.IsActive;
                    userUpd.IsManager = newuserdata.IsManager;
                    //Only admins can set the admin role
                    if (User.IsInRole("ADMIN"))
                    { userUpd.IsAdmin = newuserdata.IsAdmin; }
                    userUpd.Save();
                }
                else
                { return new PortalUtility.PlainTextResult("Unauthorized access", HttpStatusCode.Unauthorized); }
            }
            catch (Exception ex)
            { responsemessage = ex.Message; }

            if (String.IsNullOrEmpty(responsemessage))
            { return Ok(); }
            else if (responsemessage == "Existing User")
            { return new PortalUtility.PlainTextResult("UserName is not available.  Please enter a different UserName.", HttpStatusCode.Conflict); }
            else
            { return new PortalUtility.PlainTextResult(responsemessage, HttpStatusCode.InternalServerError); }
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        /// <param name="id">UserID</param>
        [Route("users/{id}")]
        public void Delete(string id)
        {
            PortalUser userUpd = new PortalUser(id);
            if (User.IsInRole("MANAGER") //Need the manager role to update any users
                && (User.IsInRole("ADMIN") || !userUpd.IsAdmin)) //Only admins can edit other admins
            {
                userUpd.Delete();
            }
            else
            { throw new System.Security.Authentication.AuthenticationException("Unauthorized access"); }
        }
    }
}