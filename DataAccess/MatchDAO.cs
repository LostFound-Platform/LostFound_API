using Microsoft.EntityFrameworkCore;
using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class MatchDAO
    {
        #region Variables
        private readonly FBLADbContext db;
        #endregion

        #region Constructor
        public MatchDAO(FBLADbContext db)
        {
            this.db = db;
        }
        #endregion

        #region Create Match
        public async Task<bool> CreateMatch(Match match)
        {
            match.MatchId = new Random().Next();
            var isAdded = db.Match.Add(match);
            if (isAdded != null)
            {
                await db.SaveChangesAsync();
                return true;
            }
            return false;
        }
        #endregion

        #region All matches
        public IQueryable<Match> AllMatches()
        {
            var listMatches = db.Match.AsNoTracking();
            return listMatches;
        }
        #endregion

        #region Get Match By Id
        public async Task<Match> GetMatchById(int matchId)
        {
            var match = await db.Match.FirstOrDefaultAsync(m => m.MatchId == matchId);
            return match;
        }
        #endregion

        #region Get Matches By User Id
        public IQueryable<object> GetMatchesByUserId(int userId)
        {
            var matches = db.Match.Include(m => m.LostPost)
                                      .Include(m => m.FoundPost)
                                      .ThenInclude(p => p.User)
                                      .Include(m => m.VerificationCode)
                                      .Where(m => m.LostPost.UserId == userId ||
                                             m.FoundPost.UserId == userId)
                                      .Select(m => new
                                      {
                                          MatchId = m.MatchId,
                                          Code = m.VerificationCode.Code,
                                          CreatedAt = m.CreatedAt,
                                          FirstNameFound = m.FoundPost.User.FirstName,
                                          LastNameFound = m.FoundPost.User.LastName,
                                          FirstNameLost = m.LostPost.User.FirstName,
                                          LastNameLost = m.LostPost.User.LastName,
                                          UserIdLost = m.LostPost.User.UserId,
                                          UserIdFound = m.FoundPost.User.UserId,
                                          TitlePost = m.LostPost.Title,
                                          PostId = m.LostPost.PostId,
                                          IsUsed = m.VerificationCode.IsUsed
                                      });
            return matches;
        }
        #endregion

        #region Get Match By Post Id
        public async Task<Match> GetMatchByPostId(int postId)
        {
            var match = await db.Match.FirstOrDefaultAsync(m => m.LostPostId == postId);
            return match;
        }
        #endregion

        #region Check Post Lost and Found Matched
        public async Task<Match> CheckPostLostFoundMatched(int postIdLost, int postIdFound)
        {
            var match = await db.Match.FirstOrDefaultAsync(m => m.LostPostId == postIdLost &&
                                                           m.FoundPostId == postIdFound);
            return match;
        }
        #endregion

        #region Update match
        public async Task<bool> UpdateMatch(Match match)
        {
            return false;
        }
        #endregion

        #region Delete match
        public async Task<bool> DeleteMatch(int matchId)
        {
            var match = await GetMatchById(matchId);

            if (match == null)
            {
                return false;
            }

            try
            {
                var requestDeleted = db.Match.Remove(match);
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
