using AutoMapper;
using ExpenseManagement.Models;

namespace ExpenseManagement.DTOs.Mappings
{
    public class MappingProfile : Profile
    {

        public MappingProfile()
        {
            CreateMap<Expense, ExpenseDTO>().ReverseMap();
            CreateMap<Category, CategoryDTO>().ReverseMap();

        }
    }
}
