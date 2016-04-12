using Quobject.SocketIoClientDotNet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Security.Authentication.Web;
using Windows.UI.Xaml.Controls.Maps;

namespace CRT.Controls
{
    public class StaticData
    {
        public static CRT.Models.User currentUser;
        public static string accessToken;
        public static string SID_debug = WebAuthenticationBroker.GetCurrentApplicationCallbackUri().ToString();
        public static AppShell appShell;
        public static Geocoordinate LastLocation;
        public static Socket socket;
        public static MapControl CrtMap;
        public static List<SocketHandler.SocketMember> CrtMembers;

    }
}
