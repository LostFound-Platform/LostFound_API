using Microsoft.EntityFrameworkCore;
using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class InstitutionDAO
    {
        #region Variables
        private readonly BackToMeDbContext db;
        #endregion

        #region Constructor
        public InstitutionDAO(BackToMeDbContext db)
        {
            this.db = db;
        }
        #endregion

        #region Create Institution
        public async Task<bool> CreateInstitution(Institution institution)
        {
            institution.InstitutionId = new Random().Next();
            var isAdded = db.Institutions.Add(institution);
            if (isAdded != null)
            {
                await db.SaveChangesAsync();
                return true;
            }
            return false;
        }
        #endregion

        #region All Institutions
        public IQueryable<Institution> AllInstitutions()
        {
            var listUsers = db.Institutions.AsNoTracking()
                .OrderByDescending(u => u.InstitutionName);

            return listUsers;
        }
        #endregion

        #region Get Institution By Name
        public async Task<Institution?> GetInstitutionByNameAndAddress(string institutionName, string institutionAddress)
        {
            if (string.IsNullOrWhiteSpace(institutionName))
            {
                return null;
            }

            institutionName = institutionName.Trim();
            institutionAddress = institutionAddress.Trim();

            try
            {
                var institution = await db.Institutions.AsNoTracking()
                                                   .FirstOrDefaultAsync(i => i.InstitutionName.Contains(institutionName) &&
                                                   i.InstitutionAddress.Contains(institutionAddress));
                return institution;
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }
        #endregion

        #region Get Institution By ID
        public async Task<Institution> GetInstitutionByID(int? institutionId)
        {
            var institution = await db.Institutions.FirstOrDefaultAsync(u => u.InstitutionId == institutionId);
            return institution;
        }
        #endregion

        #region Update Institution
        public async Task<bool> UpdateInstitution()
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

        #region Delete Institution
        public async Task<bool> DeleteInstitution(int? institutionId)
        {
            return false;
        }
        #endregion
    }
}
