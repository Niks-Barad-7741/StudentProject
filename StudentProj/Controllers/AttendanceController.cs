using AutoMapper;
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
    public class AttendanceController : Controller
    {
        private readonly IAttendenceRepository _repository;
        private readonly IMapper _mapper;

        public AttendanceController(IAttendenceRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> RecordAttendance([FromBody] RecordAttendanceDTO dto)
        {
            if (!ModelState.IsValid)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.InvalidData);
                return StatusCode(error.StatusCodes);
            }
            var attendance = await _repository.RecordAsync(dto);
            if (attendance == null)
            {
                var res = ApiResponse<object>.Create(ResponseStatus.SubjectNotFound);
                return StatusCode(res.StatusCodes, res);
            }
            var response = ApiResponse<object>.Create(ResponseStatus.AttendanceAddedSuccessfully, attendance);
            return StatusCode(response.StatusCodes,response);
        }

        [HttpGet("subject/{subjectId}")]
        public async Task<IActionResult> GetBySubject(int subjectId, [FromQuery] DateTime data) 
        {
            var attendance = await _repository.GetBySubjectIdAsync(subjectId, data);
            if (attendance == null) 
            {
                var res = ApiResponse<object>.Create(ResponseStatus.AttendanceNotFound);
                return StatusCode(res.StatusCodes, res);
            }         
       
            var response = ApiResponse<IEnumerable<AttendanceDTO>>.Create(ResponseStatus.AttendanceRetriveSuccessfully, attendance);
            return StatusCode(response.StatusCodes, response);
        }

        [HttpGet("report/student/{studentId}")]
        public async Task<IActionResult> GetReport(int studentId) 
        {
            var report = await _repository.GetRecordAsync(studentId);
            if(report == null)
            {
                var res = ApiResponse<object>.Create(ResponseStatus.AttendanceNotFound);
                return StatusCode(res.StatusCodes, res);
            }
            var response = ApiResponse<ReportAttendenceDTO>.Create(ResponseStatus.AttendanceRetriveSuccessfully,report);
            return StatusCode(response.StatusCodes, response);
        }
        
    }
}
