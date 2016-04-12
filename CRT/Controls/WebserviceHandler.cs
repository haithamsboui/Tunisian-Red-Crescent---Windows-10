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
using Facebook;
using Windows.Storage;
using System.IO;
using System.Net.Http.Headers;

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
            Debug.WriteLine(requestUri.AbsolutePath + " " + requestUri.AbsoluteUri);
            HttpResponseMessage httpResponse = new HttpResponseMessage();
            string httpResponseBody = "";
            User user = null;
            try
            {
                httpResponse = await httpClient.GetAsync(requestUri);
                httpResponse.EnsureSuccessStatusCode();
                httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
                user = JsonConvert.DeserializeObject<User>(JObject.Parse(httpResponseBody).GetValue("user").ToString());
                Debug.WriteLine(httpResponseBody);
                Debug.WriteLine(JObject.Parse(httpResponseBody).GetValue("user").ToString());
                Debug.WriteLine(user.Firstname);


            }
            catch (Exception ex)
            {
                httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
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
                httpResponse.EnsureSuccessStatusCode();
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

            public static async Task<IAccidentReport> ReportAccident(string Description, byte[] image,string Location)
        {

            var formContent = new MultipartFormDataContent();
       
            formContent.Add(new ByteArrayContent(image), "ImageFile");
            formContent.Add(new StringContent(Description), "Description");
            formContent.Add(new StringContent(Location), "Location");
            formContent.Add(new StringContent(StaticData.accessToken), "access_token");
            string token = StaticData.accessToken;
            var myHttpClient = new HttpClient();
            myHttpClient.DefaultRequestHeaders.Add("access_token", token);
            var response = await myHttpClient.PostAsync(BaseURL + "/accident/report", formContent);
            var body = await response.Content.ReadAsStringAsync();
            IAccidentReport auth = new IAccidentReport();
            auth = JsonConvert.DeserializeObject<IAccidentReport>(body);
            return auth;
        }

    }
}
