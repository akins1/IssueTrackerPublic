using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IssueTracker.Models
{
    public class IssueTrackerUser : IdentityUser
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [NotMapped]
        [Display(Name = "Full Name")]
        public string FullName { get { return $"{FirstName} {LastName}";  } }

        public int? CompanyId { get; set; } = 1;


        // Profile avatar
        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile AvatarFormFile { get; set; }

        [DisplayName("Avatar")]
        public string AvatarFileName { get; set; } = "Avatar";

        public byte[] AvatarFileData { get; set; } = new byte[0];

        [Display(Name = "File Extension")]
        public string AvatarContentType { get; set; } = "File Extension";
        

        // Navigation properties
        public virtual Company Company { get; set; }

        public virtual ICollection<Project> Projects { get; set; }
    }
}
