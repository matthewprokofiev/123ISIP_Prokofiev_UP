/* =============================================================================
   Информационная система «Читай, Пиши и не спиши»
   Скрипт создания базы данных (схема).
   СУБД: Microsoft SQL Server.
   Нормальная форма: 3НФ. Имена объектов в CamelCase, только латиница.
   ============================================================================= */

-- При необходимости создать БД (раскомментируйте при первом запуске):
-- CREATE DATABASE ReadWriteDB;
-- GO
-- USE ReadWriteDB;
-- GO

/* -----------------------------------------------------------------------------
   Удаление таблиц при повторном запуске (в порядке, обратном зависимостям).
   ----------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.UnfreezeRequests', 'U') IS NOT NULL DROP TABLE dbo.UnfreezeRequests;
IF OBJECT_ID('dbo.RoleRequests',     'U') IS NOT NULL DROP TABLE dbo.RoleRequests;
IF OBJECT_ID('dbo.Complaints',       'U') IS NOT NULL DROP TABLE dbo.Complaints;
IF OBJECT_ID('dbo.ReadingListItems', 'U') IS NOT NULL DROP TABLE dbo.ReadingListItems;
IF OBJECT_ID('dbo.Reviews',          'U') IS NOT NULL DROP TABLE dbo.Reviews;
IF OBJECT_ID('dbo.BookGenres',       'U') IS NOT NULL DROP TABLE dbo.BookGenres;
IF OBJECT_ID('dbo.Books',            'U') IS NOT NULL DROP TABLE dbo.Books;
IF OBJECT_ID('dbo.Genres',           'U') IS NOT NULL DROP TABLE dbo.Genres;
IF OBJECT_ID('dbo.Users',            'U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID('dbo.ReadingStatuses',  'U') IS NOT NULL DROP TABLE dbo.ReadingStatuses;
IF OBJECT_ID('dbo.RequestStatuses',  'U') IS NOT NULL DROP TABLE dbo.RequestStatuses;
IF OBJECT_ID('dbo.Roles',            'U') IS NOT NULL DROP TABLE dbo.Roles;
GO

/* =============================================================================
   СПРАВОЧНИКИ
   ============================================================================= */

-- Роли пользователей: Читатель, Автор, Администратор.
CREATE TABLE dbo.Roles
(
    RoleId   INT           IDENTITY(1,1) NOT NULL,
    RoleName NVARCHAR(50)  NOT NULL,
    CONSTRAINT PK_Roles      PRIMARY KEY (RoleId),
    CONSTRAINT UQ_Roles_Name UNIQUE (RoleName)
);
GO

-- Статусы заявок/жалоб: На рассмотрении, Принято, Отклонено.
CREATE TABLE dbo.RequestStatuses
(
    StatusId   INT          IDENTITY(1,1) NOT NULL,
    StatusName NVARCHAR(50) NOT NULL,
    CONSTRAINT PK_RequestStatuses      PRIMARY KEY (StatusId),
    CONSTRAINT UQ_RequestStatuses_Name UNIQUE (StatusName)
);
GO

-- Разделы списков чтения: Заброшено, В планах, Читаю, Прочитано.
CREATE TABLE dbo.ReadingStatuses
(
    ReadingStatusId INT          IDENTITY(1,1) NOT NULL,
    StatusName      NVARCHAR(50) NOT NULL,
    CONSTRAINT PK_ReadingStatuses      PRIMARY KEY (ReadingStatusId),
    CONSTRAINT UQ_ReadingStatuses_Name UNIQUE (StatusName)
);
GO

/* =============================================================================
   ОСНОВНЫЕ ТАБЛИЦЫ
   ============================================================================= */

-- Пользователи системы.
CREATE TABLE dbo.Users
(
    UserId       INT            IDENTITY(1,1) NOT NULL,
    Login        NVARCHAR(50)   NOT NULL,
    PasswordHash NVARCHAR(255)  NOT NULL,
    Email        NVARCHAR(255)  NOT NULL,
    DisplayName  NVARCHAR(100)  NOT NULL,
    RoleId       INT            NOT NULL,
    IsFrozen     BIT            NOT NULL CONSTRAINT DF_Users_IsFrozen  DEFAULT (0),
    CreatedAt    DATETIME       NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (GETDATE()),
    CONSTRAINT PK_Users        PRIMARY KEY (UserId),
    CONSTRAINT UQ_Users_Login  UNIQUE (Login),
    CONSTRAINT UQ_Users_Email  UNIQUE (Email),
    CONSTRAINT FK_Users_Roles  FOREIGN KEY (RoleId) REFERENCES dbo.Roles (RoleId)
);
GO

