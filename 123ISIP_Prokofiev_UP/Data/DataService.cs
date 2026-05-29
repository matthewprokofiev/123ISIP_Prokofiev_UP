using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using _123ISIP_Prokofiev_UP.Models;

namespace _123ISIP_Prokofiev_UP.Data
{
    /// <summary>
    /// Единая точка доступа к данным приложения. Все обращения к БД ReadWriteDB
    /// выполняются через ADO.NET (System.Data.SqlClient).
    /// </summary>
    public static class DataService
    {
        // ---- Идентификаторы справочников (соответствуют seed-данным) ----
        public const int StatusPending  = 1; // На рассмотрении
        public const int StatusAccepted = 2; // Принято
        public const int StatusRejected = 3; // Отклонено

        public const int RoleReader = 1; // Читатель
        public const int RoleAuthor = 2; // Автор
        public const int RoleAdmin  = 3; // Администратор

        #region Вспомогательные методы чтения

        private static int? GetNullableInt(SqlDataReader r, string col)
        {
            int i = r.GetOrdinal(col);
            return r.IsDBNull(i) ? (int?)null : r.GetInt32(i);
        }

        private static string GetString(SqlDataReader r, string col)
        {
            int i = r.GetOrdinal(col);
            return r.IsDBNull(i) ? null : r.GetString(i);
        }

        private static double GetDouble(SqlDataReader r, string col)
        {
            int i = r.GetOrdinal(col);
            return r.IsDBNull(i) ? 0 : Convert.ToDouble(r.GetValue(i));
        }

        #endregion

        #region Аутентификация и пользователи

        private const string UserSelect =
            "SELECT u.UserId, u.Login, u.Email, u.DisplayName, u.RoleId, r.RoleName, u.IsFrozen, u.CreatedAt " +
            "FROM dbo.Users u JOIN dbo.Roles r ON r.RoleId = u.RoleId ";

        private static User MapUser(SqlDataReader r) => new User
        {
            Id = (int)r["UserId"],
            Login = (string)r["Login"],
            Email = (string)r["Email"],
            DisplayName = (string)r["DisplayName"],
            RoleId = (int)r["RoleId"],
            RoleName = (string)r["RoleName"],
            IsFrozen = (bool)r["IsFrozen"],
            CreatedAt = (DateTime)r["CreatedAt"]
        };

        /// <summary>Проверка логина и хэша пароля. Возвращает пользователя или null.</summary>
        public static User Authenticate(string login, string passwordHash)
        {
            using (var c = Db.Open())
            using (var cmd = new SqlCommand(UserSelect + "WHERE u.Login = @login AND u.PasswordHash = @ph", c))
            {
                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@ph", passwordHash);
                using (var r = cmd.ExecuteReader())
                    return r.Read() ? MapUser(r) : null;
            }
        }

        public static User GetUserById(int id)
        {
            using (var c = Db.Open())
            using (var cmd = new SqlCommand(UserSelect + "WHERE u.UserId = @id", c))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using (var r = cmd.ExecuteReader())
                    return r.Read() ? MapUser(r) : null;
            }
        }

