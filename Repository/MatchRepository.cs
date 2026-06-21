using DataAccess;
using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class MatchRepository : IMatchRepository
    {
        #region Variables
        private readonly MatchDAO matchDAO;
        #endregion

        #region Constructor
        public MatchRepository(MatchDAO matchDAO)
        {
            this.matchDAO = matchDAO;
        }
        #endregion

        #region All Matches
        public IQueryable<Match> AllMatches()
        {
            return matchDAO.AllMatches();
        }
        #endregion

        #region Create Post
        public async Task<bool> CreateMatch(Match match)
        {
            var isAdded = await matchDAO.CreateMatch(match);
            return isAdded;
        }
        #endregion

        #region Get Match By User Id
        public IQueryable<object> GetMatchesByUserId(int userId)
        {
            var isAdded = matchDAO.GetMatchesByUserId(userId);
            return isAdded;
        }
        #endregion

        #region Get Match By Post Id
        public async Task<Match> GetMatchByPostId(int postId)
        {
            var isAdded = await matchDAO.GetMatchByPostId(postId);
            return isAdded;
        }
        #endregion

        #region Check Post Lost and Found Matched
        public async Task<Match> CheckPostLostFoundMatched(int postIdLost, int postIdFound)
        {
            var matched = await matchDAO.CheckPostLostFoundMatched(postIdLost, postIdFound);
            return matched;
        }
        #endregion
    }
}
