using System;

namespace Chat.Web.MiccSdk.OpenMedia
{
    public class OpenMediaRequestBody
    {
        public string UrlBase { get; set; }

        private string _urlTarget;
        public string TargetUri
        {
            get
            {
                var s = new string( _urlTarget);
                if (s == null) return null;
                if (!string.IsNullOrEmpty(UrlBase)) s = UrlBase + _urlTarget;
                return s;

            }
            set {
                if (string.IsNullOrEmpty(value) 
                    || string.IsNullOrEmpty(UrlBase) 
                    || !(value.IndexOf(UrlBase) == 0 && UrlBase.Length <= value.Length))
                {
                    _urlTarget = value;
                    return;
                }
                
                _urlTarget = value.Substring(UrlBase.Length);
                
            }
        }
               
        private string _urlHistory;
        public string HistoryUrl
        {
            get
            {
                var s = _urlHistory;
                if (s == null) return null;
                if (!string.IsNullOrEmpty(UrlBase)) s = UrlBase + _urlHistory;
                return s;

            }
            set {

                if (string.IsNullOrEmpty(value)
                    || string.IsNullOrEmpty(UrlBase)
                    || !(value.IndexOf(UrlBase) == 0 && UrlBase.Length <= value.Length))
                {
                    _urlHistory = value;
                    return;
                }
                                
                
                _urlHistory = value.Substring(UrlBase.Length);
                

            }
        }
        private string _urlPreview;
        public string PreviewUrl
        {
            get
            {
                var s = _urlPreview;
                if (s == null) return null;
                if (!string.IsNullOrEmpty(UrlBase)) s = UrlBase + _urlPreview;
                return s;

            }
            set {
                if (string.IsNullOrEmpty(value)
                    || string.IsNullOrEmpty(UrlBase)
                    || !(value.IndexOf(UrlBase) == 0 && UrlBase.Length <= value.Length))
                {
                    _urlPreview = value;
                    return;
                }

                
                _urlPreview = value.Substring(UrlBase.Length);
                
            }
        }
        
        public string Id { get; set; }
        public string Name { get; set; }
        
        public bool TargeturiEmbedded { get; set; }
        
        public string Tenant { get; set; }
        public string Queue { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }

        public VariableData VariableData { get; set; } 
        public OpenMediaRequestBody()
        {
        
        }

        internal void SetValue(string chatId, string caseId, string name, string email)
        {
            var body = this;
            body.Id = caseId;

            _urlTarget = string.Format(_urlTarget, chatId, caseId);
            _urlHistory = string.Format(_urlHistory, chatId, caseId);
            _urlPreview = string.Format(_urlPreview, chatId, caseId);

            body.From = string.Format("{0} <{1}>", name, email);
            body.VariableData ??= new VariableData();
            body.VariableData.Name = name;
            body.VariableData.Email = email;
        }
    }

    public class VariableData
    {
        public VariableData()
        {

        }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Extension { get; set; }
        public string MobilePhoneNumber { get; set; }
    }
}
