using System;
using System.Windows;
using System.Windows.Controls;
using DailyPlaner.Entities;
using DailyPlaner.Services;

namespace DailyPlaner.Views
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string username = UsernameBox.Text.Trim();
            string password = PasswordBox.Password;
            if (username == "" || password == "")
            {
                MessageBox.Show("Введите имя пользователя и пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                string hash = DatabaseService.GetHash(password);
                User user = DatabaseService.FindUser(username, hash);
                if (user == null)
                {
                    MessageBox.Show("Неверный логин или пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                App.CurrentUser = user;
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                Window window = Window.GetWindow(this);
                window.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Нет подключения к базе данных. Проверьте, что SQL Server запущен", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Registration_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService.Navigate(new RegisterPage());
        }
    }
}
