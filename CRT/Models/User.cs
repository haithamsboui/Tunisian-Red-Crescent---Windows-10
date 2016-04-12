using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace CRT.Models
{
   public class User
    {
        public string id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string ImageFile { get; set; }
        public DateTime BirthDate { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsMember { get; set; }
        public Address Adress { get; set; }
        public User()
        {

        }
    }

   
}
