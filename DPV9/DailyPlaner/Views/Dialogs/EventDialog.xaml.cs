using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DailyPlaner.Entities;
using DailyPlaner.Services;

namespace DailyPlaner.Views.Dialogs
{
    public partial class EventDialog : Window
    {
        private Event currentEvent;
        private bool isNew;

        public DateTime SelectedDate;

        public EventDialog()
        {
            InitializeComponent();
            isNew = true;
            currentEvent = new Event();
        }

        public void LoadEvent(Event item)
        {
            isNew = false;
            currentEvent = item;
            TitleBox.Text = item.Title;
            DescriptionBox.Text = item.Description;
            StartDatePicker.SelectedDate = item.StartDate.Date;
            TimeComboBox.Text = item.StartDate.ToString("HH:mm");
            EndDatePicker.SelectedDate = item.EndDate.Date;
            EndTimeComboBox.Text = item.EndDate.ToString("HH:mm");
            LocationBox.Text = item.Location;
        }

        private void FillTime()
        {
            TimeComboBox.Items.Clear();
            EndTimeComboBox.Items.Clear();
            for (int hour = 0; hour < 24; hour++)
            {
                for (int minute = 0; minute < 60; minute = minute + 30)
                {
                    TimeComboBox.Items.Add(hour.ToString("00") + ":" + minute.ToString("00"));
                    EndTimeComboBox.Items.Add(hour.ToString("00") + ":" + minute.ToString("00"));
                }
            }
        }

        private string CheckErrors()
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
            if (StartDatePicker.SelectedDate == null)
            {
                errors = errors + "Выберите дату начала\n";
            }
            if (TimeComboBox.Text == "")
            {
                errors = errors + "Выберите время начала\n";
            }
            if (EndDatePicker.SelectedDate == null)
            {
                errors = errors + "Выберите дату окончания\n";
            }
            if (EndTimeComboBox.Text == "")
            {
                errors = errors + "Выберите время окончания\n";
            }
            if (LocationBox.Text.Trim() == "")
            {
                errors = errors + "Введите место проведения\n";
            }
            return errors;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string errors = CheckErrors();
            if (errors != "")
            {
                MessageBox.Show(errors, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            DateTime startDate = StartDatePicker.SelectedDate.Value.Date;
            string[] startParts = TimeComboBox.Text.Split(':');
            startDate = startDate.AddHours(Convert.ToDouble(startParts[0]));
            startDate = startDate.AddMinutes(Convert.ToDouble(startParts[1]));
            DateTime endDate = EndDatePicker.SelectedDate.Value.Date;
            string[] endParts = EndTimeComboBox.Text.Split(':');
            endDate = endDate.AddHours(Convert.ToDouble(endParts[0]));
            endDate = endDate.AddMinutes(Convert.ToDouble(endParts[1]));
            if (endDate <= startDate)
            {
                MessageBox.Show("Время окончания должно быть позже времени начала", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                currentEvent.UserId = App.CurrentUser.Id;
                currentEvent.Title = TitleBox.Text.Trim();
                currentEvent.Description = DescriptionBox.Text;
                currentEvent.StartDate = startDate;
                currentEvent.EndDate = endDate;
                currentEvent.Location = LocationBox.Text.Trim();
                currentEvent.Status = "Запланировано";
                DatabaseService.SaveEvent(currentEvent, isNew);
                int offsetMinutes = 0;
                if (ReminderCheckBox.IsChecked == true && ReminderOffsetComboBox.Text != "")
                {
                    offsetMinutes = GetOffsetMinutes();
                }
                DatabaseService.SaveReminder("Событие", currentEvent.Id, startDate, currentEvent.Title, offsetMinutes);
                DialogResult = true;
            }
            catch (Exception)
            {
                MessageBox.Show("Не удалось сохранить событие. Проверьте подключение к базе данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (isNew)
            {
                StartDatePicker.SelectedDate = SelectedDate.Date;
                EndDatePicker.SelectedDate = SelectedDate.Date;
                TimeComboBox.SelectedIndex = 0;
                EndTimeComboBox.SelectedIndex = 2;
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

        private void LocationBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (LocationBox.Text == "")
            {
                LocationWatermark.Visibility = Visibility.Visible;
            }
            else
            {
                LocationWatermark.Visibility = Visibility.Hidden;
            }
        }
    }
}
