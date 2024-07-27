using ConnectingDatabase.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ConnectingDatabase.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Student> Students { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Documents> Documents { get; set; }
        public DbSet<DegreeMaster> tbl_degree_master { get; set; }
        public DbSet<SemesterEnrollment> tbl_semester_enrollment { get; set; }
        public DbSet<SubjectMaster> tbl_subject_master { get; set; }
        public DbSet<YearMaster> tbl_year_master { get; set; }
        public DbSet<StudentDegreeEnrollment> tbl_student_degree_enrollment { get; set; }
    }
}