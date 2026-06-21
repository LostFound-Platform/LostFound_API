using DataAccess;
using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class StudentRepository : IStudentRepository
    {
        #region Variables
        private readonly StudentDAO studentDAO;
        #endregion

        #region Constructor
        public StudentRepository(StudentDAO studentDAO)
        {
            this.studentDAO = studentDAO;
        }
        #endregion

        #region Create student (Sign up for student after adding user)
        public async Task<bool> SignUpStudent(Student student)
        {
            var isAdded = await studentDAO.SignUpStudent(student);
            return isAdded;
        }
        #endregion

        #region All students
        public IQueryable<Student> AllStudents()
        {
            return studentDAO.AllStudents();
        }
        #endregion

        #region Get Student By Id
        public async Task<Student> GetStudentById(int studentId)
        {
            return await studentDAO.GetStudentById(studentId);
        }
        #endregion

        #region Get Student By User Id
        public Student GetStudentByUserId(int userId)
        {
            return studentDAO.GetStudentByUserId(userId);
        }
        #endregion
    }
}
