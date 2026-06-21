using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface IMatchRepository
    {
        public IQueryable<Match> AllMatches();
        public Task<bool> CreateMatch(Match match);
        public IQueryable<object> GetMatchesByUserId(int userId);
        public Task<Match> GetMatchByPostId(int postId);
        public Task<Match> CheckPostLostFoundMatched(int postIdLost, int postIdFound);
    }
}
