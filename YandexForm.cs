using EO.WebBrowser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace CheckPosition
{
    public partial class YandexForm : Form
    {
        String ClientID = "07dedec763ed4a3d89717ced916df7f8";
        String backUrl = "http://yandex.localhost/";
        public String access_key = "";
        String expires_in = "";
        BrowserEO browserEO = null;
        public YandexForm()
        {
            InitializeComponent();
            browserEO = new BrowserEO(this.pictureBox1);
            this.browserEO.browser.LoadCompleted += OnLoadComplited;
        }
 

        private void OnLoadComplited(object sender, EventArgs e)
        {
            
            String url = this.browserEO.browser.Url.ToString();
            if (url.IndexOf("access_token=") > 0)
            {
                url = url.Substring(url.IndexOf("access_token=") + "access_token=".Length);
                access_key = url.Substring(0, url.IndexOf("&token_type="));
                expires_in = url.Substring(url.IndexOf("&expires_in=") + "&expires_in=".Length);
                this.Close();
            }
 
        }
        private void YandexForm_Shown(object sender, EventArgs e)
        {
            this.browserEO.loadUrl("https://oauth.yandex.ru/authorize?response_type=token&client_id=" + ClientID);
            
        }

        public void getStatisticById(int id)
        {
            this.Show();
            string url = $"https://api-metrika.yandex.net/stat/v1/data?dimensions=ym:s:searchEngineName&metrics=ym:s:visits&ids=94131720&date1=30daysAgo&date2=today";
            
            var client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", this.access_key);
            string json_string = "";
            var task = client.SendAsync(request)
                .ContinueWith((taskwithmsg) =>
                {
                    var response = taskwithmsg.Result;
                    var stringTask = response.Content.ReadAsStringAsync();
                    stringTask.Wait();
                    json_string = stringTask.Result;

                });
            task.Wait();

          
            var json = new JavaScriptSerializer();
            JsonResponse data = json.Deserialize<JsonResponse>(json_string);
            data = data;
        }
    }

   
}
