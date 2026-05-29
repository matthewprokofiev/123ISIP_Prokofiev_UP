using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using _123ISIP_Prokofiev_UP.Data;
using _123ISIP_Prokofiev_UP.Models;
using _123ISIP_Prokofiev_UP.Services;

namespace _123ISIP_Prokofiev_UP.Views
{

    public partial class CatalogPage : Page
    {
        private bool _loaded;
        private List<ReadingStatus> _statuses;

        public CatalogPage()
        {
            InitializeComponent();
            Loaded += (s, e) => Init();
        }

        private void Init()
        {
            if (_loaded) return;

            var genres = new List<Genre> { new Genre { Id = 0, Name = "Все жанры" } };
            genres.AddRange(DataService.GetGenres());
            GenreFilter.ItemsSource = genres;
            GenreFilter.SelectedIndex = 0;

            _statuses = DataService.GetReadingStatuses();
            _loaded = true;
            LoadBooks();
        }

        private void LoadBooks()
        {
            string search = SearchBox.Text;
            int? genreId = (GenreFilter.SelectedItem as Genre)?.Id;
            if (genreId == 0) genreId = null;
            string sort = (SortBox.SelectedItem as ComboBoxItem)?.Tag as string ?? "title";

            var books = DataService.GetBooks(search, genreId, sort);
            BooksItems.ItemsSource = books;
            EmptyText.Visibility = books.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            if (_loaded) LoadBooks();
        }

        private void SearchArea_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Focus();
        }

        private void Card_Open(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is Book book)
                OpenBook(book);
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if ((sender as FrameworkElement)?.DataContext is Book book)
                OpenBook(book);
        }

        private void OpenBook(Book book)
        {
            MainWindow.Current.Navigate(new BookPage(book.Id));
        }

        private void AddToList_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var btn = sender as Button;
            if (!(btn?.DataContext is Book book)) return;

            var menu = new ContextMenu();
            foreach (var st in _statuses)
            {
                var item = new MenuItem { Header = st.Name, Tag = st.Id };
                item.Click += (s, args) =>
                {
                    DataService.SetReadingStatus(Session.CurrentUser.Id, book.Id, (int)((MenuItem)s).Tag);
                    MessageBox.Show($"«{book.Title}» добавлена в раздел «{((MenuItem)s).Header}».",
                        "Список чтения", MessageBoxButton.OK, MessageBoxImage.Information);
                };
                menu.Items.Add(item);
            }
            menu.PlacementTarget = btn;
            menu.IsOpen = true;
        }
    }
}
