namespace P01_StudentSystem.Data
{
    using Microsoft.EntityFrameworkCore;
    using P01_StudentSystem.Data.Models;

    public class StudentSystemContext : DbContext
    {
        public StudentSystemContext()
        {

        }
        public StudentSystemContext(DbContextOptions options)
           : base(options)
        {

        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<StudentCourse> StudentCourses { get; set; }
        public DbSet<Homework> HomeworkSubmissions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=.;Database=StudentSystem; Integrated Security=true");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>().ToTable("Students");
            modelBuilder.Entity<Course>().ToTable("Courses");
            modelBuilder.Entity<Resource>().ToTable("Resources");
            modelBuilder.Entity<StudentCourse>().ToTable("StudentCourses");
            modelBuilder.Entity<Homework>().ToTable("HomeworkSubmissions");

     

            modelBuilder.Entity<Student>().HasKey(st => st.StudentId);

            modelBuilder.Entity<Student>()
                .Property(t => t.Name)
                .HasMaxLength(100)
                .IsUnicode(true)
                .IsRequired(true);

            modelBuilder.Entity<Student>()
               .Property(t => t.PhoneNumber)
               .HasMaxLength(50).IsFixedLength()
               .IsUnicode(false)
               .IsRequired(false);

            modelBuilder.Entity<Student>()
              .Property(t => t.RegisteredOn)
              .IsRequired(true);

            modelBuilder.Entity<Student>()
             .Property(t => t.Birthday)
             .IsRequired(false);



            modelBuilder.Entity<Course>().HasKey(co => co.CourseId);

            modelBuilder.Entity<Course>().Property(co => co.Name)
                .IsRequired(true)
                .HasMaxLength(80)
                .IsUnicode(true);

            modelBuilder.Entity<Course>().Property(co => co.Description)
               .IsRequired(false)
               .IsUnicode(true);

            modelBuilder.Entity<Course>().Property(co => co.StartDate)
               .IsRequired(true);

            modelBuilder.Entity<Course>().Property(co => co.EndDate)
               .IsRequired(true);

            modelBuilder.Entity<Course>().Property(co => co.Price)
               .IsRequired(true);




            modelBuilder.Entity<Resource>().HasKey(co => co.ResourceId);

            modelBuilder.Entity<Resource>().Property(re => re.Name)
                .IsRequired(true)
                .HasMaxLength(50)
                .IsUnicode(true);

            modelBuilder.Entity<Resource>().Property(re => re.Url)
               .IsRequired(true)
               .IsUnicode(false);

            modelBuilder.Entity<Resource>()
            .HasOne(p => p.Course)
            .WithMany(r => r.Resources)
            .HasForeignKey(r => r.CourseId).OnDelete(DeleteBehavior.Restrict);



            modelBuilder.Entity<Homework>().HasKey(co => co.HomeworkId);

            modelBuilder.Entity<Homework>().Property(re => re.Content)
                .IsRequired(true)
                .IsUnicode(false);

            modelBuilder.Entity<Homework>()
                .HasOne(c => c.Course)
                .WithMany(h => h.HomeworkSubmissions)
                .HasForeignKey(c => c.CourseId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Homework>()
              .HasOne(c => c.Student)
              .WithMany(h => h.HomeworkSubmissions)
              .HasForeignKey(c => c.StudentId).OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<StudentCourse>()
             .HasKey(o => new { o.StudentId, o.CourseId });

            modelBuilder.Entity<StudentCourse>()
                .HasOne(sc => sc.Course)
                .WithMany(c => c.StudentsEnrolled)
                .HasForeignKey(sc => sc.CourseId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentCourse>()
                .HasOne(sc => sc.Student)
                .WithMany(c => c.CourseEnrollments)
                .HasForeignKey(sc => sc.StudentId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
