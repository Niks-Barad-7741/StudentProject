using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using StudentProj.Attributes;
using StudentProj.DTO;
using StudentProj.Enums;
using StudentProj.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using StudentProj.Common;
using System.Threading.Tasks;
using StudentProj.Repository_Interface;
using AutoMapper;

namespace StudentProj.Controllers
{
    [Route("api/students")]
    [ApiController]
    [Authorize]
    public class StudentController : ControllerBase
    {
        private readonly IStudent _student;
        private readonly IRegisterRepository _registerepository;
        private readonly IMapper _mapper;
        public StudentController(IStudent student, IRegisterRepository registerRepository, IMapper mapper) 
        {
            _student = student;
            _registerepository = registerRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAll() 
        {
            var students = await _student.GetAllStudentsasync();
            var response = ApiResponse<IEnumerable<StudentDTO>>.Create(ResponseStatus.UserRetriveSuccessfully, students);
            return StatusCode(response.StatusCodes, response);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetbyId(int id) 
        {
            var student = await _student.GetStudentbyid(id);
            if (student == null) 
            {
                var error = ApiResponse<object>.Create(ResponseStatus.UserNotFound, $"Student with id {id} not found.");
                return StatusCode(error.StatusCodes, error);
            }
            var studentDTO = _mapper.Map<StudentDTO>(student);
            var response = ApiResponse<StudentDTO>.Create(ResponseStatus.UserRetriveSuccessfully, studentDTO);
            return StatusCode(response.StatusCodes, response);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> CreateStudent(RegisterDTO dto) 
        {
            if (dto == null) 
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Student data is required.");
                return StatusCode(error.StatusCodes, error);
            }

            var existing = await _registerepository.GetStudentbyphoneasync(dto.Phone);
            if (existing != null)
            {
                var errorResponse = ApiResponse<object>.Create(ResponseStatus.UserAlreadyExist, "Phone number already registered!");
                return StatusCode(errorResponse.StatusCodes, errorResponse);
            }

            var creatorRole = HttpContext.User.Identity?.IsAuthenticated == true 
                ? HttpContext.User.FindFirst(ClaimTypes.Role)?.Value ?? "Anonymous" 
                : "Anonymous";

            var student = _mapper.Map<Student>(dto);
            student.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            student.CreatedAt = DateTimeHelper.GetIndianStandardTime();
            student.CreatedBy = creatorRole;
            student.IpAddress = IpHelper.GetClientIpAddress(HttpContext);

            await _student.Createstudentasync(student);
            if (student == null) 
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Could not create student");
                return StatusCode(error.StatusCodes, error);
            }
            var studentrole = await _registerepository.GetRoleByIdAsync(3);
            if ( studentrole != null)
            {  
                await _registerepository.AssignRoleAsync(student.Id, studentrole.Id);
            }
            var studentDTO = _mapper.Map<StudentDTO>(student);
            var response = ApiResponse<StudentDTO>.Create(ResponseStatus.UserAddedSuccessfully, studentDTO);
            return CreatedAtAction(nameof(GetbyId), new { id = student.Id }, response);
        }

        [HttpGet("by-name/{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetbyName(string name) 
        {
            var student = await _student.Getstudentbynameasync(name);
            if (student == null) 
            {
                var error = ApiResponse<object>.Create(ResponseStatus.UserNotFound, $"Student with name {name} not found.");
                return StatusCode(error.StatusCodes, error);
            }
            var studentDTO = _mapper.Map<StudentDTO>(student);
            var response = ApiResponse<StudentDTO>.Create(ResponseStatus.UserRetriveSuccessfully, studentDTO);
            return StatusCode(response.StatusCodes, response);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]   
        public async Task<ActionResult> UpdateStudent(int id, StudentDTO dto) 
        {
            if (id <= 0) 
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Invalid student ID.");
                return StatusCode(error.StatusCodes, error);
            }
            var existingstudent = await _student.GetStudentbyid(id);
            if (existingstudent == null) 
            {
                var error = ApiResponse<object>.Create(ResponseStatus.UserNotFound, $"Student with id {id} not found.");
                return StatusCode(error.StatusCodes, error);
            }

            var existingByPhone = await _registerepository.GetStudentbyphoneasync(dto.Phone);
            if (existingByPhone != null && existingByPhone.Id != id)
            {
                var errorResponse = ApiResponse<object>.Create(ResponseStatus.UserAlreadyExist, "Phone number already registered!");
                return StatusCode(errorResponse.StatusCodes, errorResponse);
            }

            _mapper.Map(dto, existingstudent);
            
            var actorRole = HttpContext.User.Identity?.IsAuthenticated == true 
                ? HttpContext.User.FindFirst(ClaimTypes.Role)?.Value ?? "Anonymous" 
                : "Anonymous";
            existingstudent.UpdatedAt = DateTimeHelper.GetIndianStandardTime();
            existingstudent.UpdatedBy = actorRole;

            await _student.UpdateStudentasync(id,existingstudent);

            var response = ApiResponse<object>.Create(ResponseStatus.UserUpdatedSuccessfully);
            return StatusCode(response.StatusCodes, response);
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]  
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdatePartial(int id, [FromBody]JsonPatchDocument<StudentDTO> patchDocument) 
        {
            if (id <= 0) 
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Invalid Student Id");
                return StatusCode(error.StatusCodes, error);
            }
            var existingstudent = await _student.GetStudentbyid(id);
            if (existingstudent == null) 
            {
                var error = ApiResponse<object>.Create(ResponseStatus.UserNotFound, $"Student with id {id} not found.");
                return StatusCode(error.StatusCodes, error);
            }
            var studentdto = _mapper.Map<StudentDTO>(existingstudent);
            patchDocument.ApplyTo(studentdto, ModelState);
            if (!ModelState.IsValid) 
            {
                return BadRequest(ModelState);
            }

            var existingByPhone = await _registerepository.GetStudentbyphoneasync(studentdto.Phone);
            if (existingByPhone != null && existingByPhone.Id != id)
            {
                var errorResponse = ApiResponse<object>.Create(ResponseStatus.UserAlreadyExist, "Phone number already registered!");
                return StatusCode(errorResponse.StatusCodes, errorResponse);
            }

            _mapper.Map(studentdto, existingstudent);
            
            var actorRole = HttpContext.User.Identity?.IsAuthenticated == true 
                ? HttpContext.User.FindFirst(ClaimTypes.Role)?.Value ?? "Anonymous" 
                : "Anonymous";
            existingstudent.UpdatedAt = DateTimeHelper.GetIndianStandardTime();
            existingstudent.UpdatedBy = actorRole;

            await _student.UpdateStudentasync(id, existingstudent);

            var response = ApiResponse<object>.Create(ResponseStatus.UserUpdatedSuccessfully);
            return StatusCode(response.StatusCodes, response);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> DeleteStudentId(int id) 
        {
            if (id <= 0) 
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Invalid student ID.");
                return StatusCode(error.StatusCodes, error);
            }

            var student = await _student.GetStudentbyid(id);
            if (student == null)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.UserNotFound, $"Student with id {id} not found.");
                return StatusCode(error.StatusCodes, error);
            }

