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
        CourseNotFound,
        CourseCreatedSuccessfully,
        CourseUpdatedSuccessfully,
        CourseRetriveSuccessfully,
        CourseSoftDeletedSuccessfully,
        EnrollmentNotFound,
        EnrollmentAddedSuccessfully,
        EnrollmentRetriveSuccessfully,
        EnrollmentDeletedSuccessfully,
        EnrollmentUpdatedSuccessfully,
        StudentAlreadyEnrolled,
        GradeUpdatedSuccessfully,
        GradeUpdateFailed,
        StudentNotEnrolled,
        InvalidGradeValue,
        EnrollmentSoftDeletedSuccessfully,
        EnrollmentRestoredSuccessfully,
        AttendanceNotFound,
        AttendanceAddedSuccessfully,
        AttendanceRetriveSuccessfully,
        AttendanceDeletedSuccessfully,
        SubjectNotFound,
        SubjectAddedSuccessfully,
        SubjectUpdatedSuccessfully,
        SubjectRetriveSuccessfully,
        SubjectSoftDeletedSuccessfully,
        SubjectRestoredSuccessfully,
        SubjectAlreadyExists,
        SubjectNotBelongsToCourse,
        SubjectAlreadyEnrolled,
        SubjectNotEnrolled,

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
        InternalServerError,
        InvalidData
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


            ResponseStatus.CourseNotFound => 404,
            ResponseStatus.CourseCreatedSuccessfully => 201,
            ResponseStatus.CourseUpdatedSuccessfully => 200,
            ResponseStatus.CourseRetriveSuccessfully => 200,
            ResponseStatus.CourseSoftDeletedSuccessfully => 200,
            
            
            ResponseStatus.EnrollmentNotFound => 404,
            ResponseStatus.EnrollmentAddedSuccessfully => 201,
            ResponseStatus.EnrollmentRetriveSuccessfully=> 200,
            ResponseStatus.EnrollmentDeletedSuccessfully => 200,
            ResponseStatus.EnrollmentUpdatedSuccessfully => 200,
            ResponseStatus.StudentAlreadyEnrolled => 409,
            ResponseStatus.GradeUpdatedSuccessfully => 200,
            ResponseStatus.GradeUpdateFailed => 400,
            ResponseStatus.StudentNotEnrolled => 404,
            ResponseStatus.InvalidGradeValue => 400,
            ResponseStatus.EnrollmentSoftDeletedSuccessfully => 200,
            ResponseStatus.EnrollmentRestoredSuccessfully => 200,

            ResponseStatus.AttendanceNotFound => 404,
            ResponseStatus.AttendanceAddedSuccessfully => 201,
            ResponseStatus.AttendanceRetriveSuccessfully => 200,
            ResponseStatus.AttendanceDeletedSuccessfully => 200,


            ResponseStatus.SubjectNotFound => 404,
            ResponseStatus.SubjectAddedSuccessfully => 201,
            ResponseStatus.SubjectUpdatedSuccessfully => 200,
            ResponseStatus.SubjectRetriveSuccessfully => 200,
            ResponseStatus.SubjectSoftDeletedSuccessfully => 200,
            ResponseStatus.SubjectRestoredSuccessfully => 200,
            ResponseStatus.SubjectAlreadyExists => 409,
            ResponseStatus.SubjectNotBelongsToCourse => 400,
            ResponseStatus.SubjectAlreadyEnrolled => 409,
            ResponseStatus.SubjectNotEnrolled => 404,

            ResponseStatus.InvalidData => 400,
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
            ResponseStatus.InvalidData => "Invalid data provided. Please check your input and try again.",
            ResponseStatus.BadRequest => "Bad Request",
            ResponseStatus.Unauthorized => "Unauthorized. Please log in.",
            ResponseStatus.Forbidden => "Forbidden. You do not have the required permission.",
            ResponseStatus.NotFound => "Not Found",
            ResponseStatus.InternalServerError => "An unexpected error occurred.",


            ResponseStatus.CourseNotFound => ApiMessages.CourseNotFound,
            ResponseStatus.CourseCreatedSuccessfully => ApiMessages.CourseCreatedSuccessfully,
            ResponseStatus.CourseUpdatedSuccessfully => ApiMessages.CourseUpdatedSuccessfully,
            ResponseStatus.CourseRetriveSuccessfully => ApiMessages.CourseRetriveSuccessfully,
            ResponseStatus.CourseSoftDeletedSuccessfully => ApiMessages.CourseSoftDeletedSuccessfully,


            ResponseStatus.EnrollmentNotFound => ApiMessages.EnrollmentNotFound,
            ResponseStatus.EnrollmentAddedSuccessfully => ApiMessages.EnrollmentAddedSuccessfully,
            ResponseStatus.EnrollmentRetriveSuccessfully => ApiMessages.EnrollmentRetriveSuccessfully,
            ResponseStatus.EnrollmentDeletedSuccessfully => ApiMessages.EnrollmentDeletedSuccessfully,
            ResponseStatus.EnrollmentUpdatedSuccessfully => ApiMessages.EnrollmentUpdatedSuccessfully,
            ResponseStatus.StudentAlreadyEnrolled => ApiMessages.StudentAlreadyEnrolled,
            ResponseStatus.GradeUpdatedSuccessfully => ApiMessages.GradeUpdatedSuccessfully,
            ResponseStatus.GradeUpdateFailed => ApiMessages.GradeUpdateFailed,
            ResponseStatus.StudentNotEnrolled => ApiMessages.StudentNotEnrolled,
            ResponseStatus.InvalidGradeValue => ApiMessages.InvalidGradeValue,
            ResponseStatus.EnrollmentSoftDeletedSuccessfully => ApiMessages.EnrollmentSoftDeletedSuccessfully,
            ResponseStatus.EnrollmentRestoredSuccessfully => ApiMessages.EnrollmentRestoredSuccessfully,


            ResponseStatus.AttendanceNotFound => ApiMessages.AttendanceNotFound,
            ResponseStatus.AttendanceAddedSuccessfully => ApiMessages.AttendanceAddedSuccessfully,
            ResponseStatus.AttendanceRetriveSuccessfully => ApiMessages.AttendanceRetriveSuccessfully,
            ResponseStatus.AttendanceDeletedSuccessfully => ApiMessages.AttendanceDeletedSuccessfully,


            ResponseStatus.SubjectNotFound => ApiMessages.SubjectNotFound,
            ResponseStatus.SubjectAddedSuccessfully => ApiMessages.SubjectAddedSuccessfully,
            ResponseStatus.SubjectUpdatedSuccessfully => ApiMessages.SubjectUpdatedSuccessfully,
            ResponseStatus.SubjectRetriveSuccessfully => ApiMessages.SubjectRetriveSuccessfully,
            ResponseStatus.SubjectSoftDeletedSuccessfully => ApiMessages.SubjectSoftDeletedSuccessfully,
            ResponseStatus.SubjectRestoredSuccessfully => ApiMessages.SubjectRestoredSuccessfully,
            ResponseStatus.SubjectAlreadyExists => ApiMessages.SubjectAlreadyExists,
            ResponseStatus.SubjectNotBelongsToCourse => ApiMessages.SubjectNotBelongsToCourse,
            ResponseStatus.SubjectAlreadyEnrolled => ApiMessages.SubjectAlreadyEnrolled,
            ResponseStatus.SubjectNotEnrolled => ApiMessages.SubjectNotEnrolled,

            _ => status.ToString()
        };

        public static bool IsSuccess(this ResponseStatus status)
        {
            int code = status.GetStatusCode();
            return code >= 200 && code < 300;
        }
    }
}
