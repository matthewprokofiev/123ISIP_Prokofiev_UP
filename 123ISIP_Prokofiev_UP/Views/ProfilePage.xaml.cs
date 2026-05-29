using System.Windows;
using System.Windows.Controls;
using _123ISIP_Prokofiev_UP.Data;
using _123ISIP_Prokofiev_UP.Models;
using _123ISIP_Prokofiev_UP.Services;

namespace _123ISIP_Prokofiev_UP.Views
{
    /// <summary>Профиль пользователя: данные, заморозка, заявка на роль, свои отзывы.</summary>
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            Loaded += (s, e) => LoadProfile();
        }

        private void LoadProfile()
        {
            // Перечитываем пользователя из БД (роль/заморозка могли измениться).
            var fresh = DataService.GetUserById(Session.CurrentUser.Id);
            if (fresh != null) Session.CurrentUser = fresh;
            var u = Session.CurrentUser;

            NameText.Text = u.DisplayName;
            LoginText.Text = "Логин: " + u.Login;
            EmailText.Text = "Email: " + u.Email;
            RoleText.Text = u.RoleName;

            FrozenPanel.Visibility = u.IsFrozen ? Visibility.Visible : Visibility.Collapsed;

            // Заявку на автора может подать читатель, который ещё не автор/админ.
            bool canRequest = u.RoleId == DataService.RoleReader;
            RoleRequestPanel.Visibility = canRequest ? Visibility.Visible : Visibility.Collapsed;
            if (canRequest)
            {
                bool pending = DataService.HasPendingRoleRequest(u.Id);
                RoleRequestBtn.Visibility = pending ? Visibility.Collapsed : Visibility.Visible;
                RoleRequestPending.Visibility = pending ? Visibility.Visible : Visibility.Collapsed;
            }

            // Обновим видимость пунктов меню (роль/заморозка).
            MainWindow.Current?.ApplyRoleVisibility();

            var reviews = DataService.GetReviewsByUser(u.Id);
            ReviewsItems.ItemsSource = reviews;
            NoReviewsText.Visibility = reviews.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ContestFreeze_Click(object sender, RoutedEventArgs e)
        {
            string reason = InputDialog.Show(Window.GetWindow(this), "Оспорить заморозку",
                "Опишите, почему заморозку вашего аккаунта следует отменить:");
            if (reason == null) return;
            DataService.AddUnfreezeRequest(Session.CurrentUser.Id, Session.CurrentUser.Id, null, reason);
            MessageBox.Show("Заявка на разморозку аккаунта отправлена администрации.",
                "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RequestAuthor_Click(object sender, RoutedEventArgs e)
        {
            DataService.AddRoleRequest(Session.CurrentUser.Id, DataService.RoleAuthor);
            MessageBox.Show("Заявка на роль Автора отправлена. Ожидайте решения администратора.",
                "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadProfile();
        }

        private void Review_Open(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is Review review)
                MainWindow.Current.Navigate(new BookPage(review.BookId));
        }
    }
}
