using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using StudentProj.Attributes;
using StudentProj.DTO;
using StudentProj.Enums;
using StudentProj.Models;
using StudentProj.Repository;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using StudentProj.Common;
using System.Threading.Tasks;

namespace StudentProj.Controllers
{
    [Route("api/students")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IStudent _student;
        private readonly IRegisterRepository _registerepository;
        public StudentController(IStudent student, IRegisterRepository registerRepository) 
        {
            _student = student;
            _registerepository = registerRepository;
        }

        [HttpGet]
        [HasPrivilege("read:student")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAll() 
        {
            var students = await _student.GetAllStudentsasync();
            var response = ApiResponse<IEnumerable<StudentDTO>>.Create(ResponseStatus.UserRetriveSuccessfully, students);
            return StatusCode(response.StatusCodes, response);
        }

        [HasPrivilege("read:student")]
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
            var studentDTO = new StudentDTO
            {
                Name = student.Name,
                Email = student.Email,
                Address = student.Address,
                Phone = student.Phone
            };
            var response = ApiResponse<StudentDTO>.Create(ResponseStatus.UserRetriveSuccessfully, studentDTO);
            return StatusCode(response.StatusCodes, response);
        }

        [HasPrivilege("write:student")]
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
            var creatorRole = HttpContext.User.Identity?.IsAuthenticated == true 
                ? HttpContext.User.FindFirst(ClaimTypes.Role)?.Value ?? "Anonymous" 
                : "Anonymous";

            var student = new Student
            {
                Name = dto.Name,
                Email = dto.Email,
                Address = dto.Address,
                Phone = dto.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = creatorRole,
                IpAddress = IpHelper.GetClientIpAddress(HttpContext)
            };

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
            var studentDTO = new StudentDTO
            {
                Name = student.Name,
                Email = student.Email,
                Address = student.Address,
                Phone = student.Phone
            };
            var response = ApiResponse<StudentDTO>.Create(ResponseStatus.UserAddedSuccessfully, studentDTO);
            return CreatedAtAction(nameof(GetbyId), new { id = student.Id }, response);
        }

        [HasPrivilege("read:student")]
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
            var studentDTO = new StudentDTO
            {
                Name = student.Name,
                Email = student.Email,
                Address = student.Address,
                Phone = student.Phone
            };
            var response = ApiResponse<StudentDTO>.Create(ResponseStatus.UserRetriveSuccessfully, studentDTO);
            return StatusCode(response.StatusCodes, response);
        }

        [HasPrivilege("update:student")]
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
            existingstudent.Name = dto.Name;
            existingstudent.Email = dto.Email;
            existingstudent.Address = dto.Address;
            existingstudent.Phone = dto.Phone;
            await _student.UpdateStudentasync(id,existingstudent);

            var response = ApiResponse<object>.Create(ResponseStatus.UserUpdatedSuccessfully);
            return StatusCode(response.StatusCodes, response);
        }

        [HasPrivilege("write:student")]
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
            var studentdto = new StudentDTO
            {
                Name = existingstudent.Name,
                Email = existingstudent.Email,
                Address = existingstudent.Address,
                Phone = existingstudent.Phone
            };
            patchDocument.ApplyTo(studentdto, ModelState);
            if (!ModelState.IsValid) 
            {
                return BadRequest(ModelState);
            }
            existingstudent.Name = studentdto.Name;
            existingstudent.Email = studentdto.Email;
            existingstudent.Address = studentdto.Address;
            existingstudent.Phone = studentdto.Phone;
            await _student.UpdateStudentasync(id, existingstudent);

            var response = ApiResponse<object>.Create(ResponseStatus.UserUpdatedSuccessfully);
            return StatusCode(response.StatusCodes, response);
        }

        [HasPrivilege("delete:student")]
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
            student.DeletedAt = DateTime.UtcNow;
            student.DeletedBy = deleterRole;
            await _student.DeleteStudentasync(student);

            var response = ApiResponse<bool>.Create(ResponseStatus.UserSoftDeleteSuccessfully, true);
            return StatusCode(response.StatusCodes, response);
        }

        [HasPrivilege("write:student")]
        [HttpPut("upsert/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpsertStudent(int id, [FromBody] RegisterDTO dto) 
        {
            if (id < 0)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Invalid student ID.");
                return StatusCode(error.StatusCodes, error);
            }
            if (dto == null) 
            {
                var error = ApiResponse<object>.Create(ResponseStatus.BadRequest, "Student data is required.");
                return StatusCode(error.StatusCodes, error);
            }
            var actorRole = HttpContext.User.Identity?.IsAuthenticated == true 
                ? HttpContext.User.FindFirst(ClaimTypes.Role)?.Value ?? "Anonymous" 
                : "Anonymous";

            var student = new Student
            {
                Id = id,
                Name = dto.Name,
                Email = dto.Email,
                Address = dto.Address,
                Phone = dto.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = actorRole,
                IpAddress = IpHelper.GetClientIpAddress(HttpContext)
            };
            var resultid = await _student.UpsertStudentAsync(student);
            if (resultid == 0)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.UserNotFound, $"Student with ID {id} not found.");
                return StatusCode(error.StatusCodes, error);
            }
            if (id <= 0)
            {
                var studentrole = await _registerepository.GetRoleByIdAsync(3);
                if (studentrole != null)
                {
                    await _registerepository.AssignRoleAsync(resultid, studentrole.Id);
                }
            }

            var status = id <= 0 ? ResponseStatus.UserAddedSuccessfully : ResponseStatus.UserUpdatedSuccessfully;
            var response = ApiResponse<string>.Create(status, $"Student with ID {resultid} was successfully saved (inserted/updated).", resultid.ToString());
            return StatusCode(response.StatusCodes, response);
        }
    }
}
