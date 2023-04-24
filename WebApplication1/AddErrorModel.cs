using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1
{
    public class AddErrorModel
    {
        public string Description { get; set; }
        public string Type { get; set; }
        public int TechnicianID { get; set; }
        public int MoldID { get; set; }
        public string ErrorPicture { get; set; }

    }

}