using Chat.Web.MiccSdk;
using Chat.Web.MiccSdk.OpenMedia;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Chat.Web.Models
{
    public class MessageUserAction: Message
    {
        public ResponseResult ResponseResult;

        

        public MessageUserAction()
        {
            
        }
        internal void SetSuccess()
        {
            this.Content = ResponseResult.SUCCESS_MESSAGE;// result.GetContentMessage();
        }
        internal void SetResult(ResponseResult result)
        {
            this.ResponseResult = result;
            if (result == null) return;

            this.Content = result.GetContentMessage();            
        }
    }
}
