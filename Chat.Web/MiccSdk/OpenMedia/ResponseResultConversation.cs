using System;

namespace Chat.Web.MiccSdk.OpenMedia
{
    public class ResponseResultConversations : ResponseResult
    {
        #region "Open media list / queue list"
        public override ResponseLinks _links { get; set; }
        public int Count { get; set; }
        public override bool IsSuccess => IsSuccessStatusCode && _embedded!=null;
        public ResponseResultEmbedded<ResponseResultConversation> _embedded { get; set; }
        #endregion
    }
    public class ResponseResultConversation : ResponseResult
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

        internal void CopyHttpResult(ResponseResult r)
        {
            var result = this;
            result.StatusCode = r.StatusCode;
            result.ResponseCode = r.ResponseCode;
            result.Error = r.Error;
            result.Message = r.Message;
        }
    }
}
