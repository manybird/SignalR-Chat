using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using Chat.Web.MiccSdk.OpenMedia;
using System.Text;
using System.Net.Mime;
using System.Net.Http.Headers;
using Microsoft.Net.Http.Headers;
using System.Net.Security;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Xml.Linq;
using System.Net;
using Chat.Web.MiccSdk.Conversation;

namespace Chat.Web.MiccSdk
{
    public class MiccRunner : IDisposable
    {
        public readonly Micc micc;
        private HttpClientHandler _clientHandler;// = new HttpClientHandler();
        private HttpClient _client;

        public MiccRunner(Micc _micc)
        {
            this.micc = _micc ?? throw new ArgumentNullException(nameof(_micc));
            _clientHandler = new HttpClientHandler();
            _clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            _client = new HttpClient(_clientHandler);
        }

        public void Dispose()
        {
            if (_client != null) _client.Dispose();
            if (_clientHandler != null) _clientHandler.Dispose();
            GC.SuppressFinalize(this);
        }

        private void AddTokenToHeader()
        {
            if (!_client.DefaultRequestHeaders.Contains(HeaderNames.Authorization))
                _client.DefaultRequestHeaders.TryAddWithoutValidation(HeaderNames.Authorization, micc.GetAuthorization());
        }

        public async Task<ResponseResultServerStatus> CheckServerStatus()
        {
            var result = new ResponseResultServerStatus();
            try
            {
                //client.DefaultRequestHeaders.Add(HeaderNames.ContentType, "application/x-www-form-urlencoded");
                var request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(micc.UrlServerStatus),
                    Content = new FormUrlEncodedContent(micc.GetLoginContent()),

                };
                AddTokenToHeader();

                HttpResponseMessage rep = await _client.SendAsync(request);
                var body = await rep.Content.ReadAsStringAsync();

                if (rep.IsSuccessStatusCode)
                {
                    result = JsonConvert.DeserializeObject<ResponseResultServerStatus>(body);
                }
                else if (rep.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    result = JsonConvert.DeserializeObject<ResponseResultServerStatus>(body);
                }
                else
                {
                    result.ResponseBody = body;
                }

                result.ResponseCode = rep.StatusCode.ToString();
                result.StatusCode = (int)rep.StatusCode;

            }
            catch (Exception ex)
            {
                result.SetException(ex);
            }
            return result;
        }

