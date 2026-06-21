using Azure.Core;
using Microsoft.EntityFrameworkCore;
using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class PickUpRequestDAO
    {
        #region Variables
        private readonly FBLADbContext db;
        #endregion

        #region Constructor
        public PickUpRequestDAO(FBLADbContext db)
        {
            this.db = db;
        }
        #endregion

        #region Create Request
        public async Task<bool> CreateRequest(PickUpRequest request)
        {
            request.RequestId = new Random().Next();
            request.Status = StatusRequest.Pending;
            try
            {
                var isAdded = db.PickUpRequest.Add(request);
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
        public async Task<PickUpRequest> AcceptTime(int requestId)
        {
            var request = await db.PickUpRequest.FirstOrDefaultAsync(r => r.RequestId == requestId);
            if (request == null) return null;

            request.UpdatedDate = DateTime.Now;
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

        #region Change Time
        public async Task<PickUpRequest> ChangeTime(int requestId, DateTime date)
        {
            var request = await db.PickUpRequest.FirstOrDefaultAsync(r => r.RequestId == requestId);
            if (request == null) return null;

            request.PickUpDate = date;
            request.Status = StatusRequest.Reschedule;
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
        public IQueryable<PickUpRequest> AllRequests()
        {
            var listRequests = (from t in db.PickUpRequest
                                join p in db.Posts
                                on t.PostId equals p.PostId
                                select new PickUpRequest
                                {
                                    PostId = t.PostId,
                                    CreatedDate = t.CreatedDate,
                                    Description = t.Description,
                                    PickUpDate = t.PickUpDate,
                                    RequestId = t.RequestId,
                                    Status = t.Status,
                                }).OrderByDescending(t => t.CreatedDate);
            return listRequests;
        }
        #endregion

        #region Search Request
        public IQueryable<PickUpRequest> SearchRequest(string query)
        {
            var listRequests = (from t in db.PickUpRequest
                                join p in db.Posts
                                on t.PostId equals p.PostId
                                where p.TypePost == TypePost.Lost && t.Description.Contains(query)
                                select new PickUpRequest
                                {
                                    PostId = t.PostId,
                                    CreatedDate = t.CreatedDate,
                                    Description = t.Description,
                                    PickUpDate = t.PickUpDate,
                                    RequestId = t.RequestId,
                                    Status = t.Status,
                                }).OrderByDescending(t => t.CreatedDate);
            return listRequests;
        }
        #endregion

        #region Check Contains Post
        public async Task<PickUpRequest> CheckContainsPost(int postId)
        {
            var result = await db.PickUpRequest.FirstOrDefaultAsync(t => t.PostId == postId);
            return result;
        }
        #endregion

        #region Get Request By ID
        public async Task<PickUpRequest> GetRequestByID(int requestId)
        {
            var result = await db.PickUpRequest.FirstOrDefaultAsync(t => t.RequestId == requestId);
            return result;
        }
        #endregion

        #region Delete Pick Up Request
        public async Task<bool> DeletePickUpRequest(int postId)
        {
            var request = await CheckContainsPost(postId);

            if (request == null)
            {
                return false;
            }

            try
            {
                var requestDeleted = db.PickUpRequest.Remove(request);
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
