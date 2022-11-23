
using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

using Chat.Web.MiccSdk.OpenMedia;
using System.Xml.Linq;
using System.Runtime.Serialization;

namespace Chat.Web.MiccSdk
{
    public class Micc
    {
        public DateTime? lastCheckSignIn { get; set; }
        public bool shouldCheckSignIn()
        {
            if (lastCheckSignIn == null) return true;

            return DateTime.Now.Subtract(lastCheckSignIn.Value).TotalSeconds > 60;
        }
        public string LoginName { get; set; }
        public string Password { get; set; }
        public string UrlBase { get; set; }
        public string ServerIP { get; set; }

        private string _urlAuthorizationToken;
        public string UrlAuthorizationToken
        {
            get
            {
                if (string.IsNullOrEmpty(UrlBase)) return _urlAuthorizationToken;
                return UrlBase + _urlAuthorizationToken;
            }
            set { _urlAuthorizationToken = value; }
        }

        private string _urlServerStatus;
        public string UrlServerStatus
        {
            get
            {
                if (string.IsNullOrEmpty(UrlBase)) return _urlServerStatus;
                return UrlBase + _urlServerStatus;
            }
            set { _urlServerStatus = value; }
        }

        public string ErrorMsg { get; set; }
        public string AuthorizationToken { get; set; }

        public Micc()
        {         
        }
        public Micc(string loginName, string password, string urlBase)
        {
            LoginName = loginName;
            Password = password;
            UrlBase = urlBase;
            
            UrlAuthorizationToken = UrlBase + "/AuthorizationServer/Token";
            UrlOpenMedia = UrlBase + "/api/v1/openmedia";

            //OpenMediaRequestBodyJson = openMediaRequestBodyJson;
        }

        internal Dictionary<string,string> GetLoginContent()
        {
            var micc = this;
            var nvc = new Dictionary<string, string>();
            nvc.Add("grant_type", "password");
            nvc.Add("username", micc.LoginName);
            nvc.Add("password", micc.Password);
            return nvc;

        }

        internal string GetAuthorization()
        {
            return String.Format("Bearer {0}", AuthorizationToken);
        }

        private string _urlOpenMedia;
        public string UrlOpenMedia
        {
            get
            {
                if (string.IsNullOrEmpty(UrlBase)) return _urlOpenMedia;
                return UrlBase + _urlOpenMedia;
            }
            set => _urlOpenMedia = value;
        }

        public string UrlOpenMediaById(string id) { 
            return string.Format("{0}/{1}", UrlOpenMedia,id);
            
        }


        private string _urlOpenMediaQueues;
        public string UrlOpenMediaQueues
        {
            get
            {
                if (string.IsNullOrEmpty(UrlBase)) return _urlOpenMediaQueues;
                return UrlBase + _urlOpenMediaQueues;
            }
            set => _urlOpenMediaQueues = value;
        }
        


        #region "OpenMediaForChat"

        public OpenMediaRequestBody OpenMediaRequestBodyDefault { get; set; }
        

        //public string OpenMediaRequestBodyJson { get; set; }

        public OpenMediaRequestBody NewOpenMediaRequest(string chatId,string name,string email)
        {
            var json = JsonConvert.SerializeObject(OpenMediaRequestBodyDefault);

            var body = JsonConvert.DeserializeObject<OpenMediaRequestBody>(json);
            body.SetValue(chatId,name,email);           

            return body;
        }



        
        #endregion
    }
}
