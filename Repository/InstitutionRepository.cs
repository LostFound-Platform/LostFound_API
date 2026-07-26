using DataAccess;
using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class InstitutionRepository : IInstitutionRepository
    {
        #region Variables
        private readonly InstitutionDAO institutionDAO;
        #endregion

        #region Constructor
        public InstitutionRepository(InstitutionDAO institutionDAO)
        {
            this.institutionDAO = institutionDAO;
        }
        #endregion

        #region GET Institutions
        public IQueryable<Institution> AllInstitutions()
        {
            var listInstitution = institutionDAO.AllInstitutions();
            return listInstitution;
        }
        #endregion

        #region Get Institution By Name And Address
        public async Task<Institution?> GetInstitutionByNameAndAddress(string institutionName, string institutionAddress)
        {
            var user = await institutionDAO.GetInstitutionByNameAndAddress(institutionName, institutionAddress);
            return user;
        }
        #endregion

        #region Get user By ID
        public async Task<Institution> GetInstitutionByID(int? institutionId)
        {
            var user = await institutionDAO.GetInstitutionByID(institutionId);
            return user;
        }
        #endregion

        #region Create Institution
        public async Task<bool> CreateInstitution(Institution institution)
        {
            var institutionExists = await institutionDAO.GetInstitutionByNameAndAddress(institution.InstitutionName, institution.InstitutionAddress);
            if (institutionExists != null)
            {
                return false; // Institution already exists
            }

            var isAdded = await institutionDAO.CreateInstitution(institution);
            return isAdded;
        }
        #endregion

        //#region Update user
        //public async Task<bool> UpdateUser()
        //{
        //    var isUpdated = await institutionDAO.UpdateUser();
        //    return isUpdated;
        //}
        //#endregion
    }
}
