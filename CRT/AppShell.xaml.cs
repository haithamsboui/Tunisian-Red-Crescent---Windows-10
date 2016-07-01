using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using CRT.Controls;
using CRT.Views;
using CRT.Models;
using Quobject.SocketIoClientDotNet.Client;
using System.Diagnostics;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using Windows.Devices.Geolocation;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.Networking.Connectivity;

namespace CRT
{
    /// <summary>
    /// The "chrome" layer of the app that provides top-level navigation with
    /// proper keyboarding navigation.
    /// </summary>
    public sealed partial class AppShell : Page
    {

        // Declare the top level nav items
        public static AppShell get;
        public Geolocator geolocator;
        public bool OfflineMode;
        public bool isConnected = false;
        public Models.User user { get; set; }
        private List<NavMenuItem> ConnectedNavlist = new List<NavMenuItem>(
            new[]
            {
                new NavMenuItem()
                {
                    Symbol = Symbol.Map,
                    Label = "Map",
                    DestinationPage = typeof(Views.Map)
                },
                new NavMenuItem()
                {
                    Symbol = Symbol.Contact,
                    Label = "Profile",
                    DestinationPage = typeof(Views.Profile)
                },
                new NavMenuItem()
                {
                    Symbol = Symbol.Bookmarks,
                    Label = "Emergency guide",
                    DestinationPage = typeof(Views.EmergencyGuide)
                },
                new NavMenuItem()
                {
                    Symbol = Symbol.Send,
                    Label = "Report Accident",
                    DestinationPage = typeof(Views.ReportAccident)
                }, new NavMenuItem()
                {
                    Symbol = Symbol.Import,
                    Label = "Logout",
                    DestinationPage = typeof(Views.Logout)
                }
            });
        private List<NavMenuItem> OfflineNavlist = new List<NavMenuItem>(
            new[]
            {
                new NavMenuItem()
                {
                    Symbol = Symbol.Map,
                    Label = "Map",
                    DestinationPage = typeof(Views.Map)
                },
                new NavMenuItem()
                {
                    Symbol = Symbol.Contact,
                    Label = "Login",
                    DestinationPage = typeof(Views.Login)
                },
                new NavMenuItem()
                {
                    Symbol = Symbol.Bookmarks,
                    Label = "Emergency guide",
                    DestinationPage = typeof(Views.EmergencyGuide)
                },
               
            });
        private List<NavMenuItem> NoInternetNavlist = new List<NavMenuItem>(
            new[]
            {
                new NavMenuItem()
                {
                    Symbol = Symbol.Map,
                    Label = "Map",
                    DestinationPage = typeof(Views.Map)
                },
                new NavMenuItem()
                {
                    Symbol = Symbol.Bookmarks,
                    Label = "Emergency guide",
                    DestinationPage = typeof(Views.EmergencyGuide)
                },

            });
        public static AppShell Current = null;

        /// <summary>
        /// Initializes a new instance of the AppShell, sets the static 'Current' reference,
        /// adds callbacks for Back requests and changes in the SplitView's DisplayMode, and
        /// provide the nav menu list with the data to display.
        /// </summary>
        public AppShell(bool OfflineMode=false)
        {
            this.InitializeComponent();
            StaticData.IsOnline = !OfflineMode;
            Controls.StaticData.appShell = this;
            this.Loaded += (sender, args) =>
            {
                Current = this;
                this.TogglePaneButton.Focus(FocusState.Programmatic);
            };
            SystemNavigationManager.GetForCurrentView().BackRequested += SystemNavigationManager_BackRequested;

            if (!OfflineMode)
            {
                ConnectionPanel.Visibility = Visibility.Collapsed;

                SetNavList();
            }
            else
            {
                SetNoInternetNavList();
                ConnectionPanel.Visibility = Visibility.Visible;

            }
            SetupLocationListner();
            NetworkInformation.NetworkStatusChanged += NetworkStatusChanged;
        }

