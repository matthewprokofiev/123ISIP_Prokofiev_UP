using System;
using System.Windows;
using System.Windows.Controls;
using _123ISIP_Prokofiev_UP.Data;
using _123ISIP_Prokofiev_UP.Models;
using _123ISIP_Prokofiev_UP.Services;

namespace _123ISIP_Prokofiev_UP.Views
{
    /// <summary>
    /// Администрирование: жалобы, заявки на разморозку и роль, замороженные объекты,
    /// управление пользователями (роль, пароль, заморозка).
    /// </summary>
    public partial class AdminPage : Page
    {
        private bool _loaded;

        public AdminPage()
        {
            InitializeComponent();
            Loaded += (s, e) => { _loaded = true; ShowSection(); };
        }

        private void Tab_Checked(object sender, RoutedEventArgs e)
        {
            if (_loaded) ShowSection();
        }

        private void ShowSection()
        {
            PanelComplaints.Visibility = TabComplaints.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            PanelUnfreeze.Visibility   = TabUnfreeze.IsChecked   == true ? Visibility.Visible : Visibility.Collapsed;
            PanelRoles.Visibility      = TabRoles.IsChecked      == true ? Visibility.Visible : Visibility.Collapsed;
            PanelFrozen.Visibility     = TabFrozen.IsChecked     == true ? Visibility.Visible : Visibility.Collapsed;
            PanelUsers.Visibility      = TabUsers.IsChecked      == true ? Visibility.Visible : Visibility.Collapsed;

            try
            {
                if (TabComplaints.IsChecked == true) ComplaintsItems.ItemsSource = DataService.GetComplaints();
                else if (TabUnfreeze.IsChecked == true) UnfreezeItems.ItemsSource = DataService.GetUnfreezeRequests();
                else if (TabRoles.IsChecked == true) RolesItems.ItemsSource = DataService.GetRoleRequests();
                else if (TabFrozen.IsChecked == true)
                {
                    FrozenBooksItems.ItemsSource = DataService.GetFrozenBooks();
                    FrozenUsersItems.ItemsSource = DataService.GetFrozenUsers();
                    FrozenReviewsItems.ItemsSource = DataService.GetFrozenReviews();
                }
                else if (TabUsers.IsChecked == true) UsersItems.ItemsSource = DataService.GetAllUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ---------- Жалобы ----------
        private void ComplaintAccept_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is Complaint c)
            { DataService.SetComplaintStatus(c.Id, DataService.StatusAccepted); ShowSection(); }
        }
        private void ComplaintReject_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is Complaint c)
            { DataService.SetComplaintStatus(c.Id, DataService.StatusRejected); ShowSection(); }
        }

        // ---------- Заявки на разморозку ----------
        private void UnfreezeAccept_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is UnfreezeRequest r)
            { DataService.SetUnfreezeRequestStatus(r.Id, DataService.StatusAccepted); ShowSection(); }
        }
        private void UnfreezeReject_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is UnfreezeRequest r)
            { DataService.SetUnfreezeRequestStatus(r.Id, DataService.StatusRejected); ShowSection(); }
        }

        // ---------- Заявки на роль ----------
        private void RoleAccept_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is RoleRequest r)
            { DataService.SetRoleRequestStatus(r.Id, DataService.StatusAccepted); ShowSection(); }
        }
        private void RoleReject_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is RoleRequest r)
            { DataService.SetRoleRequestStatus(r.Id, DataService.StatusRejected); ShowSection(); }
        }

        // ---------- Замороженные ----------
        private void UnfreezeBook_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is Book b)
            { DataService.SetBookFrozen(b.Id, false); ShowSection(); }
        }
        private void UnfreezeUser_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is User u)
            { DataService.SetUserFrozen(u.Id, false); ShowSection(); }
        }
        private void UnfreezeReview_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is Review rv)
            { DataService.SetReviewFrozen(rv.Id, false); ShowSection(); }
        }

        // ---------- Пользователи ----------
        private void AssignRole_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (!(btn?.DataContext is User user)) return;

            var menu = new ContextMenu();
            foreach (var role in DataService.GetRoles())
            {
                var mi = new MenuItem { Header = role.Name, Tag = role.Id, IsEnabled = role.Id != user.RoleId };
                mi.Click += (s, args) =>
                {
                    DataService.SetUserRole(user.Id, (int)((MenuItem)s).Tag);
                    ShowSection();
                };
                menu.Items.Add(mi);
            }
            menu.PlacementTarget = btn;
            menu.IsOpen = true;
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as FrameworkElement)?.DataContext is User user)) return;
            string pw = InputDialog.Show(Window.GetWindow(this), "Смена пароля",
                "Новый пароль для пользователя «" + user.Login + "»:", "", multiline: false);
            if (pw == null) return;
            DataService.SetUserPassword(user.Id, PasswordHasher.Hash(pw));
            MessageBox.Show("Пароль изменён.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ToggleFreezeUser_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as FrameworkElement)?.DataContext is User user)) return;
            DataService.SetUserFrozen(user.Id, !user.IsFrozen);
            ShowSection();
        }
    }
}
