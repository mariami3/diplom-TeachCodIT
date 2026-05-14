CREATE DATABASE TeachCodIT;
GO

USE TeachCodIT;
GO

CREATE TABLE Roles (
    ID_Role INT IDENTITY(1,1) PRIMARY KEY,
    NameRole VARCHAR(50) NOT NULL
);
GO

CREATE TABLE Users (
    ID_User INT IDENTITY(1,1) PRIMARY KEY,
    LoginUser VARCHAR(50) NOT NULL,
    PasswordUser VARCHAR(255) NOT NULL,
    Email VARCHAR(255) NOT NULL,
    ResetToken VARCHAR(100), 
    ResetTokenExpiry DATETIME,
    Role_ID INT FOREIGN KEY REFERENCES Roles(ID_Role) ON DELETE CASCADE,
);
ALTER TABLE Users
ADD RegistrationDate DATETIME NOT NULL DEFAULT GETDATE();

CREATE TABLE UserProfile (
    ID_Profile INT PRIMARY KEY,
    FirstName VARCHAR(100),
    LastName VARCHAR(100),
    AvatarUrl VARCHAR(255),
    Bio VARCHAR(500),
    User_ID INT FOREIGN KEY REFERENCES Users(ID_User) ON DELETE CASCADE,
);
GO

CREATE TABLE UserProgress (
    UserId INT PRIMARY KEY,   
    XP INT DEFAULT 0,
    Level INT DEFAULT 1,
    StreakDays INT DEFAULT 0,
    LastActivityDate DATE,
    CONSTRAINT FK_UserProgress_User FOREIGN KEY (UserId) 
        REFERENCES Users(ID_User) ON DELETE CASCADE
);
DROP TABLE UserProgress;

