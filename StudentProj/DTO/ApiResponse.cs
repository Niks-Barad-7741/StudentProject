namespace StudentProj.DTO
{
    public class ApiResponse<T>
    {
        public int statusCodes { get; set; }
        public string message { get; set; }
        public T? data { get; set; }
    }
}
