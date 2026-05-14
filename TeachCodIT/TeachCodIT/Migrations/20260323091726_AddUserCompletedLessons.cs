using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachCodIT.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCompletedLessons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Achievement",
                columns: table => new
                {
                    ID_Achievement = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    XPReward = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Achievem__BB2F175E45D233D6", x => x.ID_Achievement);
                });

            migrationBuilder.CreateTable(
                name: "DailyQuest",
                columns: table => new
                {
                    ID_Quest = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    XPReward = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DailyQue__CEDD8D3A98AF8B29", x => x.ID_Quest);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    ID_Role = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameRole = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Roles__43DCD32D3F0C80D2", x => x.ID_Role);
                });

            migrationBuilder.CreateTable(
                name: "UserCompletedLessons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LessonId = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCompletedLessons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    ID_User = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoginUser = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    PasswordUser = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    ResetToken = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ResetTokenExpiry = table.Column<DateTime>(type: "datetime", nullable: true),
                    Role_ID = table.Column<int>(type: "int", nullable: true),
                    RegistrationDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__ED4DE4424D5AFBE4", x => x.ID_User);
                    table.ForeignKey(
                        name: "FK__Users__Role_ID__4BAC3F29",
                        column: x => x.Role_ID,
                        principalTable: "Roles",
                        principalColumn: "ID_Role",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Course",
                columns: table => new
                {
                    ID_Course = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    GradientColor = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Course__E2B749CDA11B0F4A", x => x.ID_Course);
                    table.ForeignKey(
                        name: "FK__Course__CreatedB__571DF1D5",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "ID_User");
                });

            migrationBuilder.CreateTable(
                name: "UserAchievements",
                columns: table => new
                {
                    ID_UserAchievement = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User_ID = table.Column<int>(type: "int", nullable: true),
                    Achievement_ID = table.Column<int>(type: "int", nullable: true),
                    EarnedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserAchi__4F1106D2A6E6C806", x => x.ID_UserAchievement);
                    table.ForeignKey(
                        name: "FK__UserAchie__Achie__2B0A656D",
                        column: x => x.Achievement_ID,
                        principalTable: "Achievement",
                        principalColumn: "ID_Achievement",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__UserAchie__User___2A164134",
                        column: x => x.User_ID,
                        principalTable: "Users",
                        principalColumn: "ID_User",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserDailyQuests",
                columns: table => new
                {
                    ID_UserDailyQuest = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User_ID = table.Column<int>(type: "int", nullable: true),
                    Quest_ID = table.Column<int>(type: "int", nullable: true),
                    QuestDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserDail__20FCA313AF79D605", x => x.ID_UserDailyQuest);
                    table.ForeignKey(
                        name: "FK__UserDaily__Quest__32AB8735",
                        column: x => x.Quest_ID,
                        principalTable: "DailyQuest",
                        principalColumn: "ID_Quest",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__UserDaily__User___31B762FC",
                        column: x => x.User_ID,
                        principalTable: "Users",
                        principalColumn: "ID_User",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProfile",
                columns: table => new
                {
                    ID_Profile = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    AvatarUrl = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    Bio = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    User_ID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserProf__F1B3F50CFDA72C57", x => x.ID_Profile);
                    table.ForeignKey(
                        name: "FK__UserProfi__User___4E88ABD4",
                        column: x => x.User_ID,
                        principalTable: "Users",
                        principalColumn: "ID_User",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProgress",
                columns: table => new
                {
                    ID_User = table.Column<int>(type: "int", nullable: false),
                    XP = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    Level = table.Column<int>(type: "int", nullable: true, defaultValue: 1),
                    StreakDays = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    LastActivityDate = table.Column<DateOnly>(type: "date", nullable: true),
                    User_ID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserProg__ED4DE442ADAE2B3E", x => x.ID_User);
                    table.ForeignKey(
                        name: "FK__UserProgr__User___5441852A",
                        column: x => x.User_ID,
                        principalTable: "Users",
                        principalColumn: "ID_User",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Module",
                columns: table => new
                {
                    ID_Module = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Course_ID = table.Column<int>(type: "int", nullable: true),
                    OrderIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Module__E498BA5D5F028558", x => x.ID_Module);
                    table.ForeignKey(
                        name: "FK__Module__Course_I__5BE2A6F2",
                        column: x => x.Course_ID,
                        principalTable: "Course",
                        principalColumn: "ID_Course",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentCourses",
                columns: table => new
                {
                    ID_StudentCourse = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Student_ID = table.Column<int>(type: "int", nullable: true),
                    Course_ID = table.Column<int>(type: "int", nullable: true),
                    EnrolledAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    ProgressPercent = table.Column<int>(type: "int", nullable: true, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__StudentC__8D37F341DF705545", x => x.ID_StudentCourse);
                    table.ForeignKey(
                        name: "FK__StudentCo__Cours__245D67DE",
                        column: x => x.Course_ID,
                        principalTable: "Course",
                        principalColumn: "ID_Course",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__StudentCo__Stude__236943A5",
                        column: x => x.Student_ID,
                        principalTable: "Users",
                        principalColumn: "ID_User",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lesson",
                columns: table => new
                {
                    ID_Lesson = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    Module_ID = table.Column<int>(type: "int", nullable: true),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    XPReward = table.Column<int>(type: "int", nullable: true, defaultValue: 10)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Lesson__67381F3B754CCF23", x => x.ID_Lesson);
                    table.ForeignKey(
                        name: "FK__Lesson__Module_I__5EBF139D",
                        column: x => x.Module_ID,
                        principalTable: "Module",
                        principalColumn: "ID_Module",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LessonTask",
                columns: table => new
                {
                    ID_LessonTask = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    TaskType = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Lesson_ID = table.Column<int>(type: "int", nullable: true),
                    XPReward = table.Column<int>(type: "int", nullable: true, defaultValue: 20),
                    Deadline = table.Column<DateTime>(type: "datetime", nullable: true),
                    ExampleCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaxAttempts = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LessonTa__BDD97AA713B5EBB3", x => x.ID_LessonTask);
                    table.ForeignKey(
                        name: "FK__LessonTas__Lesso__160F4887",
                        column: x => x.Lesson_ID,
                        principalTable: "Lesson",
                        principalColumn: "ID_Lesson",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskOption",
                columns: table => new
                {
                    ID_Option = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LessonTask_ID = table.Column<int>(type: "int", nullable: true),
                    OptionText = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TaskOpti__A8AA67B55F8BDC36", x => x.ID_Option);
                    table.ForeignKey(
                        name: "FK__TaskOptio__Lesso__19DFD96B",
                        column: x => x.LessonTask_ID,
                        principalTable: "LessonTask",
                        principalColumn: "ID_LessonTask",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTaskAttempt",
                columns: table => new
                {
                    ID_Attempt = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User_ID = table.Column<int>(type: "int", nullable: true),
                    LessonTask_ID = table.Column<int>(type: "int", nullable: true),
                    SubmittedAnswer = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: true),
                    EarnedXP = table.Column<int>(type: "int", nullable: true),
                    AttemptDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    Comment = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    AttemptNumber = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserTask__701ABE91E984D707", x => x.ID_Attempt);
                    table.ForeignKey(
                        name: "FK__UserTaskA__Lesso__1EA48E88",
                        column: x => x.LessonTask_ID,
                        principalTable: "LessonTask",
                        principalColumn: "ID_LessonTask",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK__UserTaskA__User___1DB06A4F",
                        column: x => x.User_ID,
                        principalTable: "Users",
                        principalColumn: "ID_User",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Course_CreatedBy",
                table: "Course",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Lesson_Module_ID",
                table: "Lesson",
                column: "Module_ID");

            migrationBuilder.CreateIndex(
                name: "IX_LessonTask_Lesson_ID",
                table: "LessonTask",
                column: "Lesson_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Module_Course_ID",
                table: "Module",
                column: "Course_ID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCourses_Course_ID",
                table: "StudentCourses",
                column: "Course_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_StudentCourse",
                table: "StudentCourses",
                columns: new[] { "Student_ID", "Course_ID" },
                unique: true,
                filter: "[Student_ID] IS NOT NULL AND [Course_ID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TaskOption_LessonTask_ID",
                table: "TaskOption",
                column: "LessonTask_ID");

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_Achievement_ID",
                table: "UserAchievements",
                column: "Achievement_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_UserAchievement",
                table: "UserAchievements",
                columns: new[] { "User_ID", "Achievement_ID" },
                unique: true,
                filter: "[User_ID] IS NOT NULL AND [Achievement_ID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserDailyQuests_Quest_ID",
                table: "UserDailyQuests",
                column: "Quest_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_UserDailyQuest",
                table: "UserDailyQuests",
                columns: new[] { "User_ID", "Quest_ID", "QuestDate" },
                unique: true,
                filter: "[User_ID] IS NOT NULL AND [Quest_ID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfile_User_ID",
                table: "UserProfile",
                column: "User_ID");

            migrationBuilder.CreateIndex(
                name: "IX_UserProgress_User_ID",
                table: "UserProgress",
                column: "User_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role_ID",
                table: "Users",
                column: "Role_ID");

            migrationBuilder.CreateIndex(
                name: "IX_UserTaskAttempt_LessonTask_ID",
                table: "UserTaskAttempt",
                column: "LessonTask_ID");

            migrationBuilder.CreateIndex(
                name: "IX_UserTaskAttempt_User_ID",
                table: "UserTaskAttempt",
                column: "User_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentCourses");

            migrationBuilder.DropTable(
                name: "TaskOption");

            migrationBuilder.DropTable(
                name: "UserAchievements");

            migrationBuilder.DropTable(
                name: "UserCompletedLessons");

            migrationBuilder.DropTable(
                name: "UserDailyQuests");

            migrationBuilder.DropTable(
                name: "UserProfile");

            migrationBuilder.DropTable(
                name: "UserProgress");

            migrationBuilder.DropTable(
                name: "UserTaskAttempt");

            migrationBuilder.DropTable(
                name: "Achievement");

            migrationBuilder.DropTable(
                name: "DailyQuest");

            migrationBuilder.DropTable(
                name: "LessonTask");

            migrationBuilder.DropTable(
                name: "Lesson");

            migrationBuilder.DropTable(
                name: "Module");

            migrationBuilder.DropTable(
                name: "Course");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
