using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface IInstitutionRepository
    {
        public Task<bool> CreateInstitution(Institution institution);
        public IQueryable<Institution> AllInstitutions();
        public Task<Institution> GetInstitutionByID(int institutionId);
        public Task<Institution?> GetInstitutionByNameAndAddress(string institutionName, string institutionAddress);
    }
}
