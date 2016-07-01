using CRT.Controls;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CRT.Views
{

    public sealed partial class Initialize : Page
    {
        SocketHandler socketHandler=null;
        public Initialize()
        {
            this.InitializeComponent();
             socketHandler = new SocketHandler();

            socketHandler.socket.On("access_token", OnAccessToken);
            socketHandler.socket.On(Quobject.SocketIoClientDotNet.Client.Socket.EVENT_CONNECT_TIMEOUT, OnTimeOut);
            NetworkInformation.NetworkStatusChanged += NetworkStatusChanged;
            if (HasInternetAccess())
            {
                ValidateLocalStorage();
            }
            else
            {
                SetupOfflineUI();
                // RequestInternet();
                // NoInternetHandler("Setting up offline mode ..."); 
           
            }
            
        }
        public async void SetupOfflineUI()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                AppShell shell = new AppShell(true);
                Window.Current.Content = shell;
                Window.Current.Activate();
            });
        }
        public async void NoInternetHandler(string message)
        {
            var dialog = new MessageDialog(message);
            dialog.Commands.Add(new UICommand(
                "Close",
                new UICommandInvokedHandler(this.NointernetRequestHandler)));
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async() =>
            {
                await dialog.ShowAsync();
            });
        }
        public async void NointernetRequestHandler(IUICommand command)
        {
            switch (command.Label)
            {
                case "Close":
                  

                
                default:
                    break;
            }
        }
        public void OnTimeOut(object data)
        {
           // NoInternetHandler("Connexion time out. \n Setting up offline mode ...");
           
            socketHandler.socket.Off(Quobject.SocketIoClientDotNet.Client.Socket.EVENT_CONNECT_TIMEOUT);
        }
        private async void NetworkStatusChanged(object sender)
        {
            if (HasInternetAccess())
            {
                socketHandler.socket.Connect();
                ValidateLocalStorage();
            }
            else
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {

                    AppShell shell = new AppShell(true); ;
                    Window.Current.Content = shell;
                    Window.Current.Activate();
                });
                //RequestInternet();
            }
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
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                try
                {
                    StaticData.DialogAsyncOperation = messageDialog.ShowAsync();
                }
                catch (Exception e)
                {
                    Debug.WriteLine("error " + e.Message + " " + e.Source);
                }
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
                        AppShell shell = new AppShell(true); ;
                        Window.Current.Content = shell;
                        Window.Current.Activate();
                    });
                    break;
                default:
                    break;
            }
        }
        private bool HasInternetAccess()
        {
            Debug.WriteLine("Testing internet");

            var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            return (connectionProfile != null &&
                                 connectionProfile.GetNetworkConnectivityLevel() ==
                                 NetworkConnectivityLevel.InternetAccess);
        }

        public async void OnAccessToken(object data)
        {
            Debug.WriteLine("Token received");

            string msg = (string)data.ToString();
            AccessTokenResponse Response = JsonConvert.DeserializeObject<AccessTokenResponse>(msg);
            if(!Response.success)
            {
                StaticData.accessToken = "";
                StaticData.currentUser = null;               
            }
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                AppShell shell = new AppShell(); ;
                Window.Current.Content = shell;
                Window.Current.Activate();
            });
        }
        async void ValidateLocalStorage()
        {
            Debug.WriteLine("Testing Local storage");

            string token = IsolatedStorageHelper.GetObject<string>("access_token");
            StaticData.accessToken = token;
            Task<Models.User> UserTask=null;
               UserTask = WebserviceHandler.GetUserByID("me", token);
                await Task.WhenAll(UserTask);
            if (UserTask.Result == null)
            {
                Debug.WriteLine("User not found");
                StaticData.currentUser = null;
                StaticData.accessToken = "";
                StaticData.IsSharing = false;
                IsolatedStorageHelper.SaveObject<bool>("IsSharing",false);

            }
            else
            {
                Debug.WriteLine("User found");

                StaticData.currentUser = UserTask.Result;
                StaticData.accessToken = token;
                if (StaticData.currentUser.IsAdmin)
                {
                    StaticData.IsSharing = IsolatedStorageHelper.GetObject<bool>("IsSharing");


                }
               
            }
            Debug.WriteLine("Sending Token");
            StaticData.socket.Emit("access_token", token);
        }
        public class AccessTokenResponse
        {
            public bool success { get; set; }
            public string message { get; set; }
        }

    }
}
