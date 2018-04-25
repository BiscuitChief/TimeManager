using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeManager.Models
{
    public partial class PortalUser
    {
        public string UserID { get; set; }

        public string UserName { get; set; }

        public byte[] Password { get; set; }

        public string EncryptionSeed { get; set; }

        public bool IsActive { get; set; }

        public bool IsManager { get; set; }

        public bool IsAdmin { get; set; }

        public DateTime? Added { get; set; }

        public DateTime? LastModified { get; set; }

        public List<UserSetting> UserSettings { get; set; }

        //public List<Meal> Meals { get; set; }
    }
}