        public async Task<ResponseResultAuthorization> SignInMicc()
        {
            var result = new ResponseResultAuthorization();

            if (!string.IsNullOrEmpty(micc.AuthorizationToken))
            {
                var r = await CheckServerStatus();
                if (r.IsSuccess)
                {
                    result.StatusCode = r.StatusCode;
                    result.Access_token = micc.AuthorizationToken;
                    return result; //Keep using it token if it is still valid
                }
                else
                {
                    //Server status is not valid, reset it authorization token
                    micc.AuthorizationToken = null;
                }
            }

            try
            {
                //client.DefaultRequestHeaders.Add(HeaderNames.ContentType, "application/x-www-form-urlencoded");
                var request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(micc.UrlAuthorizationToken),
                    Content = new FormUrlEncodedContent(micc.GetLoginContent()),

                };

                //request.Content.Headers.Add(HeaderNames.ContentType, "application/x-www-form-urlencoded");

                HttpResponseMessage rep = await _client.SendAsync(request);
                var body = await rep.Content.ReadAsStringAsync();

                if (rep.IsSuccessStatusCode)
                {
                    result = JsonConvert.DeserializeObject<ResponseResultAuthorization>(body);
                    micc.AuthorizationToken = result.Access_token;
                }
                else if (rep.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    result = JsonConvert.DeserializeObject<ResponseResultAuthorization>(body);
                }
                else
                {
                    result.ResponseBody = body;
                }

                result.ResponseCode = rep.StatusCode.ToString();
                result.StatusCode = (int)rep.StatusCode;

                if (result.IsSuccess) micc.lastCheckSignIn = DateTime.Now;

            }
            catch (Exception ex)
            {
                result.SetException(ex);
            }
            return result;
        }



        public async Task<ResponseResultOpenMediaConversation> PostOpenMediaConversation(string chatId, string caseId, string name, string email)
        {
            var result = new ResponseResultOpenMediaConversation();

            if (micc.shouldCheckSignIn())
            {
                var r = await SignInMicc();
                if (!r.IsSuccess)
                {
                    result.CopyHttpResult(r);
                    return result;
                }
            }

            OpenMediaRequestBody requestBody = micc.NewOpenMediaRequest(chatId, caseId, name, email);
            try
            {

                var request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(micc.UrlOpenMedia),
                    Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, MediaTypeNames.Application.Json)

                };

                AddTokenToHeader();

                //request.Content.Headers.TryAddWithoutValidation(HeaderNames.Authorization, micc.GetAuthorization());

                using (HttpResponseMessage rep = await _client.SendAsync(request))
                {
                    var body = await rep.Content.ReadAsStringAsync();

                    if (rep.IsSuccessStatusCode)
                    {
                        result = JsonConvert.DeserializeObject<ResponseResultOpenMediaConversation>(body);
                    }
                    else if (rep.StatusCode == System.Net.HttpStatusCode.BadRequest || rep.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        result = JsonConvert.DeserializeObject<ResponseResultOpenMediaConversation>(body);
                    }
                    else
                    {
                        result.ResponseBody = body;
                    }
                    result.ResponseCode = rep.StatusCode.ToString();
                    //result.ResponseBody = body;
                    result.StatusCode = (int)rep.StatusCode;
                }


            }
            catch (Exception ex)
            {
                result.SetException(ex);
            }

            return result;
        }


        public async Task<ResponseResultOpenMediaConversation> GetOpenMediaConversationById(string caseId)
        {
            var result = new ResponseResultOpenMediaConversation();

            var r = await SignInMicc();
            if (!r.IsSuccess)
            {
                result.CopyHttpResult(r);
                return result;
            }

            try
            {

                var request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(micc.UrlOpenMediaById(caseId)),
                    //Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, MediaTypeNames.Application.Json)

                };

                AddTokenToHeader();

                //request.Content.Headers.TryAddWithoutValidation(HeaderNames.Authorization, micc.GetAuthorization());

                using (HttpResponseMessage rep = await _client.SendAsync(request))
                {
                    var body = await rep.Content.ReadAsStringAsync();

                    if (rep.IsSuccessStatusCode)
                    {
                        result = JsonConvert.DeserializeObject<ResponseResultOpenMediaConversation>(body);
                    }
                    else if (rep.StatusCode == HttpStatusCode.BadRequest || rep.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        result = JsonConvert.DeserializeObject<ResponseResultOpenMediaConversation>(body);
                    }
                    else
                    {
                        result.ResponseBody = body;
                    }
                    result.ResponseCode = rep.StatusCode.ToString();
                    //result.ResponseBody = body;
                    result.StatusCode = (int)rep.StatusCode;

                    if (rep.StatusCode == HttpStatusCode.NotFound)
                    {
                        result.Error = result.ToString();
                    }
                }


            }
            catch (Exception ex)
            {
                result.SetException(ex);
            }

            return result;
        }

        public async Task<ResponseResultConversation> GetConversationById(string caseId)
        {
            var r1 = await MiccGet<ResponseResultConversation>(micc.UrlConversationById(caseId));
            if (r1 == null) return r1;
            if (!r1.IsInQueue()) return r1;            
            var r2 = await GetOpenMediaConversationById(caseId);

            if (r2 != null && r2.IsSuccess)                        
                r1.PositionInQueue = r2.PositionInQueue;
           
            return r1;
        }

        public async Task<ResponseResultConversations> GetConversationsById(string caseId)
        {
            return await MiccGet<ResponseResultConversations>(micc.UrlConversationById(caseId));
        }
        public async Task<T> MiccGet<T>(string url) where T : ResponseResult
        {
            T result = (T)Activator.CreateInstance(typeof(T));

            var r = await SignInMicc();
            if (!r.IsSuccess)
            {
                result.CopyHttpResult(r);
                return result;
            }

            try
            {

                var request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    //RequestUri = new Uri(micc.UrlConversationById(adminId)),
                    RequestUri = new Uri(url),
                };

                AddTokenToHeader();

                using (HttpResponseMessage rep = await _client.SendAsync(request))
                {
                    var body = await rep.Content.ReadAsStringAsync();

                    if (rep.IsSuccessStatusCode)
                    {
                        result = JsonConvert.DeserializeObject<T>(body);
                    }
                    else if (rep.StatusCode == HttpStatusCode.BadRequest || rep.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        result = JsonConvert.DeserializeObject<T>(body);
                    }
                    else
                    {
                        result.ResponseBody = body;
                    }
                    result.ResponseCode = rep.StatusCode.ToString();
                    //result.ResponseBody = body;
                    result.StatusCode = (int)rep.StatusCode;

                    if (rep.StatusCode == HttpStatusCode.NotFound)
                    {
                        result.Error = result.ToString();
                    }
                    result.SetChildStatus();
                }


            }
            catch (Exception ex)
            {
                result.SetException(ex);
            }


            return result;
        }
    }
}
