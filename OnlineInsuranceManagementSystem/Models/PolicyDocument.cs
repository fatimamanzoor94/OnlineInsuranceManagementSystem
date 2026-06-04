using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineInsuranceManagementSystem.Models
{
    public class PolicyDocument
    {
        [Key]
        public int DocumentId { get; set; }

        [ForeignKey("Application")]
        public int ApplicationId { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; set; }

        public DateTime UploadDate { get; set; }

        public Application Application { get; set; }
    }
}