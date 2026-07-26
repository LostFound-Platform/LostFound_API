using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface IInstitutionRequestRepository
    {
        public Task<bool> CreateInstitutionRequest(InstitutionRequest institutionRequest);
        public IQueryable<InstitutionRequest> AllInstitutionRequests();
        public Task<bool> UpdateInstitutionRequest();
        public Task<InstitutionRequest> GetInstitutionRequestByID(int requestId);
        public Task<InstitutionRequest> GetInstitutionRequestByWorkEmail(string workEmail);
        public Task<InstitutionRequest?> GetInstitutionRequestByNameAndAddress(string institutionName, string institutionAddress);
    }
}
