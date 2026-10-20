using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DailyPlaner.Entities;

namespace DailyPlaner.Services
{
    public static class DatabaseService
    {
        public static string GetHash(string text)
        {
            SHA256 sha = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            byte[] hash = sha.ComputeHash(bytes);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                builder.Append(hash[i].ToString("x2"));
            }
            return builder.ToString();
        }

        public static User FindUser(string username, string passwordHash)
        {
            User user = App.Context.Users.FirstOrDefault(x => x.Username == username && x.PasswordHash == passwordHash);
            return user;
        }

        public static bool UserExists(string username)
        {
            bool exists = App.Context.Users.Any(x => x.Username == username);
            return exists;
        }

        public static void AddUser(User user)
        {
            App.Context.Users.Add(user);
            App.Context.SaveChanges();
        }

        public static List<Task> GetTasksForDate(int userId, DateTime date)
        {
            DateTime start = date.Date;
            DateTime end = date.Date.AddDays(1);
            List<Task> tasks = App.Context.Tasks.Where(x => x.UserId == userId && x.DueDate >= start && x.DueDate < end).OrderBy(x => x.DueDate).ToList();
            return tasks;
        }

        public static List<Event> GetEventsForDate(int userId, DateTime date)
        {
            DateTime start = date.Date;
            DateTime end = date.Date.AddDays(1);
            List<Event> events = App.Context.Events.Where(x => x.UserId == userId && x.StartDate >= start && x.StartDate < end).OrderBy(x => x.StartDate).ToList();
            return events;
        }

        public static List<Note> GetNotes(int userId)
        {
            List<Note> notes = App.Context.Notes.Where(x => x.UserId == userId).OrderBy(x => x.CreatedAt).ToList();
            return notes;
        }

        public static int CountTasksForDate(int userId, DateTime date)
        {
            DateTime start = date.Date;
            DateTime end = date.Date.AddDays(1);
            int count = App.Context.Tasks.Count(x => x.UserId == userId && x.DueDate >= start && x.DueDate < end);
            return count;
        }

        public static void SaveTask(Task task, bool isNew)
        {
            if (isNew)
            {
                App.Context.Tasks.Add(task);
            }
            App.Context.SaveChanges();
        }

        public static void SaveEvent(Event item, bool isNew)
        {
            if (isNew)
            {
                App.Context.Events.Add(item);
            }
            App.Context.SaveChanges();
        }

        public static void SaveNote(Note note, bool isNew)
        {
            if (isNew)
            {
                App.Context.Notes.Add(note);
            }
            App.Context.SaveChanges();
        }

        public static void SaveReminder(string kind, int itemId, DateTime startDate, string title, int offsetMinutes)
        {
            if (kind == "Задача")
            {
                List<Reminder> old = App.Context.Reminders.Where(x => x.TaskId == itemId).ToList();
                for (int i = 0; i < old.Count; i++)
                {
                    App.Context.Reminders.Remove(old[i]);
                }
            }
            if (kind == "Событие")
            {
                List<Reminder> old = App.Context.Reminders.Where(x => x.EventId == itemId).ToList();
                for (int i = 0; i < old.Count; i++)
                {
                    App.Context.Reminders.Remove(old[i]);
                }
            }
            App.Context.SaveChanges();
            if (offsetMinutes > 0)
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
                reminder.ReminderTime = startDate.AddMinutes(-offsetMinutes);
                reminder.Message = "Скоро начнётся: " + title + " (" + startDate.ToString("dd.MM.yyyy HH:mm") + ")";
                reminder.IsActive = true;
                App.Context.Reminders.Add(reminder);
                App.Context.SaveChanges();
            }
        }

        public static void DeleteTask(Task task)
        {
            List<Reminder> reminders = App.Context.Reminders.Where(x => x.TaskId == task.Id).ToList();
            for (int i = 0; i < reminders.Count; i++)
            {
                App.Context.Reminders.Remove(reminders[i]);
            }
            App.Context.Tasks.Remove(task);
            App.Context.SaveChanges();
        }

        public static void DeleteEvent(Event item)
        {
            List<Reminder> reminders = App.Context.Reminders.Where(x => x.EventId == item.Id).ToList();
            for (int i = 0; i < reminders.Count; i++)
            {
                App.Context.Reminders.Remove(reminders[i]);
            }
            App.Context.Events.Remove(item);
            App.Context.SaveChanges();
        }

        public static void DeleteNote(Note note)
        {
            App.Context.Notes.Remove(note);
            App.Context.SaveChanges();
        }

        public static void SetTaskStatus(Task task, string status)
        {
            task.Status = status;
            App.Context.SaveChanges();
        }
    }
}