CREATE TABLE Course (
    ID_Course INT IDENTITY(1,1) PRIMARY KEY,
    Title VARCHAR(200) NOT NULL,
    Description VARCHAR(MAX),
    CreatedBy INT FOREIGN KEY REFERENCES Users(ID_User),
    IsPublished BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO
ALTER TABLE Course
ADD GradientColor VARCHAR(100) NULL;

CREATE TABLE Module (
    ID_Module INT IDENTITY(1,1) PRIMARY KEY,
    Title VARCHAR(200) NOT NULL,
    Course_ID INT FOREIGN KEY REFERENCES Course(ID_Course) ON DELETE CASCADE,
    OrderIndex INT NOT NULL
);
GO

CREATE TABLE Lesson (
    ID_Lesson INT IDENTITY(1,1) PRIMARY KEY,
    Title VARCHAR(200) NOT NULL,
    Content VARCHAR(MAX),
    Module_ID INT FOREIGN KEY REFERENCES Module(ID_Module) ON DELETE CASCADE,
    OrderIndex INT NOT NULL,
    XPReward INT DEFAULT 10
);
GO


CREATE TABLE LessonTask (
    ID_LessonTask INT IDENTITY(1,1) PRIMARY KEY,
    Title VARCHAR(200),
    Description VARCHAR(MAX),
    TaskType VARCHAR(50), 
    Lesson_ID INT FOREIGN KEY REFERENCES Lesson(ID_Lesson) ON DELETE CASCADE,
    XPReward INT DEFAULT 20
);
GO
ALTER TABLE LessonTask
ADD Deadline DATETIME NULL;

ALTER TABLE LessonTask
ADD ExampleCode NVARCHAR(MAX);

ALTER TABLE LessonTask
ADD MaxAttempts INT NULL;


CREATE TABLE TaskOption (
    ID_Option INT IDENTITY(1,1) PRIMARY KEY,
    LessonTask_ID INT FOREIGN KEY REFERENCES LessonTask(ID_LessonTask) ON DELETE CASCADE,
    OptionText VARCHAR(MAX),
    IsCorrect BIT DEFAULT 0
);
GO

CREATE TABLE UserTaskAttempt (
    ID_Attempt INT IDENTITY(1,1) PRIMARY KEY,
    User_ID INT FOREIGN KEY REFERENCES Users(ID_User) ON DELETE CASCADE,
    LessonTask_ID INT FOREIGN KEY REFERENCES LessonTask(ID_LessonTask) ON DELETE CASCADE,
    SubmittedAnswer VARCHAR(MAX),
    IsCorrect BIT,
    EarnedXP INT,
    AttemptDate DATETIME DEFAULT GETDATE()
);
GO

ALTER TABLE UserTaskAttempt
ADD Comment VARCHAR(MAX) NULL,
    ReviewedAt DATETIME NULL;


ALTER TABLE UserTaskAttempt
ADD AttemptNumber INT NULL;

CREATE TABLE StudentCourses (
    ID_StudentCourse INT IDENTITY(1,1) PRIMARY KEY,
    Student_ID INT FOREIGN KEY REFERENCES Users(ID_User) ON DELETE CASCADE,
    Course_ID INT FOREIGN KEY REFERENCES Course(ID_Course) ON DELETE CASCADE,
    EnrolledAt DATETIME DEFAULT GETDATE(),
    ProgressPercent INT DEFAULT 0,
    CONSTRAINT UQ_StudentCourse UNIQUE (Student_ID, Course_ID)
);
GO

CREATE TABLE Achievement (
    ID_Achievement INT IDENTITY(1,1) PRIMARY KEY,
    Title VARCHAR(200),
    Description VARCHAR(500),
    XPReward INT
);
GO 

CREATE TABLE UserLessonCompletion (
    ID_UserLessonCompletion INT IDENTITY(1,1) PRIMARY KEY,
    User_ID INT NOT NULL FOREIGN KEY REFERENCES Users(ID_User),
    Lesson_ID INT NOT NULL FOREIGN KEY REFERENCES Lesson(ID_Lesson),
    CompletedAt DATETIME NOT NULL DEFAULT GETDATE(),
    EarnedXP INT NOT NULL DEFAULT 5,
    CONSTRAINT UQ_UserLesson UNIQUE (User_ID, Lesson_ID)
);
GO 

CREATE TABLE UserAchievements (
    ID_UserAchievement INT IDENTITY(1,1) PRIMARY KEY,
    User_ID INT FOREIGN KEY REFERENCES Users(ID_User) ON DELETE CASCADE,
    Achievement_ID INT FOREIGN KEY REFERENCES Achievement(ID_Achievement) ON DELETE CASCADE,
    EarnedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT UQ_UserAchievement UNIQUE (User_ID, Achievement_ID)
);
GO


CREATE TABLE DailyQuest (
    ID_Quest INT IDENTITY(1,1) PRIMARY KEY,
    Title VARCHAR(200),
    Description VARCHAR(500),
    XPReward INT
);
GO 

CREATE TABLE UserDailyQuests (
    ID_UserDailyQuest INT IDENTITY(1,1) PRIMARY KEY,
    User_ID INT FOREIGN KEY REFERENCES Users(ID_User) ON DELETE CASCADE,
    Quest_ID INT FOREIGN KEY REFERENCES DailyQuest(ID_Quest) ON DELETE CASCADE,
    QuestDate DATE NOT NULL,
    IsCompleted BIT DEFAULT 0,
    CONSTRAINT UQ_UserDailyQuest UNIQUE (User_ID, Quest_ID, QuestDate)
);
GO

CREATE TABLE UserSettings (
    ID_Setting INT IDENTITY(1,1) PRIMARY KEY,
    User_ID INT NOT NULL UNIQUE, 

    Theme VARCHAR(20) DEFAULT 'light',
    Language VARCHAR(10) DEFAULT 'ru',
    TimeZone VARCHAR(50) DEFAULT 'UTC',

    CONSTRAINT FK_UserSettings_User 
        FOREIGN KEY (User_ID) REFERENCES Users(ID_User) ON DELETE CASCADE
);
GO

CREATE TABLE UserDailyLogin (
    ID_DailyLogin INT IDENTITY(1,1) PRIMARY KEY,
    User_ID INT NOT NULL,
    LoginDate DATE NOT NULL,
    EarnedXP INT NOT NULL DEFAULT 0,

    CONSTRAINT FK_UserDailyLogin_User 
        FOREIGN KEY (User_ID) REFERENCES Users(ID_User) ON DELETE CASCADE,

    CONSTRAINT UQ_UserDailyLogin UNIQUE (User_ID, LoginDate)
);
GO

INSERT INTO Roles (NameRole) VALUES
('Админ'),
('Учитель'),
('Студент');
GO

INSERT INTO Users (LoginUser, PasswordUser, Email, Role_ID)
VALUES 
('admin', '123456A', 'admin@mail.ru', 1),
('teacher1', '123456A', 'teacher1@mail.ru', 2),
('teacher2', '123456A', 'teacher2@mail.ru', 2),
('student1', '123456A', 'student1@mail.ru', 3),
('student2', '123456A', 'student2@mail.ru', 3);
GO

INSERT INTO UserProfile (ID_Profile, FirstName, LastName, AvatarUrl, Bio, User_ID)
VALUES
(1, 'Иван', 'Админов', '/avatars/admin.png', 'Администратор платформы', 1),
(2, 'Ольга', 'Петрова', '/avatars/teacher1.png', 'Преподаватель C#', 2),
(3, 'Алексей', 'Смирнов', '/avatars/teacher2.png', 'Преподаватель Python', 3),
(4, 'Мария', 'Иванова', '/avatars/student1.png', 'Изучаю программирование', 4),
(5, 'Дмитрий', 'Кузнецов', '/avatars/student2.png', 'Хочу стать backend-разработчиком', 5);
GO

INSERT INTO UserProgress (ID_User, XP, Level, StreakDays, LastActivityDate, User_ID)
VALUES
(4, 120, 2, 3, GETDATE(), 4),
(5, 80, 1, 1, GETDATE(), 5);
GO

INSERT INTO Course (Title, Description, CreatedBy, IsPublished)
VALUES
('Основы C#', 'Базовый курс по языку C# и .NET', 2, 1),
('Основы Python', 'Введение в Python для начинающих', 3, 1),
('Основы Java', 'Введение курса Java для начинающих', 3, 1);
GO

INSERT INTO Module (Title, Course_ID, OrderIndex)
VALUES
('Введение в C#', 1, 1),
('Типы данных в C#', 1, 2),
('Введение в Java', 2, 2),
('Введение в Python', 2, 1);
GO

INSERT INTO Lesson (Title, Content, Module_ID, OrderIndex, XPReward)
VALUES
('Что такое C#?', 'C# — это объектно-ориентированный язык...', 1, 1, 10),
('Переменные в C#', 'В C# существуют разные типы данных...', 2, 1, 15),
('Что такое Python?', 'Python — интерпретируемый язык...', 3, 1, 10);
GO

INSERT INTO LessonTask (Title, Description, TaskType, Lesson_ID, XPReward)
VALUES
('Тест по C#', 'Выберите правильный ответ', 'test', 1, 20),
('Типы данных', 'Какой тип используется для целых чисел?', 'test', 2, 20),
('Базовый тест Python', 'Выберите правильный синтаксис вывода', 'test', 3, 20);
GO

INSERT INTO TaskOption (LessonTask_ID, OptionText, IsCorrect)
VALUES
(1, 'Язык программирования', 1),
(1, 'Операционная система', 0),

(2, 'int', 1),
(2, 'string', 0),

(3, 'print("Hello")', 1),
(3, 'echo "Hello"', 0);
GO

INSERT INTO UserTaskAttempt (User_ID, LessonTask_ID, SubmittedAnswer, IsCorrect, EarnedXP)
VALUES
(4, 1, 'Язык программирования', 1, 20),
(4, 2, 'int', 1, 20),
(5, 3, 'echo "Hello"', 0, 0);
GO

INSERT INTO StudentCourses (Student_ID, Course_ID, ProgressPercent)
VALUES
(4, 1, 50),
(5, 2, 20);
GO

INSERT INTO Achievement (Title, Description, XPReward)
VALUES
('Первый шаг', 'Пройти первый урок', 50),
('100 XP', 'Набрать 100 XP', 100);
GO

INSERT INTO UserAchievements (User_ID, Achievement_ID)
VALUES
(4, 1),
(4, 2);
GO

INSERT INTO DailyQuest (Title, Description, XPReward)
VALUES
('Решить 2 задачи', 'Выполните любые две задачи', 30),
('Пройти урок', 'Завершите один урок', 20);
GO

INSERT INTO UserDailyQuests (User_ID, Quest_ID, QuestDate, IsCompleted)
VALUES
(4, 1, GETDATE(), 1),
(5, 2, GETDATE(), 0);
GO 

INSERT INTO Users (LoginUser, PasswordUser, Email, Role_ID)
VALUES 
('teach', 'nHgamgG8rRcDgTAroRYpoa8soPhzSxrLQ6qIiIz0NWo=', 'isip_m.z.petriashvili@mpt.ru', 2); --пароль как у mesa: qwerty123!
GO 

INSERT INTO Users (LoginUser, PasswordUser, Email, Role_ID)
VALUES 
('admin1', 'nHgamgG8rRcDgTAroRYpoa8soPhzSxrLQ6qIiIz0NWo=', 'isip_m.z.petriashvili@mpt.ru', 1); --пароль как у mesa: qwerty123!
GO 

INSERT INTO UserProfile (ID_Profile, FirstName, LastName, AvatarUrl, Bio, User_ID)
VALUES
(6, 'Антон', 'Сидоров', '/avatars/teacher3.png', 'Преподаватель JavaScript', 9),
(7, 'Елена', 'Кузьмина', '/avatars/admin2.png', 'Администратор платформы', 10);

SELECT CONVERT(VARCHAR(64), HASHBYTES('SHA2_256','123456A'),2)

SELECT * FROM Users;
SELECT * FROM Course;
SELECT * FROM Lesson;
SELECT * FROM LessonTask;
SELECT * FROM TaskOption;
SELECT * FROM StudentCourses;
SELECT * FROM UserProgress;
SELECT * FROM Achievement;
SELECT * FROM DailyQuest;
SELECT * FROM Roles;
SELECT * FROM UserProfile;
SELECT * FROM UserTaskAttempt;
SELECT * FROM UserDailyQuests;

SELECT ID_User, LoginUser, Role_ID
FROM Users

SELECT *
FROM StudentCourses
WHERE Course_ID = 6

UPDATE UserProgress
SET XP = 50
WHERE XP IS NULL OR XP = 0;


SELECT LessonTask_ID, IsCorrect, AttemptDate, SubmittedAnswer
FROM UserTaskAttempt
WHERE User_ID = 6
  AND LessonTask_ID IN (SELECT ID_LessonTask FROM LessonTask WHERE Lesson_ID = 8)
ORDER BY LessonTask_ID, AttemptDate DESC

DELETE FROM UserTaskAttempt
WHERE User_ID = 6
  AND LessonTask_ID IN (
      SELECT ID_LessonTask 
      FROM LessonTask 
      WHERE Lesson_ID = 8
  )

ALTER TABLE Lesson
ADD 
    ManuallyCompleted BIT NOT NULL DEFAULT 0,
    ManuallyCompletedAt DATETIME NULL;
GO
ALTER TABLE Lesson
DROP COLUMN ManuallyCompleted, ManuallyCompletedAt;
GO
-- Проверяем, что поля появились
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Lesson'
  AND COLUMN_NAME IN ('ManuallyCompleted', 'ManuallyCompletedAt');

SELECT 
    dc.name AS ConstraintName,
    OBJECT_NAME(dc.parent_object_id) AS TableName,
    c.name AS ColumnName
FROM sys.default_constraints dc
INNER JOIN sys.columns c 
    ON dc.parent_object_id = c.object_id 
    AND dc.parent_column_id = c.column_id
WHERE 
    OBJECT_NAME(dc.parent_object_id) = 'Lesson'
    AND c.name = 'ManuallyCompleted';

ALTER TABLE Lesson
DROP CONSTRAINT DF__Lesson__Manually__634EBE90;

ALTER TABLE Lesson
DROP COLUMN ManuallyCompleted, ManuallyCompletedAt;
GO

SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Lesson'
  AND COLUMN_NAME IN ('ManuallyCompleted', 'ManuallyCompletedAt');

ALTER TABLE Achievement
ADD Icon VARCHAR(100);

SELECT ID_LessonTask, Title, Xpreward
FROM LessonTask

-- 1. Создаём недостающие записи для ВСЕХ студентов
INSERT INTO UserProgress (UserId, XP, Level, StreakDays, LastActivityDate)
SELECT 
    u.ID_User,
    0,          
    1,          
    0,          
    GETDATE()
FROM Users u
WHERE u.Role_ID = 3 
  AND NOT EXISTS (SELECT 1 FROM UserProgress WHERE UserId = u.ID_User);

UPDATE UserProgress SET XP = 96, Level = 2 WHERE UserId = 6;   
UPDATE UserProgress SET XP = 50, Level = 1 WHERE UserId = 13;

SELECT u.ID_User, u.LoginUser, u.Role_ID, up.XP, up.Level, up.StreakDays
FROM Users u
LEFT JOIN UserProgress up ON u.ID_User = up.UserId
WHERE u.Role_ID = 3
ORDER BY up.XP DESC;

SELECT ID_User, LoginUser FROM Users WHERE LoginUser = 'admin1';

INSERT INTO UserProfile (ID_Profile, FirstName, LastName, Bio, User_ID)
VALUES (10, 'admin1', '', 'Администратор платформы TeachCodIT', 10);

SELECT 
    uq.ID_UserDailyQuest,
    uq.User_ID,
    uq.QuestDate,
    uq.IsCompleted,
    dq.Title as QuestTitle
FROM UserDailyQuests uq
JOIN DailyQuest dq ON uq.Quest_ID = dq.ID_Quest
WHERE uq.User_ID = 6   
ORDER BY uq.QuestDate DESC;

SELECT * FROM UserDailyQuests WHERE User_ID = 6 ORDER BY QuestDate DESC;

ALTER TABLE Achievement
ADD 
    Type VARCHAR(50),        
    TargetValue INT;      
	

DELETE FROM Achievement;

INSERT INTO Achievement (Title, Description, XPReward, Type, TargetValue)
VALUES
('Первый урок', 'Пройти 1 урок', 50, 'lessons', 1),
('Новичок', 'Набрать 100 XP', 100, 'xp', 100),
('Практик', 'Решить 10 задач', 150, 'tasks', 10),
('Серия', '3 дня подряд', 200, 'streak', 3);

ALTER TABLE DailyQuest
ADD 
    Type VARCHAR(50),
    TargetValue INT;

DELETE FROM DailyQuest;

INSERT INTO DailyQuest (Title, Description, XPReward, Type, TargetValue)
VALUES
('2 задачи', 'Решить 2 задачи', 30, 'tasks', 2),
('1 урок', 'Пройти 1 урок', 20, 'lessons', 1);

DELETE FROM UserAchievements;
DELETE FROM UserDailyQuests;

INSERT INTO UserProgress (UserId, XP, Level, StreakDays, LastActivityDate)
VALUES
(4, 120, 2, 3, GETDATE()),
(5, 80, 1, 1, GETDATE());

ALTER TABLE UserDailyQuests
ADD 
    CurrentValue INT DEFAULT 0,   
    CompletedAt DATETIME NULL;   

ALTER TABLE UserDailyQuests
ADD 
    TargetValue INT NULL,
    ProgressPercent INT NULL;

ALTER TABLE DailyQuest
ADD 
    IncrementStep INT DEFAULT 10;

UPDATE DailyQuest SET 
    Title = 'Решить правильно 1 задание',
    Type = 'correct_tasks',
    TargetValue = 1
WHERE Type = 'tasks';

UPDATE DailyQuest SET 
    Title = 'Пройти 1 модуль',
    Type = 'modules',
    TargetValue = 1
WHERE Type = 'lessons';

UPDATE DailyQuest SET 
    Title = 'Отметить серию (войти в систему)',
    Type = 'streak',
    TargetValue = 1
WHERE Type = 'xp';

DELETE FROM DailyQuest;

INSERT INTO DailyQuest (Title, Description, XPReward, Type, TargetValue)
VALUES
    ('Решить правильно 1 задание', 'Решить 1 задание правильно сегодня', 30, 'correct_tasks', 1),
    ('Пройти 1 урок','Завершить хотя бы 1 урок сегодня',20, 'lessons',1),
    ('Заработать 75 XP','Набрать 75 XP за сегодня',50, 'xp',75),
    ('Пройти 1 модуль','Пройти хотя бы 1 модуль (3+ урока)',40, 'modules',1);

SELECT * FROM UserDailyLogin WHERE User_ID = 6 AND LoginDate = CAST(GETDATE() AS DATE);
SELECT SUM(EarnedXp) FROM UserTaskAttempt WHERE User_ID = 6 AND AttemptDate >= CAST(GETDATE() AS DATE);

SELECT SUM(EarnedXp) FROM UserTaskAttempt WHERE User_ID = 6 AND AttemptDate >= CAST(GETDATE() AS DATE);

-- XP из заданий сегодня
SELECT SUM(EarnedXp) FROM UserTaskAttempt 
WHERE User_ID = 6 AND AttemptDate >= CAST(GETDATE() AS DATE);

-- XP из уроков сегодня
SELECT SUM(EarnedXp) FROM UserLessonCompletion 
WHERE User_ID = 6 AND CompletedAt >= CAST(GETDATE() AS DATE);

-- XP из логина сегодня
SELECT SUM(EarnedXp) FROM UserDailyLogin 
WHERE User_ID = 6 AND LoginDate = CAST(GETDATE() AS DATE);

SELECT 
    AttemptDate, 
    IsCorrect, 
    EarnedXp, 
    LessonTask_ID 
FROM UserTaskAttempt 
WHERE User_ID = 6 
  AND AttemptDate >= CAST(GETDATE() AS DATE)
ORDER BY AttemptDate DESC;	

SELECT TOP 3 * FROM UserDailyQuests 
WHERE User_ID = 6 AND QuestDate = CAST(GETDATE() AS DATE)
ORDER BY ID_UserDailyQuest DESC;

SELECT TOP 5 * FROM UserTaskAttempt 
WHERE User_ID = 6 
ORDER BY AttemptDate DESC;

ALTER TABLE LessonTask
ADD 
    ExpectedOutput NVARCHAR(MAX) NULL,
    CheckerType VARCHAR(50) DEFAULT 'exact';

ALTER TABLE LessonTask
ADD 
StarterCode NVARCHAR(MAX),
TestInput NVARCHAR(MAX);