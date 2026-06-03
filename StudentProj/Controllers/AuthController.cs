using BCrypt.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentProj.Attributes;
using StudentProj.DTO;
using StudentProj.Enums;
using StudentProj.Models;
using StudentProj.Services;
using StudentProj.Common;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;
using StudentProj.Repository_Interface;

namespace StudentProj.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IRegisterRepository _auth;
        private readonly ILoginRepository _login;
        private readonly JwtService _JWT_service;
        private readonly ILoggingService _logging;
        private readonly IStudent _student;
        private readonly IConfiguration _config;

        public AuthController(
            IRegisterRepository auth,
            ILoginRepository login,
            JwtService JWT_service,
            ILoggingService logging,
            IStudent student,
            IConfiguration config)
        {
            _auth = auth;
            _login = login;
            _JWT_service = JWT_service;
            _logging = logging;
            _student = student;
            _config = config;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Register(RegisterDTO dto)
        {
            // check Phone Number exists
            var existing = await _auth.GetStudentbyphoneasync(dto.Phone);
            if (existing != null)
            {
                await _logging.LogActivityAsync(dto.Name, dto.Email, "Registration Failed: Phone number already registered", HttpContext);
                var errorResponse = ApiResponse<object>.Create(ResponseStatus.UserAlreadyExist, "Phone number already registered!");
                return StatusCode(errorResponse.StatusCodes, errorResponse);
            }

            // create student
            var student = new Student
            {
                Name = dto.Name,
                Email = dto.Email,
                Address = dto.Address,
                Phone = dto.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreatedAt = DateTimeHelper.GetIndianStandardTime(),
                CreatedBy = "User", // Default for self-registration
                IpAddress = IpHelper.GetClientIpAddress(HttpContext)
            };

            await _auth.RegisterAsync(student);

            var studentRole = await _auth.GetRoleByIdAsync(3);
            if (studentRole != null)
                await _auth.AssignRoleAsync(student.Id, studentRole.Id);

            var roles = await _auth.GetStudentRolesAsync(student.Id);
            var token = _JWT_service.GenerateToken(student, roles);
            var refreshToken = _JWT_service.GenerateRefreshToken();

            student.RefereshToken = refreshToken;
            student.RefereshTokenExpiryTime = DateTimeHelper.GetIndianStandardTime().AddDays(7);
            await _student.UpdateStudentasync(student.Id, student);

            await _logging.LogActivityAsync(student.Name, student.Email, "Registration Succeeded", HttpContext);

            // Standardize return payload to match login format (using ApiResponse<LoginResponseDTO>)
            var authData = new LoginResponseDTO
            {
                Token = token,
                RefreshToken = refreshToken
            };
            var response = ApiResponse<LoginResponseDTO>.Create(ResponseStatus.UserRegisterSuccessfully, authData);
            return StatusCode(response.StatusCodes, response);
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Login(LoginDTO dto)
        {
            // find student
            var student = await _login.GetStudentbyemailasync(dto.Email);
            if (student == null)
            {
                await _logging.LogActivityAsync("Anonymous", dto.Email, "Login Failed: Invalid Email", HttpContext);
                var errorResponse = ApiResponse<object>.Create(ResponseStatus.InvalidCredentials, "Invalid email.");
                return StatusCode(errorResponse.StatusCodes, errorResponse);
            }

            // verify password
            bool isValid = BCrypt.Net.BCrypt.Verify(dto.Password, student.PasswordHash);
            if (!isValid)
            {
                await _logging.LogActivityAsync("Anonymous", dto.Email, "Login Failed: Invalid Password", HttpContext);
                var errorResponse = ApiResponse<object>.Create(ResponseStatus.InvalidCredentials, "Invalid Password.");
                return StatusCode(errorResponse.StatusCodes, errorResponse);
            }

            var roles = await _login.GetStudentRolesAsync(student.Id);
            var token = _JWT_service.GenerateToken(student, roles);
            var refreshToken = _JWT_service.GenerateRefreshToken();

            student.RefereshToken = refreshToken;
            student.RefereshTokenExpiryTime = DateTimeHelper.GetIndianStandardTime().AddDays(7);
            await _student.UpdateStudentasync(student.Id, student);

            // Return standardized ApiResponse wrapped around LoginResponseDTO (token only)
            await _logging.LogActivityAsync(student.Name, student.Email, "Login Succeeded", HttpContext);
            var authData = new LoginResponseDTO
            {
                Token = token,
                RefreshToken = refreshToken
            };
            var response = ApiResponse<LoginResponseDTO>.Create(ResponseStatus.UserLoginSuccessfully, authData);
            return StatusCode(response.StatusCodes, response);
        }
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordDTO dto)
        {
            var student = await _login.GetStudentbyemailasync(dto.Email);
            if (student == null)
            {
                var errorResponse = ApiResponse<object>.Create(ResponseStatus.UserNotFound, "User not found with that email.");
                return StatusCode(errorResponse.StatusCodes, errorResponse);
            }

            // Prevent spamming and potential thread DoS (Rate Limit: 1 minute between OTP requests)
            if (student.ResetOtpExpiry != null && DateTimeHelper.GetIndianStandardTime() < student.ResetOtpExpiry)
            {
                var errorResponse = ApiResponse<object>.Create(ResponseStatus.BadRequest, "An active OTP was already sent. Please wait 1 minute before requesting a new one.");
                return StatusCode(errorResponse.StatusCodes, errorResponse);
            }

            // Generate 6-digit OTP
            var otp = new Random().Next(100000, 999999).ToString();
            
            // Save to DB with 1-minute expiration
            student.ResetOtp = otp;
            student.ResetOtpExpiry = DateTimeHelper.GetIndianStandardTime().AddMinutes(1);
            await _student.UpdateStudentasync(student.Id, student);

            // Send email
            try
            {
                var emailSettings = _config.GetSection("EmailSettings");
                using var client = new SmtpClient(emailSettings["SmtpServer"], int.Parse(emailSettings["SmtpPort"]))
                {
                    Credentials = new NetworkCredential(emailSettings["SenderEmail"], emailSettings["AppPassword"]),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(emailSettings["SenderEmail"], emailSettings["SenderName"]),
                    Subject = "Your Password Reset OTP",
                    Body = $"Your 6-digit OTP for password reset is: <b>{otp}</b><br/><br/>This code expires in 1 minute.",
                    IsBodyHtml = true
                };
                mailMessage.To.Add(dto.Email);

                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Create(ResponseStatus.BadRequest, $"Failed to send email. Error: {ex.Message}");
                return StatusCode(errorResponse.StatusCodes, errorResponse);
            }

            var response = ApiResponse<object>.Create(ResponseStatus.UserUpdatedSuccessfully, "OTP has been sent to your email.");
            return StatusCode(response.StatusCodes, response);
        }

        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            var student = await _login.GetStudentbyemailasync(dto.Email);
            if (student == null)
            {
                var errorResponse = ApiResponse<object>.Create(ResponseStatus.UserNotFound, "User not found with that email.");
                return StatusCode(errorResponse.StatusCodes, errorResponse);
            }

            if (student.ResetOtp != dto.Otp)
            {
                var errorResponse = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Invalid OTP.");
                return StatusCode(errorResponse.StatusCodes, errorResponse);
            }

            if (student.ResetOtpExpiry == null || DateTimeHelper.GetIndianStandardTime() > student.ResetOtpExpiry)
            {
                var errorResponse = ApiResponse<object>.Create(ResponseStatus.BadRequest, "OTP has expired.");
                return StatusCode(errorResponse.StatusCodes, errorResponse);
            }

            // Update password
            student.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            student.ResetOtp = null; // Clear OTP
            student.ResetOtpExpiry = null;
            
            await _student.UpdateStudentasync(student.Id, student);

            var response = ApiResponse<object>.Create(ResponseStatus.UserUpdatedSuccessfully, "Password reset successfully. You can now login.");
            return StatusCode(response.StatusCodes, response);
        }
        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Refresh([FromBody] TokenRequestDTO dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.AccessToken) || string.IsNullOrEmpty(dto.RefereshToken))
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Invalid client request.");
                return StatusCode(error.StatusCodes, error);
            }

            ClaimsPrincipal? principal;
            try
            {
                principal = _JWT_service.GetClaimsPrincipalFromExpiredToken(dto.AccessToken);
            }
            catch (Exception)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Invalid access token.");
                return StatusCode(error.StatusCodes, error);
            }

            if (principal == null)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Invalid access token.");
                return StatusCode(error.StatusCodes, error);
            }

            var emailClaim = principal.FindFirst("Email") ?? principal.FindFirst(ClaimTypes.Email);
            if (emailClaim == null)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Invalid access token claims.");
                return StatusCode(error.StatusCodes, error);
            }

            var student = await _login.GetStudentbyemailasync(emailClaim.Value);
            if (student == null || student.RefereshToken != dto.RefereshToken || student.RefereshTokenExpiryTime <= DateTimeHelper.GetIndianStandardTime())
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Invalid refresh token or token has expired.");
                return StatusCode(error.StatusCodes, error);
            }

            var roles = await _login.GetStudentRolesAsync(student.Id);
            var newAccessToken = _JWT_service.GenerateToken(student, roles);
            var newRefreshToken = _JWT_service.GenerateRefreshToken();

            // Rotate refresh token (save new one to DB)
            student.RefereshToken = newRefreshToken;
            student.RefereshTokenExpiryTime = DateTimeHelper.GetIndianStandardTime().AddDays(7);
            await _student.UpdateStudentasync(student.Id, student);

            var responseData = new LoginResponseDTO
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken
            };

            var success = ApiResponse<LoginResponseDTO>.Create(ResponseStatus.UserLoginSuccessfully, responseData);
            return StatusCode(success.StatusCodes, success);
        }
    }
}