            var deleterRole = HttpContext.User.Identity?.IsAuthenticated == true 
                ? HttpContext.User.FindFirst(ClaimTypes.Role)?.Value ?? "Anonymous" 
                : "Anonymous";

            student.IsDeleted = true;
            student.DeletedAt = DateTimeHelper.GetIndianStandardTime();
            student.DeletedBy = deleterRole;
            await _student.DeleteStudentasync(student);

            var response = ApiResponse<bool>.Create(ResponseStatus.UserSoftDeleteSuccessfully, true);
            return StatusCode(response.StatusCodes, response);
        }

        [HttpPut("upsert/{id?}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpsertStudent(int? id, [FromBody] RegisterDTO dto) 
        {
            int studentId = id ?? 0;

            if (studentId < 0)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Invalid student ID.");
                return StatusCode(error.StatusCodes, error);
            }
            if (dto == null) 
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Student data is required.");
                return StatusCode(error.StatusCodes, error);
            }

            var existingByPhone = await _registerepository.GetStudentbyphoneasync(dto.Phone);
            if (existingByPhone != null && (studentId <= 0 || existingByPhone.Id != studentId))
            {
                var errorResponse = ApiResponse<object>.Create(ResponseStatus.UserAlreadyExist, "Phone number already registered!");
                return StatusCode(errorResponse.StatusCodes, errorResponse);
            }

            var actorRole = HttpContext.User.Identity?.IsAuthenticated == true 
                ? HttpContext.User.FindFirst(ClaimTypes.Role)?.Value ?? "Anonymous" 
                : "Anonymous";

            var student = _mapper.Map<Student>(dto);
            student.Id = studentId;
            student.PasswordHash = studentId <= 0 ? BCrypt.Net.BCrypt.HashPassword(dto.Password) : null!; // Only hash for new inserts
            student.CreatedAt = DateTimeHelper.GetIndianStandardTime();
            student.CreatedBy = actorRole;
            student.UpdatedAt = studentId > 0 ? DateTimeHelper.GetIndianStandardTime() : null;
            student.UpdatedBy = studentId > 0 ? actorRole : null;
            student.IpAddress = IpHelper.GetClientIpAddress(HttpContext);
            var resultid = await _student.UpsertStudentAsync(student);
            if (resultid == 0)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.UserNotFound, $"Student with ID {studentId} not found.");
                return StatusCode(error.StatusCodes, error);
            }
            if (studentId <= 0)
            {
                var studentrole = await _registerepository.GetRoleByIdAsync(3);
                if (studentrole != null)
                {
                    await _registerepository.AssignRoleAsync(resultid, studentrole.Id);
                }
            }

            var status = studentId <= 0 ? ResponseStatus.UserAddedSuccessfully : ResponseStatus.UserUpdatedSuccessfully;
            var response = ApiResponse<string>.Create(status, $"Student with ID {resultid} was successfully saved (inserted/updated).", resultid.ToString());
            return StatusCode(response.StatusCodes, response);
        }
    }
}
