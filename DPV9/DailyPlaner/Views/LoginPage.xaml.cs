using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DailyPlaner.Entities;

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
                string hash = App.GetHash(password);
                User user = App.Context.Users.FirstOrDefault(x => x.Username == username && x.PasswordHash == hash);
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
