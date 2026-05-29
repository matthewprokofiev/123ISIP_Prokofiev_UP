using System.Windows;

namespace _123ISIP_Prokofiev_UP.Views
{

    public partial class InputDialog : Window
    {
        public string ResponseText { get; private set; }

        public InputDialog(string title, string prompt, string initial = "", bool multiline = true)
        {
            InitializeComponent();
            Title = title;
            PromptText.Text = prompt;
            InputBox.Text = initial;
            InputBox.AcceptsReturn = multiline;
            if (!multiline) Height = 200;
            Loaded += (s, e) => { InputBox.Focus(); InputBox.SelectAll(); };
        }

        public static string Show(Window owner, string title, string prompt, string initial = "", bool multiline = true)
        {
            var dlg = new InputDialog(title, prompt, initial, multiline) { Owner = owner };
            return dlg.ShowDialog() == true ? dlg.ResponseText : null;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputBox.Text))
            {
                MessageBox.Show("Введите текст.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            ResponseText = InputBox.Text.Trim();
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
