using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeManager.Models
{
    public partial class Task
    {
        public int RecordID { get; set; }

        public string UserID { get; set; }

        public string Notes { get; set; }

        public int Hours { get; set; }

        public int DailyHours { get; set; }

        public DateTime TaskDate { get; set; }
    }
}