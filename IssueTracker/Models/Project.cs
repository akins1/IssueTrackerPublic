using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IssueTracker.Models
{
    public class Project
    {
        public int Id { get; set; }

        [DisplayName("Company")]
        public int? CompanyId { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Project Name")]
        public string Name { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Start Date")]
        [DataType(DataType.Date)]
        public DateTimeOffset StartDate { get; set; }

        [DisplayName("End Date")]
        [DataType(DataType.Date)]
        public DateTimeOffset EndDate { get; set; }

        [DisplayName("Priority")]
        public int? ProjectPriorityId { get; set; }

        [DisplayName("Archived")]
        public bool Archived { get; set; }


        // Project picture
        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile? ImageFormFile { get; set; }

        [DisplayName("File Name")]
        public string ImageFileName { get; set; } = "Avatar";

        public byte[] ImageFileData { get; set; } = new byte[0];

        [DisplayName("File Extension")]
        public string ImageContentType { get; set; } = "File Extension";


        // Navigation properties
        public virtual Company Company { get; set; }

        public virtual ProjectPriority ProjectPriority { get; set; }

        public virtual ICollection<IssueTrackerUser> Members { get; set; } = new HashSet<IssueTrackerUser>();
        
        public virtual ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();

    }
}
