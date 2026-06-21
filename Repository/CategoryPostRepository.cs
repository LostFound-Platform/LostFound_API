using DataAccess;
using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class CategoryPostRepository : ICategoryPostRepository
    {
        #region Variables
        private readonly CategoryPostDAO categoryPostDAO;
        #endregion

        #region Constructor
        public CategoryPostRepository(CategoryPostDAO categoryPostDAO)
        {
            this.categoryPostDAO = categoryPostDAO;
        }
        #endregion

        #region All Category Posts
        public IQueryable<CategoryPost> AllCategoryPosts()
        {
            return categoryPostDAO.AllCategoryPosts();
        }
        #endregion

        #region Search Category Post
        public IQueryable<CategoryPost> SearchCategoryPost(string query)
        {
            return categoryPostDAO.SearchCategoryPost(query);
        }
        #endregion

        #region Search Category Post
        public CategoryPost GetCategoryPostById(int categoryId)
        {
            return categoryPostDAO.GetCategoryPostById(categoryId);
        }
        #endregion
    }
}
