using CRT.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

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
            Firstname.Text = StaticData.currentUser.Firstname;
            lastname.Text = StaticData.currentUser.Lastname;
            email.Text = StaticData.currentUser.Email;
            Birth.Date = StaticData.currentUser.BirthDate.Date;
            
            SetImage();
        }
        public async void SetImage()
        {
           /* Task <BitmapImage> task = LoadImage(new Uri(StaticData.currentUser.ImageFile));
            await Task.WhenAll(task);
            BitmapImage img = task.Result;
            Debug.WriteLine("uri : " + img.PixelHeight+ " uri : "+img.PixelWidth);*/
            image.Source = new BitmapImage(new Uri(StaticData.currentUser.ImageFile, UriKind.Absolute)); ;
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
