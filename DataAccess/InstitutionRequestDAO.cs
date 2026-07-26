using Microsoft.EntityFrameworkCore;
using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class InstitutionRequestDAO
    {
        #region Variables
        private readonly BackToMeDbContext db;
        #endregion

        #region Constructor
        public InstitutionRequestDAO(BackToMeDbContext db)
        {
            this.db = db;
        }
        #endregion

        #region Create Institution Request
        public async Task<bool> CreateInstitutionRequest(InstitutionRequest institutionRequest)
        {
            institutionRequest.InstitutionRequestId = new Random().Next();
            var isAdded = db.InstitutionRequests.Add(institutionRequest);
            if (isAdded != null)
            {
                await db.SaveChangesAsync();
                return true;
            }
            return false;
        }
        #endregion

        #region All Institution Requests
        public IQueryable<InstitutionRequest> AllInstitutionRequests()
        {
            var listRequests = db.InstitutionRequests.Where(r => r.IsVerifiedEmail).AsNoTracking()
                .OrderByDescending(u => u.InstitutionName);

            return listRequests;
        }
        #endregion

        #region Get Institution Request By ID
        public async Task<InstitutionRequest> GetInstitutionRequestByID(int requestId)
        {
            var institutionRequest = await db.InstitutionRequests.FirstOrDefaultAsync(r => r.InstitutionRequestId == requestId);
            return institutionRequest;
        }
        #endregion

        #region Get Institution Request By Work Email
        public async Task<InstitutionRequest> GetInstitutionRequestByWorkEmail(string workEmail)
        {
            var institutionRequest = await db.InstitutionRequests.FirstOrDefaultAsync(r => r.WorkEmail == workEmail);
            return institutionRequest;
        }
        #endregion

        #region Get Institution Request By Name and Address
        public async Task<InstitutionRequest?> GetInstitutionRequestByNameAndAddress(string institutionName, string institutionAddress)
        {
            if (string.IsNullOrWhiteSpace(institutionName) || string.IsNullOrWhiteSpace(institutionAddress))
            {
                return null;
            }

            institutionName = institutionName.Trim();
            institutionAddress = institutionAddress.Trim();

            try
            {
                var canConnect = await db.Database.CanConnectAsync();
                var request = await db.InstitutionRequests.AsNoTracking()
                                                   .FirstOrDefaultAsync(i => i.InstitutionName.Contains(institutionName) &&
                                                   i.InstitutionAddress.Contains(institutionAddress));
                return request;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Update Institution Request
        public async Task<bool> UpdateInstitutionRequest()
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

        #region Delete Institution Request
        public async Task<bool> DeleteInstitutionRequest(int? requestId)
        {
            return false;
        }
        #endregion
    }
}
