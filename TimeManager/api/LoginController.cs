using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data;
using System.Data.SqlClient;
using TimeManager.Models;
using System.Web.Security;
using System.Web;

namespace TimeManager.api
{
    public class LoginController : ApiController
    {
        /// <summary>
        /// Login to the site
        /// </summary>
        /// <param name="username">UserName</param>
        /// <param name="password">Password</param>
        /// <returns></returns>
        [Route("login")]
        [HttpPost]
        public IHttpActionResult Login(PortalLogin logininfo)
        {
            bool isvalidlogin = false;
            PortalUser loginuser = new PortalUser();
            //Need a username and password to login
            if (!String.IsNullOrEmpty(logininfo.UserName) && !String.IsNullOrEmpty(logininfo.Password))
            {
                using (SqlConnection conn = new SqlConnection(PortalUtility.GetConnectionString("default")))
                {
                    conn.Open();
                    //lookup the user based on username
                    SqlCommand cmd = new SqlCommand("UserLogin_Sel", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserName", logininfo.UserName);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            //load user class
                            loginuser.UserID = dr["UserID"].ToString();
                            loginuser.UserName = dr["UserName"].ToString();
                            loginuser.Password = (byte[])(dr["Password"]);
                            loginuser.EncryptionSeed = dr["EncryptionSeed"].ToString();
                            dr.Close();
                        }
                    }

                    conn.Close();
                }
                //confirm the username and password matches what is in the database
                byte[] passwordhash = PortalUtility.HashString(loginuser.EncryptionSeed, logininfo.Password);
                if (loginuser.UserName.ToLower() == logininfo.UserName.ToLower() && PortalUtility.CompareHash(loginuser.Password, passwordhash))
                { isvalidlogin = true; }
            }

            if (isvalidlogin)
            {
                //if the login is valid create the authentication ticket
                FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1, loginuser.UserID, DateTime.Now, DateTime.Now.AddMinutes(30), true, "");
                String cookiecontents = FormsAuthentication.Encrypt(authTicket);
                HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, cookiecontents) { Expires = authTicket.Expiration, Path = FormsAuthentication.FormsCookiePath };
                HttpContext.Current.Response.Cookies.Add(cookie);

                return Ok();
            }
            else
            {
                //If there is any kind of error clear any existing login and return an error
                Logout();
                return new PortalUtility.PlainTextResult("Authentication Exception", HttpStatusCode.Unauthorized);
            }
        }

        /// <summary>
        /// Logout the user
        /// </summary>
        /// <returns></returns>
        [Route("logout")]
        [HttpPost]
        public IHttpActionResult Logout()
        {
            LogoutTasks();
            return Ok();
        }

        /// <summary>
        /// Logout method, created as a seperate method so we can call it if login fails or on logout
        /// It's only one method at this time but in the event that we have other objects we need to dispose of on logout we can do it here
        /// </summary>
        private void LogoutTasks()
        {
            FormsAuthentication.SignOut();
        }
    }
}
