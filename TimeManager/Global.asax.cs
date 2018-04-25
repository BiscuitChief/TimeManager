using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Security.Principal;
using System.Security;
using System.Web.Security;
using System.Data;
using System.Data.SqlClient;

namespace TimeManager
{

    public class WebApiApplication : System.Web.HttpApplication
    {
        #region Properties

        private const string RoleCookieName = "TimeManagerRoles";

        #endregion

        #region Events

        protected void Application_Start()
        {
            RegisterRoutes(RouteTable.Routes);

            AreaRegistration.RegisterAllAreas();

            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            // Extract the forms authentication cookie
            string cookieName = FormsAuthentication.FormsCookieName;
            HttpCookie authCookie = Context.Request.Cookies[cookieName];

            if (null == authCookie)
            {
                // There is no authentication cookie.
                return;
            }

            //Extract and ecrypt authentication ticket
            FormsAuthenticationTicket authTicket = null;
            try
            {
                authTicket = FormsAuthentication.Decrypt(authCookie.Value);
            }
            catch
            {
                // Log exception details (omitted for simplicity)
                return;
            }

            if (null == authTicket)
            {
                // Cookie failed to decrypt.
                return;
            }

            // Create an Identity object
            GenericIdentity id = new GenericIdentity(authTicket.Name, "LdapAuthentication");
            string rolestring = string.Empty;
            string[] roles;

            //Get the users roles
            if (RoleCookieIsValid(id.Name))
            {
                //get roles from the cookie
                HttpCookie rolecookie = HttpContext.Current.Request.Cookies[RoleCookieName];
                rolestring = PortalUtility.Decrypt(rolecookie.Values["userid"], rolecookie.Values["roles"]);
            }

            //Either the cookie doesn't exist, or the user has no roles so we check again
            //This is done so new users won't have to restart their browser windows (user has no roles, gets access, refreshes the page and now they have access)
            if (string.IsNullOrEmpty(rolestring))
            {
                rolestring = GetUserRoles(id.Name);
                CreateRoleCookie(id.Name, rolestring);
            }

            //Get the users security rights for the site
            roles = rolestring.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

            //This principal will flow throughout the request.
            GenericPrincipal principal = new GenericPrincipal(id, roles);

            // Attach the new principal object to the current HttpContext object
            Context.User = principal;
        }

        #endregion

        #region Methods

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("DefaultRoute", "", new { controller = "Home", action = "Index" });
        }

        /// <summary>
        /// Store the user's roles in a cookie to cut down on database calls
        /// </summary>
        /// <param name="userid">The user id</param>
        /// <param name="roles">list of roles delimited by |</param>
        private void CreateRoleCookie(string userid, string roles)
        {
            HttpCookie rolecookie = new HttpCookie(RoleCookieName);
            rolecookie.Values["roles"] = PortalUtility.Encrypt(userid, roles);
            rolecookie.Values["userid"] = userid;
            rolecookie.Values["created"] = DateTime.Now.ToString();
            HttpContext.Current.Response.Cookies.Add(rolecookie);
        }

        /// <summary>
        /// Get the user's role from the database
        /// </summary>
        /// <param name="userid">User id to look up</param>
        /// <returns>string of roles delimited by |</returns>
        /// <remarks>This method is only called if the roles are not found in a cookie</remarks>
        private string GetUserRoles(string userid)
        {
            string rolelist = string.Empty;

            //build a delimited string of the roles
            using (SqlConnection conn = new SqlConnection(PortalUtility.GetConnectionString("default")))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("UserLogin_Sel", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserID", userid);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        if (Convert.ToBoolean(dr["IsAdmin"]))
                        { rolelist = "MANAGER|ADMIN"; }
                        else if (Convert.ToBoolean(dr["IsManager"]))
                        { rolelist = "MANAGER"; }
                    }
                }
                conn.Close();
            }
            rolelist = rolelist.TrimEnd('|');

            return rolelist;
        }

        /// <summary>
        // - If the cookie doesn't exist get roles from the DB and create cookie
        // - If logged in as a different user get new roles and create cookie
        // - If the cookie is too old refresh user roles from database, note this is not done using the Expires property because
        //       I still want the cookie to expire when the user closes their browser.
        /// </summary>
        /// <param name="userid">logged in user id</param>
        /// <returns></returns>
        private bool RoleCookieIsValid(string userid)
        {
            HttpCookie rolecookie = HttpContext.Current.Request.Cookies[RoleCookieName];
            bool returnval = true;

            try
            {
                if (rolecookie == null)
                { returnval = false; }
                else if (Convert.ToString(rolecookie.Values["userid"]) != userid)
                { returnval = false; }
                else if (DateTime.Parse(rolecookie.Values["created"]) < DateTime.Now.AddMinutes(-15))
                {
                    //Note: we do this check instead of setting an expiration on the cookie, because we still want the cookie to
                    //be deleted when the session ends. If we set a 15 minute expiration on the cookie it would last after the session ended
                    returnval = false;
                }
            }
            catch
            {
                returnval = false;
            }

            return returnval;
        }

        #endregion


    }
}