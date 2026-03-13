using KIursachTugin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIursachTugin
{
    public static class Session
    {
        public static int UserID { get; set; }
        public static string RoleName { get; set; }
        public static User CurrentUser { get; set; }
    }
}
