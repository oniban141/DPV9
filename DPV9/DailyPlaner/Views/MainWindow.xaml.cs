using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using DailyPlaner.Entities;
using DailyPlaner.Services;
using DailyPlaner.Views.Dialogs;
using Microsoft.Win32;

namespace DailyPlaner
{
    public class DayItem
    {
        public int Id { get; set; }
        public string Kind { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public string Place { get; set; }
        public string Time { get; set; }
        public string KindLabel { get; set; }
        public string StatusLabel { get; set; }
        public Task Task { get; set; }
        public Event Event { get; set; }
        public Note Note { get; set; }
    }

    public partial class MainWindow : Window
    {
        private DispatcherTimer reminderTimer;
        private List<DayItem> currentItems;

        public MainWindow()
        {
            InitializeComponent();
            reminderTimer = new DispatcherTimer();
            reminderTimer.Interval = TimeSpan.FromSeconds(30);
            reminderTimer.Tick += ReminderTimer_Tick;
            currentItems = new List<DayItem>();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            HelloUsernameText.Text = App.CurrentUser.Username;
            MainCalendar.SelectedDate = DateTime.Today;
            LoadItems();
            LoadAutoStart();
            reminderTimer.Start();
        }

        private void MainCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadItems();
        }

        private string GetKind()
        {
            ComboBoxItem item = (ComboBoxItem)ItemTypeComboBox.SelectedItem;
            string kind = item.Content.ToString();
            return kind;
        }

        private string GetAction()
        {
            ComboBoxItem item = (ComboBoxItem)ActionComboBox.SelectedItem;
            string action = item.Content.ToString();
            return action;
        }

