namespace StudentProj.DTO
{
    public class LoginResponseDTO
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public List<string> Permission { get; set; } = new();
    }
}
