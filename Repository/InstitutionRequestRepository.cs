using DataAccess;
using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class InstitutionRequestRepository : IInstitutionRequestRepository
    {
        #region Variables
        private readonly InstitutionRequestDAO institutionRequestDAO;
        #endregion

        #region Constructor
        public InstitutionRequestRepository(InstitutionRequestDAO institutionRequestDAO)
        {
            this.institutionRequestDAO = institutionRequestDAO;
        }
        #endregion

        #region GET All Institution Requests
        public IQueryable<InstitutionRequest> AllInstitutionRequests()
        {
            var listInstitution = institutionRequestDAO.AllInstitutionRequests();
            return listInstitution;
        }
        #endregion

        #region Get Institution Request By Name And Address
        public async Task<InstitutionRequest?> GetInstitutionRequestByNameAndAddress(string institutionName, string institutionAddress)
        {
            var request = await institutionRequestDAO.GetInstitutionRequestByNameAndAddress(institutionName, institutionAddress);
            return request;
        }
        #endregion

        #region Create Institution Request
        public async Task<bool> CreateInstitutionRequest(InstitutionRequest institutionRequest)
        {
            var requestExists = await institutionRequestDAO
                .GetInstitutionRequestByNameAndAddress(institutionRequest.InstitutionName, institutionRequest.InstitutionAddress);
            if (requestExists != null)
            {
                return false; // Request already exists
            }

            var isAdded = await institutionRequestDAO.CreateInstitutionRequest(institutionRequest);
            return isAdded;
        }
        #endregion

        #region Get Institution Request By ID
        public async Task<InstitutionRequest> GetInstitutionRequestByID(int requestId)
        {
            var request = await institutionRequestDAO.GetInstitutionRequestByID(requestId);
            return request;
        }
        #endregion

        #region Get Institution Request By Work Email
        public async Task<InstitutionRequest> GetInstitutionRequestByWorkEmail(string workEmail)
        {
            var institutionRequest = await institutionRequestDAO.GetInstitutionRequestByWorkEmail(workEmail);
            return institutionRequest;
        }
        #endregion

        #region Update Institution Request
        public async Task<bool> UpdateInstitutionRequest()
        {
            var isUpdated = await institutionRequestDAO.UpdateInstitutionRequest();
            return isUpdated;
        }
        #endregion
    }
}
