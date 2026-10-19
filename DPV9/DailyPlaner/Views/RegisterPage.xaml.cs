using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DailyPlaner.Entities;
using DailyPlaner.Services;

namespace DailyPlaner.Views
{
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
            LoadGenders();
        }

        private void LoadGenders()
        {
            try
            {
                GenderComboBox.ItemsSource = App.Context.Genders.ToList();
                GenderComboBox.DisplayMemberPath = "Name";
                GenderComboBox.SelectedIndex = 0;
            }
            catch (Exception)
            {
                MessageBox.Show("Нет подключения к базе данных. Проверьте, что SQL Server запущен", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Register_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string username = UsernameBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirm = ConfirmPasswordBox.Password;
            string email = EmailBox.Text.Trim();
            Gender gender = GenderComboBox.SelectedItem as Gender;

            if (username == "" || password == "" || email == "")
            {
                MessageBox.Show("Заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (password != confirm)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                if (DatabaseService.UserExists(username))
                {
                    MessageBox.Show("Пользователь с таким именем уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                User user = new User();
                user.Username = username;
                user.PasswordHash = DatabaseService.GetHash(password);
                user.Email = email;
                if (gender != null)
                {
                    user.GenderId = gender.Id;
                }
                // TODO: укажите здесь нужную роль, например: user.RoleId = 1;
                user.CreatedAt = DateTime.Now;
                DatabaseService.AddUser(user);
                MessageBox.Show("Регистрация прошла успешно", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.GoBack();
            }
            catch (Exception)
            {
                MessageBox.Show("Не удалось сохранить пользователя. Проверьте подключение к базе данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Back_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
