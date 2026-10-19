using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DailyPlaner.Entities;

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
            FillTime();
            DueDatePicker.SelectedDate = task.DueDate.Date;
            TimeComboBox.Text = task.DueDate.ToString("HH:mm");
            FillPriorities();
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
            if (errors == "")
            {
                if (isNew)
                {
                    DateTime start = dueDate.Date;
                    DateTime end = dueDate.Date.AddDays(1);
                    int count = App.Context.Tasks.Count(x => x.UserId == App.CurrentUser.Id && x.DueDate >= start && x.DueDate < end);
                    if (count >= 100)
                    {
                        errors = errors + "На этот день уже добавлено максимум 100 задач\n";
                    }
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
                    App.Context.Tasks.Add(currentTask);
                }
                App.Context.SaveChanges();
                SaveReminder("Задача", currentTask.Id, dueDate, currentTask.Title);
                DialogResult = true;
            }
            catch (Exception)
            {
                MessageBox.Show("Не удалось сохранить задачу. Проверьте подключение к базе данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveReminder(string kind, int itemId, DateTime startDate, string title)
        {
            try
            {
                if (kind == "Задача")
                {
                    List<Reminder> old = App.Context.Reminders.Where(x => x.TaskId == itemId).ToList();
                    for (int i = 0; i < old.Count; i++)
                    {
                        App.Context.Reminders.Remove(old[i]);
                    }
                    App.Context.SaveChanges();
                }
                if (kind == "Событие")
                {
                    List<Reminder> old = App.Context.Reminders.Where(x => x.EventId == itemId).ToList();
                    for (int i = 0; i < old.Count; i++)
                    {
                        App.Context.Reminders.Remove(old[i]);
                    }
                    App.Context.SaveChanges();
                }
                if (ReminderCheckBox.IsChecked == true && ReminderOffsetComboBox.Text != "")
                {
                    Reminder reminder = new Reminder();
                    reminder.UserId = App.CurrentUser.Id;
                    if (kind == "Задача")
                    {
                        reminder.TaskId = itemId;
                    }
                    else
                    {
                        reminder.EventId = itemId;
                    }
                    reminder.ReminderTime = startDate.AddMinutes(-GetOffsetMinutes());
                    reminder.Message = "Скоро начнётся: " + title + " (" + startDate.ToString("dd.MM.yyyy HH:mm") + ")";
                    reminder.IsActive = true;
                    App.Context.Reminders.Add(reminder);
                    App.Context.SaveChanges();
                }
            }
            catch (Exception)
            {
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
