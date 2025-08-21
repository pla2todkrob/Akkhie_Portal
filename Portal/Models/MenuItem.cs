namespace Portal.Models
{
    public class MenuItem
    {
        public string Text { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Action { get; set; } = "Index";
        public string Controller { get; set; } = "Home";
        public string? PermissionKey { get; set; }
        public List<MenuItem> Children { get; set; } = new();
    }
}
