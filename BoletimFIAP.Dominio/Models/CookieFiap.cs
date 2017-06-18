using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoletimFIAP.Dominio.Models
{
    public class CookieFiap
    {
        public string Location { get; set; } //Headers
        public string ASPNET_SessionId { get; set; } //Cookie, ASP.NET_SessionId
        public string Jwt { get; set; } //Cookie
        public string UltimoUsuarioFIAP { get; set; } //Cookie
        public string ASPSESSIONID { get; set; } //Cookie

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
