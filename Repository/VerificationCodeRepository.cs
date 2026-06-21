using DataAccess;
using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class VerificationCodeRepository : IVerificationCodeRepository
    {
        #region Variables
        private readonly VerificationCodeDAO verificationCodeDAO;
        #endregion

        #region Constructor
        public VerificationCodeRepository(VerificationCodeDAO verificationCodeDAO)
        {
            this.verificationCodeDAO = verificationCodeDAO;
        }
        #endregion

        #region Create Verification Code
        public async Task<bool> CreateVerificationCode(VerificationCode verificationCode)
        {
            var isAdded = await verificationCodeDAO.CreateVerificationCode(verificationCode);
            return isAdded;
        }
        #endregion

        #region Get Verification Code By Match Id
        public async Task<VerificationCode> GetVerificationCodeByMatchId(int matchId)
        {
            var verificationCode = await verificationCodeDAO.GetVerificationCodeByMatchId(matchId);
            return verificationCode;
        }
        #endregion
    }
}
