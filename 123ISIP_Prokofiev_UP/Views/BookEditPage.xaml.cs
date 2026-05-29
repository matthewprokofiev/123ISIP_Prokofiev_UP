using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using _123ISIP_Prokofiev_UP.Data;
using _123ISIP_Prokofiev_UP.Models;
using _123ISIP_Prokofiev_UP.Services;

namespace _123ISIP_Prokofiev_UP.Views
{

    public partial class BookEditPage : Page
    {
        private readonly int? _bookId;
        private List<GenreChoice> _genreChoices;

        public class GenreChoice
        {
            public Genre Genre { get; set; }
            public bool IsSelected { get; set; }
        }

        public BookEditPage(int? bookId)
        {
            InitializeComponent();
            _bookId = bookId;
            Loaded += (s, e) => Load();
        }

        private void Load()
        {
            _genreChoices = DataService.GetGenres()
                .Select(g => new GenreChoice { Genre = g, IsSelected = false })
                .ToList();

            if (_bookId.HasValue)
            {
                HeaderText.Text = "Редактирование книги";
                var book = DataService.GetBookById(_bookId.Value);
                if (book != null)
                {
                    TitleBox.Text = book.Title;
                    DescriptionBox.Text = book.Description;
                    CoverBox.Text = book.CoverPath;
                    ContentBox.Text = book.Content;
                    var selected = new HashSet<int>(DataService.GetGenresForBook(_bookId.Value).Select(g => g.Id));
                    foreach (var c in _genreChoices) c.IsSelected = selected.Contains(c.Genre.Id);
                }
            }

            GenresItems.ItemsSource = _genreChoices;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Введите название книги.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var genreIds = _genreChoices.Where(c => c.IsSelected).Select(c => c.Genre.Id).ToList();
            string description = string.IsNullOrWhiteSpace(DescriptionBox.Text) ? null : DescriptionBox.Text.Trim();
            string cover = string.IsNullOrWhiteSpace(CoverBox.Text) ? null : CoverBox.Text.Trim();
            string content = string.IsNullOrWhiteSpace(ContentBox.Text) ? null : ContentBox.Text;

            if (_bookId.HasValue)
                DataService.UpdateBook(_bookId.Value, title, description, cover, content, genreIds);
            else
                DataService.AddBook(title, description, cover, content, Session.CurrentUser.Id, genreIds);

            MessageBox.Show("Книга сохранена.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            MainWindow.Current.Navigate(new AuthorPage());
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Current.Navigate(new AuthorPage());
        }
    }
}
