using mshtml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CheckPosition
{
 
    class BrowserEO
    {
        public EO.WebBrowser.WebView browser = new EO.WebBrowser.WebView();
        public String CodePage = "utf-8";
        public BrowserEO(System.Windows.Forms.PictureBox pictureBox1) => browser.Create(pictureBox1.Handle);

        public async void loadUrl(String url, String oauth = "")
        {
            EO.WebBrowser.Request req = new EO.WebBrowser.Request(url); 
			oauth = "";
            req.Headers["Authorization"] = "OAuth " + oauth;
            req.Headers["Content-Type"] = "application/json";

            browser.LoadRequestAndWait(req);
            wait(1);
        }

        static public string GetDomainName(string domain)
        {
            IdnMapping idn = new IdnMapping();
            return idn.GetAscii(domain.Trim());
        }
        public void loadUrl(String url)
        {
   
            browser.LoadUrlAndWait(url);
            wait(3);
        }
        public void loadRequest(String url)
        {
            EO.WebBrowser.Request req = new EO.WebBrowser.Request(url);
            req.Headers.Add("User-Agent", getRandomUserAgent());
            browser.LoadRequest(req);
            wait(3);
        }

        public static string getRandomUserAgent()
        {
            string[] str = File.ReadAllLines("settings/user-agent-chrome.txt");
            Random r = new Random();
            r.Next(0, DateTime.Now.Second);
            int a = r.Next(0, str.Length - 1);
            return str[a];
        }

        public bool waitUntilDataFound(string dataToSearch, int waitSeconds)
        {
            for(int i = 0; i < waitSeconds; i++)
            {
                if (GetTextPage().ToLower().IndexOf(dataToSearch.ToLower()) > 0) return true;
                wait(1);
            }
            return false;
        }


        public void wait(int seconds)
        {
            for (int i = 0; i < seconds * 10; i++)
            {
                Thread.Sleep(100);
                Application.DoEvents();
            }
            
        }

        public String GetJsonText() => browser.GetText();
        public String GetTextPage() => browser.GetHtml();

        public List<string> getAllElementInnerText(String regular, RegexOptions opt = RegexOptions.Compiled | RegexOptions.IgnoreCase)
        {
            
            List<string> URL = new List<string>();
            Regex regex = new Regex(regular, opt);
            String html = GetTextPage();
            MatchCollection matches = regex.Matches(GetTextPage());
            for (int i = 0; i < matches.Count; i++)
            {
                URL.Add( matches[i].Groups[matches[i].Groups.Count-1].Value.ToLower());
            }

            /* IHTMLDocument2 doc = (IHTMLDocument2)browser.GetDOMWindow().document;
             System.Windows.Forms.HtmlElementCollection colCol = browser. GetElementsByTagName(tag);
             foreach (HtmlElement elm in colCol)
             {
                 if (elm.TagName.ToLower() == tag.ToLower() &&
                     elm.GetAttribute(attrName).ToLower().IndexOf(value.ToLower()) >= 0)
                     URL.Add(elm.InnerText);
             }
             return URL; */
            return URL;
        }

        public static List<string> getRegularText(String html, String regular, RegexOptions opt = RegexOptions.Compiled | RegexOptions.IgnoreCase)
        {

            List<string> data = new List<string>();
            Regex regex = new Regex(regular, opt);
            
            MatchCollection matches = regex.Matches(html);
            for (int i = 0; i < matches.Count; i++)
            {
                data.Add(matches[i].Groups[matches[i].Groups.Count - 1].Value.ToLower());
            }
 
            return data;
        }

        public static string getTitlePage(string address)
        {
            String text =  loadJsonUrl(address, true);
            int pos = text.ToLower().IndexOf("<title>");
            if (pos >= 0) text = text.Remove(0, pos + 7);
            pos = text.ToLower().IndexOf("<");
            if (pos >= 0) text = text.Remove(pos);
            return text;
        }
        public static int getCounterID(string address)
        {
            String text =  loadJsonUrl(address, true);
            string pattern = @"https:\/\/mc.yandex.ru\/watch\/(\d*?)\W";
            Regex regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            try
            {
                MatchCollection matches = regex.Matches(text);
                if (matches.Count <= 0) return -1;
                return int.Parse(matches[0].Groups[matches[0].Groups.Count - 1].Value.ToLower());
            } catch (Exception e) 
            { return -1; }
        }

        public static string loadJsonUrl(string address,bool isSilentMode = false)
        {
            using (WebClient webClient = new WebClient())
            {
                // Некоторые сайты требуют наличия User-Agent

                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                ServicePointManager.ServerCertificateValidationCallback += (send, certificate, chain, sslPolicyErrors) =>
                {
                    return true;
                };

                try
                { //https://довериевсети.рф/site/ifish2.ru
                    byte[] data = webClient.DownloadData(new Uri(address));
                    return   Encoding.UTF8.GetString(data);
                }
                catch (Exception ex)
                {
                    if (!isSilentMode)
                        MessageBox.Show("Error " + ex.Message + " -> " + address);
                }
            }
            return "";

        }
    }
}
