using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRT.Models
{
    public class Accident
    {
        public string id { get; set; }
        public string ReporterId { get; set; }
        public string Description { get; set; }
        public string ImageFile { get; set; }
        public bool IsHandled { get; set; }
        public Location Location { get; set; }
    }
}
