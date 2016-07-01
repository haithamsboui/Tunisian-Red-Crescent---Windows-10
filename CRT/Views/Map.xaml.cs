using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Devices.Geolocation;
using Windows.UI.Core;
using System.Diagnostics;
using Windows.UI.Xaml.Controls.Maps;
using Windows.Foundation;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRT.Controls;

using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Windows.ApplicationModel;
using Windows.Storage.Streams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml;
using Quobject.EngineIoClientDotNet.ComponentEmitter;

namespace CRT.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 

    public sealed partial class Map : Page
    {
        MapIcon MyMarker;
        bool FirstZoom;
        List<CRT.Models.Accident> Accidents;
        Dictionary<SocketHandler.SocketMember,MapElement> CrtMembers;

        public DependencyProperty dpLocal = DependencyProperty.Register("local", typeof(Models.CrtLocal), typeof(MapIcon), new PropertyMetadata(false));
        public DependencyProperty dpAcc = DependencyProperty.Register("accident", typeof(Models.Accident), typeof(MapIcon), new PropertyMetadata(false));
        public DependencyProperty dpMember = DependencyProperty.Register("member", typeof(SocketHandler.SocketMember), typeof(MapIcon), new PropertyMetadata(false));


        public Map()
        {
           FirstZoom = true;
            StaticData.CrtMap = CrtMap; 
            this.InitializeComponent();
            CrtMap.MapElementClick += onMarkerClick;

            OnMapLoaded();
            if (StaticData.IsOnline)
                LoadCrtLocals();
            else
            {
                LoadCrtLocalsOffline();
                LoadAccOff();
            }

            if (StaticData.currentUser != null)
            {
                if(StaticData.currentUser.IsAdmin)
                LoadAccidents();
            }
            StaticData.socket.Emit("Members");
            StaticData.socket.On("Members", LoadCrtMembersMarkers);
            StaticData.socket.On("Accident", OnAccident);
            StaticData.socket.On("SharingOFF", OnSharingOFF);
            StaticData.socket.On("SharingON", OnSharingON);
            StaticData.socket.On("Location", OnLocation);

        }
        public async void OnSharingOFF(object data)
        {
            string id = ((JObject)data).GetValue("id").ToString();
            KeyValuePair<SocketHandler.SocketMember, MapElement> memb = new KeyValuePair<SocketHandler.SocketMember, MapElement>(null,null) ;
            foreach (var member in CrtMembers)
           // for (int i=0;i<CrtMembers.Keys.Count;i++)
            {
                if (member.Key.id.Equals(id))
                {
                    memb = member;

                }
            }
            if (memb.Key != null)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    MapElement marker;
                    if (CrtMembers.TryGetValue(memb.Key, out marker))
                    {
                        CrtMap.MapElements.Remove(marker);
                        CrtMembers.Remove(memb.Key);
                        return;
                    }
                });
            }
        }
        public async void OnSharingON(object data)
        {

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {

                JToken newMemToken = (JObject)data;
                SocketHandler.SharingSocket newMem = JsonConvert.DeserializeObject<SocketHandler.SharingSocket>(newMemToken.ToString());
                SocketHandler.SocketMember mem = new SocketHandler.SocketMember { id = newMem.id, IsAdmin = true, IsMember = true, Sharing = true, Location = newMem.Location };
                if (mem.id != StaticData.socket.Io().EngineSocket.Id)
                {

                    MapIcon MemberMarker = new MapIcon();
                    MemberMarker.Location = new Geopoint(new BasicGeoposition()
                    {
                        Latitude = mem.Location.Latitude,
                        Longitude = mem.Location.Longitude
                    });
                    MemberMarker.SetValue(dpMember, mem);
                    MemberMarker.Title = "Member";
                    MemberMarker.ZIndex = 0;
                    MemberMarker.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/member_shadow.png"));
                    CrtMembers.Add(mem, MemberMarker);
                    
                        CrtMap.MapElements.Add(MemberMarker);
                }
            });


            Debug.WriteLine("Sharing ON : \n" + CrtMembers.Count + " " + CrtMembers.Keys.Count + " " + CrtMembers.Values.Count);

        }
        public void OnLocation(object data)
        {
            Debug.WriteLine("OnLocation : \n" + data.ToString());

        }
        public void LoadAccOff()
        {
  
             Accidents = IsolatedStorageHelper.GetObject<List<CRT.Models.Accident>>("acc");
            if (Accidents != null)
            {
                foreach (CRT.Models.Accident acc in Accidents)
                {
                    MapIcon CrtMarker = new MapIcon();
                    CrtMarker.Location = new Geopoint(new BasicGeoposition()
                    {
                        Latitude = acc.Location.Latitude,
                        Longitude = acc.Location.Longitude
                    });
                    CrtMarker.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/bw-accident.png"));
                    CrtMarker.Title = acc.Description;
                    CrtMarker.ZIndex = 0;
                    CrtMarker.SetValue(dpAcc, acc);
                    if (!CrtMap.MapElements.Contains(CrtMarker))
                        CrtMap.MapElements.Add(CrtMarker);
                }
            }
        }

        private async void onMarkerClick(MapControl sender, MapElementClickEventArgs args)
        {

            bool a = args.MapElements[0].GetValue(dpLocal) is Models.CrtLocal;
            if (a)
            {
                Models.CrtLocal crt =(Models.CrtLocal) args.MapElements[0].GetValue(dpLocal);
                var uriCrt = new Uri(@"ms-drive-to:?destination.latitude="+crt.Location.Latitude+"&destination.longitude="+ crt.Location.Longitude );
                var success = await Windows.System.Launcher.LaunchUriAsync(uriCrt);
            }
            bool b = args.MapElements[0].GetValue(dpAcc) is Models.Accident;
            if (b)
            {
                Models.Accident acc = (Models.Accident)args.MapElements[0].GetValue(dpAcc);
                Controls.StaticData.appShell.AppFrame.Navigate(typeof(AccidentHandle),acc);


            }
            bool c = args.MapElements[0].GetValue(dpMember) is SocketHandler.SocketMember;
            if (c)
            {

            }




        }

        public async void OnMapLoaded()
        {
       
            StaticData.appShell.geolocator.PositionChanged += OnPositionChanged;
            GeolocationAccessStatus accessStatus = await Geolocator.RequestAccessAsync();
            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    SearchingLocation.IsActive = true;
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        try
                        {
                            Geoposition gp = await StaticData.appShell.geolocator.GetGeopositionAsync();
                            if (gp != null)
                                UpdateLocationData(gp);
                        }
                        catch
                        {
                            var dialog = new MessageDialog("Couldn't locate your position");
                            await dialog.ShowAsync();
                        }
                    });
                    break;
                    
            }
            SearchingLocation.IsActive = false;
        }

        private async void OnPositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                UpdateLocationData(args.Position);
            });
         

        }

        private async void UpdateLocationData(Geoposition pos)
        {
            if (pos != null)
            {
                if (MyMarker == null)
                    MyMarker = new MapIcon();
                SocketHandler.SocketMember me = new SocketHandler.SocketMember { id = "me" };
                MyMarker.SetValue(dpMember, me);

                MyMarker.Location = pos.Coordinate.Point;
                MyMarker.Title = "Me";
                MyMarker.ZIndex = 0;
                MyMarker.Image= RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/male_shadow.png"));
                if (!CrtMap.MapElements.Contains(MyMarker))
                    CrtMap.MapElements.Add(MyMarker);
                if (FirstZoom)
                {
                    await CrtMap.TrySetViewAsync(pos.Coordinate.Point, 14);
                    FirstZoom = false;
                }

            }

        }


      
        private async void MyLocation_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SearchingLocation.IsActive = true;
            GeolocationAccessStatus accessStatus = await Geolocator.RequestAccessAsync();
            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        try
                        {
                            Geoposition gp = await StaticData.appShell.geolocator.GetGeopositionAsync();
                            if (gp != null)
                                await CrtMap.TrySetViewAsync(gp.Coordinate.Point, 16);
                        }
                        catch
                        {
                            var dialog = new MessageDialog("Couldn't locate your position");
                            await dialog.ShowAsync();
                        }
                    });
                        break;

                case GeolocationAccessStatus.Denied:
                    StaticData.appShell.RequestLocation();
                    break;

                case GeolocationAccessStatus.Unspecified:
                    StaticData.appShell.RequestLocation();
                    break;
            }
            SearchingLocation.IsActive = false;

        }
        List<CRT.Models.CrtLocal> CrtLocals;
        private async void LoadCrtLocals()
        {
            CrtLocals = null;
            
                Task<List<CRT.Models.CrtLocal>> task = CRT.Controls.WebserviceHandler.GetCrtPlaces(CRT.Controls.StaticData.accessToken);
            await Task.WhenAll(task);
            CrtLocals = task.Result;
                IsolatedStorageHelper.SaveObject<List<CRT.Models.CrtLocal>>("crt_locals", CrtLocals);       
            foreach (CRT.Models.CrtLocal local in CrtLocals)
            {
                MapIcon CrtMarker = new MapIcon();
                CrtMarker.Location = new Geopoint(new BasicGeoposition()
                {
                    Latitude = local.Location.Latitude,
                    Longitude = local.Location.Longitude
                });
                CrtMarker.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/redcres1.png"));
                CrtMarker.Title = local.Title;
                CrtMarker.ZIndex = 0;
                CrtMarker.SetValue(dpLocal, local);

                if (!CrtMap.MapElements.Contains(CrtMarker))
                    CrtMap.MapElements.Add(CrtMarker);
            }
        }
        private void LoadCrtLocalsOffline()
        {
            List<CRT.Models.CrtLocal> CrtLocals = null;
            CrtLocals = IsolatedStorageHelper.GetObject<List<Models.CrtLocal>>("crt_locals");
            if (CrtLocals != null)
            {
                foreach (CRT.Models.CrtLocal local in CrtLocals)
                {
                    MapIcon CrtMarker = new MapIcon();
                    CrtMarker.Location = new Geopoint(new BasicGeoposition()
                    {
                        Latitude = local.Location.Latitude,
                        Longitude = local.Location.Longitude
                    });
                    CrtMarker.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/redcres1.png"));
                    CrtMarker.Title = local.Title;
                    CrtMarker.ZIndex = 0;
                    CrtMarker.SetValue(dpLocal, local);
                    if (!CrtMap.MapElements.Contains(CrtMarker))
                        CrtMap.MapElements.Add(CrtMarker);
                }
            }
        }

        private async void ReportAccident_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if(StaticData.currentUser!=null)
                StaticData.appShell.AppFrame.Navigate(typeof(ReportAccident));
            else
            {
                var dialog = new MessageDialog("You must login first !");
                await dialog.ShowAsync();
            }
        }

        public async void OnAccident(object data)
        {

            Debug.WriteLine("Map new Accident");
            JToken LocationObj= ((JObject)data).GetValue("Location");
            SocketHandler.LocationSocket loc= JsonConvert.DeserializeObject<SocketHandler.LocationSocket>(LocationObj.ToString());
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MapIcon MemberMarker = new MapIcon();
                MemberMarker.Location = new Geopoint(new BasicGeoposition()
                {
                    Latitude = loc.Latitude,
                    Longitude = loc.Longitude
                });
                MemberMarker.Title = "Accident";
                MemberMarker.ZIndex = 0;
                MemberMarker.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/headinjuryicon.png"));
                if (!CrtMap.MapElements.Contains(MemberMarker))
                    CrtMap.MapElements.Add(MemberMarker);
            });
        }
        public async void LoadAccidents()
        {
            Debug.WriteLine("Laoding Accidents");
            Task<List<CRT.Models.Accident>> task = CRT.Controls.WebserviceHandler.GetAccidents(CRT.Controls.StaticData.accessToken);
            await Task.WhenAll(task);
            Accidents = task.Result;
            IsolatedStorageHelper.SaveObject<List<CRT.Models.Accident>>("acc", Accidents);
            Debug.WriteLine("Laoding Accidents : "+ Accidents.Count);

            foreach (CRT.Models.Accident acc in Accidents)
            {
                MapIcon CrtMarker = new MapIcon();
                CrtMarker.Location = new Geopoint(new BasicGeoposition()
                {
                    Latitude = acc.Location.Latitude,
                    Longitude = acc.Location.Longitude
                });
                CrtMarker.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/bw-accident.png"));
                CrtMarker.Title = acc.Description;
                CrtMarker.ZIndex = 0;
                CrtMarker.SetValue(dpAcc, acc);

                if (!CrtMap.MapElements.Contains(CrtMarker))
                    CrtMap.MapElements.Add(CrtMarker);
            }
        }
        public async void LoadCrtMembersMarkers(object data)
        {
            CrtMembers = new Dictionary<SocketHandler.SocketMember, MapElement>();
            List < SocketHandler.SocketMember>   CrtMembersLIST = JsonConvert.DeserializeObject<List<SocketHandler.SocketMember>>(((Newtonsoft.Json.Linq.JArray)data).ToString());
            StaticData.CrtMembers = CrtMembersLIST;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                foreach (var member in CrtMembersLIST)
                {
                    if (StaticData.currentUser!=null)
                    {
                        if (member.id != StaticData.socket.Io().EngineSocket.Id) { 
                    MapIcon MemberMarker = new MapIcon();
                    MemberMarker.Location = new Geopoint(new BasicGeoposition()
                    {
                        Latitude = member.Location.Latitude,
                        Longitude = member.Location.Longitude
                    });
                    MemberMarker.SetValue(dpMember, member);
                    MemberMarker.Title = "Member";
                    MemberMarker.ZIndex = 0;
                    MemberMarker.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/member_shadow.png"));
                            CrtMembers.Add(member, MemberMarker);
                            if (!CrtMap.MapElements.Contains(MemberMarker) )
                        CrtMap.MapElements.Add(MemberMarker);
                    }
                }
            }
            });
        }

        private void TerrainView_Click(object sender, RoutedEventArgs e)
        {
            CrtMap.Style = MapStyle.Aerial3DWithRoads;

        }

        private void RoadsView_Click(object sender, RoutedEventArgs e)
        {
            CrtMap.Style = MapStyle.Road;

        }

        private void textBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}
