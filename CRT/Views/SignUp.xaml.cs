using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using CRT.Controls;
using Windows.UI.Popups;
using Windows.UI.Core;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CRT.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SignUp : Page
    {
        public SignUp()
        {
            this.InitializeComponent();
        }

        private async void Accept_Click(object sender, RoutedEventArgs e)
        {
            Task<WebserviceHandler.IUserAdded> task;
            task = WebserviceHandler.AddUser(textBox_Email.Text, passwordBox.Password, textBoxFirstName.Text, textBox_Lastname.Text, Birth.Date.Day + "/" + Birth.Date.Month + "/" + Birth.Date.Year);
            try
            {
                await Task.WhenAll(task);
                WebserviceHandler.IUserAdded userAddResult = task.Result;
                if (userAddResult.success)
                {
                    StaticData.appShell.AppFrame.Navigate(typeof(Login));

                }
                else
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        var dialog = new MessageDialog("An Error Has occured");
                        await dialog.ShowAsync();
                    });
                }
            }
            catch (Exception ex)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    var dialog = new MessageDialog("An Error Has occured");
                    await dialog.ShowAsync();
                });
            }
               

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            StaticData.appShell.AppFrame.Navigate(typeof(Login));

        }
    }
}
