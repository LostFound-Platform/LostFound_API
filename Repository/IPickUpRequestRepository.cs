using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface IPickUpRequestRepository
    {
        public Task<bool> CreateRequest(PickUpRequest request);
        public Task<PickUpRequest> CheckContainsPost(int postId);
        public IQueryable<PickUpRequest> SearchRequest(string query);
        public IQueryable<PickUpRequest> AllRequests();
        public Task<PickUpRequest> AcceptTime(int requestId);
        public Task<PickUpRequest> AcceptTimeRescheduled(int requestId);
        public Task<PickUpRequest> ChangeTime(int requestId, DateTime date);
        public Task<bool> DeletePickUpRequest(int postId);
    }
}
