using Chat.Web.MiccSdk.Conversation;
using System;
using System.Collections.Generic;

namespace Chat.Web.MiccSdk.OpenMedia
{
    public class ResponseResultOpenMediaConversations : ResponseResult
    {        
        public override ResponseLinks _links { get; set; }
        public int Count { get; set; }
        public override bool IsSuccess => IsSuccessStatusCode && _embedded!=null;
        public ResponseResultEmbedded<ResponseResultOpenMediaConversation> _embedded { get; set; }
        public ICollection<ResponseResultOpenMediaConversation> Items { get { return _embedded?._items; } }
        public override void SetChildStatus()
        {
            if (Items == null) return;
            foreach (var i in Items)
            {
                i.StatusCode = StatusCode;
                i.ResponseCode = ResponseCode;
            }
        }
    }
    public class ResponseResultOpenMediaConversation : ResponseResult
    {
        public string Id { get; set; }
        public string ConverationState { get; set; }
        public int PositionInQueue { get; set; }
        public string QueueId { get; set; }
        public string QueueName { get; set; }
        public string QueueReporting { get; set; }
        public string AgentName { get; set; }
        public string AgentId { get; set; }
        public string AgentReporting { get; set; }

        public DateTime? TimeOfferedToAgent { get; set; }
        public DateTime? TimeOfferedToQueue { get; set; }
        public DateTime? TimeOfferedToSystem { get; set; }

        public override ResponseLinks _links { get; set; }
        public override bool IsSuccess => IsSuccessStatusCode && !string.IsNullOrEmpty(Id);

        
    }
}
