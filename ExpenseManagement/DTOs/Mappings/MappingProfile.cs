using AutoMapper;
using ExpenseManagement.Models;

namespace ExpenseManagement.DTOs.Mappings
{
    public class MappingProfile : Profile
    {

        public MappingProfile()
        {
            CreateMap<Expense, ExpenseDTO>()
                .ForMember(
                    dest => dest.CategoryName,
                    opt => opt.MapFrom(src =>
                        src.Category != null ? src.Category.Name : null
                    )
                );

            CreateMap<ExpenseDTO, Expense>();


            CreateMap<Category, CategoryDTO>().ReverseMap();

        }
    }
} 
