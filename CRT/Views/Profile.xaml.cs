using CRT.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml;
namespace CRT.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Profile : Page
    {
        public Profile()
        {
            this.InitializeComponent();
            toggleSwitch.IsOn= IsolatedStorageHelper.GetObject<bool>("IsSharing"); 
            Firstname.Text = (StaticData.currentUser.Firstname+" "+ StaticData.currentUser.Lastname).ToUpper();
          
            email.Text = StaticData.currentUser.Email;
            Birthdate.Text = StaticData.currentUser.BirthDate.Date.Day+"/"+ 
                StaticData.currentUser.BirthDate.Date.Month + "/"
                + StaticData.currentUser.BirthDate.Date.Year;
            SetImage();
            if(!StaticData.currentUser.IsAdmin)
            {
                toggleSwitch.Visibility = Visibility.Collapsed;
            }
          //  toggleSwitch.DataContextChanged += SharingChanged;
            toggleSwitch.Toggled += ToggleChange;
        }

        private void ToggleChange(object sender, RoutedEventArgs e)
        {
            if (toggleSwitch.IsOn)
            {
                SocketHandler.EmitSharingOn();
                IsolatedStorageHelper.SaveObject<bool>("IsSharing", true);

            }
            else
            {
           

                SocketHandler.EmitSharingOFF();
                IsolatedStorageHelper.SaveObject<bool>("IsSharing", false);

            }
        }

        private void SharingChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            
            if (toggleSwitch.IsOn)
            {
                SocketHandler.EmitSharingOn();
            }
            else
            {

                SocketHandler.EmitSharingOFF();
            }
        }

      
        public void SetImage()
        {
      
            BitmapImage img = new BitmapImage(new Uri(StaticData.currentUser.ImageFile, UriKind.Absolute)); ;
            img.DecodePixelHeight = 200;
            img.DecodePixelWidth = 200;
            image.ImageSource = img; 
        }


        private void textBlock2_SelectionChanged(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }

        private void textBlock_Copy3_SelectionChanged(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }

        private void email_SelectionChanged(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }

        private void toggleSwitch_Toggled(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }
        public async static Task<BitmapImage> LoadImage(Uri uri)
        {
            BitmapImage bitmapImage = new BitmapImage();

            try
            {
                using (Windows.Web.Http.HttpClient client = new Windows.Web.Http.HttpClient())
                {
                    using (var response = await client.GetAsync(uri))
                    {
                        response.EnsureSuccessStatusCode();

                        using (MemoryStream inputStream = new MemoryStream())
                        {
                            await inputStream.CopyToAsync(inputStream);
                            bitmapImage.SetSource(inputStream.AsRandomAccessStream());
                        }
                    }
                }
                return bitmapImage;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to load the image: {0}", ex.Message);
            }

            return null;
        }
    }

}
