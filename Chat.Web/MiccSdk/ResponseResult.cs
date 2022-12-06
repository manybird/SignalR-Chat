using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Chat.Web.MiccSdk
{
    public abstract class ResponseResult
    {
        public abstract ResponseLinks _links { get; set; }

        
        public ResponseLinks Next { get; set; }
        public abstract bool IsSuccess { get; }
        
        public string GetErrorOrMessage()
        {
            if (!string.IsNullOrEmpty(Error)) return Error;
            return Message;
        }
        public string Error { get; set; }
        public string Message { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseHeaders { get; set; }
        public string ResponseBody { get; set; }

        public int StatusCode;

        [JsonIgnore]
        public Exception ex;

        public void SetException(Exception _ex)
        {
            ex = _ex;
            Error = _ex.Message + Environment.NewLine + _ex.StackTrace;
        }

        public bool IsSuccessStatusCode
        {
            get { return ((StatusCode >= 200) && (StatusCode <= 299)); }
        }

        public override string ToString()
        {
            return String.Format("MICC: {0} {1} {2} {3}", StatusCode, ResponseCode, Message, Error);
        }

        public void CopyHttpResult(ResponseResult r)
        {
            var result = this;
            result.StatusCode = r.StatusCode;
            result.ResponseCode = r.ResponseCode;
            result.Error = r.Error;
            result.Message = r.Message;
        }

        public static string SUCCESS_MESSAGE = "Request Success";
        public virtual void SetChildStatus() { }
        public virtual string GetContentMessage()
        {            
            var result = this;
            if (result.IsSuccess)
                return SUCCESS_MESSAGE;
            else
                return result.Error;
        }
    }

    public class ResponseResultEmbedded<T>
    {
        public ICollection<T> _items { get; set; }
    }

    public class ResponseResultEmbeddedItems<T>
    {
        public ICollection<T> Items { get; set; }
    }

    public class ResponseLinks
    {
        public ResponseLink Self { get; set; }
        public ResponseLink State { get; set; }
        public ResponseLink Conversations { get; set; }
    }
    public class ResponseLink
    {
        public string Href { get; set; }
    }
}
