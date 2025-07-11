namespace Portal.Models
{
    public class ApiSettings
    {
        public static string SectionName => "ApiSettings";
        public string BaseUrl { get; set; } = string.Empty;
        public string EmployeeLogin { get; set; } = string.Empty;
        public string EmployeeRegister { get; set; } = string.Empty;
        public string EmployeeSearch { get; set; } = string.Empty;
        public string EmployeeSearchByUsername { get; set; } = string.Empty;
        public string EmployeeSearchByEmail { get; set; } = string.Empty;
        public string EmployeeAll { get; set; } = string.Empty;
        public string RoleAll { get; set; } = string.Empty;
        public string CompanyAll { get; set; } = string.Empty;
        public string CompanySearch { get; set; } = string.Empty;
        public string CompanySave { get; set; } = string.Empty;
        public string CompanyDelete { get; set; } = string.Empty;
        public string BranchesByCompany { get; set; } = string.Empty;
        public string DivisionAll { get; set; } = string.Empty;
        public string DivisionSearch { get; set; } = string.Empty;
        public string DivisionSave { get; set; } = string.Empty;
        public string DivisionDelete { get; set; } = string.Empty;
        public string DepartmentAll { get; set; } = string.Empty;
        public string DepartmentSearch { get; set; } = string.Empty;
        public string DepartmentsByDivision { get; set; } = string.Empty;
        public string DepartmentSave { get; set; } = string.Empty;
        public string DepartmentDelete { get; set; } = string.Empty;
        public string SectionAll { get; set; } = string.Empty;
        public string SectionSearch { get; set; } = string.Empty;
        public string SectionsByDepartment { get; set; } = string.Empty;
        public string SectionSave { get; set; } = string.Empty;
        public string SectionDelete { get; set; } = string.Empty;
        public string SupportTicketCreate { get; set; } = string.Empty;
        public string SupportTicketGetCategories { get; set; } = string.Empty;
        public string SupportTicketGetMyTickets { get; set; } = string.Empty;
        public string ITStockItemsGetAvailable { get; set; } = string.Empty;
        public string SupportTicketCreateWithdrawal { get; set; } = string.Empty;
        public string SupportTicketCreatePurchase { get; set; } = string.Empty;
        public string SupportTicketGetAll { get; set; } = string.Empty;
        public string SupportTicketGetDetails { get; set; } = string.Empty;
        public string SupportCategoryGetAll { get; set; } = string.Empty;
        public string SupportCategoryGetById { get; set; } = string.Empty;
        public string SupportCategoryCreate { get; set; } = string.Empty;
        public string SupportCategoryUpdate { get; set; } = string.Empty;
        public string SupportCategoryDelete { get; set; } = string.Empty;
        public string SupportTicketAccept { get; set; } = string.Empty;
        public string SupportTicketResolve { get; set; } = string.Empty;
    }
}
