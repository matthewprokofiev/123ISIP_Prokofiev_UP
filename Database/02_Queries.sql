/* =============================================================================
   Обязательные запросы к БД «Читай, Пиши и не спиши».
   Каждый запрос выполняется отдельно; в отчёт прикладывается скриншот результата.
   ============================================================================= */

/* -----------------------------------------------------------------------------
   Запрос 1. Простая выборка.
   Логин и отображаемое имя всех пользователей с ролью «Автор»,
   отсортированные по логину.
   ----------------------------------------------------------------------------- */
SELECT u.Login, u.DisplayName
FROM dbo.Users AS u
JOIN dbo.Roles AS r ON r.RoleId = u.RoleId
WHERE r.RoleName = N'Автор'
ORDER BY u.Login;
GO

/* -----------------------------------------------------------------------------
   Запрос 2. Внутреннее соединение (INNER JOIN).
   Все отзывы: название книги, логин пользователя, рейтинг, дата создания отзыва.
   ----------------------------------------------------------------------------- */
SELECT b.Title          AS BookTitle,
       u.Login          AS UserLogin,
       rv.Rating        AS Rating,
       rv.CreatedAt     AS ReviewDate
FROM dbo.Reviews AS rv
INNER JOIN dbo.Books AS b ON b.BookId = rv.BookId
INNER JOIN dbo.Users AS u ON u.UserId = rv.UserId
ORDER BY rv.CreatedAt;
GO

/* -----------------------------------------------------------------------------
   Запрос 3. Внешнее соединение (LEFT JOIN).
   Все пользователи и количество книг, добавленных ими в список чтения
   (включая пользователей с пустым списком). Результат: логин, количество книг.
   ----------------------------------------------------------------------------- */
SELECT u.Login,
       COUNT(rli.ReadingListItemId) AS BooksInList
FROM dbo.Users AS u
LEFT JOIN dbo.ReadingListItems AS rli ON rli.UserId = u.UserId
GROUP BY u.Login
ORDER BY BooksInList DESC, u.Login;
GO

/* -----------------------------------------------------------------------------
   Запрос 4. Внешнее соединение (RIGHT JOIN).
   Все книги и количество отзывов на каждую (включая книги без отзывов).
   Результат: название книги, количество отзывов.
   ----------------------------------------------------------------------------- */
SELECT b.Title,
       COUNT(rv.ReviewId) AS ReviewsCount
FROM dbo.Reviews AS rv
RIGHT JOIN dbo.Books AS b ON b.BookId = rv.BookId
GROUP BY b.Title
ORDER BY ReviewsCount DESC, b.Title;
GO

/* -----------------------------------------------------------------------------
   Запрос 5. Агрегация и группировка.
   Для каждого жанра: количество книг этого жанра и средний рейтинг
   (по всем отзывам на книги этого жанра). Сортировка по убыванию среднего рейтинга.
   ----------------------------------------------------------------------------- */
SELECT g.GenreName,
       COUNT(DISTINCT b.BookId) AS BooksCount,
       AVG(CAST(rv.Rating AS DECIMAL(4,2))) AS AvgRating
FROM dbo.Genres AS g
LEFT JOIN dbo.BookGenres AS bg ON bg.GenreId = g.GenreId
LEFT JOIN dbo.Books      AS b  ON b.BookId  = bg.BookId
LEFT JOIN dbo.Reviews    AS rv ON rv.BookId = b.BookId
GROUP BY g.GenreName
ORDER BY AvgRating DESC;
GO

/* -----------------------------------------------------------------------------
   Запрос 6. Подзапрос (EXISTS).
   Пользователи, оставившие хотя бы один отзыв с рейтингом 10. Логин и email.
   ----------------------------------------------------------------------------- */
SELECT u.Login, u.Email
FROM dbo.Users AS u
WHERE EXISTS
(
    SELECT 1
    FROM dbo.Reviews AS rv
    WHERE rv.UserId = u.UserId
      AND rv.Rating = 10
)
ORDER BY u.Login;
GO

/* -----------------------------------------------------------------------------
   Запрос 7. Запрос с вычисляемым полем и сортировкой.
   Топ-5 книг по количеству отзывов (название, количество отзывов).
   При равенстве — сортировка по названию.
   ----------------------------------------------------------------------------- */
SELECT TOP (5)
       b.Title,
       COUNT(rv.ReviewId) AS ReviewsCount
FROM dbo.Books AS b
LEFT JOIN dbo.Reviews AS rv ON rv.BookId = b.BookId
GROUP BY b.Title
ORDER BY ReviewsCount DESC, b.Title;
GO

/* -----------------------------------------------------------------------------
   Запрос 8. Оконная функция (ROW_NUMBER).
   Для каждой книги: название, автор (DisplayName), средний рейтинг и
   порядковый номер книги внутри каждого автора по убыванию среднего рейтинга
   (лучшая книга автора = 1). Если отзывов нет, средний рейтинг = 0.
   ----------------------------------------------------------------------------- */
WITH BookRatings AS
(
    SELECT b.BookId,
           b.Title,
           b.AuthorId,
           COALESCE(AVG(CAST(rv.Rating AS DECIMAL(4,2))), 0) AS AvgRating
    FROM dbo.Books AS b
    LEFT JOIN dbo.Reviews AS rv ON rv.BookId = b.BookId
    GROUP BY b.BookId, b.Title, b.AuthorId
)
SELECT br.Title,
       u.DisplayName AS Author,
       br.AvgRating,
       ROW_NUMBER() OVER (PARTITION BY br.AuthorId ORDER BY br.AvgRating DESC, br.Title) AS RankInAuthor
FROM BookRatings AS br
INNER JOIN dbo.Users AS u ON u.UserId = br.AuthorId
ORDER BY u.DisplayName, RankInAuthor;
GO

/* -----------------------------------------------------------------------------
   Запрос 9. Оконная функция (LAG / LEAD).
   Для каждого пользователя, оставлявшего отзывы: логин, дата отзыва, рейтинг,
   рейтинг предыдущего отзыва (по дате) и рейтинг следующего отзыва.
   Если предыдущего/следующего нет — «нет отзыва».
   ----------------------------------------------------------------------------- */
SELECT u.Login,
       rv.CreatedAt AS ReviewDate,
       rv.Rating    AS CurrentRating,
       COALESCE(CAST(LAG(rv.Rating)  OVER (PARTITION BY rv.UserId ORDER BY rv.CreatedAt) AS NVARCHAR(20)), N'нет отзыва') AS PrevRating,
       COALESCE(CAST(LEAD(rv.Rating) OVER (PARTITION BY rv.UserId ORDER BY rv.CreatedAt) AS NVARCHAR(20)), N'нет отзыва') AS NextRating
FROM dbo.Reviews AS rv
INNER JOIN dbo.Users AS u ON u.UserId = rv.UserId
ORDER BY u.Login, rv.CreatedAt;
GO
