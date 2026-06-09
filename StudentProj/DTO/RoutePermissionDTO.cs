namespace StudentProj.DTO
{
    public class RoutePermissionDTO
    {
        public string HttpMethod { get; set; } = string.Empty;
        public string PathPattern { get; set; } = string.Empty;
        public string RequiredMenuName { get; set; } = string.Empty;
        public string RequiredPermissionName { get; set; } = string.Empty;
    }
}
