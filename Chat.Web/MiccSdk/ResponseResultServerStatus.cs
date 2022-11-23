using AutoMapper.Configuration.Conventions;

namespace Chat.Web.MiccSdk
{
    public class ResponseResultAuthorization : ResponseResult
    {
        #region "Authorization"
        public override ResponseLinks _links { get; set; }
        public string Access_token { get; set; }
        public string Token_type { get; set; }
        public int Expires_in { get; set; }

        public string Refresh_token { get; set; }

        public override bool IsSuccess
        {
            get
            {
                return IsSuccessStatusCode && !string.IsNullOrEmpty(Access_token);
            }
        }

        #endregion
    }
}
