using System;
using System.Windows;

namespace DailyPlaner.Views.Dialogs
{
    public partial class DetailsDialog : Window
    {
        public DetailsDialog()
        {
            InitializeComponent();
        }

        public void ShowItem(DayItem item)
        {
            KindLabel.Text = item.Kind;
            TitleText.Text = item.Title;
            DescriptionText.Text = item.Description;
            if (item.Kind == "Задача")
            {
                TimeLabel.Text = "Срок";
                TimeText.Text = item.Date.ToString("dd.MM.yyyy HH:mm");
                ExtraLabel.Text = "Приоритет";
                ExtraText.Text = item.Priority;
                StatusLabel.Text = "Статус";
                StatusText.Text = item.Status;
            }
            if (item.Kind == "Событие")
            {
                TimeLabel.Text = "Начало";
                TimeText.Text = item.Date.ToString("dd.MM.yyyy HH:mm");
                ExtraLabel.Text = "Место";
                ExtraText.Text = item.Place;
                StatusLabel.Text = "Статус";
                StatusText.Text = item.Event.Status;
            }
            if (item.Kind == "Заметка")
            {
                TimeLabel.Text = "Создана";
                TimeText.Text = item.Note.CreatedAt.ToString("dd.MM.yyyy HH:mm");
                ExtraLabel.Text = "";
                ExtraText.Text = "";
                StatusLabel.Text = "";
                StatusText.Text = "";
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