        public static bool LoginExists(string login)
        {
            using (var c = Db.Open())
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Users WHERE Login = @login", c))
            {
                cmd.Parameters.AddWithValue("@login", login);
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        public static bool EmailExists(string email)
        {
            using (var c = Db.Open())
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Users WHERE Email = @email", c))
            {
                cmd.Parameters.AddWithValue("@email", email);
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        /// <summary>Регистрация нового пользователя с ролью «Читатель».</summary>
        public static User Register(string login, string passwordHash, string email, string displayName)
        {
            using (var c = Db.Open())
            using (var cmd = new SqlCommand(
                "INSERT INTO dbo.Users (Login, PasswordHash, Email, DisplayName, RoleId) " +
                "VALUES (@login, @ph, @email, @name, @role); SELECT CAST(SCOPE_IDENTITY() AS INT);", c))
            {
                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@ph", passwordHash);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@name", displayName);
                cmd.Parameters.AddWithValue("@role", RoleReader);
                int newId = (int)cmd.ExecuteScalar();
                return GetUserById(newId);
            }
        }

        public static List<User> GetAllUsers()
        {
            var list = new List<User>();
            using (var c = Db.Open())
            using (var cmd = new SqlCommand(UserSelect + "ORDER BY u.Login", c))
            using (var r = cmd.ExecuteReader())
                while (r.Read()) list.Add(MapUser(r));
            return list;
        }

        public static List<User> GetFrozenUsers()
        {
            var list = new List<User>();
            using (var c = Db.Open())
            using (var cmd = new SqlCommand(UserSelect + "WHERE u.IsFrozen = 1 ORDER BY u.Login", c))
            using (var r = cmd.ExecuteReader())
                while (r.Read()) list.Add(MapUser(r));
            return list;
        }

        public static void SetUserRole(int userId, int roleId)
        {
            using (var c = Db.Open())
            using (var cmd = new SqlCommand("UPDATE dbo.Users SET RoleId = @role WHERE UserId = @id", c))
            {
                cmd.Parameters.AddWithValue("@role", roleId);
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void SetUserPassword(int userId, string passwordHash)
        {
            using (var c = Db.Open())
            using (var cmd = new SqlCommand("UPDATE dbo.Users SET PasswordHash = @ph WHERE UserId = @id", c))
            {
                cmd.Parameters.AddWithValue("@ph", passwordHash);
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void SetUserFrozen(int userId, bool frozen)
        {
            using (var c = Db.Open())
            using (var cmd = new SqlCommand("UPDATE dbo.Users SET IsFrozen = @f WHERE UserId = @id", c))
            {
                cmd.Parameters.AddWithValue("@f", frozen);
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        #region Справочники

        public static List<Role> GetRoles()
        {
            var list = new List<Role>();
            using (var c = Db.Open())
            using (var cmd = new SqlCommand("SELECT RoleId, RoleName FROM dbo.Roles ORDER BY RoleId", c))
            using (var r = cmd.ExecuteReader())
                while (r.Read()) list.Add(new Role { Id = (int)r["RoleId"], Name = (string)r["RoleName"] });
            return list;
        }

        public static List<Genre> GetGenres()
        {
            var list = new List<Genre>();
            using (var c = Db.Open())
            using (var cmd = new SqlCommand("SELECT GenreId, GenreName, Description FROM dbo.Genres ORDER BY GenreName", c))
            using (var r = cmd.ExecuteReader())
                while (r.Read())
                    list.Add(new Genre { Id = (int)r["GenreId"], Name = (string)r["GenreName"], Description = GetString(r, "Description") });
            return list;
        }

        public static List<ReadingStatus> GetReadingStatuses()
        {
            var list = new List<ReadingStatus>();
            using (var c = Db.Open())
            using (var cmd = new SqlCommand("SELECT ReadingStatusId, StatusName FROM dbo.ReadingStatuses ORDER BY ReadingStatusId", c))
            using (var r = cmd.ExecuteReader())
                while (r.Read()) list.Add(new ReadingStatus { Id = (int)r["ReadingStatusId"], Name = (string)r["StatusName"] });
            return list;
        }

        #endregion

        #region Книги

        // Базовый SELECT для карточек книг (без текста книги).
        private const string BookSelect =
            "SELECT b.BookId, b.Title, b.Description, b.CoverPath, b.AuthorId, u.DisplayName AS AuthorName, " +
            "       b.IsFrozen, b.CreatedAt, " +
            "       ISNULL(rv.AvgRating, 0) AS AvgRating, ISNULL(rv.Cnt, 0) AS ReviewsCount, " +
            "       ISNULL(g.GenresText, '') AS GenresText " +
            "FROM dbo.Books b " +
            "JOIN dbo.Users u ON u.UserId = b.AuthorId " +
            "OUTER APPLY (SELECT AVG(CAST(Rating AS float)) AS AvgRating, COUNT(*) AS Cnt " +
            "             FROM dbo.Reviews r WHERE r.BookId = b.BookId AND r.IsFrozen = 0) rv " +
            "OUTER APPLY (SELECT STRING_AGG(gn.GenreName, ', ') AS GenresText " +
            "             FROM dbo.BookGenres bg JOIN dbo.Genres gn ON gn.GenreId = bg.GenreId " +
            "             WHERE bg.BookId = b.BookId) g ";

        private static Book MapBook(SqlDataReader r, bool withContent = false) => new Book
        {
            Id = (int)r["BookId"],
            Title = (string)r["Title"],
            Description = GetString(r, "Description"),
            CoverPath = GetString(r, "CoverPath"),
            Content = withContent ? GetString(r, "Content") : null,
            AuthorId = (int)r["AuthorId"],
            AuthorName = (string)r["AuthorName"],
            IsFrozen = (bool)r["IsFrozen"],
            CreatedAt = (DateTime)r["CreatedAt"],
            AvgRating = GetDouble(r, "AvgRating"),
            ReviewsCount = (int)r["ReviewsCount"],
            GenresText = (string)r["GenresText"]
        };

        /// <summary>
        /// Каталог книг с поиском (название/автор), фильтром по жанру и сортировкой.
        /// </summary>
        /// <param name="sort">"title" — по названию, "rating" — по оценке (убыв.).</param>
        public static List<Book> GetBooks(string search = null, int? genreId = null, string sort = "title", bool includeFrozen = false)
        {
            var sql = BookSelect + "WHERE 1 = 1 ";
            if (!includeFrozen) sql += "AND b.IsFrozen = 0 ";
            if (!string.IsNullOrWhiteSpace(search)) sql += "AND (b.Title LIKE @s OR u.DisplayName LIKE @s) ";
            if (genreId.HasValue) sql += "AND EXISTS (SELECT 1 FROM dbo.BookGenres bg WHERE bg.BookId = b.BookId AND bg.GenreId = @g) ";
            sql += sort == "rating" ? "ORDER BY AvgRating DESC, b.Title" : "ORDER BY b.Title";

            var list = new List<Book>();
            using (var c = Db.Open())
            using (var cmd = new SqlCommand(sql, c))
            {
                if (!string.IsNullOrWhiteSpace(search)) cmd.Parameters.AddWithValue("@s", "%" + search.Trim() + "%");
                if (genreId.HasValue) cmd.Parameters.AddWithValue("@g", genreId.Value);
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(MapBook(r));
            }
            return list;
        }

        public static Book GetBookById(int bookId)
        {
            var sql = BookSelect.Replace("b.CoverPath,", "b.CoverPath, b.Content,") + "WHERE b.BookId = @id";
            using (var c = Db.Open())
            using (var cmd = new SqlCommand(sql, c))
            {
                cmd.Parameters.AddWithValue("@id", bookId);
                using (var r = cmd.ExecuteReader())
                    return r.Read() ? MapBook(r, withContent: true) : null;
            }
        }

        public static List<Book> GetBooksByAuthor(int authorId, bool includeFrozen = true)
        {
            var sql = BookSelect + "WHERE b.AuthorId = @author ";
            if (!includeFrozen) sql += "AND b.IsFrozen = 0 ";
            sql += "ORDER BY b.CreatedAt DESC";
            var list = new List<Book>();
            using (var c = Db.Open())
            using (var cmd = new SqlCommand(sql, c))
            {
                cmd.Parameters.AddWithValue("@author", authorId);
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(MapBook(r));
            }
            return list;
        }

        public static List<Book> GetFrozenBooks()
        {
            var list = new List<Book>();
            using (var c = Db.Open())
            using (var cmd = new SqlCommand(BookSelect + "WHERE b.IsFrozen = 1 ORDER BY b.Title", c))
            using (var r = cmd.ExecuteReader())
                while (r.Read()) list.Add(MapBook(r));
            return list;
        }

        public static List<Genre> GetGenresForBook(int bookId)
        {
            var list = new List<Genre>();
            using (var c = Db.Open())
            using (var cmd = new SqlCommand(
                "SELECT g.GenreId, g.GenreName, g.Description FROM dbo.BookGenres bg " +
                "JOIN dbo.Genres g ON g.GenreId = bg.GenreId WHERE bg.BookId = @id ORDER BY g.GenreName", c))
            {
                cmd.Parameters.AddWithValue("@id", bookId);
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new Genre { Id = (int)r["GenreId"], Name = (string)r["GenreName"], Description = GetString(r, "Description") });
            }
            return list;
        }

        public static int AddBook(string title, string description, string coverPath, string content, int authorId, IEnumerable<int> genreIds)
        {
            using (var c = Db.Open())
            {
                int bookId;
                using (var cmd = new SqlCommand(
                    "INSERT INTO dbo.Books (Title, Description, CoverPath, Content, AuthorId) " +
                    "VALUES (@t, @d, @cp, @ct, @a); SELECT CAST(SCOPE_IDENTITY() AS INT);", c))
                {
                    cmd.Parameters.AddWithValue("@t", title);
                    cmd.Parameters.AddWithValue("@d", (object)description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@cp", (object)coverPath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ct", (object)content ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@a", authorId);
                    bookId = (int)cmd.ExecuteScalar();
                }
                SetBookGenres(c, bookId, genreIds);
                return bookId;
            }
        }

        public static void UpdateBook(int bookId, string title, string description, string coverPath, string content, IEnumerable<int> genreIds)
        {
            using (var c = Db.Open())
            {
                using (var cmd = new SqlCommand(
                    "UPDATE dbo.Books SET Title = @t, Description = @d, CoverPath = @cp, Content = @ct WHERE BookId = @id", c))
                {
                    cmd.Parameters.AddWithValue("@t", title);
                    cmd.Parameters.AddWithValue("@d", (object)description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@cp", (object)coverPath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ct", (object)content ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@id", bookId);
                    cmd.ExecuteNonQuery();
                }
                SetBookGenres(c, bookId, genreIds);
            }
        }

        private static void SetBookGenres(SqlConnection c, int bookId, IEnumerable<int> genreIds)
        {
            using (var del = new SqlCommand("DELETE FROM dbo.BookGenres WHERE BookId = @id", c))
            {
                del.Parameters.AddWithValue("@id", bookId);
                del.ExecuteNonQuery();
            }
            if (genreIds == null) return;
            foreach (int gid in genreIds)
                using (var ins = new SqlCommand("INSERT INTO dbo.BookGenres (BookId, GenreId) VALUES (@b, @g)", c))
                {
                    ins.Parameters.AddWithValue("@b", bookId);
                    ins.Parameters.AddWithValue("@g", gid);
                    ins.ExecuteNonQuery();
                }
        }

        public static void SetBookFrozen(int bookId, bool frozen)
        {
            using (var c = Db.Open())
            using (var cmd = new SqlCommand("UPDATE dbo.Books SET IsFrozen = @f WHERE BookId = @id", c))
            {
                cmd.Parameters.AddWithValue("@f", frozen);
                cmd.Parameters.AddWithValue("@id", bookId);
                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        #region Отзывы

        private const string ReviewSelect =
            "SELECT rv.ReviewId, rv.BookId, b.Title AS BookTitle, rv.UserId, u.Login AS UserLogin, " +
            "       rv.ReviewText, rv.Rating, rv.IsFrozen, rv.CreatedAt " +
            "FROM dbo.Reviews rv " +
            "JOIN dbo.Books b ON b.BookId = rv.BookId " +
            "JOIN dbo.Users u ON u.UserId = rv.UserId ";

        private static Review MapReview(SqlDataReader r) => new Review
        {
            Id = (int)r["ReviewId"],
            BookId = (int)r["BookId"],
            BookTitle = (string)r["BookTitle"],
            UserId = (int)r["UserId"],
            UserLogin = (string)r["UserLogin"],
            ReviewText = GetString(r, "ReviewText"),
            Rating = (int)r["Rating"],
            IsFrozen = (bool)r["IsFrozen"],
            CreatedAt = (DateTime)r["CreatedAt"]
        };

        public static List<Review> GetReviewsForBook(int bookId, bool includeFrozen = false)
        {
            var sql = ReviewSelect + "WHERE rv.BookId = @id ";
            if (!includeFrozen) sql += "AND rv.IsFrozen = 0 ";
            sql += "ORDER BY rv.CreatedAt DESC";
            var list = new List<Review>();
            using (var c = Db.Open())
            using (var cmd = new SqlCommand(sql, c))
            {
                cmd.Parameters.AddWithValue("@id", bookId);
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(MapReview(r));
            }
            return list;
        }

        public static List<Review> GetReviewsByUser(int userId)
        {
            var list = new List<Review>();
            using (var c = Db.Open())
            using (var cmd = new SqlCommand(ReviewSelect + "WHERE rv.UserId = @id ORDER BY rv.CreatedAt DESC", c))
            {
                cmd.Parameters.AddWithValue("@id", userId);
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(MapReview(r));
            }
            return list;
        }

        public static List<Review> GetFrozenReviews()
        {
            var list = new List<Review>();
            using (var c = Db.Open())
            using (var cmd = new SqlCommand(ReviewSelect + "WHERE rv.IsFrozen = 1 ORDER BY rv.CreatedAt DESC", c))
            using (var r = cmd.ExecuteReader())
                while (r.Read()) list.Add(MapReview(r));
            return list;
        }

        public static bool UserHasReviewed(int bookId, int userId)
        {
            using (var c = Db.Open())
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Reviews WHERE BookId = @b AND UserId = @u", c))
            {
                cmd.Parameters.AddWithValue("@b", bookId);
                cmd.Parameters.AddWithValue("@u", userId);
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        /// <summary>Добавляет отзыв; если пользователь уже оставлял отзыв на книгу — обновляет его.</summary>
        public static void AddOrUpdateReview(int bookId, int userId, string text, int rating)
        {
            using (var c = Db.Open())
            using (var cmd = new SqlCommand(
                "IF EXISTS (SELECT 1 FROM dbo.Reviews WHERE BookId = @b AND UserId = @u) " +
                "  UPDATE dbo.Reviews SET ReviewText = @t, Rating = @r, CreatedAt = GETDATE() WHERE BookId = @b AND UserId = @u; " +
                "ELSE " +
                "  INSERT INTO dbo.Reviews (BookId, UserId, ReviewText, Rating) VALUES (@b, @u, @t, @r);", c))
            {
                cmd.Parameters.AddWithValue("@b", bookId);
                cmd.Parameters.AddWithValue("@u", userId);
                cmd.Parameters.AddWithValue("@t", (object)text ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@r", rating);
                cmd.ExecuteNonQuery();
            }
        }

        public static void SetReviewFrozen(int reviewId, bool frozen)
        {
            using (var c = Db.Open())
            using (var cmd = new SqlCommand("UPDATE dbo.Reviews SET IsFrozen = @f WHERE ReviewId = @id", c))
            {
                cmd.Parameters.AddWithValue("@f", frozen);
                cmd.Parameters.AddWithValue("@id", reviewId);
                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        #region Списки чтения

        public static List<ReadingListItem> GetReadingList(int userId, int statusId, string search = null, int? genreId = null, string sort = "title")
        {
            var sql =
                "SELECT rli.ReadingListItemId, rli.UserId, rli.BookId, rli.ReadingStatusId, rs.StatusName, rli.AddedAt, " +
                "       b.Title AS BookTitle, b.CoverPath, u.DisplayName AS AuthorName, " +
                "       ISNULL(rv.AvgRating, 0) AS AvgRating, ISNULL(rv.Cnt, 0) AS ReviewsCount " +
                "FROM dbo.ReadingListItems rli " +
                "JOIN dbo.ReadingStatuses rs ON rs.ReadingStatusId = rli.ReadingStatusId " +
                "JOIN dbo.Books b ON b.BookId = rli.BookId " +
                "JOIN dbo.Users u ON u.UserId = b.AuthorId " +
                "OUTER APPLY (SELECT AVG(CAST(Rating AS float)) AS AvgRating, COUNT(*) AS Cnt " +
                "             FROM dbo.Reviews r WHERE r.BookId = b.BookId AND r.IsFrozen = 0) rv " +
                "WHERE rli.UserId = @u AND rli.ReadingStatusId = @s ";
            if (!string.IsNullOrWhiteSpace(search)) sql += "AND (b.Title LIKE @search OR u.DisplayName LIKE @search) ";
            if (genreId.HasValue) sql += "AND EXISTS (SELECT 1 FROM dbo.BookGenres bg WHERE bg.BookId = b.BookId AND bg.GenreId = @g) ";
            sql += sort == "rating" ? "ORDER BY AvgRating DESC, b.Title" : "ORDER BY b.Title";

            var list = new List<ReadingListItem>();
            using (var c = Db.Open())
            using (var cmd = new SqlCommand(sql, c))
            {
                cmd.Parameters.AddWithValue("@u", userId);
                cmd.Parameters.AddWithValue("@s", statusId);
                if (!string.IsNullOrWhiteSpace(search)) cmd.Parameters.AddWithValue("@search", "%" + search.Trim() + "%");
                if (genreId.HasValue) cmd.Parameters.AddWithValue("@g", genreId.Value);
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new ReadingListItem
                        {
                            Id = (int)r["ReadingListItemId"],
                            UserId = (int)r["UserId"],
                            BookId = (int)r["BookId"],
                            ReadingStatusId = (int)r["ReadingStatusId"],
                            StatusName = (string)r["StatusName"],
                            AddedAt = (DateTime)r["AddedAt"],
                            BookTitle = (string)r["BookTitle"],
                            CoverPath = GetString(r, "CoverPath"),
                            AuthorName = (string)r["AuthorName"],
                            AvgRating = GetDouble(r, "AvgRating"),
                            ReviewsCount = (int)r["ReviewsCount"]
                        });
            }
            return list;
        }

        /// <summary>Возвращает раздел, в котором у пользователя находится книга (или null).</summary>
        public static int? GetReadingStatusForBook(int userId, int bookId)
        {
            using (var c = Db.Open())
            using (var cmd = new SqlCommand("SELECT ReadingStatusId FROM dbo.ReadingListItems WHERE UserId = @u AND BookId = @b", c))
            {
                cmd.Parameters.AddWithValue("@u", userId);
                cmd.Parameters.AddWithValue("@b", bookId);
                object res = cmd.ExecuteScalar();
                return res == null ? (int?)null : (int)res;
            }
        }

        /// <summary>Добавляет книгу в раздел списка чтения или перемещает её в другой раздел.</summary>
        public static void SetReadingStatus(int userId, int bookId, int statusId)
        {
            using (var c = Db.Open())
            using (var cmd = new SqlCommand(
                "IF EXISTS (SELECT 1 FROM dbo.ReadingListItems WHERE UserId = @u AND BookId = @b) " +
                "  UPDATE dbo.ReadingListItems SET ReadingStatusId = @s, AddedAt = GETDATE() WHERE UserId = @u AND BookId = @b; " +
                "ELSE " +
                "  INSERT INTO dbo.ReadingListItems (UserId, BookId, ReadingStatusId) VALUES (@u, @b, @s);", c))
            {
                cmd.Parameters.AddWithValue("@u", userId);
                cmd.Parameters.AddWithValue("@b", bookId);
                cmd.Parameters.AddWithValue("@s", statusId);
                cmd.ExecuteNonQuery();
            }
        }

        public static void RemoveFromReadingList(int userId, int bookId)
        {
            using (var c = Db.Open())
            using (var cmd = new SqlCommand("DELETE FROM dbo.ReadingListItems WHERE UserId = @u AND BookId = @b", c))
            {
                cmd.Parameters.AddWithValue("@u", userId);
                cmd.Parameters.AddWithValue("@b", bookId);
                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        #region Жалобы

        public static void AddComplaint(int complainantId, int? targetBookId, int? targetReviewId, int? targetUserId, string reason)
        {
            using (var c = Db.Open())
            using (var cmd = new SqlCommand(
                "INSERT INTO dbo.Complaints (ComplainantId, TargetBookId, TargetReviewId, TargetUserId, Reason) " +
                "VALUES (@c, @tb, @tr, @tu, @reason)", c))
            {
                cmd.Parameters.AddWithValue("@c", complainantId);
                cmd.Parameters.AddWithValue("@tb", (object)targetBookId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@tr", (object)targetReviewId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@tu", (object)targetUserId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@reason", reason);
                cmd.ExecuteNonQuery();
            }
        }

        public static List<Complaint> GetComplaints(int? statusId = null)
        {
            var sql =
                "SELECT cp.ComplaintId, cp.ComplainantId, uc.Login AS ComplainantLogin, " +
                "       cp.TargetBookId, cp.TargetReviewId, cp.TargetUserId, cp.Reason, " +
                "       cp.StatusId, st.StatusName, cp.CreatedAt, " +
                "       b.Title AS BookTitle, ut.Login AS TargetUserLogin " +
                "FROM dbo.Complaints cp " +
                "JOIN dbo.Users uc ON uc.UserId = cp.ComplainantId " +
                "JOIN dbo.RequestStatuses st ON st.StatusId = cp.StatusId " +
                "LEFT JOIN dbo.Books b ON b.BookId = cp.TargetBookId " +
                "LEFT JOIN dbo.Users ut ON ut.UserId = cp.TargetUserId ";
            if (statusId.HasValue) sql += "WHERE cp.StatusId = @st ";
            sql += "ORDER BY cp.CreatedAt DESC";

            var list = new List<Complaint>();
            using (var c = Db.Open())
            using (var cmd = new SqlCommand(sql, c))
            {
                if (statusId.HasValue) cmd.Parameters.AddWithValue("@st", statusId.Value);
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                    {
                        var item = new Complaint
                        {
                            Id = (int)r["ComplaintId"],
                            ComplainantId = (int)r["ComplainantId"],
                            ComplainantLogin = (string)r["ComplainantLogin"],
                            TargetBookId = GetNullableInt(r, "TargetBookId"),
                            TargetReviewId = GetNullableInt(r, "TargetReviewId"),
                            TargetUserId = GetNullableInt(r, "TargetUserId"),
                            Reason = GetString(r, "Reason"),
                            StatusId = (int)r["StatusId"],
                            StatusName = (string)r["StatusName"],
                            CreatedAt = (DateTime)r["CreatedAt"]
                        };
                        if (item.TargetBookId.HasValue) item.TargetText = "Книга: " + GetString(r, "BookTitle");
                        else if (item.TargetReviewId.HasValue) item.TargetText = "Отзыв #" + item.TargetReviewId.Value;
                        else if (item.TargetUserId.HasValue) item.TargetText = "Пользователь: " + GetString(r, "TargetUserLogin");
                        list.Add(item);
                    }
            }
            return list;
        }

        public static void SetComplaintStatus(int complaintId, int statusId)
        {
            using (var c = Db.Open())
            using (var cmd = new SqlCommand("UPDATE dbo.Complaints SET StatusId = @s WHERE ComplaintId = @id", c))
            {
                cmd.Parameters.AddWithValue("@s", statusId);
                cmd.Parameters.AddWithValue("@id", complaintId);
                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        #region Заявки на роль

        public static bool HasPendingRoleRequest(int userId)
        {
            using (var c = Db.Open())
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.RoleRequests WHERE UserId = @u AND StatusId = @s", c))
            {
                cmd.Parameters.AddWithValue("@u", userId);
                cmd.Parameters.AddWithValue("@s", StatusPending);
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        public static void AddRoleRequest(int userId, int requestedRoleId)
        {
            using (var c = Db.Open())
            using (var cmd = new SqlCommand(
                "INSERT INTO dbo.RoleRequests (UserId, RequestedRoleId) VALUES (@u, @r)", c))
            {
                cmd.Parameters.AddWithValue("@u", userId);
                cmd.Parameters.AddWithValue("@r", requestedRoleId);
                cmd.ExecuteNonQuery();
            }
        }

        public static List<RoleRequest> GetRoleRequests(int? statusId = null)
        {
            var sql =
                "SELECT rr.RoleRequestId, rr.UserId, u.Login AS UserLogin, rr.RequestedRoleId, ro.RoleName AS RequestedRoleName, " +
                "       rr.StatusId, st.StatusName, rr.CreatedAt " +
                "FROM dbo.RoleRequests rr " +
                "JOIN dbo.Users u ON u.UserId = rr.UserId " +
                "JOIN dbo.Roles ro ON ro.RoleId = rr.RequestedRoleId " +
                "JOIN dbo.RequestStatuses st ON st.StatusId = rr.StatusId ";
            if (statusId.HasValue) sql += "WHERE rr.StatusId = @st ";
            sql += "ORDER BY rr.CreatedAt DESC";

            var list = new List<RoleRequest>();
            using (var c = Db.Open())
            using (var cmd = new SqlCommand(sql, c))
            {
                if (statusId.HasValue) cmd.Parameters.AddWithValue("@st", statusId.Value);
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new RoleRequest
                        {
                            Id = (int)r["RoleRequestId"],
                            UserId = (int)r["UserId"],
                            UserLogin = (string)r["UserLogin"],
                            RequestedRoleId = (int)r["RequestedRoleId"],
                            RequestedRoleName = (string)r["RequestedRoleName"],
                            StatusId = (int)r["StatusId"],
                            StatusName = (string)r["StatusName"],
                            CreatedAt = (DateTime)r["CreatedAt"]
                        });
            }
            return list;
        }

        /// <summary>Меняет статус заявки на роль; при принятии назначает пользователю запрошенную роль.</summary>
        public static void SetRoleRequestStatus(int roleRequestId, int statusId)
        {
            using (var c = Db.Open())
            {
                int userId = 0, roleId = 0;
                using (var get = new SqlCommand("SELECT UserId, RequestedRoleId FROM dbo.RoleRequests WHERE RoleRequestId = @id", c))
                {
                    get.Parameters.AddWithValue("@id", roleRequestId);
                    using (var r = get.ExecuteReader())
                        if (r.Read()) { userId = (int)r["UserId"]; roleId = (int)r["RequestedRoleId"]; }
                }
                using (var upd = new SqlCommand("UPDATE dbo.RoleRequests SET StatusId = @s WHERE RoleRequestId = @id", c))
                {
                    upd.Parameters.AddWithValue("@s", statusId);
                    upd.Parameters.AddWithValue("@id", roleRequestId);
                    upd.ExecuteNonQuery();
                }
                if (statusId == StatusAccepted && userId > 0)
                    using (var role = new SqlCommand("UPDATE dbo.Users SET RoleId = @r WHERE UserId = @u", c))
                    {
                        role.Parameters.AddWithValue("@r", roleId);
                        role.Parameters.AddWithValue("@u", userId);
                        role.ExecuteNonQuery();
                    }
            }
        }

        #endregion

        #region Заявки на разморозку

        public static void AddUnfreezeRequest(int userId, int? targetUserId, int? targetBookId, string reason)
        {
            using (var c = Db.Open())
            using (var cmd = new SqlCommand(
                "INSERT INTO dbo.UnfreezeRequests (UserId, TargetUserId, TargetBookId, Reason) " +
                "VALUES (@u, @tu, @tb, @reason)", c))
            {
                cmd.Parameters.AddWithValue("@u", userId);
                cmd.Parameters.AddWithValue("@tu", (object)targetUserId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@tb", (object)targetBookId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@reason", reason);
                cmd.ExecuteNonQuery();
            }
        }

        public static List<UnfreezeRequest> GetUnfreezeRequests(int? statusId = null)
        {
            var sql =
                "SELECT ur.UnfreezeRequestId, ur.UserId, u.Login AS UserLogin, ur.TargetUserId, ur.TargetBookId, " +
                "       ur.Reason, ur.StatusId, st.StatusName, ur.CreatedAt, " +
                "       b.Title AS BookTitle, ut.Login AS TargetUserLogin " +
                "FROM dbo.UnfreezeRequests ur " +
                "JOIN dbo.Users u ON u.UserId = ur.UserId " +
                "JOIN dbo.RequestStatuses st ON st.StatusId = ur.StatusId " +
                "LEFT JOIN dbo.Books b ON b.BookId = ur.TargetBookId " +
                "LEFT JOIN dbo.Users ut ON ut.UserId = ur.TargetUserId ";
            if (statusId.HasValue) sql += "WHERE ur.StatusId = @st ";
            sql += "ORDER BY ur.CreatedAt DESC";

            var list = new List<UnfreezeRequest>();
            using (var c = Db.Open())
            using (var cmd = new SqlCommand(sql, c))
            {
                if (statusId.HasValue) cmd.Parameters.AddWithValue("@st", statusId.Value);
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                    {
                        var item = new UnfreezeRequest
                        {
                            Id = (int)r["UnfreezeRequestId"],
                            UserId = (int)r["UserId"],
                            UserLogin = (string)r["UserLogin"],
                            TargetUserId = GetNullableInt(r, "TargetUserId"),
                            TargetBookId = GetNullableInt(r, "TargetBookId"),
                            Reason = GetString(r, "Reason"),
                            StatusId = (int)r["StatusId"],
                            StatusName = (string)r["StatusName"],
                            CreatedAt = (DateTime)r["CreatedAt"]
                        };
                        if (item.TargetBookId.HasValue) item.TargetText = "Книга: " + GetString(r, "BookTitle");
                        else if (item.TargetUserId.HasValue) item.TargetText = "Аккаунт: " + GetString(r, "TargetUserLogin");
                        list.Add(item);
                    }
            }
            return list;
        }

        /// <summary>Меняет статус заявки на разморозку; при принятии размораживает цель (аккаунт или книгу).</summary>
        public static void SetUnfreezeRequestStatus(int requestId, int statusId)
        {
            using (var c = Db.Open())
            {
                int? targetUserId = null, targetBookId = null;
                using (var get = new SqlCommand("SELECT TargetUserId, TargetBookId FROM dbo.UnfreezeRequests WHERE UnfreezeRequestId = @id", c))
                {
                    get.Parameters.AddWithValue("@id", requestId);
                    using (var r = get.ExecuteReader())
                        if (r.Read())
                        {
                            targetUserId = GetNullableInt(r, "TargetUserId");
                            targetBookId = GetNullableInt(r, "TargetBookId");
                        }
                }
                using (var upd = new SqlCommand("UPDATE dbo.UnfreezeRequests SET StatusId = @s WHERE UnfreezeRequestId = @id", c))
                {
                    upd.Parameters.AddWithValue("@s", statusId);
                    upd.Parameters.AddWithValue("@id", requestId);
                    upd.ExecuteNonQuery();
                }
                if (statusId == StatusAccepted)
                {
                    if (targetUserId.HasValue)
                        using (var f = new SqlCommand("UPDATE dbo.Users SET IsFrozen = 0 WHERE UserId = @id", c))
                        { f.Parameters.AddWithValue("@id", targetUserId.Value); f.ExecuteNonQuery(); }
                    if (targetBookId.HasValue)
                        using (var f = new SqlCommand("UPDATE dbo.Books SET IsFrozen = 0 WHERE BookId = @id", c))
                        { f.Parameters.AddWithValue("@id", targetBookId.Value); f.ExecuteNonQuery(); }
                }
            }
        }

        #endregion
    }
}
