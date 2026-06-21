using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface ITransferRequestRepository
    {
        public Task<bool> CreateRequest(TransferRequests request);
        public IQueryable<TransferRequests> AllRequests();
        public IQueryable<TransferRequests> SearchRequest(string query);
        public Task<TransferRequests> CheckStatusRequestPost(int postId);
        public Task<TransferRequests> MarkReceived(int requestId);
        public Task<TransferRequests> CancelRequest(int requestId);
    }
}
