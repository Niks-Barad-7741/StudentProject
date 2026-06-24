using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentProj.DTO;
using StudentProj.Enums;
using StudentProj.Repository;
using StudentProj.Repository_Interface;

namespace StudentProj.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LogsController : Controller
    {
        private readonly ILogsRepository _repository;
        private readonly IMapper _mapper;

        public LogsController(ILogsRepository repository,IMapper mapper) 
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> getlogs([FromQuery] LogQueryDTO dto) 
        {
            var logs = await _repository.GetLogsAsync(dto);
            if (logs == null) 
            {
                var error = ApiResponse<object>.Create(ResponseStatus.LogsNotFound);
                return StatusCode(error.StatusCodes, error);
            }

            var response = ApiResponse<object>.Create(ResponseStatus.LogsRetriveSuccessfully,logs);
            return StatusCode(response.StatusCodes, response);
        }
    }
}
