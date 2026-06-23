using StackExchange.Redis;

namespace StudentProj.Common
{
    public static class ApiMessages
    {
        // User/Student Operations
        public const string UserAlreadyExist = "User Already Exist";
        public const string UserNotFound = "User not Found";
        public const string UserAddedSuccessfully = "User Added Successfully";
        public const string UserUpdatedSuccessfully = "User Updated Successfully";
        public const string UserRetriveSuccessfully = "User Retrieve Successfully";
        public const string UserSoftDeleteSuccessfully = "User Soft Delete Successfully";
        
        // Auth Operations
        public const string PasswordRequired = "Password is Required";
        public const string InvalidCredentials = "Invalid Credentials";
        public const string UserRegisterSuccessfully = "User registered successfully.";
        public const string UserLoginSuccessfully = "User Login Successfully";

        // Role Operations
        public const string RoleNotFound = "Role Not Found";
        public const string DefaultRoleNotFound = "Manage Default Role not Found";
        public const string RoleAssignedSuccessfully = "Role Assigned Successfully";
        public const string RoleRevokedSuccessfully = "Role Revoked Successfully";
        public const string RoleCreatedSuccessfully = "Role Created Successfully";
        public const string RoleDeletedSuccessfully = "Role Deleted Successfully";
        public const string RoleUpdatedSuccessfully = "Role Updated Successfully";
        public const string RoleRetriveSuccessfully = "Roles retrieved successfully.";
        
        // Permission Operations
        public const string PermissionNotFound = "Permission Not Found";
        public const string PermissionAssignedSuccessfully = "Permission Assigned Successfully";
        public const string PermissionRevokedSuccessfully = "Permission Revoked Successfully";
        public const string PermissionRetriveSuccessfully = "Permissions retrieved successfully.";

        //Course Operations 
        public const string CourseNotFound = "Course Not Found";
        //public const string InvalidCourse = "Invalid Course";
        public const string CourseCreatedSuccessfully = "Course Added Successfully";
        public const string CourseUpdatedSuccessfully = "Course Updated Successfully";
        public const string CourseRetriveSuccessfully = "Course Retrieve Successfully";
        public const string CourseSoftDeletedSuccessfully = "Course Soft Deleted Successfully";

        // Enrollment Operations
        public const string EnrollmentNotFound = "Enrollment Not Found";
        public const string EnrollmentAddedSuccessfully = "Enrollment Added Successfully";
        public const string EnrollmentRetriveSuccessfully = "Enrollment Retrieve Successfully";
        public const string EnrollmentDeletedSuccessfully = "Enrollment Deleted Successfully";
        public const string EnrollmentUpdatedSuccessfully = "Enrollment Updated Successfully";
        public const string StudentAlreadyEnrolled = "Student is already enrolled in this course.";
        public const string GradeUpdatedSuccessfully = "Grade Updated Successfully";
        public const string GradeUpdateFailed = "Grade Update Failed";
        public const string StudentNotEnrolled = "Student is not enrolled in this course.";
        public const string InvalidGradeValue = "Invalid grade value. Grade must be between 0 and 100.";
        public const string EnrollmentSoftDeletedSuccessfully = "Enrollment Soft Deleted Successfully";
        public const string EnrollmentRestoredSuccessfully = "Enrollment Restored Successfully";

        //Attendance Operations
        public const string AttendanceNotFound = "Attendance Not Found";
        public const string AttendanceAddedSuccessfully = "Attendance Added Successfully";
        public const string AttendanceRetriveSuccessfully = "Attendance Retrieve Successfully";
        public const string AttendanceDeletedSuccessfully = "Attendance Deleted Successfully";


        //Subject Operations
        public const string SubjectNotFound = "Subject Not Found";
        public const string SubjectAddedSuccessfully = "Subject Added Successfully";
        public const string SubjectUpdatedSuccessfully = "Subject Updated Successfully";
        public const string SubjectRetriveSuccessfully = "Subject Retrieve Successfully";
        public const string SubjectSoftDeletedSuccessfully = "Subject Soft Deleted Successfully";
        public const string SubjectRestoredSuccessfully = "Subject Restored Successfully";
        public const string SubjectAlreadyExists = "Subject Already Exists";
        public const string SubjectNotBelongsToCourse = "Subject does not belong to the specified course.";
        public const string SubjectAlreadyEnrolled = "Student is already enrolled in this subject.";
        public const string SubjectNotEnrolled = "Student is not enrolled in this subject.";

    }
}
