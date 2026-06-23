using AutoMapper;
using StudentProj.Models;
using StudentProj.DTO;

namespace StudentProj.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Student mappings
            CreateMap<Student, StudentDTO>().ReverseMap();
            CreateMap<RegisterDTO, Student>();

            // Role mappings
            CreateMap<Roles, RoleDTO>().ReverseMap();
            CreateMap<Roles, RoleResponseDTO>().ReverseMap();

            // Menu mappings
            CreateMap<Menu, MenuDTO>().ReverseMap();

            // Permission mappings
            CreateMap<Permissions, PermissionDTO>().ReverseMap();

            // Route Permission mappings
            CreateMap<RoutePermissions, RoutePermissionDTO>().ReverseMap();

            //Course Mapping
            CreateMap<Course, CourseDTO>().ReverseMap();
            CreateMap<CreateCourseDTO, Course>();
            CreateMap<UpdateCourseDTO, Course>();

            //Subject Mapping
            CreateMap<Subject, SubjectDTO>().ReverseMap();
            CreateMap<CreateSubjectDTO, Subject>();

            //Enrollment Mapping
            CreateMap<EnrollStudentDTO, Enrollment>();
            CreateMap<Enrollment, EnrollmentDTO>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.Name))
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.CourseName));

            //Attendance Mapping
            CreateMap<RecordAttendanceDTO, Attendance>();
            CreateMap<Attendance, AttendanceDTO>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.Name))
                .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.Subject.SubjectName));

            //Logs Mapping
            CreateMap<Logs,LogResponseDTO>();
        }
    }
}
