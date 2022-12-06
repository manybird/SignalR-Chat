
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Chat.Web.Models
{
    [Table("Cases")]
    public class Case
    {
        [Key]
        [MaxLength(100)]
        public string Id { get; set; }                
        public DateTime CaseDate { get; set; }
        public DateTime? CaseStartingDate { get; set; }        
        public DateTime? CaseCompletionDate { get; set; }

        [MaxLength(300)]
        public string AgentName { get; set; }

        [MaxLength(100)]
        public string AgentReporting { get; set; }
        public DateTime? AgentJoinDate { get; set; }

        [MaxLength(100)] 
        public string MiccCaseId { get; set; }

        [MaxLength(200)]
        public string MiccCaseGuid { get; set; }
        public string AdminId { get; set; }
        public ApplicationUser Admin { get; set; }
        public ICollection<Message> Messages { get; set; }
        public int RoomId { get; set; }
        public Room Room { get; set; }

        [MaxLength(300)]
        public string Option01 { get; set; }
        [MaxLength(300)]
        public string Option02 { get; set; }

        [MaxLength(300)]
        public string Folder { get; internal set; }

        public Case() { }
        public Case(string id)
        {
            this.Id = id;
        }
    }
}
