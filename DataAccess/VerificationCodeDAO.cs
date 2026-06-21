using Microsoft.EntityFrameworkCore;
using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class VerificationCodeDAO
    {
        #region Variables
        private readonly FBLADbContext db;
        #endregion

        #region Constructor
        public VerificationCodeDAO(FBLADbContext db)
        {
            this.db = db;
        }
        #endregion

        #region Create Verification Code
        public async Task<bool> CreateVerificationCode(VerificationCode verificationCode)
        {
            verificationCode.VerificationCodeId = new Random().Next();
            var isAdded = db.VerificationCode.Add(verificationCode);
            if (isAdded != null)
            {
                await db.SaveChangesAsync();
                return true;
            }
            return false;
        }
        #endregion

        #region All Verification Codes
        public IQueryable<VerificationCode> AllVerificationCodes()
        {
            var listVerificationCodes = db.VerificationCode.AsNoTracking();
            return listVerificationCodes;
        }
        #endregion

        #region Get Verification Code By Id
        public async Task<VerificationCode> GetVerificationCodeById(int verificationCodeId)
        {
            var verificationCode = await db.VerificationCode.FirstOrDefaultAsync(v => v.VerificationCodeId == verificationCodeId);
            return verificationCode;
        }
        #endregion

        #region Get Verification Code By Match Id
        public async Task<VerificationCode> GetVerificationCodeByMatchId(int matchId)
        {
            var verificationCode = await db.VerificationCode.FirstOrDefaultAsync(v => v.MatchId == matchId);
            return verificationCode;
        }
        #endregion

        #region Delete Verification Code
        public async Task<bool> DeleteVerificationCode(int verificationCodeId)
        {
            var code = await GetVerificationCodeById(verificationCodeId);

            if (code == null)
            {
                return false;
            }

            try
            {
                var codeDeleted = db.VerificationCode.Remove(code);
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
