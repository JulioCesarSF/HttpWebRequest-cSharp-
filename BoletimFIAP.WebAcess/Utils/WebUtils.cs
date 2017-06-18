using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            return htmlDoc.DocumentNode.SelectSingleNode("//table[@class='i-boletim-table']").InnerText;
        }

        public static bool MontarBoletim(string html, TreeView tree)
        {
            tree.Nodes.Clear();
            html = WebUtility.HtmlDecode(html);
            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(html);

            HtmlAgilityPack.HtmlNodeCollection tabela = htmlDoc.DocumentNode.SelectNodes("//table[@class='i-boletim-table']//tr[@class='i-boletim-table-row']");

            if (tabela == null) return false;

            foreach (HtmlAgilityPack.HtmlNode tr in tabela)
            {
                TreeNode root = null;
                foreach (HtmlAgilityPack.HtmlNode cell in tr.SelectNodes("td"))
                {
                    if (!cell.Id.Equals(""))
                    {
                        root = tree.Nodes.Add(cell.InnerText);
                    }
                    else if (root != null)
                    {
                        TreeNode no = root.Nodes.Add(cell.InnerText);
                    }
                }
            }

            return true;
        }
    }
}