-- Жанры.
CREATE TABLE dbo.Genres
(
    GenreId     INT            IDENTITY(1,1) NOT NULL,
    GenreName   NVARCHAR(100)  NOT NULL,
    Description  NVARCHAR(500) NULL,
    CONSTRAINT PK_Genres      PRIMARY KEY (GenreId),
    CONSTRAINT UQ_Genres_Name UNIQUE (GenreName)
);
GO

-- Книги. Автор книги — пользователь с ролью «Автор».
CREATE TABLE dbo.Books
(
    BookId      INT            IDENTITY(1,1) NOT NULL,
    Title       NVARCHAR(200)  NOT NULL,
    Description NVARCHAR(MAX)  NULL,
    CoverPath   NVARCHAR(500)  NULL,
    Content     NVARCHAR(MAX)  NULL,
    AuthorId    INT            NOT NULL,
    IsFrozen    BIT            NOT NULL CONSTRAINT DF_Books_IsFrozen  DEFAULT (0),
    CreatedAt   DATETIME       NOT NULL CONSTRAINT DF_Books_CreatedAt DEFAULT (GETDATE()),
    CONSTRAINT PK_Books        PRIMARY KEY (BookId),
    CONSTRAINT FK_Books_Users  FOREIGN KEY (AuthorId) REFERENCES dbo.Users (UserId)
);
GO

-- Связь «многие-ко-многим»: книга может относиться к нескольким жанрам.
CREATE TABLE dbo.BookGenres
(
    BookId  INT NOT NULL,
    GenreId INT NOT NULL,
    CONSTRAINT PK_BookGenres        PRIMARY KEY (BookId, GenreId),
    CONSTRAINT FK_BookGenres_Books  FOREIGN KEY (BookId)  REFERENCES dbo.Books (BookId),
    CONSTRAINT FK_BookGenres_Genres FOREIGN KEY (GenreId) REFERENCES dbo.Genres (GenreId)
);
GO

-- Отзывы на книги с оценкой от 1 до 10.
CREATE TABLE dbo.Reviews
(
    ReviewId   INT           IDENTITY(1,1) NOT NULL,
    BookId     INT           NOT NULL,
    UserId     INT           NOT NULL,
    ReviewText NVARCHAR(MAX) NULL,
    Rating     INT           NOT NULL,
    IsFrozen   BIT           NOT NULL CONSTRAINT DF_Reviews_IsFrozen  DEFAULT (0),
    CreatedAt  DATETIME      NOT NULL CONSTRAINT DF_Reviews_CreatedAt DEFAULT (GETDATE()),
    CONSTRAINT PK_Reviews         PRIMARY KEY (ReviewId),
    CONSTRAINT FK_Reviews_Books   FOREIGN KEY (BookId) REFERENCES dbo.Books (BookId),
    CONSTRAINT FK_Reviews_Users   FOREIGN KEY (UserId) REFERENCES dbo.Users (UserId),
    CONSTRAINT CK_Reviews_Rating  CHECK (Rating BETWEEN 1 AND 10),
    CONSTRAINT UQ_Reviews_BookUser UNIQUE (BookId, UserId)   -- один отзыв пользователя на книгу
);
GO

-- Списки чтения. У одного пользователя книга может быть только в одном разделе.
CREATE TABLE dbo.ReadingListItems
(
    ReadingListItemId INT      IDENTITY(1,1) NOT NULL,
    UserId            INT      NOT NULL,
    BookId            INT      NOT NULL,
    ReadingStatusId   INT      NOT NULL,
    AddedAt           DATETIME NOT NULL CONSTRAINT DF_ReadingListItems_AddedAt DEFAULT (GETDATE()),
    CONSTRAINT PK_ReadingListItems          PRIMARY KEY (ReadingListItemId),
    CONSTRAINT FK_ReadingListItems_Users    FOREIGN KEY (UserId)          REFERENCES dbo.Users (UserId),
    CONSTRAINT FK_ReadingListItems_Books    FOREIGN KEY (BookId)          REFERENCES dbo.Books (BookId),
    CONSTRAINT FK_ReadingListItems_Statuses FOREIGN KEY (ReadingStatusId) REFERENCES dbo.ReadingStatuses (ReadingStatusId),
    CONSTRAINT UQ_ReadingListItems_UserBook UNIQUE (UserId, BookId)
);
GO

