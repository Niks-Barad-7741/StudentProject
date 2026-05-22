using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using StudentProj.DTO;
using StudentProj.Models;
using StudentProj.Repository;

namespace StudentProj.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class StudentController : ControllerBase
    {
        private readonly IStudent _student;
        public StudentController(IStudent student) 
        {
            _student = student;
        }

        [HttpGet("Getall")]
        [Authorize(Roles = "Admin,User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        //[AllowAnonymous]
        public async Task<ActionResult<IEnumerable<StudentDTO>>> GetAll() 
        {
            var students =await _student.GetAllStudentsasync();
            return Ok(students);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetbyId(int id) 
        {
            var student = await _student.GetStudentbyid(id);
            if (student == null) 
            {
                return NotFound($"Student with id {id} not found.");
            }
            return Ok(student);

        }

        [Authorize(Roles = "Admin")]
        [HttpPost("Createstudent")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> CreateStudent(StudentDTO dto) 
        {
            if (dto == null) 
            {
                return BadRequest("Student data is required.");
            }
            var student = new Student
            {
                Name = dto.Name,
                Email = dto.Email,
                Address = dto.Address,
                Phone = dto.Phone
            };
             await _student.Createstudentasync(student);
            if (student == null) 
            {
                return BadRequest("Could not create student");
            }
            return CreatedAtAction(nameof(GetbyId), new { id = student.Id }, student);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetbyName/{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetbyName(string name) 
        {
            var student = await _student.Getstudentbynameasync(name);
            if (student == null) 
            {
                return NotFound($"Student with name {name} not found.");
            }
            return Ok(student);
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateStudent/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]   
        public async Task<ActionResult> UpdateStudent(int id, StudentDTO dto) 
        {
            if (id <= 0) 
            {
                return BadRequest("Invalid student ID.");
            }
            var existingstudent = await _student.GetStudentbyid(id);
            if (existingstudent == null) 
            {
                return NotFound($"Student with id {id} not found.");
            }
            existingstudent.Name = dto.Name;
            existingstudent.Email = dto.Email;
            existingstudent.Address = dto.Address;
            existingstudent.Phone = dto.Phone;
            await _student.UpdateStudentasync(id,existingstudent);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("UpdateStudentPartial/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]  
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdatePartial(int id, [FromBody]JsonPatchDocument<StudentDTO> patchDocument) 
        {
            if (id <= 0) 
            {
                return BadRequest("Invalid Student Id");
            }
            var existingstudent = await _student.GetStudentbyid(id);
            if (existingstudent == null) 
            {
                return NotFound($"Student with id {id} not found.");
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
            return NoContent();



        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("DeletebyId/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> DeleteStudentId(int id) 
        {
            if (id <= 0) 
            {
                return BadRequest();
            }
            var student = await _student.GetStudentbyid(id);
            if (student == null) 
            {
                return NotFound($"Student with id {id} not found.");
            }
            await _student.DeleteStudentasync(student);
            return Ok(true);
        }

    }
}
