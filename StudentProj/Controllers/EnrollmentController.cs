using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentProj.DTO;
using StudentProj.Enums;
using StudentProj.Repository_Interface;

namespace StudentProj.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnrollmentController : Controller
    {
        private readonly IEnrollmentRepository _repository;

        public EnrollmentController(IEnrollmentRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> Enroll([FromBody] EnrollStudentDTO dto)
        {
            if (!ModelState.IsValid)
            {
                var badresponse = ApiResponse<object>.Create(ResponseStatus.InvalidData);
                return StatusCode(badresponse.StatusCodes);
            }
            var enrollment = await _repository.EnrollStudentAsync(dto);
            if (enrollment == null)
            {
                var res = ApiResponse<object>.Create(ResponseStatus.StudentAlreadyEnrolled);
                return StatusCode(res.StatusCodes, res);
            }

            var response = ApiResponse<object>.Create(ResponseStatus.EnrollmentAddedSuccessfully);
            return StatusCode(response.StatusCodes, response);
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetByStudent(int studentId)
        {
            var enrollment = await _repository.GetStudentByIdAsync(studentId);
            var response = ApiResponse<IEnumerable<EnrollmentDTO>>.Create(ResponseStatus.EnrollmentRetriveSuccessfully, enrollment);
            return StatusCode(response.StatusCodes, response);
        }

        [HttpPut("{id}/grade")]
        public async Task<IActionResult> UpdateGrade(int id, [FromBody] UpdateGradeDTO dto) 
        {
            if (!ModelState.IsValid)
            {
                var badresponse = ApiResponse<object>.Create(ResponseStatus.InvalidData);
                return StatusCode(badresponse.StatusCodes);
            }
            var enrollment = await _repository.UpdateGradeAsync(id, dto);
            if (enrollment == null) 
            {
                var res = ApiResponse<object>.Create(ResponseStatus.GradeUpdateFailed);
                return StatusCode(res.StatusCodes, res);
            }

            var response = ApiResponse<object>.Create(ResponseStatus.GradeUpdatedSuccessfully);
            return StatusCode(response.StatusCodes, response);
        }

    }


}
