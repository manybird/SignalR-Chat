using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chat.Web.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public ApplicationUser FromUser { get; set; }
        public int ToRoomId { get; set; }
        public Room ToRoom { get; set; }

        public int MessageType { get; set; }    
        public string FileFullPath { get; set; }
        public string RelativePath { get; set; }

        public string CaseId { get; set; }
        public Case Case { get; set; }
    }

    public class MessageExt:Message
    {
        public bool IsFromUserRole { get; set; }
    }
}
