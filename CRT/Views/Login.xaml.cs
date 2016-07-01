using CRT.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using winsdkfb;
using winsdkfb.Graph;
using CRT.Controls;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CRT.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Login : Page
    {
        public Login()
        {
            this.InitializeComponent();

        }

        private void textBox_Copy_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            BlackScreenLogin.Visibility = Visibility.Visible;
            LoadingLogin.Visibility = Visibility.Visible;
            LoadingLogin.IsActive = true;
            Task<WebserviceHandler.IAuthenticate> LoginTask = WebserviceHandler.Authenticate(Email.Text, Password.Password);
            await Task.WhenAll(LoginTask);
            WebserviceHandler.IAuthenticate LoginResult = LoginTask.Result;
            if (LoginResult.success)
            {
                StaticData.accessToken = LoginResult.token;
                Task<User> UserTask = WebserviceHandler.GetUserByID("me", LoginResult.token);
                await Task.WhenAll(UserTask);
                
                StaticData.currentUser = UserTask.Result;
                StaticData.socket.Emit("access_token", LoginResult.token);
                if (UserTask.Result.IsAdmin) { 
                if ( StaticData.LastLocation!= null)
                {
                        IsolatedStorageHelper.SaveObject<bool>("IsSharing", true);

                        SocketHandler.EmitSharingOn();
                }
                }
                if (RememberMe.IsChecked==true)
                {
                    IsolatedStorageHelper.SaveObject<string>("access_token", LoginResult.token);

                }
                else
                {
                    IsolatedStorageHelper.SaveObject<string>("access_token", "");
                    IsolatedStorageHelper.SaveObject<bool>("IsSharing", false);

                }
                BlackScreenLogin.Visibility = Visibility.Collapsed;
                LoadingLogin.Visibility = Visibility.Collapsed;
                LoadingLogin.IsActive = false;

                Controls.StaticData.appShell.SetConnectedNavList();
                StaticData.appShell.AppFrame.Navigate(typeof(Map));
            }
        }

        private async void LoginFb_Click(object sender, RoutedEventArgs e)
        {
            FBSession sess1 = FBSession.ActiveSession;
            sess1.FBAppId = "686873071454910";
            sess1.WinAppId = WebAuthenticationBroker.GetCurrentApplicationCallbackUri().ToString();

            await sess1.LogoutAsync();
            FBSession sess = FBSession.ActiveSession;
            sess.FBAppId = "686873071454910";
            sess.WinAppId = WebAuthenticationBroker.GetCurrentApplicationCallbackUri().ToString();
            List<String> permissionList = new List<String>();
            permissionList.Add("public_profile");
            permissionList.Add("user_friends");
            permissionList.Add("user_likes");
            permissionList.Add("user_location");
            permissionList.Add("user_photos");
            permissionList.Add("publish_actions");
            FBPermissions permissions = new FBPermissions(permissionList);

            // Login to Facebook
            FBResult result = await sess.LoginAsync(permissions);
            if (result.Succeeded)
            {
                FBUser user = sess.User;
                ProfilePic.UserId = sess.User.Id;
                Controls.StaticData.appShell.SetConnectedNavList();
            }
            else
            {
                Debug.WriteLine("err " + result.ErrorInfo.Message+"  "+ result.ErrorInfo.ErrorUserMessage);
            }
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void SignUp_Click(object sender, RoutedEventArgs e)
        {
            StaticData.appShell.AppFrame.Navigate(typeof(SignUp));

        }
    }
}
