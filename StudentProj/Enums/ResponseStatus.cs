using StudentProj.Common;

namespace StudentProj.Enums
{
    public enum ResponseStatus
    {
        // Success Operations
        UserRegisterSuccessfully,
        UserLoginSuccessfully,
        UserAddedSuccessfully,
        UserUpdatedSuccessfully,
        UserRetriveSuccessfully,
        UserSoftDeleteSuccessfully,
        RoleAssignedSuccessfully,
        RoleRevokedSuccessfully,
        RoleCreatedSuccessfully,
        RoleDeletedSuccessfully,
        RoleUpdatedSuccessfully,
        PermissionAssignedSuccessfully,
        PermissionRevokedSuccessfully,
        RoleRetriveSuccessfully,
        PermissionRetriveSuccessfully,

        // Failure/Validation Operations
        UserAlreadyExist,
        UserNotFound,
        PasswordRequired,
        InvalidCredentials,
        RoleNotFound,
        DefaultRoleNotFound,
        PermissionNotFound,
        BadRequest,
        Unauthorized,
        Forbidden,
        NotFound,
        InternalServerError
    }

    public static class ResponseStatusExtensions
    {
        public static int GetStatusCode(this ResponseStatus status) => status switch
        {
            ResponseStatus.UserRegisterSuccessfully => 200,
            ResponseStatus.UserLoginSuccessfully => 200,
            ResponseStatus.UserAddedSuccessfully => 201,
            ResponseStatus.UserUpdatedSuccessfully => 200,
            ResponseStatus.UserRetriveSuccessfully => 200,
            ResponseStatus.UserSoftDeleteSuccessfully => 200,
            ResponseStatus.RoleAssignedSuccessfully => 200,
            ResponseStatus.RoleRevokedSuccessfully => 200,
            ResponseStatus.RoleCreatedSuccessfully => 201,
            ResponseStatus.RoleDeletedSuccessfully => 200,
            ResponseStatus.RoleUpdatedSuccessfully => 200,
            ResponseStatus.PermissionAssignedSuccessfully => 200,
            ResponseStatus.PermissionRevokedSuccessfully => 200,
            ResponseStatus.RoleRetriveSuccessfully => 200,
            ResponseStatus.PermissionRetriveSuccessfully => 200,

            ResponseStatus.UserAlreadyExist => 400,
            ResponseStatus.UserNotFound => 404,
            ResponseStatus.PasswordRequired => 400,
            ResponseStatus.InvalidCredentials => 401,
            ResponseStatus.RoleNotFound => 404,
            ResponseStatus.DefaultRoleNotFound => 404,
            ResponseStatus.PermissionNotFound => 404,
            ResponseStatus.BadRequest => 400,
            ResponseStatus.Unauthorized => 401,
            ResponseStatus.Forbidden => 403,
            ResponseStatus.NotFound => 404,
            ResponseStatus.InternalServerError => 500,
            _ => 200
        };

        public static string ToFriendlyMessage(this ResponseStatus status) => status switch
        {
            ResponseStatus.UserRegisterSuccessfully => ApiMessages.UserRegisterSuccessfully,
            ResponseStatus.UserLoginSuccessfully => ApiMessages.UserLoginSuccessfully,
            ResponseStatus.UserAddedSuccessfully => ApiMessages.UserAddedSuccessfully,
            ResponseStatus.UserUpdatedSuccessfully => ApiMessages.UserUpdatedSuccessfully,
            ResponseStatus.UserRetriveSuccessfully => ApiMessages.UserRetriveSuccessfully,
            ResponseStatus.UserSoftDeleteSuccessfully => ApiMessages.UserSoftDeleteSuccessfully,
            ResponseStatus.RoleAssignedSuccessfully => ApiMessages.RoleAssignedSuccessfully,
            ResponseStatus.RoleRevokedSuccessfully => ApiMessages.RoleRevokedSuccessfully,
            ResponseStatus.RoleCreatedSuccessfully => ApiMessages.RoleCreatedSuccessfully,
            ResponseStatus.RoleDeletedSuccessfully => ApiMessages.RoleDeletedSuccessfully,
            ResponseStatus.RoleUpdatedSuccessfully => ApiMessages.RoleUpdatedSuccessfully,
            ResponseStatus.PermissionAssignedSuccessfully => ApiMessages.PermissionAssignedSuccessfully,
            ResponseStatus.PermissionRevokedSuccessfully => ApiMessages.PermissionRevokedSuccessfully,
            ResponseStatus.RoleRetriveSuccessfully => ApiMessages.RoleRetriveSuccessfully,
            ResponseStatus.PermissionRetriveSuccessfully => ApiMessages.PermissionRetriveSuccessfully,

            ResponseStatus.UserAlreadyExist => ApiMessages.UserAlreadyExist,
            ResponseStatus.UserNotFound => ApiMessages.UserNotFound,
            ResponseStatus.PasswordRequired => ApiMessages.PasswordRequired,
            ResponseStatus.InvalidCredentials => ApiMessages.InvalidCredentials,
            ResponseStatus.RoleNotFound => ApiMessages.RoleNotFound,
            ResponseStatus.DefaultRoleNotFound => ApiMessages.DefaultRoleNotFound,
            ResponseStatus.PermissionNotFound => ApiMessages.PermissionNotFound,
            ResponseStatus.BadRequest => "Bad Request",
            ResponseStatus.Unauthorized => "Unauthorized. Please log in.",
            ResponseStatus.Forbidden => "Forbidden. You do not have the required permission.",
            ResponseStatus.NotFound => "Not Found",
            ResponseStatus.InternalServerError => "An unexpected error occurred.",
            _ => status.ToString()
        };

        public static bool IsSuccess(this ResponseStatus status)
        {
            int code = status.GetStatusCode();
            return code >= 200 && code < 300;
        }
    }
}
