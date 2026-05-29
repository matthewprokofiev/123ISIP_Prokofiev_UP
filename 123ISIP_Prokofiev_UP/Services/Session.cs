using _123ISIP_Prokofiev_UP.Models;

namespace _123ISIP_Prokofiev_UP.Services
{

    public static class Session
    {
        public static User CurrentUser { get; set; }

        public static bool IsAuthenticated => CurrentUser != null;
        public static bool IsAdmin => CurrentUser != null && CurrentUser.IsAdmin;
        public static bool IsAuthor => CurrentUser != null && CurrentUser.IsAuthor;
        public static bool IsFrozen => CurrentUser != null && CurrentUser.IsFrozen;

        public static void Clear() => CurrentUser = null;
    }
}
