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

            CreateMap<ExpenseDTO, Expense>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());


            CreateMap<Category, CategoryDTO>().ReverseMap();

        }
    }
} 
