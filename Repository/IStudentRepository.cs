using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface IStudentRepository
    {
        public Task<bool> SignUpStudent(Student student);
        public IQueryable<Student> AllStudents();
        public Task<Student> GetStudentById(int studentId);
        public Student GetStudentByUserId(int userId);
    }
}
