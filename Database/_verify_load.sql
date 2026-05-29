/* Вспомогательный скрипт ПРОВЕРКИ: загружает CSV в БД через BULK INSERT.
   Не является частью отчёта — данные в отчёт импортируются мастером SSMS.
   Очистка таблиц перед загрузкой. */
SET NOCOUNT ON;
DELETE FROM dbo.UnfreezeRequests;
DELETE FROM dbo.RoleRequests;
DELETE FROM dbo.Complaints;
DELETE FROM dbo.ReadingListItems;
DELETE FROM dbo.Reviews;
DELETE FROM dbo.BookGenres;
DELETE FROM dbo.Books;
DELETE FROM dbo.Genres;
DELETE FROM dbo.Users;
DELETE FROM dbo.ReadingStatuses;
DELETE FROM dbo.RequestStatuses;
DELETE FROM dbo.Roles;
GO

DECLARE @dir NVARCHAR(400) = N'D:\ProgrammingProjects\VisualStudioProjects\123ISIP_Prokofiev_UP\Database\csv\';

DECLARE @opt NVARCHAR(400) = N'WITH (FORMAT=''CSV'', FIRSTROW=2, FIELDQUOTE=''"'', CODEPAGE=''65001'', KEEPIDENTITY, KEEPNULLS, ROWTERMINATOR=''0x0a'')';
DECLARE @sql NVARCHAR(MAX);

SET @sql = N'BULK INSERT dbo.Roles           FROM ''' + @dir + N'Roles.csv'' '           + @opt + N';
BULK INSERT dbo.RequestStatuses FROM ''' + @dir + N'RequestStatuses.csv'' ' + @opt + N';
BULK INSERT dbo.ReadingStatuses FROM ''' + @dir + N'ReadingStatuses.csv'' ' + @opt + N';
BULK INSERT dbo.Users           FROM ''' + @dir + N'Users.csv'' '           + @opt + N';
BULK INSERT dbo.Genres          FROM ''' + @dir + N'Genres.csv'' '          + @opt + N';
BULK INSERT dbo.Books           FROM ''' + @dir + N'Books.csv'' '           + @opt + N';
BULK INSERT dbo.BookGenres      FROM ''' + @dir + N'BookGenres.csv'' '      + @opt + N';
BULK INSERT dbo.Reviews         FROM ''' + @dir + N'Reviews.csv'' '         + @opt + N';
BULK INSERT dbo.ReadingListItems FROM ''' + @dir + N'ReadingListItems.csv'' ' + @opt + N';
BULK INSERT dbo.Complaints      FROM ''' + @dir + N'Complaints.csv'' '      + @opt + N';
BULK INSERT dbo.RoleRequests    FROM ''' + @dir + N'RoleRequests.csv'' '    + @opt + N';
BULK INSERT dbo.UnfreezeRequests FROM ''' + @dir + N'UnfreezeRequests.csv'' ' + @opt + N';';
EXEC sp_executesql @sql;
GO

SELECT 'Roles' AS TableName, COUNT(*) AS Cnt FROM dbo.Roles
UNION ALL SELECT 'RequestStatuses', COUNT(*) FROM dbo.RequestStatuses
UNION ALL SELECT 'ReadingStatuses', COUNT(*) FROM dbo.ReadingStatuses
UNION ALL SELECT 'Users', COUNT(*) FROM dbo.Users
UNION ALL SELECT 'Genres', COUNT(*) FROM dbo.Genres
UNION ALL SELECT 'Books', COUNT(*) FROM dbo.Books
UNION ALL SELECT 'BookGenres', COUNT(*) FROM dbo.BookGenres
UNION ALL SELECT 'Reviews', COUNT(*) FROM dbo.Reviews
UNION ALL SELECT 'ReadingListItems', COUNT(*) FROM dbo.ReadingListItems
UNION ALL SELECT 'Complaints', COUNT(*) FROM dbo.Complaints
UNION ALL SELECT 'RoleRequests', COUNT(*) FROM dbo.RoleRequests
UNION ALL SELECT 'UnfreezeRequests', COUNT(*) FROM dbo.UnfreezeRequests;
GO
