using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Chat.Web.ViewModels
{
    public class MessageViewModel
    {
        public int Id { get; set; }
        
        /// <summary>
        /// User the for message body
        /// </summary>
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public string From { get; set; }
        
        /// <summary>
        /// Used for room name
        /// </summary>
        public string Room { get; set; }
        public string Avatar { get; set; }

        public int MessageType { get; set; }
        public string FileFullPath { get; set; }
        public string RelativePath { get; set; }

        public int TempSystemMessage;
        public int RoomId { get; set; }
        public string AdminId { get; set; }
        public string CaseId { get; set; }
    }
}
