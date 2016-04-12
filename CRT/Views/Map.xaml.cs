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

namespace CRT.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Map : Page
    {
        HttpClient http;
        Geoposition MyPosition;
        MapIcon MyMarker;
        bool FirstZoom;
        public Map()
        {
            FirstZoom = true;
            StaticData.CrtMap = CrtMap;
            this.InitializeComponent();
            OnMapLoaded();
            LoadCrtLocals();
            StaticData.socket.Emit("Members");
            StaticData.socket.On("Members", LoadCrtMembersMarkers);
            StaticData.socket.On("Accident", OnAccident);
        }
      
        public async void OnMapLoaded()
        {
            GeolocationAccessStatus accessStatus = await Geolocator.RequestAccessAsync();
            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:

                    // If DesiredAccuracy or DesiredAccuracyInMeters are not set (or value is 0), DesiredAccuracy.Default is used.
                    Geolocator geolocator = new Geolocator();
                    geolocator.ReportInterval = 4000;
                    geolocator.DesiredAccuracyInMeters = 100;
                    geolocator.MovementThreshold = 0.1;
                    // Subscribe to the StatusChanged event to get updates of location status changes.
                    geolocator.StatusChanged += OnLocationStatusChanged;
                    geolocator.PositionChanged += OnPositionChanged;
                    // Carry out the operation.
                    MyPosition = await geolocator.GetGeopositionAsync();

                    UpdateLocationData(MyPosition);
                    break;

                case GeolocationAccessStatus.Denied:
                    break;

                case GeolocationAccessStatus.Unspecified:
                    break;
            }
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
                StaticData.LastLocation = pos.Coordinate;
                if (MyMarker == null)
                    MyMarker = new MapIcon();
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


        public async void OnLocationStatusChanged(Geolocator sender, StatusChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Show the location setting message only if status is disabled.

                switch (e.Status)
                {
                    case PositionStatus.Ready:

                        // Location platform is providing valid data.
                        break;

                    case PositionStatus.Initializing:

                        // Location platform is attempting to acquire a fix. 
                        break;

                    case PositionStatus.NoData:
                        // Location platform could not obtain location data.
                        break;

                    case PositionStatus.Disabled:

                        LocationDisabledMessage.Visibility = Windows.UI.Xaml.Visibility.Visible;

                        // Clear any cached location data.
                        UpdateLocationData(null);
                        break;

                    case PositionStatus.NotInitialized:

                        // The location platform is not initialized. This indicates that the application 
                        // has not made a request for location data.
                        break;

                    case PositionStatus.NotAvailable:

                        // The location platform is not available on this version of the OS.
                        break;

                    default:
                        // ScenarioOutput_Status.Text = "Unknown";
                        //rootPage.NotifyUser(string.Empty, NotifyType.StatusMessage);
                        break;
                }
            });
        }

        private async void MyLocation_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (MyPosition != null)
                await CrtMap.TrySetViewAsync(MyPosition.Coordinate.Point, 14);
        }

        private async void LoadCrtLocals()
        {
            Task<List<CRT.Models.CrtLocal>> task = CRT.Controls.WebserviceHandler.GetCrtPlaces(CRT.Controls.StaticData.accessToken);
            await Task.WhenAll(task);
            List<CRT.Models.CrtLocal> CrtLocals = task.Result;

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
                if (!CrtMap.MapElements.Contains(CrtMarker))
                    CrtMap.MapElements.Add(CrtMarker);
            }
        }

        private void ReportAccident_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            StaticData.appShell.AppFrame.Navigate(typeof(ReportAccident));
        }

        public  void OnAccident(object data)
        {
           JToken LocationObj= ((JObject)data).GetValue("Location");
            SocketHandler.LocationSocket loc= JsonConvert.DeserializeObject<SocketHandler.LocationSocket>(LocationObj.ToString());
            Debug.WriteLine(loc.Latitude + " "+ loc.Longitude);
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

        }

        public async void LoadCrtMembersMarkers(object data)
        {
            List<SocketHandler.SocketMember> CrtMembers = JsonConvert.DeserializeObject<List<SocketHandler.SocketMember>>(((Newtonsoft.Json.Linq.JArray)data).ToString());
            StaticData.CrtMembers = CrtMembers;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                foreach (var member in CrtMembers)
                {

                    MapIcon MemberMarker = new MapIcon();
                    MemberMarker.Location = new Geopoint(new BasicGeoposition()
                    {
                        Latitude = member.Location.Latitude,
                        Longitude = member.Location.Longitude
                    });
                    MemberMarker.Title = "Member";
                    MemberMarker.ZIndex = 0;
                    MemberMarker.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/member_shadow.png"));
                    if (!CrtMap.MapElements.Contains(MemberMarker) )
                        CrtMap.MapElements.Add(MemberMarker);

                }
            });
        }
    }
}
