using Microsoft.EntityFrameworkCore;
using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class UsersDAO
    {
        #region Variables
        private readonly FBLADbContext db;
        #endregion

        #region Constructor
        public UsersDAO(FBLADbContext db)
        {
            this.db = db;
        }
        #endregion

        #region Create user (Sign up)
        public async Task<bool> SignUp(Users user)
        {
            user.UserId = new Random().Next();
            var isAdded = db.Users.Add(user);
            if (isAdded != null)
            {
                await db.SaveChangesAsync();
                return true;
            }
            return false;
        }
        #endregion

        #region All users
        public IQueryable<Users> AllUsers()
        {
            var listUsers = (from s in db.Student
                             join u in db.Users
                             on s.UserId equals u.UserId
                             where u.Role == Role.Student
                             select new Users
                             {
                                 UserId = u.UserId,
                                 FirstName = u.FirstName,
                                 LastName = u.LastName,
                                 Email = u.Email,
                                 Role = u.Role,
                                 Avatar = u.Avatar,
                                 IsActive = u.IsActive,
                                 IsAgreedToTerms = u.IsAgreedToTerms,
                                 IsVerifiedEmail = u.IsVerifiedEmail,
                                 CreatedAt = u.CreatedAt,
                                 StudentId = s.StudentId
                             })
                             .AsNoTracking()
                             .OrderByDescending(u => u.FirstName);

            return listUsers;
        }
        #endregion

        #region Get Admin
        public Users GetAdmin()
        {
            var user = db.Users.FirstOrDefault(u => u.Role == Role.Admin);
            return user;
        }
        #endregion

        #region Get User By Student Id
        public async Task<Users> GetUserByStudentId(int studentId)
        {
            var user = await db.Users
                         .Include(u => u.Student)
                         .FirstOrDefaultAsync(s => s.Student.StudentId == studentId);
            return user;
        }
        #endregion

        #region Get User By Email
        public async Task<Users> GetUserByEmail(string email)
        {
            var user = await db.Users.Include(s => s.Student)
                                     .FirstOrDefaultAsync(s => s.Email.Equals(email));
            return user;
        }
        #endregion

        #region Search User By Email
        public IQueryable<Users> SearchUserByEmail(string query)
        {
            var users = (from s in db.Student
                         join u in db.Users
                         on s.UserId equals u.UserId
                         where u.Role == Role.Student && u.Email.Contains(query)
                         select new Users
                         {
                             UserId = u.UserId,
                             FirstName = u.FirstName,
                             LastName = u.LastName,
                             Email = u.Email,
                             Role = u.Role,
                             Avatar = u.Avatar,
                             IsActive = u.IsActive,
                             IsAgreedToTerms = u.IsAgreedToTerms,
                             IsVerifiedEmail = u.IsVerifiedEmail,
                             CreatedAt = u.CreatedAt,
                             StudentId = s.StudentId
                         })
                         .AsNoTracking()
                         .OrderByDescending(u => u.FirstName);
            return users;
        }
        #endregion

        #region Get User By ID
        public async Task<Users> GetUserByID(int userId)
        {
            var user = await db.Users.Include(s => s.Student)
                                     .FirstOrDefaultAsync(u => u.UserId == userId);
            return user;
        }
        #endregion

        #region Update user
        public async Task<bool> UpdateUser()
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

        #region Suspend user
        public async Task<bool> SuspendUser()
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

        #region Unsuspend user
        public async Task<bool> UnsuspendUser()
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

        #region Delete user
        public async Task<bool> DeleteUser(int userId)
        {
            return false;
        }
        #endregion
    }
}