        private async void NetworkStatusChanged(object sender)
        {
            if (HasInternetAccess())
            {

                /* var dialog = new MessageDialog("Switching Online mode ...");
                 await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                 {
                     StaticData.DialogAsyncOperation = null;
                     StaticData.DialogAsyncOperation=  dialog.ShowAsync();
                     await StaticData.DialogAsyncOperation;
                 });
                 */
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ConnectionPanel.Visibility = Visibility.Collapsed;
                });
                if (StaticData.currentUser == null)
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        SetOfflineNavList();
                    });
                }
                else
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        SetConnectedNavList();
                    });
                }
            }
            else
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ConnectionPanel.Visibility = Visibility.Visible;
                });

              //  RequestInternet();
            }
        }

        void SetNavList()
        {
            if (StaticData.currentUser == null)
            {
                SetOfflineNavList();
            }
            else
            {
                SetConnectedNavList();
            }
        }
        public Frame AppFrame { get { return this.frame; } }

        /// <summary>
        /// Default keyboard focus movement for any unhandled keyboarding
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppShell_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            FocusNavigationDirection direction = FocusNavigationDirection.None;
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Left:
                case Windows.System.VirtualKey.GamepadDPadLeft:
                case Windows.System.VirtualKey.GamepadLeftThumbstickLeft:
                case Windows.System.VirtualKey.NavigationLeft:
                    direction = FocusNavigationDirection.Left;
                    break;
                case Windows.System.VirtualKey.Right:
                case Windows.System.VirtualKey.GamepadDPadRight:
                case Windows.System.VirtualKey.GamepadLeftThumbstickRight:
                case Windows.System.VirtualKey.NavigationRight:
                    direction = FocusNavigationDirection.Right;
                    break;

                case Windows.System.VirtualKey.Up:
                case Windows.System.VirtualKey.GamepadDPadUp:
                case Windows.System.VirtualKey.GamepadLeftThumbstickUp:
                case Windows.System.VirtualKey.NavigationUp:
                    direction = FocusNavigationDirection.Up;
                    break;

                case Windows.System.VirtualKey.Down:
                case Windows.System.VirtualKey.GamepadDPadDown:
                case Windows.System.VirtualKey.GamepadLeftThumbstickDown:
                case Windows.System.VirtualKey.NavigationDown:
                    direction = FocusNavigationDirection.Down;
                    break;
            }

            if (direction != FocusNavigationDirection.None)
            {
                var control = FocusManager.FindNextFocusableElement(direction) as Control;
                if (control != null)
                {
                    control.Focus(FocusState.Programmatic);
                    e.Handled = true;
                }
            }
        }

        #region BackRequested Handlers

        private void SystemNavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            bool handled = e.Handled;
            this.BackRequested(ref handled);
            e.Handled = handled;
        }

    

        private void BackRequested(ref bool handled)
        {
            // Get a hold of the current frame so that we can inspect the app back stack.

            if (this.AppFrame == null)
                return;

            // Check to see if this is the top-most page on the app back stack.
            if (this.AppFrame.CanGoBack && !handled)
            {
                // If not, set the event to handled and go back to the previous page in the app.
                handled = true;
                this.AppFrame.GoBack();
            }
        }

        #endregion

     

        #region Navigation

        /// <summary>
        /// Navigate to the Page for the selected <paramref name="listViewItem"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="listViewItem"></param>
        /// 
        private async void NavMenuList_ItemInvoked(object sender, ListViewItem listViewItem)
        {
            var item = (NavMenuItem)((NavMenuListView)sender).ItemFromContainer(listViewItem);

            if (item != null)
            {
                if (item.DestinationPage != null)
                {
                    if (item.DestinationPage == typeof(Uri))
                    {
                        // Grab the URL from the argument
                        Uri url = null;
                        if (Uri.TryCreate(item.Arguments as string, UriKind.Absolute, out url))
                        {
                            await Launcher.LaunchUriAsync(url);
                        }
                    }
                    else if (item.DestinationPage != this.AppFrame.CurrentSourcePageType)
                    {
                        this.AppFrame.Navigate(item.DestinationPage, item.Arguments);
                    }
                }
            }
        }

        /// <summary>
        /// Ensures the nav menu reflects reality when navigation is triggered outside of
        /// the nav menu buttons.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNavigatingToPage(object sender, NavigatingCancelEventArgs e)
        {
            List<NavMenuItem> NavList;
            if (e.NavigationMode == NavigationMode.Back)
            {
                if (isConnected)
                    NavList = ConnectedNavlist;
                else
                    NavList = OfflineNavlist;

                var item = (from p in NavList where p.DestinationPage == e.SourcePageType select p).SingleOrDefault();
                if (item == null && this.AppFrame.BackStackDepth > 0)
                {
                    // In cases where a page drills into sub-pages then we'll highlight the most recent
                    // navigation menu item that appears in the BackStack
                    foreach (var entry in this.AppFrame.BackStack.Reverse())
                    {
                        item = (from p in NavList where p.DestinationPage == entry.SourcePageType select p).SingleOrDefault();
                        if (item != null)
                            break;
                    }
                }

                var container = (ListViewItem)NavMenuList.ContainerFromItem(item);

                // While updating the selection state of the item prevent it from taking keyboard focus.  If a
                // user is invoking the back button via the keyboard causing the selected nav menu item to change
                // then focus will remain on the back button.
                if (container != null) container.IsTabStop = false;
                NavMenuList.SetSelectedItem(container);
                if (container != null) container.IsTabStop = true;
            }
        }

        private void OnNavigatedToPage(object sender, NavigationEventArgs e)
        {
            // After a successful navigation set keyboard focus to the loaded page
            if (e.Content is Page && e.Content != null)
            {
                var control = (Page)e.Content;
                control.Loaded += Page_Loaded;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ((Page)sender).Focus(FocusState.Programmatic);
            ((Page)sender).Loaded -= Page_Loaded;
            this.CheckTogglePaneButtonSizeChanged();
        }

        #endregion

        public Rect TogglePaneButtonRect
        {
            get;
            private set;
        }

        /// <summary>
        /// An event to notify listeners when the hamburger button may occlude other content in the app.
        /// The custom "PageHeader" user control is using this.
        /// </summary>
        public event TypedEventHandler<AppShell, Rect> TogglePaneButtonRectChanged;

        /// <summary>
        /// Callback when the SplitView's Pane is toggled open or close.  When the Pane is not visible
        /// then the floating hamburger may be occluding other content in the app unless it is aware.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TogglePaneButton_Checked(object sender, RoutedEventArgs e)
        {
            this.CheckTogglePaneButtonSizeChanged();
        }

        /// <summary>
        /// Check for the conditions where the navigation pane does not occupy the space under the floating
        /// hamburger button and trigger the event.
        /// </summary>
        private void CheckTogglePaneButtonSizeChanged()
        {
            if (this.RootSplitView.DisplayMode == SplitViewDisplayMode.Inline ||
                this.RootSplitView.DisplayMode == SplitViewDisplayMode.Overlay)
            {
                var transform = this.TogglePaneButton.TransformToVisual(this);
                var rect = transform.TransformBounds(new Rect(0, 0, this.TogglePaneButton.ActualWidth, this.TogglePaneButton.ActualHeight));
                this.TogglePaneButtonRect = rect;
            }
            else
            {
                this.TogglePaneButtonRect = new Rect();
            }

            var handler = this.TogglePaneButtonRectChanged;
            if (handler != null)
            {
                 handler(this, this.TogglePaneButtonRect);
              //  handler.DynamicInvoke(this, this.TogglePaneButtonRect);
            }
        }

        /// <summary>
        /// Enable accessibility on each nav menu item by setting the AutomationProperties.Name on each container
        /// using the associated Label of each item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void NavMenuItemContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (!args.InRecycleQueue && args.Item != null && args.Item is NavMenuItem)
            {
                args.ItemContainer.SetValue(AutomationProperties.NameProperty, ((NavMenuItem)args.Item).Label);
            }
            else
            {
                args.ItemContainer.ClearValue(AutomationProperties.NameProperty);
            }
        }

       
        public void OnAccesToken()
        {
            NavMenuList.ItemsSource = ConnectedNavlist;
        }

        public void SetConnectedNavList()
        {
            NavMenuList.ItemsSource = ConnectedNavlist;
        }
        public void SetOfflineNavList()
        {
            NavMenuList.ItemsSource = OfflineNavlist;

        }
        public void SetNoInternetNavList()
        {
            NavMenuList.ItemsSource = NoInternetNavlist;

        }
        public static void ShowNotification(string Title, string Content)
        {
            var xmlToastTemplate = "<toast launch=\"app-defined-string\">" +
                                     "<visual>" +
                                       "<binding template =\"ToastGeneric\">" +
                                         "<text>" + Title + "</text>" +
                                         "<text>" +
                                           Content +
                                         "</text>" +
                                       "</binding>" +
                                     "</visual>" +
                                   "</toast>";

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlToastTemplate);
            var toastNotification = new ToastNotification(xmlDocument);
            var notification = ToastNotificationManager.CreateToastNotifier();
            notification.Show(toastNotification);
        }

        public async void SetupLocationListner()
        {
           // Debug.WriteLine("Setup : " + " " + (StaticData.currentUser != null) + " " + (StaticData.currentUser.IsAdmin) + " " + (StaticData.IsSharing));
            geolocator = new Geolocator();
            geolocator.ReportInterval = 4000;
            geolocator.DesiredAccuracyInMeters = 100;
            geolocator.MovementThreshold = 0.1;
            geolocator.StatusChanged += OnLocationStatusChanged;
            geolocator.PositionChanged += OnPositionChanged;
            this.AppFrame.Navigate(typeof(Map),null);

            GeolocationAccessStatus accessStatus = await Geolocator.RequestAccessAsync();
            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:

                    try
                    { 
                    Geoposition gp = await geolocator.GetGeopositionAsync();
                    StaticData.LastLocation = gp.Coordinate;
                        if (StaticData.currentUser != null)
                        {
                            if (StaticData.currentUser.IsAdmin && StaticData.IsSharing)
                            {
                                SocketHandler.EmitSharingOn();
                        }
                        }
                    
                    }
                    catch (Exception e)
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        {
                            var dialog = new MessageDialog("Couldn't locate your position");
                            await dialog.ShowAsync();
                        });
                        }
                    break;

                case GeolocationAccessStatus.Denied:
                    RequestLocation();
                    break;

                case GeolocationAccessStatus.Unspecified:
                    RequestLocation();
                    break;
            }

        }

        private async void OnPositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                StaticData.LastLocation = args.Position.Coordinate;
            });


        }

        public async void OnLocationStatusChanged(Geolocator sender, StatusChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, (DispatchedHandler)(() =>
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
                        this.RequestLocation();

                        // Location platform could not obtain location data.
                        break;

                    case PositionStatus.Disabled:

                        this.RequestLocation();
                        // Clear any cached location data.
                        break;

                    case PositionStatus.NotInitialized:
                        this.RequestLocation();

                        // The location platform is not initialized. This indicates that the application 
                        // has not made a request for location data.
                        break;

                    case PositionStatus.NotAvailable:
                        this.RequestLocation();

                        // The location platform is not available on this version of the OS.
                        break;

                    default:
                        // ScenarioOutput_Status.Text = "Unknown";
                        //rootPage.NotifyUser(string.Empty, NotifyType.StatusMessage);
                        break;
                }
            }));
        }
        public async void RequestLocation()
        {
            var messageDialog = new MessageDialog("Couldn't access location service.");

            messageDialog.Commands.Add(new UICommand(
                "enable",
                new UICommandInvokedHandler(this.locationRequestHandler)));
            messageDialog.Commands.Add(new UICommand(
                "cancel",
                new UICommandInvokedHandler(this.locationRequestHandler)));
            messageDialog.DefaultCommandIndex = 0;
            messageDialog.CancelCommandIndex = 1;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                try { 
                await messageDialog.ShowAsync();
            }
                catch (Exception e)
            {
                Debug.WriteLine("error " + e.Message + " " + e.Source);
            }
        });
        }
        public async void RequestInternet()
        {
            var messageDialog = new MessageDialog("No internet connection found! enter offline mode ?");

            messageDialog.Commands.Add(new UICommand(
                "yes",
                new UICommandInvokedHandler(this.internetRequestHandler)));
            messageDialog.Commands.Add(new UICommand(
                "no",
                new UICommandInvokedHandler(this.internetRequestHandler)));
            messageDialog.DefaultCommandIndex = 0;
            messageDialog.CancelCommandIndex = 1;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                
                StaticData.DialogAsyncOperation = null;
                StaticData.DialogAsyncOperation = messageDialog.ShowAsync();
                await StaticData.DialogAsyncOperation;
            });
        }
        public async void internetRequestHandler(IUICommand command)
        {
            switch (command.Label)
            {
                case "no":
                    bool result = await Launcher.LaunchUriAsync(new Uri("ms-settings:network-wifi"));
                    break;

                case "yes":
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        SetNoInternetNavList();
                    });
                    break;
                default:
                    break;
            }
        }
        public async void locationRequestHandler(IUICommand command)
        {
            switch (command.Label)
            {
                case "enable":
                    bool result = await Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-location"));

                    break;
                case "cancel":
                    break;
                default:
                    break;
            }
        }

        private bool HasInternetAccess()
        {
            var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            return  (connectionProfile != null &&
                                 connectionProfile.GetNetworkConnectivityLevel() ==
                                 NetworkConnectivityLevel.InternetAccess);
        }

        public static async Task<Geocoordinate> RequestLocationAsync(bool IgnoreNull=true)
        {
            Geocoordinate gCoord = null;
            GeolocationAccessStatus accessStatus = await Geolocator.RequestAccessAsync();
            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                        Geoposition gp = await StaticData.appShell.geolocator.GetGeopositionAsync();
                        StaticData.LastLocation = gp.Coordinate;
                    break;

                case GeolocationAccessStatus.Denied:
                    if(IgnoreNull)
                    {

                    }
                        break;

                case GeolocationAccessStatus.Unspecified:
                    if (IgnoreNull)
                    {

                    }
                    break;
            }
            return gCoord;
        }
    }
}
