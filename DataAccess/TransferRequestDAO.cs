using Microsoft.EntityFrameworkCore;
using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class TransferRequestDAO
    {
        #region Variables
        private readonly FBLADbContext db;
        #endregion

        #region Constructor
        public TransferRequestDAO(FBLADbContext db)
        {
            this.db = db;
        }
        #endregion

        #region Create Request
        public async Task<bool> CreateRequest(TransferRequests request)
        {
            request.RequestId = new Random().Next();
            try
            {
                var isAdded = db.TransferRequests.Add(request);
                if (isAdded != null)
                {
                    await db.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        #region Mark Received
        public async Task<TransferRequests> MarkReceived(int requestId)
        {
            var request = await db.TransferRequests.FirstOrDefaultAsync(r => r.RequestId == requestId);
            if (request == null) return null;

            request.ConfirmedAt = DateTime.Now;
            request.Status = StatusRequest.Confirmed;
            try
            {
                await db.SaveChangesAsync();
                return request;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region Cancel Request
        public async Task<TransferRequests> CancelRequest(int requestId)
        {
            var request = await db.TransferRequests.FirstOrDefaultAsync(r => r.RequestId == requestId);
            if (request == null) return null;

            request.Status = StatusRequest.Cancelled;
            try
            {
                await db.SaveChangesAsync();
                return request;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region All Requests
        public IQueryable<TransferRequests> AllRequests()
        {
            var listRequests = (from t in db.TransferRequests
                                join u in db.Users
                                on t.UserId equals u.UserId
                                join p in db.Posts
                                on t.PostId equals p.PostId
                                where p.TypePost == TypePost.Found
                                select new TransferRequests
                                {
                                    UserId = t.UserId,
                                    PostId = t.PostId,
                                    Status = t.Status,
                                    CreatedAt = t.CreatedAt,
                                    ConfirmedAt = t.ConfirmedAt,
                                    RequestId = t.RequestId,
                                    FirstName = u.FirstName,
                                    LastName = u.LastName,
                                    NameItem = p.Title,
                                    Role = u.Role
                                }).OrderByDescending(t => t.CreatedAt);
            return listRequests;
        }
        #endregion

        #region Search Request
        public IQueryable<TransferRequests> SearchRequest(string query)
        {
            var listRequests = (from t in db.TransferRequests
                                join u in db.Users
                                on t.UserId equals u.UserId
                                join p in db.Posts
                                on t.PostId equals p.PostId
                                where p.TypePost == TypePost.Found && (u.FirstName + u.LastName).Contains(query)
                                select new TransferRequests
                                {
                                    UserId = t.UserId,
                                    PostId = t.PostId,
                                    Status = t.Status,
                                    CreatedAt = t.CreatedAt,
                                    ConfirmedAt = t.ConfirmedAt,
                                    RequestId = t.RequestId,
                                    FirstName = u.FirstName,
                                    LastName = u.LastName,
                                    NameItem = p.Title,
                                    Role = u.Role
                                }).OrderByDescending(t => t.CreatedAt);
            return listRequests;
        }
        #endregion

        #region All Requests
        public async Task<List<TransferRequests>> AllRequestsByPostId(int postId)
        {
            var listRequests = await db.TransferRequests.Where(t => t.PostId == postId).ToListAsync();
            return listRequests;
        }
        #endregion

        #region Delete All Transfer requests Of Post
        public async Task<bool> DeleteTransfers(List<TransferRequests> transferRequests)
        {
            if (transferRequests == null || !transferRequests.Any())
            {
                return false;
            }

            try
            {
                db.TransferRequests.RemoveRange(transferRequests);
                await db.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        #region Check Status of Post
        public async Task<TransferRequests> CheckStatusRequestPost(int postId)
        {
            var statusRequest = await db.TransferRequests.OrderByDescending(p => p.CreatedAt).FirstOrDefaultAsync(t => t.PostId == postId);

            return statusRequest;
        }
        #endregion
    }
}
