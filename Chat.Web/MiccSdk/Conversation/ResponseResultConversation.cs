using Chat.Web.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Chat.Web.MiccSdk.Conversation
{
    public class ResponseResultConversations : ResponseResult
    {     
        public override ResponseLinks _links { get; set; }        
        public int Count { get; set; }

        public ResponseResultConversation FirstItem { get { return Items?.FirstOrDefault(); } }

        public ICollection<ResponseResultConversation> Items { get { return _embedded?.Items; } }

        public override bool IsSuccess => IsSuccessStatusCode && _embedded!=null;
        public ResponseResultEmbeddedItems<ResponseResultConversation> _embedded { get; set; }

        public override void SetChildStatus()
        {
            if (Items == null) return;

            foreach(var i in Items)
            {
                i.StatusCode = StatusCode;
                i.ResponseCode = ResponseCode;
            }
        }
    }
    public class ResponseResultConversation : ResponseResult
    {
        [JsonIgnore]
        public override string ResponseBody { get; set; }

        [JsonIgnore]
        public override ResponseLinks _links { get; set; }
        public override bool IsSuccess => (IsSuccessStatusCode) && !string.IsNullOrEmpty(ConversationId);
        public string ConversationId { get; set; }
        public string Type { get; set; }
        public string CollectedInfo { get; set; }
        public string Folder { get; set; }

        //private string[] CompletedArray = { "Handled", "Junk", "No Reply", "Failed", "Sent" };
        public bool IsCompleted(string[] CompletedArray)
        {            
            Completed = CompletedArray.Contains(Folder);
            return Completed;
        }

        public bool Completed { get; set; }        

        public string CaseId { get; set; }
        public string FromAddress { get; set; }
        public string FromName { get; set; }
        public string MediaServerId { get; set; }
        public string MediaServerType { get; set; }
        public string MediaSpecificInfo { get; set; }

        [JsonIgnore]
        public JToken SupplementalDetails { get; set; }
        [JsonIgnore]
        public JToken SupplementalDetailsDisplayName { get; set; }

        public string ToAddress { get; set; }
        public string ToName { get; set; }

        private string _conversationState;
        public string ConversationState { 
            get
            {
                if (string.Equals("Unknown", _conversationState, StringComparison.OrdinalIgnoreCase)) return "";
                return _conversationState;
            }
            set
            {
                _conversationState = value;
            }
        }
        public string MediaFolder { get; set; }
        public TimeSpan? EstimatedWaitTime { get; set; }
        public int PositionInQueue { get; set; }
        public int Priority { get; set; }
        public string QueueId { get; set; }
        public string QueueName { get; set; }
        public string QueueReporting { get; set; }
        public bool QueueIsWrapUpTimeEnabled { get; set; }
        public bool IsAgentAutoAnswerEnabled { get; set; }

        public string AgentId { get; set; }
        public string AgentName { get; set; }
        public string AgentReporting { get; set; }
        public string LastAgentAction { get; set; }

        public DateTime? TimeOfferedToAgent { get; set; }
        public DateTime? TimeOfferedToQueue { get; set; }

        public DateTime? TimeOfLastAgentResponse { get; set; }
        public DateTime? LastAgentActionDate { get; set; }
        
        public DateTime? TimeOfLastCustomerResponse { get; set; }

        public static string IN_QUEUE_KEY = "InQueue";

        public bool IsInQueue()
        {
            return (string.Equals(this.Folder, IN_QUEUE_KEY, StringComparison.InvariantCultureIgnoreCase));
        }

        public override string GetContentMessage()
        {
            var result = this;
            if (result.IsSuccess)
            {
                if (IsInQueue()) return string.Format("{0} - Position {1}", Folder, PositionInQueue);

                return string.Format("{0}", Folder);
            }
                
            else
                return result.Error;
        }

        internal bool IsConversationStateChanged(ResponseResultConversation newConversation)
        {
            if (newConversation == null) return true;

            return !( string.Equals(newConversation.ConversationState, ConversationState, StringComparison.OrdinalIgnoreCase)
               && string.Equals(newConversation.Folder,Folder,StringComparison.InvariantCultureIgnoreCase) 
               && newConversation.PositionInQueue == PositionInQueue);
        }

        
    }
}
