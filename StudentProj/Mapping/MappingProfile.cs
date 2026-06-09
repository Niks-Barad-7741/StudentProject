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
        }
    }
}
