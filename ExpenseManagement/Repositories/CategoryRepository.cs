using ExpenseManagement.Context;
using ExpenseManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManagement.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {

        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Category> GetCategories()
        {
            return _context.Categories.ToList();
               
        }

        public Category GetCategoryById(int categoryId)
        {
            return _context.Categories.FirstOrDefault(c => c.CategoryId == categoryId);
        }
        public Category Create(Category category)
        {
            if (category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            _context.Categories.Add(category);
            _context.SaveChanges();
            return category;
        }

        public Category Update(Category category)
        {
            if (category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            _context.Entry(category).State = EntityState.Modified;
            _context.SaveChanges();
            return category;
        }

        public Category Delete(int categoryId)
        {
            var category = _context.Categories.Find(categoryId);

            if (category != null)
            {throw new ArgumentNullException(nameof(category));

            }
            _context.Categories.Remove(category);
            _context.SaveChanges();
            return category;

        }

        
    }
}
