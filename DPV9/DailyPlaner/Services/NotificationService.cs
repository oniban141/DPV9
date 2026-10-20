using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using DailyPlaner.Entities;

namespace DailyPlaner.Services
{
    public static class NotificationService
    {
        public static void CheckReminders()
        {
            try
            {
                DateTime now = DateTime.Now;
                List<Reminder> reminders = App.Context.Reminders.Where(x => x.UserId == App.CurrentUser.Id && x.IsActive).ToList();
                for (int i = 0; i < reminders.Count; i++)
                {
                    if (reminders[i].ReminderTime <= now && reminders[i].ReminderTime.AddHours(1) > now)
                    {
                        MessageBox.Show(reminders[i].Message, "Напоминание", MessageBoxButton.OK, MessageBoxImage.Information);
                        reminders[i].IsActive = false;
                    }
                }
                App.Context.SaveChanges();
            }
            catch (Exception)
            {
            }
        }
    }
}
