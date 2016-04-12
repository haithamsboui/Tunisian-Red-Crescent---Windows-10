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

namespace CRT.Controls
{
   public  class SocketHandler
    {
        public  Socket socket;

       

        public SocketHandler()
        {
            socket = IO.Socket("http://crt-server-ibicha.c9users.io");
            socket.Connect();
            socket.On("Members", OnMembers);
            socket.On("SharingON", OnSharingOn);

            StaticData.socket = socket;
            
        }
      
        
        public void OnAccident()
        {
            AppShell.ShowNotification("Emergency", "New nearby Accident");
        }
        public void OnSharingOn(object data)
        {
            Debug.WriteLine("Sharing : "+data); 
        }
        public void OnSharingOFF()
        {
        }
        public void OnDisconnect()
        {

        }
        public void OnLocation()
        {

        }
        public void OnMembers(object data)
        {
            
        }
        public class SocketMember
        {
            public string UserID { get; set; }
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
            public int Timestamp { get; set; }

        }
        public static void EmitSharingOn()
        {
            if (StaticData.LastLocation != null && StaticData.socket!=null) {
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
    }
}
