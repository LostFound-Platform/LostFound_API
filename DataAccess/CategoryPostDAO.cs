using Microsoft.EntityFrameworkCore;
using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class CategoryPostDAO
    {
        #region Variables
        private readonly FBLADbContext db;
        #endregion

        #region Constructor
        public CategoryPostDAO(FBLADbContext db)
        {
            this.db = db;
        }
        #endregion

        #region Create Category Post
        public async Task<bool> CreateCategoryPost(CategoryPost categoryPost)
        {
            categoryPost.CategoryPostId = new Random().Next();
            var isAdded = db.CategoryPost.Add(categoryPost);
            if (isAdded != null)
            {
                await db.SaveChangesAsync();
                return true;
            }
            return false;
        }
        #endregion

        #region All Category Posts
        public IQueryable<CategoryPost> AllCategoryPosts()
        {
            var listCategoryPosts = db.CategoryPost.AsNoTracking();
            return listCategoryPosts;
        }
        #endregion

        #region Get Category Post By Id
        public CategoryPost GetCategoryPostById(int categoryId)
        {
            var category = db.CategoryPost.FirstOrDefault(v => v.CategoryPostId == categoryId);
            return category;
        }
        #endregion

        #region Search Category Post
        public IQueryable<CategoryPost> SearchCategoryPost(string query)
        {
            var categoryPosts = db.CategoryPost
                                  .Where(c => c.CategoryPostName.Contains(query) ||
                                         c.CategoryPostName == "Other")
                                  .OrderBy(c => c.CategoryPostName)
                                  .AsNoTracking();
            return categoryPosts;
        }
        #endregion
    }
}
