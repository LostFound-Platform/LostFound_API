using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface IVerificationCodeRepository
    {
        public Task<bool> CreateVerificationCode(VerificationCode verificationCode);
        public Task<VerificationCode> GetVerificationCodeByMatchId(int matchId);
    }
}
