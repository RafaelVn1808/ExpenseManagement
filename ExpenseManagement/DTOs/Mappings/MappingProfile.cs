using AutoMapper;
using ExpenseManagement.Models;

namespace ExpenseManagement.DTOs.Mappings
{
    public class MappingProfile : Profile
    {

        public MappingProfile()
        {
            CreateMap<Expense, ExpenseDTO>().ForMember(x=> x.CategoryName, opt=> opt.MapFrom(src => src.Category.Name));

            CreateMap<ExpenseDTO, Expense>();

            CreateMap<Category, CategoryDTO>().ReverseMap();

        }
    }
} 
