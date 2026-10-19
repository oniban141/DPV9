using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DailyPlaner.Entities;

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
                User found = App.Context.Users.FirstOrDefault(x => x.Username == username);
                if (found != null)
                {
                    MessageBox.Show("Пользователь с таким именем уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                User user = new User();
                user.Username = username;
                user.PasswordHash = App.GetHash(password);
                user.Email = email;
                if (gender != null)
                {
                    user.GenderId = gender.Id;
                }
                Role userRole = App.Context.Roles.FirstOrDefault(x => x.Name == "Пользователь");
                if (userRole == null)
                {
                    userRole = App.Context.Roles.FirstOrDefault();
                }
                if (userRole != null)
                {
                    user.RoleId = userRole.Id;
                }
                user.CreatedAt = DateTime.Now;
                App.Context.Users.Add(user);
                App.Context.SaveChanges();
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
