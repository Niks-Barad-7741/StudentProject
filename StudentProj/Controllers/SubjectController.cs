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
    public class SubjectController : ControllerBase
    {
        private readonly ISubjectRepository _repository;
        private readonly IMapper _mapper;

        public SubjectController(ISubjectRepository repository, IMapper mapper) 
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var subjects = await _repository.GetAllAsync();
            if (subjects == null) 
            {
                var error = ApiResponse<object>.Create(ResponseStatus.SubjectNotFound);
                return StatusCode(error.StatusCodes, error);
            }

            var response = ApiResponse<object>.Create(ResponseStatus.SubjectRetriveSuccessfully, subjects);

            return StatusCode(response.StatusCodes, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetbyId(int id) 
        {
            var subject = await _repository.GetByIdAsync(id);
            if (subject ==null)
            {
                var err = ApiResponse<object>.Create(ResponseStatus.SubjectNotFound);
                return StatusCode(err.StatusCodes, err);
            }
            var response = ApiResponse<object>.Create(ResponseStatus.SubjectRetriveSuccessfully, subject);
            return StatusCode(response.StatusCodes, response);
        }

        [HttpPost]
        public async Task<IActionResult> Createsub([FromBody]CreateSubjectDTO dto) 
        {
            if (!ModelState.IsValid)
            {
                var err = ApiResponse<object>.Create(ResponseStatus.InvalidData);
                return StatusCode(err.StatusCodes, err);
            }
            var subject = await _repository.CreateAsync(dto);
            if (subject == null)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.SubjectAlreadyExists,subject);
                return StatusCode(error.StatusCodes, error);
            }
            var response = ApiResponse<object>.Create(ResponseStatus.SubjectAddedSuccessfully, subject);
            return StatusCode(response.StatusCodes, response); 
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id) 
        {
            var delete = await _repository.DeleteAsync(id);
            if (!delete) 
            {
                var error = ApiResponse<object>.Create(ResponseStatus.SubjectNotFound);
                return StatusCode(error.StatusCodes, error);
            }
            var response = ApiResponse<object>.Create(ResponseStatus.SubjectSoftDeletedSuccessfully);
            return StatusCode(response.StatusCodes, response);

        }

        [HttpPut]
        public async Task<IActionResult> Updatesubject(int id,[FromBody] UpdateSubjectDTO dto) 
        {
            if (!ModelState.IsValid)
            {
                var err = ApiResponse<object>.Create(ResponseStatus.InvalidData);
                return StatusCode(err.StatusCodes, err);
            }
            var subject = await _repository.UpdateAsync(id,dto);
            if (subject == null)
            {
                var error = ApiResponse<object>.Create(ResponseStatus.SubjectNotFound);
                return StatusCode(error.StatusCodes, error);
            }
            var response = ApiResponse<object>.Create(ResponseStatus.SubjectUpdatedSuccessfully, subject);
            return StatusCode(response.StatusCodes, response);
        }



    }
}
