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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CRT.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Tuto : Page
    {
        int navIndex;
        public Tuto()
        {
            this.InitializeComponent();
            navIndex = 0;
        }

        private void next_Click(object sender, RoutedEventArgs e)
        {
            if (navIndex < 4)
                navIndex += 1;
            HandleNavigation();
        }
        private void back_Click(object sender, RoutedEventArgs e)
        {
            if (navIndex >= 1)
                navIndex -= 1;
            HandleNavigation();
        }
        private void start_click(object sender, RoutedEventArgs e)
        {
            Initialize shell = new Initialize (); ;
            Window.Current.Content = shell;
            Window.Current.Activate();
        }
        private  void HandleNavigation()
        {

            hideAll();
            BackBar.Visibility = Visibility.Visible;
            NextBar.Visibility = Visibility.Visible;

            switch (navIndex)
            {
                case 0:
                    Welcome.Visibility = Visibility.Visible;
                    BackBar.Visibility = Visibility.Collapsed;

                    break;
                case 1:
                    Overview.Visibility = Visibility.Visible;
                    break;
                case 2:
                    emergency_guide.Visibility = Visibility.Visible;
                    break;
                case 3:
                    report_accident.Visibility = Visibility.Visible;
                    break;
                case 4:
                    start.Visibility = Visibility.Visible;
                    NextBar.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void hideAll()
        {
            Welcome.Visibility = Visibility.Collapsed;
            Overview.Visibility = Visibility.Collapsed;
            emergency_guide.Visibility = Visibility.Collapsed;
            report_accident.Visibility = Visibility.Collapsed;
            start.Visibility = Visibility.Collapsed;
        }

    }
}
