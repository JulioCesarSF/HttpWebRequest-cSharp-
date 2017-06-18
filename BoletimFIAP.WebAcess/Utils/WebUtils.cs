using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace BoletimFIAP.WebAcess.Utils
{
    public class WebUtils
    {
        public static bool internet()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }

        public static bool site(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.Timeout = 3000;
                request.Method = "GET";
                request.AllowAutoRedirect = false;

                using (var resp = request.GetResponse()) return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string ParseHtml(string html)
        {
            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(html);
            return htmlDoc.DocumentNode.SelectSingleNode("//table").InnerHtml;
        }
    }
}
