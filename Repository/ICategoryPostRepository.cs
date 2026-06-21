using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface ICategoryPostRepository
    {
        public IQueryable<CategoryPost> AllCategoryPosts();
        public IQueryable<CategoryPost> SearchCategoryPost(string query);
        public CategoryPost GetCategoryPostById(int categoryId);
    }
}
