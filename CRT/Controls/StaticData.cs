using Quobject.SocketIoClientDotNet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Security.Authentication.Web;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls.Maps;

namespace CRT.Controls
{
    public class StaticData
    {
     
        public static bool HasInternetAccess { get;  set; }
        public static CRT.Models.User currentUser { get; set; }
        public static string accessToken { get; set; }
        public static string SID_debug = WebAuthenticationBroker.GetCurrentApplicationCallbackUri().ToString();
        public static AppShell appShell { get; set; }
        public static Geocoordinate LastLocation { get; set; }
        public static Socket socket { get; set; }
        public static MapControl CrtMap { get; set; }
        public static List<SocketHandler.SocketMember> CrtMembers { get; set; }
        public static IAsyncOperation<IUICommand> DialogAsyncOperation;
        public static bool IsOnline;
        public static bool IsSharing;
    }
}
