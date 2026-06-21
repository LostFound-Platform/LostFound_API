using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface IPostRepository
    {
        public Task<bool> CreatePost(Posts post);
        public Task<bool> UpdatePost(Posts postEntry);
        public IQueryable<Posts> GetFoundPosts();
        public IQueryable<object> GetLostPostsPerMonth();
        public IQueryable<object> GetFoundPostsPerMonth();
        public IQueryable<object> GetReceivedPostsPerMonth();
        public IQueryable<object> GetFoundPostsNotReceived();
        public IQueryable<Posts> SortByStatus(TypePost typePost, int userId);
        public Task<Posts> GetPostById(int postId);
        public Task<Posts> MarkReceived(int postId);
        public Task<bool> DeletePost(int postId);
        public IQueryable<Posts> SearchCodes(string query);
        public IQueryable<Posts> AllPosts();
        public IQueryable<Posts> AllPostsByUserId(int userId);
        public Task<List<Posts>> GetNewestPosts();
        public Task<List<Posts>> GetPick60LostPosts();
        public Task<List<Posts>> GetPick60ReceivedPosts();
        public Task<List<Posts>> RegularSearch(string? status, int? categoryId, string? nameItem);
        public IQueryable<Posts> AllLostPostCodes();
        public IEnumerable<SearchResult> SearchImageSimilarity(List<double> vector);
        public Task<bool> HandOverAdmin(int postId, int oldUserId);
    }
}
