using System.Collections.Generic;

namespace StudentProj.DTO
{
    public class UserMenuPermissionDTO
    {
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public string MenuRoute { get; set; }
        public string Permission { get; set; }
    }

    public class LoginResponseDTO
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public List<UserMenuPermissionDTO> Permissions { get; set; } = new();
    }
}
