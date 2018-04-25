using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeManager.Models
{
    public partial class UserSetting
    {
        public string UserID { get; set; }

        public string SettingCode { get; set; }

        public string SettingValue { get; set; }
    }
}