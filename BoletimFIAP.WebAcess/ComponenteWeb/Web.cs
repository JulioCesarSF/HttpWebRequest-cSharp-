using BoletimFIAP.Dominio.Models;
using BoletimFIAP.WebAcess.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BoletimFIAP.WebAcess
{
    public class Web
    {
        public static string URL_BASE = "https://www2.fiap.com.br";
        private static string URL_LOGIN = "/Aluno/LogOn";
        private static string URL_BOLETIM = "/Aluno/Boletim";
        //private static int loginTry = 0;
        private static int boletimTry = 0;

        public static string oldStringHtml = "";
        public static string stringHtml = "";

        private static string verificarHtml = "";

        public static volatile bool novaNota = false;

        private static CookieContainer Cookies = new CookieContainer();

        private static CookieFiap CookieFiap { get; set; }

        public static string CookieFiapJson { get; set; }

        public static bool LoginPost(string rm, string senha)
        {
            //request POST, montar cabeçalho
            CookieCollection cookies = new CookieCollection();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL_BASE + URL_LOGIN);
            request.CookieContainer = Cookies;
            request.CookieContainer.Add(cookies);
            request.Method = "POST";
            request.Accept = "text/html, application/xhtml+xml, image/jxr, */*";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Referer = "https://www2.fiap.com.br/";
            request.KeepAlive = true;
            request.Host = "www2.fiap.com.br";
            request.AllowAutoRedirect = false;
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";
                       
            try
            {
                using (var requestStream = request.GetRequestStream())
                using (var writer = new StreamWriter(requestStream, Encoding.UTF8))
                    writer.Write("urlRedirecionamento=&usuario=" + rm + "&senha=" + senha);

                //pegar cookies da response
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.Found)
                    {
                        getLocation(response.Headers);
                        getCookies(response.Cookies);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static bool PosLoginGET()
        {
            string location = CookieFiap.Location;
            CookieCollection cookies = new CookieCollection();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL_BASE + location);
            request.CookieContainer = Cookies;
            request.CookieContainer.Add(cookies);
            request.Method = "GET";
            request.Accept = "text/html, application/xhtml+xml, image/jxr, */*";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Referer = "https://www2.fiap.com.br/";
            request.KeepAlive = true;
            request.Host = "www2.fiap.com.br";
            request.AllowAutoRedirect = false;
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.Moved)
                    {
                        getCookies(response.Cookies);
                    }
                }

            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static bool PegarBoletim()
        {
            if (!WebUtils.internet())
            {
                return false;
            }
            CookieCollection cookies = new CookieCollection();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL_BASE + URL_BOLETIM);
            request.CookieContainer = Cookies;
            request.CookieContainer.Add(cookies);
            request.Method = "GET";
            request.Accept = "text/html, application/xhtml+xml, image/jxr, */*";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Referer = "https://www2.fiap.com.br/";
            request.KeepAlive = true;
            request.Host = "www2.fiap.com.br";
            request.AllowAutoRedirect = false;
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader readStream = null;

                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding("UTF-8"));

                    stringHtml = readStream.ReadToEnd();

                    verificarHtml = WebUtils.ParseHtml(stringHtml);

                    if (boletimTry == 0)
                    {
                        oldStringHtml = verificarHtml;
                    }
                    else
                    {
                        if (verificarHtml.Equals(oldStringHtml))
                        {
                            novaNota = false;
                        }
                        else
                        {
                            novaNota = true;
                            oldStringHtml = verificarHtml;
                        }
                    }

                    boletimTry++;
                    response.Close();
                    readStream.Close();
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private static void getCookies(CookieCollection cookies)
        {
            foreach (Cookie c in cookies)
            {
                if (c.Name.Equals("ASP.NET_SessionId"))
                {
                    CookieFiap.ASPNET_SessionId = c.Value;
                }
                else if (c.Name.Equals("jwt"))
                {
                    CookieFiap.Jwt = c.Value;
                }
                else if (c.Name.Equals("ultimoUsuarioFIAP"))
                {
                    CookieFiap.UltimoUsuarioFIAP = c.Value;
                }
                else if (c.Name.Contains("ASPSESSIONID"))
                {
                    CookieFiap.ASPSESSIONID = c.Value;
                }
            }

            CookieFiapJson = CookieFiap.ToJson();
        }

        private static string getLocation(WebHeaderCollection headers)
        {
            string location = "";
            for (int i = 0; i < headers.Count; i++)
            {
                string header = headers.GetKey(i);
                if (header.Equals("Location"))
                {
                    foreach (string value in headers.GetValues(i))
                    {
                        location = value;
                    }
                }
            }

            CookieFiap = new CookieFiap()
            {
                Location = location
            };
            return location;
        }
    }
}
