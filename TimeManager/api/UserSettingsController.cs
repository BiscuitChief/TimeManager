using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace TimeManager.api
{
    public class UserSettingsController : ApiController
    {
        /// <summary>
        /// Add/Update a user setting
        /// </summary>
        /// <param name="userid">UserID</param>
        /// <param name="settingcode">Setting Code</param>
        /// <param name="settingvalue">Setting Value</param>
        /// <returns></returns>
        [Route("usersettings/{userid}/{settingcode}")]
        [HttpPost]
        [Authorize]
        public IHttpActionResult UpdateUserSetting(string userid, string settingcode, string settingvalue)
        {
            //Security: only admins can edit data for other users
            if (User.Identity.Name != userid && !User.IsInRole("ADMIN"))
            { return new PortalUtility.PlainTextResult("Unauthorized access", HttpStatusCode.Unauthorized); }

            try
            {
                Models.UserSetting.Save(userid, settingcode, settingvalue);
                return Ok(settingvalue);
            }
            catch (Exception ex)
            { return new PortalUtility.PlainTextResult(ex.Message, HttpStatusCode.InternalServerError); }
        }

        /// <summary>
        /// Get a setting value
        /// </summary>
        /// <param name="userid">UserID</param>
        /// <param name="settingcode">Setting Code</param>
        /// <returns></returns>
        [Route("usersettings/{userid}/{settingcode}")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult GetUserSetting(string userid, string settingcode)
        {
            //Security: need the manager role to view data for other users
            if (User.Identity.Name != userid && !User.IsInRole("ADMIN"))
            { return new PortalUtility.PlainTextResult("Unauthorized access", HttpStatusCode.Unauthorized); }

            try
            {
                string returnval = Models.UserSetting.GetSettingValue(userid, settingcode);
                return Ok(returnval);
            }
            catch (Exception ex)
            { return new PortalUtility.PlainTextResult(ex.Message, HttpStatusCode.InternalServerError); }
        }
    }
}
