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
    public class CourseController : Controller
    {
        private readonly ICourseRepository _repository;
        //private readonly ISubjectRepository _subjectRepositiry;

        public CourseController(ICourseRepository repository)
        {
            _repository = repository;
            //_subjectRepositiry = subjectRepository;
        }


        [HttpGet]
        public async Task<IActionResult> GetALL()
        {
            var courses = await _repository.GetAllAsync();
            var respones = ApiResponse<IEnumerable<CourseDTO>>.Create(ResponseStatus.CourseRetriveSuccessfully, courses);
            return StatusCode(respones.StatusCodes, respones);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var course = await _repository.GetByIdAsync(id);
            if (course == null)
            {
                var Bad = ApiResponse<object>.Create(ResponseStatus.CourseNotFound);
                return StatusCode(Bad.StatusCodes, Bad);
            }
            var response = ApiResponse<object>.Create(ResponseStatus.CourseRetriveSuccessfully, course);
            return StatusCode(response.StatusCodes, response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCourseDTO dto)
        {
            if (!ModelState.IsValid)
            {
                var Bad = ApiResponse<object>.Create(ResponseStatus.InvalidData);
                return StatusCode(Bad.StatusCodes, Bad);
            }
            var created = await _repository.CreateAsync(dto);
            var response = ApiResponse<object>.Create(ResponseStatus.CourseCreatedSuccessfully, created);
            return StatusCode(response.StatusCodes, response);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCourseDTO dto)
        {
            if (!ModelState.IsValid)
            {
                var Bad = ApiResponse<object>.Create(ResponseStatus.InvalidData);
                return StatusCode(Bad.StatusCodes, Bad);
            }
            var updated = await _repository.UpdateAsync(id, dto);
            var response = ApiResponse<object>.Create(ResponseStatus.CourseUpdatedSuccessfully, updated);
            return StatusCode(response.StatusCodes, response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete (int id)
        {
            var deleted = await _repository.DeleteAsync(id);
            if (!deleted)
            {
                var status = ApiResponse<object>.Create(ResponseStatus.CourseNotFound, deleted);
                return StatusCode(status.StatusCodes, status);
            }
            var response = ApiResponse<object>.Create(ResponseStatus.CourseSoftDeletedSuccessfully, deleted);
            return StatusCode(response.StatusCodes, response);
        }

        [HttpPost("{id}/subjects")]
        public async Task<IActionResult> AddSubject(int id, [FromBody] CreateSubjectDTO dto) 
        {
            if (!ModelState.IsValid) 
            {
                var bad = ApiResponse<object>.Create(ResponseStatus.InvalidData);
                return StatusCode(bad.StatusCodes,bad);
            }

            var subject = await _repository.AddSubjectAsync(id, dto);
            if (subject == null) 
            {
                var status = ApiResponse<object>.Create(ResponseStatus.CourseNotFound);
                return StatusCode(status.StatusCodes, status);
            }

            var response = ApiResponse<object>.Create(ResponseStatus.SubjectAddedSuccessfully, subject);

            return StatusCode(response.StatusCodes, response);       

        }

        [HttpGet("{id}/subjects")]
        public async Task<IActionResult> GetSubjects(int id) 
        {
            var subjects = await _repository.GetSubjectsAsync(id);
            var response = ApiResponse<IEnumerable<SubjectDTO>>.Create(ResponseStatus.SubjectRetriveSuccessfully, subjects);
            return StatusCode(response.StatusCodes, response);
        }

        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteSubject(int id)
        //{
        //    var deleted = await _subjectRepositiry.DeleteAsync(id);
        //    if (!deleted)
        //    {
        //        var status = ApiResponse<object>.Create(ResponseStatus.SubjectNotFound, deleted);
        //        return StatusCode(status.StatusCodes, status);
        //    }
        //    var response = ApiResponse<object>.Create(ResponseStatus.SubjectSoftDeletedSuccessfully, deleted);
        //    return StatusCode(response.StatusCodes, response);
        //}

    }
}
