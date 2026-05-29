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
        
        // Privilege Operations
        public const string PrivilegeNotFound = "Privilege Not Found";
        public const string PrivilegeAssignedSuccessfully = "Privilege Assigned Successfully";
        public const string PrivilegeRevokedSuccessfully = "Privilege Revoked Successfully";
    }
}
