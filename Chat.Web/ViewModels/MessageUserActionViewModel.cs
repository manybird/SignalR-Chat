using Chat.Web.MiccSdk;
using Chat.Web.MiccSdk.Conversation;
using Chat.Web.MiccSdk.OpenMedia;

namespace Chat.Web.ViewModels
{
    public class MessageUserActionViewModel :MessageViewModel
    {
        
        public string ConnectionId { get; set; }

        public ResponseResult ResponseResult;
        public ResponseResultConversation Conversation { get; set; }
        public MessageUserActionViewModel()
        {
            
        }

    }
}
