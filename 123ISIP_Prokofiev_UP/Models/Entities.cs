using System;

namespace _123ISIP_Prokofiev_UP.Models
{

    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class RequestStatus
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ReadingStatus
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsFrozen { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool IsAdmin => RoleName == "Администратор";
        public bool IsAuthor => RoleName == "Автор";
    }

    public class Genre
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public override string ToString() => Name;
    }

    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CoverPath { get; set; }
        public string Content { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public bool IsFrozen { get; set; }
        public DateTime CreatedAt { get; set; }

        public double AvgRating { get; set; }
        public int ReviewsCount { get; set; }
        public string GenresText { get; set; }

        public string AvgRatingText => ReviewsCount > 0 ? AvgRating.ToString("0.0") : "—";
    }

    public class Review
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; }
        public int UserId { get; set; }
        public string UserLogin { get; set; }
        public string ReviewText { get; set; }
        public int Rating { get; set; }
        public bool IsFrozen { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ReadingListItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }
        public int ReadingStatusId { get; set; }
        public string StatusName { get; set; }
        public DateTime AddedAt { get; set; }

        public string BookTitle { get; set; }
        public string AuthorName { get; set; }
        public string CoverPath { get; set; }
        public double AvgRating { get; set; }
        public int ReviewsCount { get; set; }
        public string AvgRatingText => ReviewsCount > 0 ? AvgRating.ToString("0.0") : "—";
    }

    public class Complaint
    {
        public int Id { get; set; }
        public int ComplainantId { get; set; }
        public string ComplainantLogin { get; set; }
        public int? TargetBookId { get; set; }
        public int? TargetReviewId { get; set; }
        public int? TargetUserId { get; set; }
        public string Reason { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public DateTime CreatedAt { get; set; }

        public string TargetText { get; set; }
    }

    public class RoleRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserLogin { get; set; }
        public int RequestedRoleId { get; set; }
        public string RequestedRoleName { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UnfreezeRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserLogin { get; set; }
        public int? TargetUserId { get; set; }
        public int? TargetBookId { get; set; }
        public string Reason { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public DateTime CreatedAt { get; set; }

        public string TargetText { get; set; }
    }
}
