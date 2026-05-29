using System;
using System.Windows;
using System.Windows.Input;
using _123ISIP_Prokofiev_UP.Data;
using _123ISIP_Prokofiev_UP.Models;
using _123ISIP_Prokofiev_UP.Services;

namespace _123ISIP_Prokofiev_UP.Views
{
    /// <summary>Окно авторизации и регистрации — первая страница приложения.</summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Tab_Checked(object sender, RoutedEventArgs e)
        {
            if (LoginPanel == null) return; // ещё не инициализировано
            bool login = TabLogin.IsChecked == true;
            LoginPanel.Visibility = login ? Visibility.Visible : Visibility.Collapsed;
            RegisterPanel.Visibility = login ? Visibility.Collapsed : Visibility.Visible;
            HideError();
        }

        private void LoginPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Login_Click(sender, e);
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginLogin.Text.Trim();
            string password = LoginPassword.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Введите логин и пароль.");
                return;
            }

            try
            {
                User user = DataService.Authenticate(login, PasswordHasher.Hash(password));
                if (user == null)
                {
                    ShowError("Неверный логин или пароль.");
                    return;
                }
                OpenMainWindow(user);
            }
            catch (Exception ex)
            {
                ShowError("Ошибка подключения к базе данных: " + ex.Message);
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string name = RegName.Text.Trim();
            string login = RegLogin.Text.Trim();
            string email = RegEmail.Text.Trim();
            string pass = RegPassword.Password;
            string pass2 = RegPassword2.Password;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(pass))
            {
                ShowError("Заполните все поля.");
                return;
            }
            if (pass != pass2)
            {
                ShowError("Пароли не совпадают.");
                return;
            }
            if (!email.Contains("@"))
            {
                ShowError("Введите корректный email.");
                return;
            }

            try
            {
                if (DataService.LoginExists(login)) { ShowError("Такой логин уже занят."); return; }
                if (DataService.EmailExists(email)) { ShowError("Такой email уже используется."); return; }

                User user = DataService.Register(login, PasswordHasher.Hash(pass), email, name);
                MessageBox.Show("Регистрация прошла успешно! Добро пожаловать, " + user.DisplayName + ".",
                    "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                OpenMainWindow(user);
            }
            catch (Exception ex)
            {
                ShowError("Ошибка регистрации: " + ex.Message);
            }
        }

        private void OpenMainWindow(User user)
        {
            Session.CurrentUser = user;
            var main = new MainWindow();
            Application.Current.MainWindow = main;
            main.Show();
            Close();
        }

        private void ShowError(string text)
        {
            ErrorText.Text = text;
            ErrorText.Visibility = Visibility.Visible;
        }

        private void HideError() => ErrorText.Visibility = Visibility.Collapsed;
    }
}
