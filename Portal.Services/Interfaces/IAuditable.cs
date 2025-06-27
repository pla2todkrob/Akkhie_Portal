namespace Portal.Services.Models
{
    public interface IAuditable
    {
        DateTime CreatedAt { get; set; }
        Guid? CreatedBy { get; set; }
        string CreatedByName { get; set; }
        DateTime? UpdatedAt { get; set; }
        Guid? UpdatedBy { get; set; }
        string UpdatedByName { get; set; }
    }
}
