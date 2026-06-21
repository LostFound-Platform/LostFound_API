using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface IUsersRepository
    {
        public IQueryable<Users> AllUsers();
        public Task<bool> SignUp(Users user);
        public IQueryable<Users> SearchUserByEmail(string query);
        public Task<bool> SuspendUser();
        public Task<bool> UnsuspendUser();
        public Task<Users> GetUserByID(int userId);
        public Task<Users> SignIn(int studentId, string password, string? email);
        public Task<Users> GetUserByStudentId(int studentId);
        public Task<Users> GetUserByEmail(string email);
        public Task<bool> UpdateUser();
        public Users GetAdmin();
    }
}
