using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using _123ISIP_Prokofiev_UP.Data;
using _123ISIP_Prokofiev_UP.Models;
using _123ISIP_Prokofiev_UP.Services;

namespace _123ISIP_Prokofiev_UP.Views
{

    public partial class BookPage : Page
    {
        private readonly int _bookId;
        private Book _book;

        public Visibility AdminVisibility => Session.IsAdmin ? Visibility.Visible : Visibility.Collapsed;

        public BookPage(int bookId)
        {
            InitializeComponent();
            _bookId = bookId;
            for (int i = 1; i <= 10; i++) RatingBox.Items.Add(i);
            RatingBox.SelectedItem = 8;
            Loaded += (s, e) => LoadBook();
        }

        private void LoadBook()
        {
            _book = DataService.GetBookById(_bookId);
            if (_book == null)
            {
                MessageBox.Show("Книга не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            TitleText.Text = _book.Title;
            AuthorText.Text = "Автор: " + _book.AuthorName;
            GenresText.Text = string.IsNullOrEmpty(_book.GenresText) ? "Жанры не указаны" : _book.GenresText;
            RatingText.Text = _book.ReviewsCount > 0
                ? $"{_book.AvgRating:0.0} ({_book.ReviewsCount} отз.)"
                : "Нет оценок";
            DescriptionText.Text = string.IsNullOrWhiteSpace(_book.Description) ? "Описание отсутствует." : _book.Description;
            ContentText.Text = string.IsNullOrWhiteSpace(_book.Content) ? "Текст книги отсутствует." : _book.Content;
            FrozenBadge.Visibility = _book.IsFrozen ? Visibility.Visible : Visibility.Collapsed;

            var cover = Covers.Load(_book.Id, _book.CoverPath);
            if (cover != null)
            {
                CoverBorder.Background = new System.Windows.Media.ImageBrush(cover)
                { Stretch = System.Windows.Media.Stretch.UniformToFill };
                CoverIcon.Visibility = Visibility.Collapsed;
            }

            FreezeBookBtn.Visibility = Session.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            FreezeBookBtn.Content = _book.IsFrozen ? "Разморозить книгу" : "Заморозить книгу";

            PrefillReview();
            LoadReviews();
        }

        private void PrefillReview()
        {
            foreach (var r in DataService.GetReviewsByUser(Session.CurrentUser.Id))
                if (r.BookId == _bookId)
                {
                    ReviewFormTitle.Text = "Изменить мой отзыв";
                    SubmitReviewBtn.Content = "Сохранить отзыв";
                    RatingBox.SelectedItem = r.Rating;
                    ReviewTextBox.Text = r.ReviewText;
                    return;
                }
        }

        private void LoadReviews()
        {
            var reviews = DataService.GetReviewsForBook(_bookId, includeFrozen: Session.IsAdmin);
            ReviewsItems.ItemsSource = reviews;
            NoReviewsText.Visibility = reviews.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack) NavigationService.GoBack();
            else MainWindow.Current.Navigate(new CatalogPage());
        }

        private void Read_Click(object sender, RoutedEventArgs e)
        {
            ReaderPanel.Visibility = ReaderPanel.Visibility == Visibility.Visible
                ? Visibility.Collapsed : Visibility.Visible;
            ReadBtn.Content = ReaderPanel.Visibility == Visibility.Visible ? "Скрыть текст" : "Читать текст";
        }

        private void AddToList_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var menu = new ContextMenu();
            foreach (var st in DataService.GetReadingStatuses())
            {
                var item = new MenuItem { Header = st.Name, Tag = st.Id };
                item.Click += (s, args) =>
                {
                    DataService.SetReadingStatus(Session.CurrentUser.Id, _bookId, (int)((MenuItem)s).Tag);
                    MessageBox.Show($"«{_book.Title}» добавлена в раздел «{((MenuItem)s).Header}».",
                        "Список чтения", MessageBoxButton.OK, MessageBoxImage.Information);
                };
                menu.Items.Add(item);
            }
            menu.PlacementTarget = btn;
            menu.IsOpen = true;
        }

        private void ComplainBook_Click(object sender, RoutedEventArgs e)
        {
            string reason = InputDialog.Show(Window.GetWindow(this), "Жалоба на книгу",
                "Укажите причину жалобы на книгу:");
            if (reason == null) return;
            DataService.AddComplaint(Session.CurrentUser.Id, _bookId, null, null, reason);
            MessageBox.Show("Жалоба на книгу отправлена администрации.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ComplainAuthor_Click(object sender, RoutedEventArgs e)
        {
            string reason = InputDialog.Show(Window.GetWindow(this), "Жалоба на автора",
                "Укажите причину жалобы на автора «" + _book.AuthorName + "»:");
            if (reason == null) return;
            DataService.AddComplaint(Session.CurrentUser.Id, null, null, _book.AuthorId, reason);
            MessageBox.Show("Жалоба на автора отправлена администрации.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ComplainReview_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as FrameworkElement)?.DataContext is Review review)) return;
            string reason = InputDialog.Show(Window.GetWindow(this), "Жалоба на отзыв",
                "Укажите причину жалобы на отзыв пользователя «" + review.UserLogin + "»:");
            if (reason == null) return;
            DataService.AddComplaint(Session.CurrentUser.Id, null, review.Id, null, reason);
            MessageBox.Show("Жалоба на отзыв отправлена администрации.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SubmitReview_Click(object sender, RoutedEventArgs e)
        {
            int rating = (int)(RatingBox.SelectedItem ?? 0);
            if (rating < 1 || rating > 10)
            {
                MessageBox.Show("Выберите оценку от 1 до 10.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DataService.AddOrUpdateReview(_bookId, Session.CurrentUser.Id, ReviewTextBox.Text?.Trim(), rating);
            MessageBox.Show("Спасибо! Ваш отзыв сохранён.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadBook();
        }

        private void FreezeBook_Click(object sender, RoutedEventArgs e)
        {
            DataService.SetBookFrozen(_bookId, !_book.IsFrozen);
            LoadBook();
        }

        private void FreezeReview_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as FrameworkElement)?.DataContext is Review review)) return;
            DataService.SetReviewFrozen(review.Id, !review.IsFrozen);
            LoadReviews();
        }
    }
}
