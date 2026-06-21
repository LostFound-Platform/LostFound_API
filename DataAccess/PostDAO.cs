using Microsoft.EntityFrameworkCore;
using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DataAccess
{
    public class PostDAO
    {
        #region Variables
        private readonly FBLADbContext db;
        private readonly UsersDAO usersDAO;
        #endregion

        #region Constructor
        public PostDAO(FBLADbContext db, UsersDAO usersDAO)
        {
            this.db = db;
            this.usersDAO = usersDAO;
        }
        #endregion

        #region Create post
        public async Task<bool> CreatePost(Posts post)
        {
            post.PostId = new Random().Next();
            var isAdded = db.Posts.Add(post);
            if (isAdded != null)
            {
                try
                {
                    await db.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return false;
        }
        #endregion

        #region All posts
        public IQueryable<Posts> AllPosts()
        {
            var listPosts = db.Posts.Include(p => p.User)
                                    .AsNoTracking()
                                    .OrderByDescending(p => p.CreatedAt);
            return listPosts;
        }
        #endregion

        #region All posts by user ID
        public IQueryable<Posts> AllPostsByUserId(int userId)
        {
            var listPosts = db.Posts
                              .Include(u => u.User)
                              .Where(p => p.UserId == userId)
                              .OrderByDescending(p => p.CreatedAt)
                              .AsNoTracking();
            return listPosts;
        }
        #endregion

        #region Sort by status
        public IQueryable<Posts> SortByStatus(TypePost typePost, int userId)
        {
            var listPosts = db.Posts
                              .Where(p => p.TypePost == typePost && p.User.UserId == userId)
                              .OrderByDescending(p => p.CreatedAt)
                              .AsNoTracking();
            return listPosts;
        }
        #endregion

        #region All lost post codes
        public IQueryable<Posts> AllLostPostCodes()
        {
            var listPosts = db.Posts
                              .Include(p => p.User)
                              .Where(p => p.TypePost == TypePost.Lost)
                              .OrderByDescending(p => p.CreatedAt)
                              .AsNoTracking();
            return listPosts;
        }
        #endregion

        #region Get Post By Id
        public async Task<Posts> GetPostById(int postId)
        {
            var post = await db.Posts
                         .Include(u => u.User)
                         .Include(c => c.CategoryPost)
                         .FirstOrDefaultAsync(s => s.PostId == postId);
            return post;
        }
        #endregion

        #region Get Found Posts
        public IQueryable<Posts> GetFoundPosts()
        {
            var posts = db.Posts.Include(u => u.User)
                               .Where(s => s.TypePost == TypePost.Found)
                               .AsNoTracking();
            return posts;
        }
        #endregion

        #region Get Lost Posts
        public IQueryable<Posts> GetLostPosts()
        {
            var posts = db.Posts.Include(u => u.User)
                               .Where(s => s.TypePost == TypePost.Lost)
                               .AsNoTracking();
            return posts;
        }
        #endregion

        #region Get Lost Posts Per Month
        public IQueryable<object> GetLostPostsPerMonth()
        {
            try
            {
                var post = db.Posts
                               .Where(p => p.TypePost == TypePost.Lost &&
                                      p.CreatedAt.Year == DateTime.Now.Year)
                               .GroupBy(p => p.CreatedAt.Month)
                               .OrderBy(p => p.Key)
                               .Select(p => new
                               {
                                   Month = p.Key,
                                   Count = p.Count()
                               })
                               .AsNoTracking();
                return post;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region Get Found Posts Per Month
        public IQueryable<object> GetFoundPostsPerMonth()
        {
            try
            {
                var post = db.Posts
                               .Where(p => p.TypePost == TypePost.Found &&
                                      p.CreatedAt.Year == DateTime.Now.Year)
                               .GroupBy(p => p.CreatedAt.Month)
                               .OrderBy(p => p.Key)
                               .Select(p => new
                               {
                                   Month = p.Key,
                                   Count = p.Count()
                               })
                               .AsNoTracking();
                return post;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region Get Received Posts Per Month
        public IQueryable<object> GetReceivedPostsPerMonth()
        {
            try
            {
                var post = db.Posts
                               .Where(p => p.TypePost == TypePost.Lost &&
                                      p.IsReceived == true &&
                                      p.CreatedAt.Year == DateTime.Now.Year)
                               .GroupBy(p => p.CreatedAt.Month)
                               .OrderBy(p => p.Key)
                               .Select(p => new
                               {
                                   Month = p.Key,
                                   Count = p.Count()
                               })
                               .AsNoTracking();
                return post;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region Get Found Posts Not Received
        public IQueryable<object> GetFoundPostsNotReceived()
        {
            try
            {
                var post = db.Posts
                               .Where(p => p.TypePost == TypePost.Found && p.IsReceived != true)
                               .GroupBy(p => p.CreatedAt.Month)
                               .OrderBy(p => p.Key)
                               .Select(p => new
                               {
                                   Month = p.Key,
                                   Count = p.Count()
                               })
                               .AsNoTracking();
                return post;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region Search Codes
        public IQueryable<Posts> SearchCodes(string query)
        {
            var posts = db.Posts.Include(u => u.User)
                               .Where(s => s.Code.Contains(query) && s.TypePost == TypePost.Lost)
                               .AsNoTracking();
            return posts;
        }
        #endregion

        #region Regular Search
        public IQueryable<Posts> RegularSearch(string? status, int? categoryId, string? nameItem)
        {
            var posts = db.Posts.Include(u => u.User)
                               .AsNoTracking();

            // Filter status
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<TypePost>(status, true, out var typePost))
                {
                    posts = posts.Where(p => p.TypePost == typePost);
                }
            }

            // Filter category
            if (categoryId.HasValue && categoryId > 0)
            {
                posts = posts.Where(p => p.CategoryPostId == categoryId);
            }

            // Filter name item
            if (!string.IsNullOrWhiteSpace(nameItem))
            {
                posts = posts.Where(p => p.Title.Contains(nameItem));
            }

            return posts;
        }
        #endregion

        #region Get Newest Posts
        public async Task<List<Posts>> GetNewestPosts()
        {
            var post = await db.Posts
                               .AsNoTracking()
                               .Include(u => u.User)
                               .OrderByDescending(p => p.CreatedAt)
                               .Take(8)
                               .ToListAsync();
            return post;
        }
        #endregion

        #region Get Pick 60 Lost Posts
        public IQueryable<Posts> GetPick60LostPosts()
        {
            var post = db.Posts.Where(p => p.TypePost == TypePost.Lost).AsNoTracking().Take(60);
            return post;
        }
        #endregion

        #region Get Pick 60 Received Posts
        public IQueryable<Posts> GetPick60ReceivedPosts()
        {
            var post = db.Posts.Where(p => p.TypePost == TypePost.Found &&
                                      p.IsReceived == true)
                               .AsNoTracking()
                               .Take(60);
            return post;
        }
        #endregion

        // Functions are not related to DB
        #region Search Image similarity
        public IEnumerable<SearchResult> SearchImageSimilarity(List<double> vector)
        {
            var posts = GetFoundPosts().AsEnumerable().Select(p =>
            {
                var dbVector = p.Vector != null
                    ? JsonSerializer.Deserialize<List<double>>(p.Vector)
                    : null;

                if (dbVector == null) return null; // Skip posts without vectors

                return new SearchResult
                {
                    Post = p,
                    Score = CosineSimilarity(dbVector, vector)
                };
            })
            .Where(p => p != null && p.Score >= 0.8)
            .OrderByDescending(p => p!.Score);

            return posts;
        }
        #endregion

        #region Search Image similarity for lost
        public IEnumerable<SearchResult> SearchImageSimilarityForLost(List<double> vector)
        {
            var posts = GetLostPosts().AsEnumerable().Select(p =>
            {
                var dbVector = p.Vector != null
                    ? JsonSerializer.Deserialize<List<double>>(p.Vector)
                    : null;

                if (dbVector == null) return null; // Skip posts without vectors

                return new SearchResult
                {
                    Post = p,
                    Score = CosineSimilarity(dbVector, vector)
                };
            })
            .Where(p => p != null && p.Score >= 0.8)
            .OrderByDescending(p => p!.Score);

            return posts;
        }
        #endregion

        #region Hand Over to Admin
        public async Task<bool> HandOverAdmin(int postId, int oldUserId)
        {
            var post = await GetPostById(postId);
            if (post == null) return false;

            post.OldUserId = oldUserId;
            post.UserId = usersDAO.GetAdmin().UserId;
            post.UpdatedAt = DateTime.Now;
            try
            {
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        #region Mark Received
        public async Task<Posts> MarkReceived(int postId)
        {
            var post = await db.Posts.Include(u => u.User).FirstOrDefaultAsync(p => p.PostId == postId);
            if (post == null) return null;

            post.UpdatedAt = DateTime.Now;
            post.IsReceived = true;
            try
            {
                await db.SaveChangesAsync();
                return post;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region Delete post
        public async Task<bool> DeletePost(int postId)
        {
            var post = await GetPostById(postId);

            if (post == null)
            {
                return false;
            }

            try
            {
                var requestDeleted = db.Posts.Remove(post);
                await db.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        // Functions are not related to DB
        #region Cosine Similarity
        public double CosineSimilarity(IReadOnlyList<double> vectorA, IReadOnlyList<double> vectorB)
        {
            double dotProduct = 0;
            double magnitudeA = 0;
            double magnitudeB = 0;
            for (int i = 0; i < vectorA.Count; i++)
            {
                double a = Convert.ToDouble(vectorA[i]);
                double b = Convert.ToDouble(vectorB[i]);
                dotProduct += a * b;
                magnitudeA += a * a;
                magnitudeB += b * b;
            }

            if (magnitudeA == 0 || magnitudeB == 0)
            {
                return 0; // Avoid division by zero
            }

            return dotProduct / (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
        }
        #endregion

        #region Update post
        public async Task<bool> UpdatePost()
        {
            try
            {
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion
    }
}
