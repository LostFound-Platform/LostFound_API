using Microsoft.EntityFrameworkCore;
using ObjectBusiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class StudentDAO
    {
        #region Variables
        private readonly FBLADbContext db;
        #endregion

        #region Constructor
        public StudentDAO(FBLADbContext db)
        {
            this.db = db;
        }
        #endregion

        #region Create student (Sign up for student after adding user)
        public async Task<bool> SignUpStudent(Student student)
        {
            var isAdded = db.Student.Add(student);
            if (isAdded != null)
            {
                await db.SaveChangesAsync();
                return true;
            }
            return false;
        }
        #endregion

        #region All students
        public IQueryable<Student> AllStudents()
        {
            var listStudents = db.Student.AsNoTracking();
            return listStudents;
        }
        #endregion

        #region Get Student By Id
        public async Task<Student> GetStudentById(int studentId)
        {
            var student = await db.Student.FirstOrDefaultAsync(s => s.StudentId == studentId);
            return student;
        }
        #endregion

        #region Get Student By User Id
        public Student GetStudentByUserId(int userId)
        {
            var student = db.Student.FirstOrDefault(s => s.UserId == userId);
            return student;
        }
        #endregion

        #region Update student
        public async Task<bool> UpdateStudent(Student student)
        {
            return false;
        }
        #endregion

        #region Delete student
        public async Task<bool> DeleteStudent(int studentId)
        {
            return false;
        }
        #endregion
    }
}
