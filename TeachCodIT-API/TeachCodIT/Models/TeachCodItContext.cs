using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TeachCodIT.Models;

public partial class TeachCodItContext : DbContext
{
    public TeachCodItContext()
    {
    }

    public TeachCodItContext(DbContextOptions<TeachCodItContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Achievement> Achievements { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<DailyQuest> DailyQuests { get; set; }

    public virtual DbSet<Lesson> Lessons { get; set; }

    public virtual DbSet<LessonTask> LessonTasks { get; set; }

    public virtual DbSet<Module> Modules { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<StudentCourse> StudentCourses { get; set; }

    public virtual DbSet<TaskOption> TaskOptions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAchievement> UserAchievements { get; set; }

    public virtual DbSet<UserDailyQuest> UserDailyQuests { get; set; }

    public virtual DbSet<UserProfile> UserProfiles { get; set; }

    public virtual DbSet<UserProgress> UserProgresses { get; set; }

    public virtual DbSet<UserTaskAttempt> UserTaskAttempts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=WIN-5O8Q88327DH\\SQLEXPRESS01;Initial Catalog=TeachCodIT;Integrated Security=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Achievement>(entity =>
        {
            entity.HasKey(e => e.IdAchievement).HasName("PK__Achievem__BB2F175E45D233D6");

            entity.ToTable("Achievement");

            entity.Property(e => e.IdAchievement).HasColumnName("ID_Achievement");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Xpreward).HasColumnName("XPReward");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.IdCourse).HasName("PK__Course__E2B749CDA11B0F4A");

            entity.ToTable("Course");

            entity.Property(e => e.IdCourse).HasColumnName("ID_Course");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.GradientColor)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.IsPublished).HasDefaultValue(false);
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Courses)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Course__CreatedB__571DF1D5");
        });

        modelBuilder.Entity<DailyQuest>(entity =>
        {
            entity.HasKey(e => e.IdQuest).HasName("PK__DailyQue__CEDD8D3A98AF8B29");

            entity.ToTable("DailyQuest");

            entity.Property(e => e.IdQuest).HasColumnName("ID_Quest");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Xpreward).HasColumnName("XPReward");
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.IdLesson).HasName("PK__Lesson__67381F3B754CCF23");

            entity.ToTable("Lesson");

            entity.Property(e => e.IdLesson).HasColumnName("ID_Lesson");
            entity.Property(e => e.Content).IsUnicode(false);
            entity.Property(e => e.ModuleId).HasColumnName("Module_ID");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Xpreward)
                .HasDefaultValue(10)
                .HasColumnName("XPReward");

            entity.HasOne(d => d.Module).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.ModuleId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Lesson__Module_I__5EBF139D");
        });

        modelBuilder.Entity<LessonTask>(entity =>
        {
            entity.HasKey(e => e.IdLessonTask).HasName("PK__LessonTa__BDD97AA713B5EBB3");

            entity.ToTable("LessonTask");

            entity.Property(e => e.IdLessonTask).HasColumnName("ID_LessonTask");
            entity.Property(e => e.Deadline).HasColumnType("datetime");
            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.LessonId).HasColumnName("Lesson_ID");
            entity.Property(e => e.TaskType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Xpreward)
                .HasDefaultValue(20)
                .HasColumnName("XPReward");

            entity.HasOne(d => d.Lesson).WithMany(p => p.LessonTasks)
                .HasForeignKey(d => d.LessonId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__LessonTas__Lesso__160F4887");
        });

        modelBuilder.Entity<Module>(entity =>
        {
            entity.HasKey(e => e.IdModule).HasName("PK__Module__E498BA5D5F028558");

            entity.ToTable("Module");

            entity.Property(e => e.IdModule).HasColumnName("ID_Module");
            entity.Property(e => e.CourseId).HasColumnName("Course_ID");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.Course).WithMany(p => p.Modules)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Module__Course_I__5BE2A6F2");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRole).HasName("PK__Roles__43DCD32D3F0C80D2");

            entity.Property(e => e.IdRole).HasColumnName("ID_Role");
            entity.Property(e => e.NameRole)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<StudentCourse>(entity =>
        {
            entity.HasKey(e => e.IdStudentCourse).HasName("PK__StudentC__8D37F341DF705545");

            entity.HasIndex(e => new { e.StudentId, e.CourseId }, "UQ_StudentCourse").IsUnique();

            entity.Property(e => e.IdStudentCourse).HasColumnName("ID_StudentCourse");
            entity.Property(e => e.CourseId).HasColumnName("Course_ID");
            entity.Property(e => e.EnrolledAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ProgressPercent).HasDefaultValue(0);
            entity.Property(e => e.StudentId).HasColumnName("Student_ID");

            entity.HasOne(d => d.Course).WithMany(p => p.StudentCourses)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__StudentCo__Cours__245D67DE");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentCourses)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__StudentCo__Stude__236943A5");
        });

        modelBuilder.Entity<TaskOption>(entity =>
        {
            entity.HasKey(e => e.IdOption).HasName("PK__TaskOpti__A8AA67B55F8BDC36");

            entity.ToTable("TaskOption");

            entity.Property(e => e.IdOption).HasColumnName("ID_Option");
            entity.Property(e => e.IsCorrect).HasDefaultValue(false);
            entity.Property(e => e.LessonTaskId).HasColumnName("LessonTask_ID");
            entity.Property(e => e.OptionText).IsUnicode(false);

            entity.HasOne(d => d.LessonTask).WithMany(p => p.TaskOptions)
                .HasForeignKey(d => d.LessonTaskId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__TaskOptio__Lesso__19DFD96B");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("PK__Users__ED4DE4424D5AFBE4");

            entity.Property(e => e.IdUser).HasColumnName("ID_User");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.LoginUser)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PasswordUser)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.RegistrationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ResetToken)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ResetTokenExpiry).HasColumnType("datetime");
            entity.Property(e => e.RoleId).HasColumnName("Role_ID");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Users__Role_ID__4BAC3F29");
        });

        modelBuilder.Entity<UserAchievement>(entity =>
        {
            entity.HasKey(e => e.IdUserAchievement).HasName("PK__UserAchi__4F1106D2A6E6C806");

            entity.HasIndex(e => new { e.UserId, e.AchievementId }, "UQ_UserAchievement").IsUnique();

            entity.Property(e => e.IdUserAchievement).HasColumnName("ID_UserAchievement");
            entity.Property(e => e.AchievementId).HasColumnName("Achievement_ID");
            entity.Property(e => e.EarnedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("User_ID");

            entity.HasOne(d => d.Achievement).WithMany(p => p.UserAchievements)
                .HasForeignKey(d => d.AchievementId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__UserAchie__Achie__2B0A656D");

            entity.HasOne(d => d.User).WithMany(p => p.UserAchievements)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__UserAchie__User___2A164134");
        });

        modelBuilder.Entity<UserDailyQuest>(entity =>
        {
            entity.HasKey(e => e.IdUserDailyQuest).HasName("PK__UserDail__20FCA313AF79D605");

            entity.HasIndex(e => new { e.UserId, e.QuestId, e.QuestDate }, "UQ_UserDailyQuest").IsUnique();

            entity.Property(e => e.IdUserDailyQuest).HasColumnName("ID_UserDailyQuest");
            entity.Property(e => e.IsCompleted).HasDefaultValue(false);
            entity.Property(e => e.QuestId).HasColumnName("Quest_ID");
            entity.Property(e => e.UserId).HasColumnName("User_ID");

            entity.HasOne(d => d.Quest).WithMany(p => p.UserDailyQuests)
                .HasForeignKey(d => d.QuestId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__UserDaily__Quest__32AB8735");

            entity.HasOne(d => d.User).WithMany(p => p.UserDailyQuests)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__UserDaily__User___31B762FC");
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.IdProfile).HasName("PK__UserProf__F1B3F50CFDA72C57");

            entity.ToTable("UserProfile");

            entity.Property(e => e.IdProfile)
                .ValueGeneratedNever()
                .HasColumnName("ID_Profile");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Bio)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UserId).HasColumnName("User_ID");

            entity.HasOne(d => d.User).WithMany(p => p.UserProfiles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__UserProfi__User___4E88ABD4");
        });

        modelBuilder.Entity<UserProgress>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("PK__UserProg__ED4DE442ADAE2B3E");

            entity.ToTable("UserProgress");

            entity.Property(e => e.IdUser)
                .ValueGeneratedNever()
                .HasColumnName("ID_User");
            entity.Property(e => e.Level).HasDefaultValue(1);
            entity.Property(e => e.StreakDays).HasDefaultValue(0);
            entity.Property(e => e.UserId).HasColumnName("User_ID");
            entity.Property(e => e.Xp)
                .HasDefaultValue(0)
                .HasColumnName("XP");

            entity.HasOne(d => d.User).WithMany(p => p.UserProgresses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__UserProgr__User___5441852A");
        });

        modelBuilder.Entity<UserTaskAttempt>(entity =>
        {
            entity.HasKey(e => e.IdAttempt).HasName("PK__UserTask__701ABE91E984D707");

            entity.ToTable("UserTaskAttempt");

            entity.Property(e => e.IdAttempt).HasColumnName("ID_Attempt");
            entity.Property(e => e.AttemptDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Comment).IsUnicode(false);
            entity.Property(e => e.EarnedXp).HasColumnName("EarnedXP");
            entity.Property(e => e.LessonTaskId).HasColumnName("LessonTask_ID");
            entity.Property(e => e.ReviewedAt).HasColumnType("datetime");
            entity.Property(e => e.SubmittedAnswer).IsUnicode(false);
            entity.Property(e => e.UserId).HasColumnName("User_ID");

            entity.HasOne(d => d.LessonTask).WithMany(p => p.UserTaskAttempts)
                .HasForeignKey(d => d.LessonTaskId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__UserTaskA__Lesso__1EA48E88");

            entity.HasOne(d => d.User).WithMany(p => p.UserTaskAttempts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__UserTaskA__User___1DB06A4F");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
