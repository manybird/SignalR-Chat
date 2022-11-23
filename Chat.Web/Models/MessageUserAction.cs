using Chat.Web.MiccSdk;
using Chat.Web.MiccSdk.OpenMedia;
using System;

namespace Chat.Web.Models
{
    public class MessageUserAction: Message
    {
        public ResponseResult ResponseResult;

        public MessageUserAction()
        {
            
        }

        internal void SetResult(ResponseResult result)
        {
            this.ResponseResult = result;

            if (result != null)
            {
                if (result.IsSuccess)
                    this.Content = "Request Success";
                else
                    this.Content = result.Error;
            }
        }
    }
}
