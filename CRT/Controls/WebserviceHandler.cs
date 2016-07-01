using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CRT.Models;
using Windows.Security.Authentication.Web;
using Windows.Storage;
using System.IO;
using System.Net.Http.Headers;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;

namespace CRT.Controls
{
    public class WebserviceHandler
    {
        private static string BaseURL = "https://crt-server-ibicha.c9users.io/api/v1/";
        //Post
        public static async Task<IAuthenticate> Authenticate(string email, string password)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                  new KeyValuePair<string, string>("Email",email),
                  new KeyValuePair<string, string>("Password", password)
            });

            var myHttpClient = new HttpClient();
            var response = await myHttpClient.PostAsync(BaseURL + "/authenticate", formContent);
            var body = await response.Content.ReadAsStringAsync();
            IAuthenticate auth = new IAuthenticate();
            auth = JsonConvert.DeserializeObject<IAuthenticate>(body);
            return auth;
        }
        public static async Task<IUserAdded> AddUser(string email, string password,string username,string lastname,string birth)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                  new KeyValuePair<string, string>("Email",email),
                  new KeyValuePair<string, string>("Password", password),
                  new KeyValuePair<string, string>("FirstName", username),
                  new KeyValuePair<string, string>("LastName", lastname),
                  new KeyValuePair<string, string>("BirthDate", birth)

            });

            var myHttpClient = new HttpClient();
            var response = await myHttpClient.PostAsync(BaseURL + "/user/add", formContent);
            var body = await response.Content.ReadAsStringAsync();
            IUserAdded auth = new IUserAdded();
            auth = JsonConvert.DeserializeObject<IUserAdded>(body);
            return auth;
        }

        /*
      public static async Task<User> AuthenticateFB(string Token)
      {
        HttpClient httpClient = new HttpClient();
          var headers = httpClient.DefaultRequestHeaders;
          Uri requestUri = new Uri(BaseURL + "user/" + ID + "/?access_token=" + Token);
          Debug.WriteLine(requestUri.AbsolutePath + " " + requestUri.AbsoluteUri);
          HttpResponseMessage httpResponse = new HttpResponseMessage();
          string httpResponseBody = "";
          User user = null;
          try
          {
              httpResponse = await httpClient.GetAsync(requestUri);
              httpResponse.EnsureSuccessStatusCode();
              httpResponseBody = await httpResponse.Content.ReadAsStringAsync();

              user = JsonConvert.DeserializeObject<User>(httpResponseBody);

          }
          catch (Exception ex)
          {
              httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
          }
          Debug.WriteLine(httpResponseBody);

          return user;
         

    }
     */
        public static async Task<User> GetUserByID(string ID, string Token)
        {
            HttpClient httpClient = new HttpClient();
            var headers = httpClient.DefaultRequestHeaders;
            Uri requestUri = new Uri(BaseURL + "user/" + ID + "/?access_token=" + Token);
            HttpResponseMessage httpResponse = new HttpResponseMessage();
            string httpResponseBody = "";
            User user = null;
            try
            {
                httpResponse = await httpClient.GetAsync(requestUri);
                httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
                if (JObject.Parse(httpResponseBody).GetValue("success").ToObject<bool>())
                    user = JsonConvert.DeserializeObject<User>(JObject.Parse(httpResponseBody).GetValue("user").ToString());
                else
                    user = null;
            }
            catch (Exception ex)
            {
                httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
                Debug.WriteLine(httpResponseBody);
                user = null;
            }

            return user;


        }
        
        public static async Task<List<CrtLocal>> GetCrtPlaces(string token)
        {
            HttpClient httpClient = new HttpClient();
            var headers = httpClient.DefaultRequestHeaders;
            Uri requestUri = new Uri(BaseURL + "crtplaces/" + "?access_token=" + token);
            HttpResponseMessage httpResponse = new HttpResponseMessage();
            string httpResponseBody = "";
            List<CrtLocal> CrtPlaces = new List<CrtLocal>();
            try
            {
                httpResponse = await httpClient.GetAsync(requestUri);
              //  httpResponse.EnsureSuccessStatusCode();
                httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
                JToken obj = JObject.Parse(httpResponseBody).GetValue("crtplaces");
                CrtPlaces = JsonConvert.DeserializeObject<List<CrtLocal>>(obj.ToString());
            }
            catch (Exception ex)
            {
                httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
                Debug.WriteLine(httpResponseBody);
            }

            return CrtPlaces;
        }

        public static async Task<List<Accident>> GetAccidents(string token)
        {
            HttpClient httpClient = new HttpClient();
            var headers = httpClient.DefaultRequestHeaders;
            Uri requestUri = new Uri(BaseURL + "accidents/" + "?access_token=" + token);
            HttpResponseMessage httpResponse = new HttpResponseMessage();
            string httpResponseBody = "";
            List<Accident> Accidents = new List<Accident>();
            try
            {
                httpResponse = await httpClient.GetAsync(requestUri);
           //     httpResponse.EnsureSuccessStatusCode();
                httpResponseBody = await httpResponse.Content.ReadAsStringAsync();

                JToken obj = JObject.Parse(httpResponseBody).GetValue("accidents");
                Accidents = JsonConvert.DeserializeObject<List<Accident>>(obj.ToString());


            }
            catch (Exception ex)
            {
                httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            }
            List<Accident> AccidentsNotHandled = new List<Accident>();
            foreach (var acc in Accidents)
            {
                

                if (!acc.IsHandled)
                    AccidentsNotHandled.Add(acc);
            }

            return AccidentsNotHandled;
        }
        public class IAuthenticate
        {
            public bool success
            {
                get;
                set;
            }
            public string token
            {
                get;
                set;
            }
            public string UserID
            {
                get;
                set;
            }
        }
        public class IUserAdded
        {
            public bool success
            {
                get;
                set;
            }
            public string message
            {
                get;
                set;
            }
            public string UserID
            {
                get;
                set;
            }
        }
        public class IAccidentReport
        {
            public bool success
            {
                get;
                set;
            }
            public string message
            {
                get;
                set;
            }
        }

            public static async Task<IAccidentReport> ReportAccident(string Description, Stream image,string Location,StorageFile file)
        {

            IInputStream inputStream = await file.OpenAsync(FileAccessMode.Read);
            Windows.Web.Http.HttpMultipartFormDataContent multipartContent =
                new Windows.Web.Http.HttpMultipartFormDataContent();

            multipartContent.Add(
                new Windows.Web.Http.HttpStreamContent(inputStream),
                "ImageFile",
                file.Name);
            multipartContent.Add(
                new Windows.Web.Http.HttpStringContent(Description),
                "Description");
            multipartContent.Add(
              new Windows.Web.Http.HttpStringContent(Location),
              "Location");
           Windows.Web.Http.HttpClient client = new Windows.Web.Http.HttpClient();
            client.DefaultRequestHeaders.Add("access_token", StaticData.accessToken);
            Windows.Web.Http.HttpResponseMessage response = await client.PostAsync(
                new Uri (BaseURL + "/accident/report"),
                multipartContent);
            var body = await response.Content.ReadAsStringAsync();
            IAccidentReport auth = new IAccidentReport();
            auth = JsonConvert.DeserializeObject<IAccidentReport>(body);
            return auth;
        }

        public static async Task<bool> HandleAccident(string ID, string Token)
        {
            HttpClient httpClient = new HttpClient();
            var headers = httpClient.DefaultRequestHeaders;
            Uri requestUri = new Uri(BaseURL + "accident/handle/" + ID + "?access_token=" + Token);
            HttpResponseMessage httpResponse = new HttpResponseMessage();
            string httpResponseBody = "";
            bool result = false;
            try
            {
                httpResponse = await httpClient.GetAsync(requestUri);
                httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
                if (JObject.Parse(httpResponseBody).GetValue("success").ToObject<bool>())
                    result = true;
               
            }
            catch (Exception ex)
            {
                httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
                Debug.WriteLine(httpResponseBody);
            
            }

            return result;


        }

    }
}
