namespace ProjectApprovalSystem.Models.Enums
{
    public enum ProjectStatus
    {
        Pending,
        UnderReview,
        Matched,
        Withdrawn
    }

    public enum MatchStatus
    {
        Interested,
        Confirmed,
        Cancelled,
        Reassigned
    }

    public enum UserRole
    {
        Student,
        Supervisor,
        ModuleLeader,
        SystemAdmin
    }
}
