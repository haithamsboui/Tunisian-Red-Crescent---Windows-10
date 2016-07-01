using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using System.Diagnostics;
using CRT.Controls;
using Windows.Devices.Geolocation;
using Windows.UI.Core;
using Windows.UI.Popups;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CRT.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ReportAccident : Page
    {
        byte[] fileBytes;
        private IRandomAccessStream imageStream;

        public byte[] bytes { get; private set; }
        public Stream streams { get; private set; }
        public StorageFile photo { get; private set; }

        public ReportAccident()
        {
            this.InitializeComponent();
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            CameraCaptureUI captureUI = new CameraCaptureUI();
            captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            captureUI.PhotoSettings.CroppedSizeInPixels = new Size(200, 200);

             photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);
            
            if (photo == null)
            {
                Debug.WriteLine("Nulll");
                // User cancelled photo capture
                return;
            }
            else
            {

                 streams = await photo.OpenStreamForReadAsync ();
                 bytes = new byte[(int)streams.Length];
                streams.Read(bytes, 0, (int)streams.Length);

                using (var fileStream = await photo.OpenStreamForReadAsync())
                {
                    var binaryReader = new BinaryReader(fileStream);
                    fileBytes = binaryReader.ReadBytes((int)fileStream.Length);
                }
                

                IRandomAccessStream stream = await photo.OpenAsync(FileAccessMode.Read);
                imageStream = stream;
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                SoftwareBitmap softwareBitmapBGR8 = SoftwareBitmap.Convert(softwareBitmap,
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Premultiplied);

                SoftwareBitmapSource bitmapSource = new SoftwareBitmapSource();
                LoadingImg.IsActive = true;
                LoadingImg.Visibility = Visibility.Visible;
                await bitmapSource.SetBitmapAsync(softwareBitmapBGR8);
                LoadingImg.IsActive = false;
                LoadingImg.Visibility = Visibility.Collapsed;
                image.Source = bitmapSource;

             }
        }

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            Geocoordinate gCoord = StaticData.LastLocation;
            Load.Visibility = Visibility.Visible;
            if (gCoord != null)
            {
                RandomAccessStreamReference rasr = RandomAccessStreamReference.CreateFromStream(imageStream);
                var streamWithContent = await rasr.OpenReadAsync();
                byte[] buffer = new byte[streamWithContent.Size];
                await streamWithContent.ReadAsync(buffer.AsBuffer(), (uint)streamWithContent.Size, InputStreamOptions.None);
                string loc = "{" +
                             "\"Longitude\":" + gCoord.Point.Position.Longitude +
                             ",\"Latitude\":" + gCoord.Point.Position.Latitude +
                             ",\"Accuracy\":" + gCoord.Accuracy +
                             ",\"Timestamp\":" + DateTime.Now.Ticks +
                             "}";
                Task<Controls.WebserviceHandler.IAccidentReport> task = Controls.WebserviceHandler.ReportAccident(Description.Text, streams, loc,photo);
                try { 
                await Task.WhenAll(task);
                    WebserviceHandler.IAccidentReport ack = task.Result;
                    if (ack.success)
                    StaticData.appShell.AppFrame.Navigate(typeof(Map));
                }
                catch (Exception exception)
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        var dialog = new MessageDialog("An error has occured");
                        await dialog.ShowAsync();
                    });
                }
            }
            else
            {
                var dialog = new MessageDialog("Couldn't report accident . Location not accessible !");
                await dialog.ShowAsync();
                Load.Visibility = Visibility.Collapsed;

                /* BlackScreen.Visibility = Visibility.Collapsed;
                 Loading.Visibility = Visibility.Collapsed;
                 Loading.IsActive = false;
                 StaticData.appShell.AppFrame.Navigate(typeof(Map));*/

            }



        }



    }
}
