using Quobject.SocketIoClientDotNet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Quobject.EngineIoClientDotNet.Client.Socket;

namespace CRT.Controls
{
   public  class SocketHandler
    {
        public  Socket socket;
        private IO.Options opts;

        public SocketHandler()
        {

            IO.Options opt = new IO.Options();
            opt.Timeout = 5000;
            socket = IO.Socket("http://crt-server-ibicha.c9users.io",opts);
            socket.Connect();
            socket.On("Accident", OnAccident);

           
            StaticData.socket = socket;
            
        }
        

        public void OnAccident()
        {
            Debug.WriteLine("Accident");
            AppShell.ShowNotification("Emergency", "New nearby Accident");
        }
        
    
        public class SocketMember
        {
            public string id { get; set; }
            public bool IsMember { get; set; }
            public bool IsAdmin { get; set; }
            public bool Sharing { get; set; }
            public LocationSocket Location { get; set; }
        }
        public class SharingSocket
        {
            public string id { get; set; }
            public string user { get; set; }
            public LocationSocket Location { get; set; }

        }
        public class LocationSocket
        {
            public double Longitude { get; set; }
            public double Latitude { get; set; }
            public double Accuracy { get; set; }
            public double Timestamp { get; set; }

        }
        public static void EmitSharingOn()
        {
            if (StaticData.LastLocation != null && StaticData.socket!=null) {
                Debug.WriteLine("emit sharing");
                JObject data=new JObject();
                JObject location = new JObject();
                location.Add("Longitude", StaticData.LastLocation.Point.Position.Longitude);
                location.Add("Latitude", StaticData.LastLocation.Point.Position.Latitude);
                location.Add("Accuracy", 1);
                location.Add("Timestamp", StaticData.LastLocation.Timestamp.Millisecond);
                data.Add("Location",location);
                StaticData.socket.Emit("SharingON", data);
             }
        }
        public static void EmitSharingOFF()
        {
            StaticData.socket.Emit("SharingOFF");

        }

    }
}
