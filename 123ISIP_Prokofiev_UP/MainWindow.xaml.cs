using System.Windows;
using System.Windows.Controls;
using _123ISIP_Prokofiev_UP.Services;
using _123ISIP_Prokofiev_UP.Views;

namespace _123ISIP_Prokofiev_UP
{
    /// <summary>
    /// Главное окно-оболочка: боковое меню (Sidebar) с кнопками-иконками и область
    /// содержимого (Frame), в которую загружаются страницы.
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Current { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            Current = this;
            ApplyRoleVisibility();
            // Каталог книг открыт по умолчанию.
            Navigate(new CatalogPage());
        }

        /// <summary>Показывает/скрывает пункты меню в зависимости от роли и статуса пользователя.</summary>
        public void ApplyRoleVisibility()
        {
            BtnAdmin.Visibility  = Session.IsAdmin  ? Visibility.Visible : Visibility.Collapsed;
            BtnAuthor.Visibility = Session.IsAuthor ? Visibility.Visible : Visibility.Collapsed;
            BtnFrozen.Visibility = Session.IsFrozen ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>Загружает страницу в область содержимого.</summary>
        public void Navigate(Page page)
        {
            ContentFrame.Navigate(page);
        }

        private void Catalog_Click(object sender, RoutedEventArgs e) => Navigate(new CatalogPage());
        private void Lists_Click(object sender, RoutedEventArgs e)   => Navigate(new ReadingListsPage());
        private void Admin_Click(object sender, RoutedEventArgs e)   => Navigate(new AdminPage());
        private void Author_Click(object sender, RoutedEventArgs e)  => Navigate(new AuthorPage());
        private void Profile_Click(object sender, RoutedEventArgs e) => Navigate(new ProfilePage());
        private void Frozen_Click(object sender, RoutedEventArgs e)  => Navigate(new ProfilePage());

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Session.Clear();
            var login = new LoginWindow();
            Application.Current.MainWindow = login;
            login.Show();
            Close();
        }
    }
}