-- Жалобы: на книгу, на отзыв или на пользователя (автора). Указывается причина.
CREATE TABLE dbo.Complaints
(
    ComplaintId    INT           IDENTITY(1,1) NOT NULL,
    ComplainantId  INT           NOT NULL,            -- кто пожаловался
    TargetBookId   INT           NULL,                -- жалоба на книгу
    TargetReviewId INT           NULL,                -- жалоба на отзыв
    TargetUserId   INT           NULL,                -- жалоба на пользователя/автора
    Reason         NVARCHAR(500) NOT NULL,
    StatusId       INT           NOT NULL CONSTRAINT DF_Complaints_StatusId  DEFAULT (1),
    CreatedAt      DATETIME      NOT NULL CONSTRAINT DF_Complaints_CreatedAt DEFAULT (GETDATE()),
    CONSTRAINT PK_Complaints              PRIMARY KEY (ComplaintId),
    CONSTRAINT FK_Complaints_Complainant  FOREIGN KEY (ComplainantId)  REFERENCES dbo.Users (UserId),
    CONSTRAINT FK_Complaints_Books        FOREIGN KEY (TargetBookId)   REFERENCES dbo.Books (BookId),
    CONSTRAINT FK_Complaints_Reviews      FOREIGN KEY (TargetReviewId) REFERENCES dbo.Reviews (ReviewId),
    CONSTRAINT FK_Complaints_TargetUser   FOREIGN KEY (TargetUserId)   REFERENCES dbo.Users (UserId),
    CONSTRAINT FK_Complaints_Statuses     FOREIGN KEY (StatusId)       REFERENCES dbo.RequestStatuses (StatusId),
    CONSTRAINT CK_Complaints_Target CHECK
    (
        TargetBookId IS NOT NULL OR TargetReviewId IS NOT NULL OR TargetUserId IS NOT NULL
    )
);
GO

-- Заявки на получение роли (например, роли «Автор»).
CREATE TABLE dbo.RoleRequests
(
    RoleRequestId   INT      IDENTITY(1,1) NOT NULL,
    UserId          INT      NOT NULL,
    RequestedRoleId INT      NOT NULL,
    StatusId        INT      NOT NULL CONSTRAINT DF_RoleRequests_StatusId  DEFAULT (1),
    CreatedAt       DATETIME NOT NULL CONSTRAINT DF_RoleRequests_CreatedAt DEFAULT (GETDATE()),
    CONSTRAINT PK_RoleRequests           PRIMARY KEY (RoleRequestId),
    CONSTRAINT FK_RoleRequests_Users     FOREIGN KEY (UserId)          REFERENCES dbo.Users (UserId),
    CONSTRAINT FK_RoleRequests_Roles     FOREIGN KEY (RequestedRoleId) REFERENCES dbo.Roles (RoleId),
    CONSTRAINT FK_RoleRequests_Statuses  FOREIGN KEY (StatusId)        REFERENCES dbo.RequestStatuses (StatusId)
);
GO

-- Заявки на разморозку: пользователь оспаривает заморозку своего аккаунта или книги.
CREATE TABLE dbo.UnfreezeRequests
(
    UnfreezeRequestId INT           IDENTITY(1,1) NOT NULL,
    UserId            INT           NOT NULL,       -- кто подал заявку
    TargetUserId      INT           NULL,           -- оспаривается заморозка аккаунта
    TargetBookId      INT           NULL,           -- оспаривается заморозка книги
    Reason            NVARCHAR(500) NOT NULL,
    StatusId          INT           NOT NULL CONSTRAINT DF_UnfreezeRequests_StatusId  DEFAULT (1),
    CreatedAt         DATETIME      NOT NULL CONSTRAINT DF_UnfreezeRequests_CreatedAt DEFAULT (GETDATE()),
    CONSTRAINT PK_UnfreezeRequests             PRIMARY KEY (UnfreezeRequestId),
    CONSTRAINT FK_UnfreezeRequests_Users       FOREIGN KEY (UserId)       REFERENCES dbo.Users (UserId),
    CONSTRAINT FK_UnfreezeRequests_TargetUser  FOREIGN KEY (TargetUserId) REFERENCES dbo.Users (UserId),
    CONSTRAINT FK_UnfreezeRequests_Books       FOREIGN KEY (TargetBookId) REFERENCES dbo.Books (BookId),
    CONSTRAINT FK_UnfreezeRequests_Statuses    FOREIGN KEY (StatusId)     REFERENCES dbo.RequestStatuses (StatusId),
    CONSTRAINT CK_UnfreezeRequests_Target CHECK
    (
        TargetUserId IS NOT NULL OR TargetBookId IS NOT NULL
    )
);
GO

PRINT 'База данных «Читай, Пиши и не спиши»: схема успешно создана.';
GO
