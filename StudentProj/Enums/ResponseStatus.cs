using System.Text.RegularExpressions;

namespace StudentProj.Enums
{
    public enum ResponseStatus
    {
        Success = 200,
        Created = 201,
        BadRequest = 400,
        Unauthorized = 401,
        Forbidden = 403,
        NotFound = 404,
        InternalServerError = 500
    }

    public static class ResponseStatusExtensions
    {
        public static string ToFriendlyMessage(this ResponseStatus status)
        {
            // Split camel case (e.g., "NotFound" becomes "Not Found")
            return Regex.Replace(status.ToString(), "([a-z])([A-Z])", "$1 $2");
        }
    }
}
