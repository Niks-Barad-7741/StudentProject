using StudentProj.Enums;

namespace StudentProj.DTO
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public int StatusCodes { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }

        public ApiResponse() { }

        public ApiResponse(bool success, int statusCode, string message, T? data)
        {
            Success = success;
            StatusCodes = statusCode;
            Message = message;
            Data = data;
        }

        // Factory using ResponseStatus enum
        public static ApiResponse<T> Create(ResponseStatus status, T? data = default)
        {
            return new ApiResponse<T>(
                status.IsSuccess(),
                status.GetStatusCode(),
                status.ToFriendlyMessage(),
                data
            );
        }

        // Factory using ResponseStatus with custom override message string
        public static ApiResponse<T> Create(ResponseStatus status, string customMessage, T? data = default)
        {
            return new ApiResponse<T>(
                status.IsSuccess(),
                status.GetStatusCode(),
                customMessage,
                data
            );
        }

        // Standard factory methods
        public static ApiResponse<T> SuccessResponse(T data, string message = "Operation successful")
        {
            return new ApiResponse<T>(true, 200, message, data);
        }

        public static ApiResponse<T> FailureResponse(string message, int statusCode = 400, T? data = default)
        {
            return new ApiResponse<T>(false, statusCode, message, data);
        }
    }
}
