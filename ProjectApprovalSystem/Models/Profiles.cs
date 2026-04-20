using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectApprovalSystem.Models
{
    public class StudentProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [Required]
        [StringLength(20)]
        public string StudentId { get; set; } = string.Empty;

        [StringLength(50)]
        public string? GroupName { get; set; }

        [StringLength(20)]
        public string? ContactNumber { get; set; }

        public virtual ICollection<ProjectProposal> Proposals { get; set; } = new List<ProjectProposal>();
    }

    public class SupervisorProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [Required]
        [StringLength(20)]
        public string StaffId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [StringLength(20)]
        public string? ContactNumber { get; set; }

        public string? Bio { get; set; }

        public virtual ICollection<SupervisorResearchArea> Expertise { get; set; } = new List<SupervisorResearchArea>();
    }

    public class ResearchArea
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class SupervisorResearchArea
    {
        [Key]
        public int Id { get; set; }

        public int SupervisorId { get; set; }
        
        [ForeignKey("SupervisorId")]
        public virtual SupervisorProfile? Supervisor { get; set; }

        public int ResearchAreaId { get; set; }

        [ForeignKey("ResearchAreaId")]
        public virtual ResearchArea? ResearchArea { get; set; }
    }
}
