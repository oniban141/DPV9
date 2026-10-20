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
            using (DailyPlannerDBV3Entities context = new DailyPlannerDBV3Entities())
            {
                User user = context.Users.FirstOrDefault(x => x.Username == username && x.PasswordHash == passwordHash);
                return user;
            }
        }

        public static bool UserExists(string username)
        {
            using (DailyPlannerDBV3Entities context = new DailyPlannerDBV3Entities())
            {
                bool exists = context.Users.Any(x => x.Username == username);
                return exists;
            }
        }

        public static void AddUser(User user)
        {
            using (DailyPlannerDBV3Entities context = new DailyPlannerDBV3Entities())
            {
                context.Users.Add(user);
                context.SaveChanges();
            }
        }

        public static List<Task> GetTasksForDate(int userId, DateTime date)
        {
            DateTime start = date.Date;
            DateTime end = date.Date.AddDays(1);
            using (DailyPlannerDBV3Entities context = new DailyPlannerDBV3Entities())
            {
                List<Task> tasks = context.Tasks.Where(x => x.UserId == userId && x.DueDate >= start && x.DueDate < end).OrderBy(x => x.DueDate).ToList();
                return tasks;
            }
        }

        public static List<Event> GetEventsForDate(int userId, DateTime date)
        {
            DateTime start = date.Date;
            DateTime end = date.Date.AddDays(1);
            using (DailyPlannerDBV3Entities context = new DailyPlannerDBV3Entities())
            {
                List<Event> events = context.Events.Where(x => x.UserId == userId && x.StartDate >= start && x.StartDate < end).OrderBy(x => x.StartDate).ToList();
                return events;
            }
        }

        public static List<Note> GetNotes(int userId)
        {
            using (DailyPlannerDBV3Entities context = new DailyPlannerDBV3Entities())
            {
                List<Note> notes = context.Notes.Where(x => x.UserId == userId).OrderBy(x => x.CreatedAt).ToList();
                return notes;
            }
        }

        public static int CountTasksForDate(int userId, DateTime date)
        {
            DateTime start = date.Date;
            DateTime end = date.Date.AddDays(1);
            using (DailyPlannerDBV3Entities context = new DailyPlannerDBV3Entities())
            {
                int count = context.Tasks.Count(x => x.UserId == userId && x.DueDate >= start && x.DueDate < end);
                return count;
            }
        }

        public static void SaveTask(Task task, bool isNew)
        {
            using (DailyPlannerDBV3Entities context = new DailyPlannerDBV3Entities())
            {
                if (isNew)
                {
                    context.Tasks.Add(task);
                }
                else
                {
                    context.Entry(task).State = System.Data.Entity.EntityState.Modified;
                }
                context.SaveChanges();
            }
        }

        public static void SaveEvent(Event item, bool isNew)
        {
            using (DailyPlannerDBV3Entities context = new DailyPlannerDBV3Entities())
            {
                if (isNew)
                {
                    context.Events.Add(item);
                }
                else
                {
                    context.Entry(item).State = System.Data.Entity.EntityState.Modified;
                }
                context.SaveChanges();
            }
        }

        public static void SaveNote(Note note, bool isNew)
        {
            using (DailyPlannerDBV3Entities context = new DailyPlannerDBV3Entities())
            {
                if (isNew)
                {
                    context.Notes.Add(note);
                }
                else
                {
                    context.Entry(note).State = System.Data.Entity.EntityState.Modified;
                }
                context.SaveChanges();
            }
        }

        public static void SaveReminder(string kind, int itemId, DateTime startDate, string title, int offsetMinutes)
        {
            using (DailyPlannerDBV3Entities context = new DailyPlannerDBV3Entities())
            {
                if (kind == "Задача")
                {
                    List<Reminder> old = context.Reminders.Where(x => x.TaskId == itemId).ToList();
                    for (int i = 0; i < old.Count; i++)
                    {
                        context.Reminders.Remove(old[i]);
                    }
                }
                if (kind == "Событие")
                {
                    List<Reminder> old = context.Reminders.Where(x => x.EventId == itemId).ToList();
                    for (int i = 0; i < old.Count; i++)
                    {
                        context.Reminders.Remove(old[i]);
                    }
                }
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
                    context.Reminders.Add(reminder);
                }
                context.SaveChanges();
            }
        }

        public static void DeleteTask(Task task)
        {
            using (DailyPlannerDBV3Entities context = new DailyPlannerDBV3Entities())
            {
                int taskId = task.Id;
                List<Reminder> reminders = context.Reminders.Where(x => x.TaskId == taskId).ToList();
                for (int i = 0; i < reminders.Count; i++)
                {
                    context.Reminders.Remove(reminders[i]);
                }
                Task found = context.Tasks.FirstOrDefault(x => x.Id == taskId);
                if (found != null)
                {
                    context.Tasks.Remove(found);
                }
                context.SaveChanges();
            }
        }

        public static void DeleteEvent(Event item)
        {
            using (DailyPlannerDBV3Entities context = new DailyPlannerDBV3Entities())
            {
                int eventId = item.Id;
                List<Reminder> reminders = context.Reminders.Where(x => x.EventId == eventId).ToList();
                for (int i = 0; i < reminders.Count; i++)
                {
                    context.Reminders.Remove(reminders[i]);
                }
                Event found = context.Events.FirstOrDefault(x => x.Id == eventId);
                if (found != null)
                {
                    context.Events.Remove(found);
                }
                context.SaveChanges();
            }
        }

        public static void DeleteNote(Note note)
        {
            using (DailyPlannerDBV3Entities context = new DailyPlannerDBV3Entities())
            {
                int noteId = note.Id;
                Note found = context.Notes.FirstOrDefault(x => x.Id == noteId);
                if (found != null)
                {
                    context.Notes.Remove(found);
                }
                context.SaveChanges();
            }
        }

        public static void SetTaskStatus(Task task, string status)
        {
            using (DailyPlannerDBV3Entities context = new DailyPlannerDBV3Entities())
            {
                Task found = context.Tasks.FirstOrDefault(x => x.Id == task.Id);
                if (found != null)
                {
                    found.Status = status;
                    context.SaveChanges();
                }
            }
        }
    }
}
