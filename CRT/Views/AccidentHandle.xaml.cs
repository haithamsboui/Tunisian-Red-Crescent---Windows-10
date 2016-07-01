using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CRT.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AccidentHandle : Page
    {
        Models.Accident acc;
        public AccidentHandle()
        {
            this.InitializeComponent();
          
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var parameter = e.Parameter as Models.Accident;
            
            acc = parameter;
            if (acc.Description!=null)
            this.text_description.Text = acc.Description;
            if (acc.ImageFile != null)

                this.image.Source = new BitmapImage(new Uri(acc.ImageFile));
        }
        private  void button_Confirm_Click(object sender, RoutedEventArgs e)
        {
           

        }

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Task<bool> task = Controls.WebserviceHandler.HandleAccident(acc.id, Controls.StaticData.accessToken);
            await Task.WhenAll<bool>(task);
            bool res = task.Result;
            if (res)
            {
                Controls.StaticData.appShell.AppFrame.Navigate(typeof(Map), null);
                var uriCrt = new Uri(@"ms-drive-to:?destination.latitude=" + acc.Location.Latitude + "&destination.longitude=" + acc.Location.Longitude);
                var success = await Windows.System.Launcher.LaunchUriAsync(uriCrt);
            }
            else
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    var dialog = new MessageDialog("An error has occured ! Please try again .");
                    await dialog.ShowAsync();
                });
            }
        }

        private void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            Controls.StaticData.appShell.AppFrame.Navigate(typeof(Map), null);

        }
    }
    }
