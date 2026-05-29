using System.Windows;
using System.Windows.Controls;
using _123ISIP_Prokofiev_UP.Data;
using _123ISIP_Prokofiev_UP.Models;
using _123ISIP_Prokofiev_UP.Services;

namespace _123ISIP_Prokofiev_UP.Views
{
    /// <summary>Страница автора: список своих книг, добавление, редактирование, оспаривание заморозки.</summary>
    public partial class AuthorPage : Page
    {
        public AuthorPage()
        {
            InitializeComponent();
            Loaded += (s, e) => LoadBooks();
        }

        private void LoadBooks()
        {
            var books = DataService.GetBooksByAuthor(Session.CurrentUser.Id, includeFrozen: true);
            BooksItems.ItemsSource = books;
            EmptyText.Visibility = books.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Current.Navigate(new BookEditPage(null));
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is Book book)
                MainWindow.Current.Navigate(new BookEditPage(book.Id));
        }

        private void Contest_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as FrameworkElement)?.DataContext is Book book)) return;
            string reason = InputDialog.Show(Window.GetWindow(this), "Оспорить заморозку книги",
                "Опишите, почему книгу «" + book.Title + "» следует разморозить:");
            if (reason == null) return;
            DataService.AddUnfreezeRequest(Session.CurrentUser.Id, null, book.Id, reason);
            MessageBox.Show("Заявка на разморозку книги отправлена администрации.",
                "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
