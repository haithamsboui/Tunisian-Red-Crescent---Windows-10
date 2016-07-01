using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CRT.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EmergencyGuide : Page
    {
        List<Models.ListItem> ListItems;
        List<Models.ListItem> ListItemsDetails;
        public EmergencyGuide()
        {

            this.InitializeComponent();

            ListItems = SetupListItem();
            LoadingWebview.IsIndeterminate = false;
            listView.ItemsSource = ListItems;
            listView.ItemClick += OnItemClick;
            listView.Visibility = Visibility.Visible;
            Details.Visibility = Visibility.Collapsed;
            /*string videoID = "tYrND5hMY3A";
           string html = @"<iframe  width=""1280"" height =""720""  src =""http://www.youtube.com/embed/tYrND5hMY3A?rel=0"" frameborder =""0"" allowfullscreen ></iframe>
           ";
           youtubePlayer.NavigateToString(html);*/

        }

        private void listView_ItemClick(object sender, ItemClickEventArgs e)
        {

        }
        public List<Models.ListItem> SetupListItem()
        {
            List<Models.ListItem> items = new List<Models.ListItem>();
            items.Add(new Models.ListItem { image= "ms-appx:///Assets/broken-bone-icon.png", text="Emergency Calls" });
            items.Add(new Models.ListItem { image = "ms-appx:///Assets/bleed-icon.png", text = "Bleeding" });
            items.Add(new Models.ListItem { image = "ms-appx:///Assets/broken-bone.png", text = "Broken Bone" });
            items.Add(new Models.ListItem { image = "ms-appx:///Assets/choking-icon.png", text = "Choking" });
            items.Add(new Models.ListItem { image = "ms-appx:///Assets/diabetic-icon.png", text = "Diabetic" });
            items.Add(new Models.ListItem { image = "ms-appx:///Assets/headinjury-icon.png", text = "Head injury" });
            items.Add(new Models.ListItem { image = "ms-appx:///Assets/poisoning-icon.png", text = "Poisoning" });
            items.Add(new Models.ListItem { image = "ms-appx:///Assets/stroke-icon.png", text = "Stroke" });
            items.Add(new Models.ListItem { image = "ms-appx:///Assets/unconscious-not-breathing-icon.png", text = "Unconscious not breathing" });
            return items;
        }


        private void youtubePlayer_Loading(FrameworkElement sender, object args)
        {
            LoadingWebview.IsIndeterminate = true;
        }
    
        private void youtubePlayer_LoadCompleted(object sender, NavigationEventArgs e)
        {
            LoadingWebview.IsIndeterminate = false;

        }

        public List<Models.ListItem> SetupListDetailsItem(string indexName)
        {
            List<Models.ListItem> items = new List<Models.ListItem>();
            youtubePlayer.Visibility = Visibility.Visible;
            youtubePlayer.Loading += youtubePlayer_Loading;
            youtubePlayer.LoadCompleted += youtubePlayer_LoadCompleted;
            switch (indexName)
            {
                case "Emergency Calls":
                    youtubePlayer.Visibility = Visibility.Collapsed;
                    items.Add(new Models.ListItem { image = "ms-appx:///Assets/police-station.png", text = "197" });
                    items.Add(new Models.ListItem { image = "ms-appx:///Assets/firefighters-icon-23274.png", text = "190" });
                    items.Add(new Models.ListItem { image = "ms-appx:///Assets/ambulance.png", text = "198" });
                    items.Add(new Models.ListItem { image = "ms-appx:///Assets/ambulance1.png", text = "+21671320630" });
                    break;
                case "Bleeding":
                  
                    youtubePlayer.Navigate(new Uri("http://www.youtube.com/embed/BQRqUxB5pn0"));
                    items.Add(new Models.ListItem { image = "ms-appx:///Assets/alert_icon.png", text = "Put pressure on the wound with whatever is available to stop or slow down the flow of blood" });
                    items.Add(new Models.ListItem { image = "ms-appx:///Assets/alert_icon.png", text = "Call 999 as soon as possible, or get someone else to do it." });
                    items.Add(new Models.ListItem { image = "ms-appx:///Assets/alert_icon.png", text = "Keep pressure on the wound until help arrives." });

                    break;
                case "Broken Bone":
                    youtubePlayer.Navigate(new Uri("http://www.youtube.com/embed/dVqhZTBV3vI"));

                    items.Add(new Models.ListItem { image = "ms-appx:///Assets/alert_icon.png", text = "Encourage the person to support the injury with their hand, or use a cushion or items of clothing to prevent unnecessary movement." });
                    items.Add(new Models.ListItem { image = "ms-appx:///Assets/alert_icon.png", text = "As soon as possible, call 999 or get someone else to do it." });
                    items.Add(new Models.ListItem { image = "ms-appx:///Assets/alert_icon.png", text = "Continue supporting the injury until help arrives." });
                    break;
                case "Choking":
                    youtubePlayer.Navigate(new Uri("http://www.youtube.com/embed/6xbQdKXXXlY"));

                    items.Add(new Models.ListItem { image = "ms-appx:///Assets/alert_icon.png", text = "Hit them firmly on their back between the shoulder blades to dislodge the object." });
                    items.Add(new Models.ListItem { image = "ms-appx:///Assets/alert_icon.png", text = "If necessary, call 999 or get someone else to do it." });

                    break;
                case "Diabetic":
                    youtubePlayer.Navigate(new Uri("http://www.youtube.com/embed/wj5_ruu6MYc"));

                    items.Add(new Models.ListItem { image = "ms-appx:///Assets/alert_icon.png", text = "Give them something sweet to eat or a non-diet drink." });
                    items.Add(new Models.ListItem { image = "ms-appx:///Assets/alert_icon.png", text = "Reassure the person. Most people will gradually improve, but if in doubt, call 999 or get someone else to do it." });
                    break;
                case "Head injury":
                    youtubePlayer.Navigate(new Uri("http://www.youtube.com/embed/OqDdLUi7kkA"));

                    items.Add(new Models.ListItem { image = "ms-appx:///Assets/alert_icon.png", text = "Ask them to rest and apply something cold to the injury (e.g. frozen vegetables wrapped in a tea towel)." });
                    items.Add(new Models.ListItem { image = "ms-appx:///Assets/alert_icon.png", text = "If they become drowsy or vomit, call 999 or get someone else to do it." });
                    break;
                case "Poisoning":
                    youtubePlayer.Navigate(new Uri("http://www.youtube.com/embed/L4o6D10AkMg"));

                    items.Add(new Models.ListItem { image = "ms-appx:///Assets/alert_icon.png", text = "Establish what they have taken. When? And how much? " });
                    items.Add(new Models.ListItem { image = "ms-appx:///Assets/alert_icon.png", text = "As soon as possible, call 999 or get someone else to do it." });
                    items.Add(new Models.ListItem { image = "ms-appx:///Assets/alert_icon.png", text = "Do not make the person sick." });
                    break;
                case "Unconscious not breathing":
                    youtubePlayer.Navigate(new Uri("http://www.youtube.com/embed/hHyVQiWFXzg"));

                    items.Add(new Models.ListItem { image = "ms-appx:///Assets/alert_icon.png", text = "Check breathing by tilting their head backwards and looking and feeling for breaths. " });
                    items.Add(new Models.ListItem { image = "ms-appx:///Assets/alert_icon.png", text = "Call 999 as soon as possible, or get someone else to do it." });
                    items.Add(new Models.ListItem { image = "ms-appx:///Assets/alert_icon.png", text = "Push firmly downwards in the middle of the chest and then release." });
                    items.Add(new Models.ListItem { image = "ms-appx:///Assets/alert_icon.png", text = "Push at a regular rate until help arrives." });

                    break;
               
                default:
                    break;
            }
            return items;
        }

       
        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void OnItemClick(object sender, ItemClickEventArgs e)
        {
            listView.Visibility = Visibility.Collapsed;
            Details.Visibility = Visibility.Visible;
            ListItemsDetails = SetupListDetailsItem(((Models.ListItem)e.ClickedItem).text);
            HelpList.ItemsSource = ListItemsDetails;
        }
        private void listViewDetail_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (listView.SelectedIndex==0)
            {
                string phone="";
                string phoneName="";
                switch (HelpList.SelectedIndex)
                {
                    case 1:
                        phone = "197";
                        phoneName = "Police";
                        break;
                    case 2:
                        phone = "190";
                        phoneName = "Civil protection";
                        break;
                    case 3:
                        phone = "198";
                        phoneName = "Ambulence";
                        break;
                    case 4:
                        phone = "0021671320630";
                        phoneName = "Red Crescent Local";
                        break;
                    default:
                        break;
                }
                Windows.ApplicationModel.Calls.PhoneCallManager.ShowPhoneCallUI(phone, phoneName);
            }
        }

        
        private void button_Click(object sender, RoutedEventArgs e)
        {
            listView.Visibility = Visibility.Visible;
            Details.Visibility = Visibility.Collapsed;
            
            youtubePlayer.NavigateToString("<html><body></body></ html >");
        }
    }
}
