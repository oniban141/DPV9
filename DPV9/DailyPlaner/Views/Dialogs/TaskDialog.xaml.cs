using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DailyPlaner.Entities;
using DailyPlaner.Services;

namespace DailyPlaner.Views.Dialogs
{
    public partial class TaskDialog : Window
    {
        private Task currentTask;
        private bool isNew;

        public DateTime SelectedDate;

        public TaskDialog()
        {
            InitializeComponent();
            isNew = true;
            currentTask = new Task();
        }

        public void LoadTask(Task task)
        {
            isNew = false;
            currentTask = task;
            TitleBox.Text = task.Title;
            DescriptionBox.Text = task.Description;
            DueDatePicker.SelectedDate = task.DueDate.Date;
            TimeComboBox.Text = task.DueDate.ToString("HH:mm");
            PriorityComboBox.Text = task.Priority;
        }

        private void FillTime()
        {
            TimeComboBox.Items.Clear();
            for (int hour = 0; hour < 24; hour++)
            {
                for (int minute = 0; minute < 60; minute = minute + 30)
                {
                    TimeComboBox.Items.Add(hour.ToString("00") + ":" + minute.ToString("00"));
                }
            }
        }

        private void FillPriorities()
        {
            PriorityComboBox.Items.Clear();
            PriorityComboBox.Items.Add("Низкий");
            PriorityComboBox.Items.Add("Средний");
            PriorityComboBox.Items.Add("Высокий");
        }

        private string CheckErrors(DateTime dueDate)
        {
            string errors = "";
            if (TitleBox.Text.Trim() == "")
            {
                errors = errors + "Введите название\n";
            }
            if (TitleBox.Text.Trim().Length > 100)
            {
                errors = errors + "Название не должно превышать 100 символов\n";
            }
            if (DueDatePicker.SelectedDate == null)
            {
                errors = errors + "Выберите дату\n";
            }
            if (TimeComboBox.Text == "")
            {
                errors = errors + "Выберите время\n";
            }
            if (PriorityComboBox.Text != "Низкий" && PriorityComboBox.Text != "Средний" && PriorityComboBox.Text != "Высокий")
            {
                errors = errors + "Выберите приоритет\n";
            }
            if (errors == "" && isNew)
            {
                int count = DatabaseService.CountTasksForDate(App.CurrentUser.Id, dueDate);
                if (count >= 100)
                {
                    errors = errors + "На этот день уже добавлено максимум 100 задач\n";
                }
            }
            return errors;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime dueDate = DateTime.Now;
            if (DueDatePicker.SelectedDate != null && TimeComboBox.Text != "")
            {
                string[] parts = TimeComboBox.Text.Split(':');
                dueDate = DueDatePicker.SelectedDate.Value.Date;
                dueDate = dueDate.AddHours(Convert.ToDouble(parts[0]));
                dueDate = dueDate.AddMinutes(Convert.ToDouble(parts[1]));
            }
            string errors = CheckErrors(dueDate);
            if (errors != "")
            {
                MessageBox.Show(errors, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                currentTask.UserId = App.CurrentUser.Id;
                currentTask.Title = TitleBox.Text.Trim();
                currentTask.Description = DescriptionBox.Text;
                currentTask.DueDate = dueDate;
                currentTask.Priority = PriorityComboBox.Text;
                if (isNew)
                {
                    currentTask.Status = "Не выполнено";
                }
                DatabaseService.SaveTask(currentTask, isNew);
                int offsetMinutes = 0;
                if (ReminderCheckBox.IsChecked == true && ReminderOffsetComboBox.Text != "")
                {
                    offsetMinutes = GetOffsetMinutes();
                }
                DatabaseService.SaveReminder("Задача", currentTask.Id, dueDate, currentTask.Title, offsetMinutes);
                DialogResult = true;
            }
            catch (Exception)
            {
                MessageBox.Show("Не удалось сохранить задачу. Проверьте подключение к базе данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetOffsetMinutes()
        {
            string text = ReminderOffsetComboBox.Text;
            int minutes = 30;
            if (text == "5 минут")
            {
                minutes = 5;
            }
            if (text == "15 минут")
            {
                minutes = 15;
            }
            if (text == "30 минут")
            {
                minutes = 30;
            }
            if (text == "1 час")
            {
                minutes = 60;
            }
            if (text == "2 часа")
            {
                minutes = 120;
            }
            if (text == "1 день")
            {
                minutes = 1440;
            }
            return minutes;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FillTime();
            FillPriorities();
            if (isNew)
            {
                DueDatePicker.SelectedDate = SelectedDate.Date;
                TimeComboBox.SelectedIndex = 0;
                PriorityComboBox.SelectedIndex = 1;
            }
            ReminderOffsetComboBox.Items.Add("5 минут");
            ReminderOffsetComboBox.Items.Add("15 минут");
            ReminderOffsetComboBox.Items.Add("30 минут");
            ReminderOffsetComboBox.Items.Add("1 час");
            ReminderOffsetComboBox.Items.Add("2 часа");
            ReminderOffsetComboBox.Items.Add("1 день");
            ReminderOffsetComboBox.SelectedIndex = 2;
        }

        private void ReminderCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (ReminderCheckBox.IsChecked == true)
            {
                ReminderPanel.Visibility = Visibility.Visible;
            }
            else
            {
                ReminderPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void TitleBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TitleBox.Text == "")
            {
                TitleWatermark.Visibility = Visibility.Visible;
            }
            else
            {
                TitleWatermark.Visibility = Visibility.Hidden;
            }
        }

        private void DescriptionBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DescriptionBox.Text == "")
            {
                DescriptionWatermark.Visibility = Visibility.Visible;
            }
            else
            {
                DescriptionWatermark.Visibility = Visibility.Hidden;
            }
        }
    }
}