        private DayItem GetSelectedItem()
        {
            DayItem item = DayItemsList.SelectedItem as DayItem;
            return item;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            string kind = GetKind();
            string action = GetAction();
            if (action == "Добавить")
            {
                if (kind == "Задача")
                {
                    TaskDialog dialog = new TaskDialog();
                    dialog.Owner = this;
                    dialog.SelectedDate = MainCalendar.SelectedDate.Value;
                    bool? result = dialog.ShowDialog();
                    if (result == true)
                    {
                        LoadItems();
                    }
                }
                if (kind == "Событие")
                {
                    EventDialog dialog = new EventDialog();
                    dialog.Owner = this;
                    dialog.SelectedDate = MainCalendar.SelectedDate.Value;
                    bool? result = dialog.ShowDialog();
                    if (result == true)
                    {
                        LoadItems();
                    }
                }
                if (kind == "Заметка")
                {
                    NoteDialog dialog = new NoteDialog();
                    dialog.Owner = this;
                    bool? result = dialog.ShowDialog();
                    if (result == true)
                    {
                        LoadItems();
                    }
                }
            }
            if (action == "Изменить")
            {
                DayItem item = GetSelectedItem();
                if (item == null)
                {
                    MessageBox.Show("Выберите запись в списке", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (item.Kind == "Задача")
                {
                    TaskDialog dialog = new TaskDialog();
                    dialog.Owner = this;
                    dialog.LoadTask(item.Task);
                    bool? result = dialog.ShowDialog();
                    if (result == true)
                    {
                        LoadItems();
                    }
                }
                if (item.Kind == "Событие")
                {
                    EventDialog dialog = new EventDialog();
                    dialog.Owner = this;
                    dialog.LoadEvent(item.Event);
                    bool? result = dialog.ShowDialog();
                    if (result == true)
                    {
                        LoadItems();
                    }
                }
                if (item.Kind == "Заметка")
                {
                    NoteDialog dialog = new NoteDialog();
                    dialog.Owner = this;
                    dialog.LoadNote(item.Note);
                    bool? result = dialog.ShowDialog();
                    if (result == true)
                    {
                        LoadItems();
                    }
                }
            }
            if (action == "Удалить")
            {
                DeleteSelectedItem();
            }
            if (action == "Отметить выполненной")
            {
                ToggleStatus();
            }
        }

        private void DeleteSelectedItem()
        {
            DayItem item = GetSelectedItem();
            if (item == null)
            {
                MessageBox.Show("Выберите запись в списке", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить выбранную запись?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }
            try
            {
                if (item.Kind == "Задача")
                {
                    DatabaseService.DeleteTask(item.Task);
                }
                if (item.Kind == "Событие")
                {
                    DatabaseService.DeleteEvent(item.Event);
                }
                if (item.Kind == "Заметка")
                {
                    DatabaseService.DeleteNote(item.Note);
                }
                LoadItems();
            }
            catch (Exception)
            {
                MessageBox.Show("Не удалось удалить запись. Проверьте подключение к базе данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToggleStatus()
        {
            DayItem item = GetSelectedItem();
            if (item == null)
            {
                MessageBox.Show("Выберите задачу в списке", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (item.Kind != "Задача")
            {
                MessageBox.Show("Статус можно менять только у задачи", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string newStatus = "";
            if (item.Status == "Выполнено")
            {
                newStatus = "Не выполнено";
            }
            else
            {
                newStatus = "Выполнено";
            }
            try
            {
                DatabaseService.SetTaskStatus(item.Task, newStatus);
                LoadItems();
            }
            catch (Exception)
            {
                MessageBox.Show("Не удалось изменить статус. Проверьте подключение к базе данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DayItemsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DayItem item = GetSelectedItem();
            if (item == null)
            {
                return;
            }
            DetailsDialog dialog = new DetailsDialog();
            dialog.Owner = this;
            dialog.ShowItem(item);
            dialog.ShowDialog();
        }

        private void LoadItems()
        {
            List<DayItem> items = new List<DayItem>();
            DateTime selectedDate = MainCalendar.SelectedDate.Value;
            SelectedDateText.Text = "Записи на " + selectedDate.ToShortDateString();
            try
            {
                List<Task> tasks = DatabaseService.GetTasksForDate(App.CurrentUser.Id, selectedDate);
                for (int i = 0; i < tasks.Count; i++)
                {
                    DayItem item = new DayItem();
                    item.Kind = "Задача";
                    item.Id = tasks[i].Id;
                    item.Title = tasks[i].Title;
                    item.Description = tasks[i].Description;
                    item.Date = tasks[i].DueDate;
                    item.Priority = tasks[i].Priority;
                    item.Status = tasks[i].Status;
                    item.Time = tasks[i].DueDate.ToString("HH:mm");
                    item.KindLabel = "[Задача]";
                    item.StatusLabel = "[" + tasks[i].Status + "]";
                    item.Task = tasks[i];
                    items.Add(item);
                }
                List<Event> events = DatabaseService.GetEventsForDate(App.CurrentUser.Id, selectedDate);
                for (int i = 0; i < events.Count; i++)
                {
                    DayItem item = new DayItem();
                    item.Kind = "Событие";
                    item.Id = events[i].Id;
                    item.Title = events[i].Title;
                    item.Description = events[i].Description;
                    item.Date = events[i].StartDate;
                    item.Place = events[i].Location;
                    item.Time = events[i].StartDate.ToString("HH:mm");
                    item.KindLabel = "[Событие]";
                    item.StatusLabel = "[" + events[i].Location + "]";
                    item.Event = events[i];
                    items.Add(item);
                }
                items.Sort(SortByTime);
                List<Note> notes = DatabaseService.GetNotes(App.CurrentUser.Id);
                for (int i = 0; i < notes.Count; i++)
                {
                    DayItem item = new DayItem();
                    item.Kind = "Заметка";
                    item.Id = notes[i].Id;
                    item.Title = notes[i].Title;
                    item.Description = notes[i].Content;
                    item.Time = "";
                    item.KindLabel = "[Заметка]";
                    item.StatusLabel = "";
                    item.Note = notes[i];
                    items.Add(item);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Нет подключения к базе данных. Проверьте, что SQL Server запущен", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            currentItems = items;
            ShowItems();
        }

        private int SortByTime(DayItem first, DayItem second)
        {
            return first.Date.CompareTo(second.Date);
        }

        private void ShowItems()
        {
            DayItemsList.Items.Clear();
            string search = SearchBox.Text.Trim();
            for (int i = 0; i < currentItems.Count; i++)
            {
                bool show = true;
                if (search != "" && search != "Поиск")
                {
                    if (currentItems[i].Title != null)
                    {
                        show = currentItems[i].Title.ToLower().Contains(search.ToLower());
                    }
                    else
                    {
                        show = false;
                    }
                }
                if (show)
                {
                    DayItemsList.Items.Add(currentItems[i]);
                }
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowItems();
        }

        private void CheckRemindersButton_Click(object sender, RoutedEventArgs e)
        {
            NotificationService.CheckReminders();
        }

        private void ReminderTimer_Tick(object sender, EventArgs e)
        {
            NotificationService.CheckReminders();
        }

        private void LoadAutoStart()
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false);
                if (key == null)
                {
                    AutoStartCheckBox.IsChecked = false;
                    return;
                }
                object value = key.GetValue("DailyPlaner");
                key.Close();
                if (value == null)
                {
                    AutoStartCheckBox.IsChecked = false;
                }
                else
                {
                    AutoStartCheckBox.IsChecked = true;
                }
            }
            catch (Exception)
            {
                AutoStartCheckBox.IsChecked = false;
            }
        }

        private void AutoStartCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SaveAutoStart(true);
        }

        private void AutoStartCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SaveAutoStart(false);
        }

        private void SaveAutoStart(bool enable)
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
                if (enable)
                {
                    string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    key.SetValue("DailyPlaner", "\"" + path + "\"");
                }
                else
                {
                    key.DeleteValue("DailyPlaner", false);
                }
                key.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Не удалось сохранить настройку автозапуска", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            reminderTimer.Stop();
        }
    }
}
