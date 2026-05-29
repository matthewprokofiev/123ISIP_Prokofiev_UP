using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using _123ISIP_Prokofiev_UP.Data;
using _123ISIP_Prokofiev_UP.Models;
using _123ISIP_Prokofiev_UP.Services;

namespace _123ISIP_Prokofiev_UP.Views
{
    public partial class ReadingListsPage : Page
    {
        private bool _loaded;
        private List<ReadingStatus> _statuses;
        private int _currentStatusId;

        public ReadingListsPage()
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
            bool first = true;
            foreach (var st in _statuses)
            {
                var tab = new RadioButton
                {
                    Content = st.Name,
                    Tag = st.Id,
                    GroupName = "lists",
                    Style = (Style)FindResource("Pill"),
                    IsChecked = first
                };
                tab.Checked += Tab_Checked;
                TabsPanel.Children.Add(tab);
                if (first) _currentStatusId = st.Id;
                first = false;
            }

            _loaded = true;
            LoadBooks();
        }

        private void Tab_Checked(object sender, RoutedEventArgs e)
        {
            _currentStatusId = (int)((RadioButton)sender).Tag;
            if (_loaded) LoadBooks();
        }

        private void LoadBooks()
        {
            string search = SearchBox.Text;
            int? genreId = (GenreFilter.SelectedItem as Genre)?.Id;
            if (genreId == 0) genreId = null;
            string sort = (SortBox.SelectedItem as ComboBoxItem)?.Tag as string ?? "title";

            var items = DataService.GetReadingList(Session.CurrentUser.Id, _currentStatusId, search, genreId, sort);
            BooksItems.ItemsSource = items;
            EmptyText.Visibility = items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            if (_loaded) LoadBooks();
        }

        private void SearchArea_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Focus();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is ReadingListItem item)
                MainWindow.Current.Navigate(new BookPage(item.BookId));
        }

        private void Move_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (!(btn?.DataContext is ReadingListItem item)) return;

            var menu = new ContextMenu();
            foreach (var st in _statuses)
            {
                var mi = new MenuItem { Header = st.Name, Tag = st.Id, IsEnabled = st.Id != _currentStatusId };
                mi.Click += (s, args) =>
                {
                    DataService.SetReadingStatus(Session.CurrentUser.Id, item.BookId, (int)((MenuItem)s).Tag);
                    LoadBooks();
                };
                menu.Items.Add(mi);
            }
            menu.PlacementTarget = btn;
            menu.IsOpen = true;
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as FrameworkElement)?.DataContext is ReadingListItem item)) return;
            if (MessageBox.Show($"Убрать «{item.BookTitle}» из списков чтения?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            DataService.RemoveFromReadingList(Session.CurrentUser.Id, item.BookId);
            LoadBooks();
        }
    }
}
