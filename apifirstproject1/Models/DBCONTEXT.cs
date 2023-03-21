using Microsoft.EntityFrameworkCore;

namespace Graduate_Project_BackEnd.Models
{
    public class DBCONTEXT : DbContext
    {
        public DbSet<AdminModel> Admin { get; set; }
        public DbSet<StudentsModel> Students { get; set; }
        public DbSet<ProffessorModel> Proffessors { get; set; }
        public DbSet<InstructorModel> Instructors { get; set; }
        public DbSet<TeamModel> Teams { get; set; }
        public DbSet<ProjectModel> Projects { get; set; }
        public DbSet<CourseModel> Courses { get; set; }
        public DbSet<Courses_StudentsModel> Courses_Students { get; set; }
        public DbSet<NotificationModel> Notifications { get; set; }
        public DbSet<ProfNotificationsModel> profNotifications { get; set; }
        //public DbSet<StudentTeamModel> Students_Teams { get; set; }
        public DbSet<SkilsModel> Skils { get; set; }
        public DbSet<ChatModel> Chat { get; set; }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=.;Database=TMS;User Id=sa;Password=12345;");

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region config for database
            modelBuilder.Entity<StudentsModel>().HasIndex(s => s.Email).IsUnique(unique: true);
            modelBuilder.Entity<StudentsModel>().HasIndex(s => s.Phone).IsUnique(unique: true);
            modelBuilder.Entity<ProffessorModel>().HasIndex(s => s.Email).IsUnique(unique: true);
            modelBuilder.Entity<ProffessorModel>().HasIndex(s => s.Phone).IsUnique(unique: true);
            modelBuilder.Entity<InstructorModel>().HasIndex(s => s.Email).IsUnique(unique: true);
            modelBuilder.Entity<InstructorModel>().HasIndex(s => s.Phone).IsUnique(unique: true);
            modelBuilder.Entity<Courses_StudentsModel>().HasKey(cs => new { cs.StudentID, cs.CourseID });
            //modelBuilder.Entity<StudentTeamModel>().HasKey(st => new { st.StudentID, st.TeamID });


            #endregion
            //relationships
            //one to many
            modelBuilder.Entity<TeamModel>().HasOne<StudentsModel>(t => t.Leader).WithMany(s => s.TeamLeader).HasForeignKey(t => t.LeaderID).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<TeamModel>().HasOne<CourseModel>(t => t.Course).WithMany(c => c.Teams).HasForeignKey(t => t.CourseID).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<TeamModel>().HasOne<ProffessorModel>(t => t.Prof).WithMany(p => p.Teams).HasForeignKey(t => t.ProfID).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ProjectModel>().HasOne<TeamModel>(p => p.Team).WithMany(t => t.Projects).HasForeignKey(p => p.TeamID).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CourseModel>().HasOne<InstructorModel>(c => c.Instructor).WithMany(I => I.Courses).HasForeignKey(c => c.InstructorID).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Courses_StudentsModel>().HasOne<CourseModel>(cs => cs.Course).WithMany(c => c.Students).HasForeignKey(cs => cs.CourseID).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Courses_StudentsModel>().HasOne<StudentsModel>(cs => cs.Student).WithMany(s => s.Courses).HasForeignKey(cs => cs.StudentID).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Courses_StudentsModel>().HasOne<TeamModel>(cs => cs.Team).WithMany(t => t.Courses_Students).HasForeignKey(cs => cs.TeamID).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<NotificationModel>().HasOne<StudentsModel>(n => n.Student).WithMany(s => s.Notifications).HasForeignKey(n => n.StudentId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<SkilsModel>().HasOne<StudentsModel>(sk => sk.Student).WithMany(s => s.Skils).HasForeignKey(sk => sk.StudentID).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ProfNotificationsModel>().HasOne<ProffessorModel>(n => n.professor).WithMany(s => s.Notifications).HasForeignKey(n => n.ProfId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ChatModel>().HasOne<TeamModel>(c => c.Team).WithMany(t => t.Chats).HasForeignKey(c => c.TeamID).OnDelete(DeleteBehavior.Cascade);
            base.OnModelCreating(modelBuilder);
        }

    }
}
