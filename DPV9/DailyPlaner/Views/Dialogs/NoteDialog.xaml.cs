using System;
using System.Windows;
using System.Windows.Controls;
using DailyPlaner.Entities;

namespace DailyPlaner.Views.Dialogs
{
    public partial class NoteDialog : Window
    {
        private Note currentNote;
        private bool isNew;

        public NoteDialog()
        {
            InitializeComponent();
            isNew = true;
            currentNote = new Note();
        }

        public void LoadNote(Note note)
        {
            isNew = false;
            currentNote = note;
            TitleBox.Text = note.Title;
            ContentBox.Text = note.Content;
        }

        private string CheckErrors()
        {
            string errors = "";
            if (TitleBox.Text.Trim() == "")
            {
                errors = errors + "Введите название\n";
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
            try
            {
                currentNote.UserId = App.CurrentUser.Id;
                currentNote.Title = TitleBox.Text.Trim();
                currentNote.Content = ContentBox.Text;
                if (currentNote.Content.Trim() == "")
                {
                    currentNote.Content = null;
                }
                if (isNew)
                {
                    currentNote.CreatedAt = DateTime.Now;
                    App.Context.Notes.Add(currentNote);
                }
                App.Context.SaveChanges();
                DialogResult = true;
            }
            catch (Exception)
            {
                MessageBox.Show("Не удалось сохранить заметку. Проверьте подключение к базе данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
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

        private void ContentBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ContentBox.Text == "")
            {
                ContentWatermark.Visibility = Visibility.Visible;
            }
            else
            {
                ContentWatermark.Visibility = Visibility.Hidden;
            }
        }
    }
}
