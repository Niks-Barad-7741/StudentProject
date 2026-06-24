using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentProj.Data;
using StudentProj.DTO;
using StudentProj.Repository_Interface;
using StudentProj.Common;

namespace StudentProj.Repository
{
    public class LogsRepository : ILogsRepository
    {
        private readonly StudentDbcontext _dbcontext;
        private readonly IMapper _mapper;

        public LogsRepository(StudentDbcontext dbcontext, IMapper mapper) 
        {
            _dbcontext = dbcontext;
            _mapper = mapper;
        }

        public async Task<IEnumerable<LogResponseDTO>> GetLogsAsync(LogQueryDTO query)
        {
            var logs =  _dbcontext.Logs.AsQueryable();
            if (!string.IsNullOrEmpty(query.Email)) 
            {
                logs = logs.Where(n => n.Email == query.Email);
            }
            if (!string.IsNullOrEmpty(query.Action))
            {
                logs = logs.Where(n => n.Action.Contains(query.Action));
            }
            if (query.FromDate.HasValue)
            {
                var fromDate = query.FromDate.Value.Date;
                logs = logs.Where(n => n.Timestamp >= fromDate);
            }
            if (query.ToDate.HasValue)
            {
                var toDate = query.ToDate.Value.Date
                .AddDays(1).AddTicks(-1);
                logs = logs.Where(n => n.Timestamp <= toDate);
            }

            var result = await logs
                .OrderByDescending(n => n.Timestamp)
                .ToListAsync();

            return _mapper.Map<List<LogResponseDTO>>(result);
        }
    }
}
