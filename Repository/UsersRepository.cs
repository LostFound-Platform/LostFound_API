using DataAccess;
using Microsoft.AspNetCore.Http;
using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class UsersRepository : IUsersRepository
    {
        #region Variables
        private readonly UsersDAO usersDAO;
        private readonly StudentDAO studentDAO;
        #endregion

        #region Constructor
        public UsersRepository(UsersDAO usersDAO, StudentDAO studentDAO)
        {
            this.usersDAO = usersDAO;
            this.studentDAO = studentDAO;
        }
        #endregion

        #region GET All Users
        public IQueryable<Users> AllUsers()
        {
            var listUser = usersDAO.AllUsers();
            return listUser;
        }
        #endregion

        #region Search Users By Email
        public IQueryable<Users> SearchUserByEmail(string query)
        {
            var listUser = usersDAO.SearchUserByEmail(query);
            return listUser;
        }
        #endregion

        #region Get user By Email
        public async Task<Users> GetUserByEmail(string email)
        {
            var user = await usersDAO.GetUserByEmail(email);
            return user;
        }
        #endregion

        #region Get Admin
        public Users GetAdmin()
        {
            var user = usersDAO.GetSystemAdmin();
            return user;
        }
        #endregion

        #region Get user By ID
        public async Task<Users> GetUserByID(int userId)
        {
            var user = await usersDAO.GetUserByID(userId);
            return user;
        }
        #endregion

        #region Sign Up
        public async Task<bool> SignUp(Users user)
        {
            // Hash password
            var hashPassword = BCrypt.Net.BCrypt.EnhancedHashPassword(user.Password);

            user.Password = hashPassword;
            user.Role = Role.Student;

            var isAdded = await usersDAO.SignUp(user);
            return isAdded;
        }
        #endregion

        #region Sign In
        public async Task<Users> SignIn(string password, string email)
        {
            var user = await GetUserByEmail(email);

            if (user != null && BCrypt.Net.BCrypt.EnhancedVerify(password, user.Password))
            {
                return user;
            }

            return null;
        }
        #endregion

        #region Update user
        public async Task<bool> UpdateUser()
        {
            var isUpdated = await usersDAO.UpdateUser();
            return isUpdated;
        }
        #endregion

        #region Suspend user
        public async Task<bool> SuspendUser()
        {
            var isSuspended = await usersDAO.SuspendUser();
            return isSuspended;
        }
        #endregion

        #region Unsuspend user
        public async Task<bool> UnsuspendUser()
        {
            var isSuspended = await usersDAO.UnsuspendUser();
            return isSuspended;
        }
        #endregion
    }
}
