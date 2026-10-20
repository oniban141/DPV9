using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using DailyPlaner.Entities;

namespace DailyPlaner
{
    public partial class App : Application
    {
        public static DailyPlannerDBV3Entities Context
        {
            get
            {
                DailyPlannerDBV3Entities context = new DailyPlannerDBV3Entities();
                return context;
            }
        }

        public static User CurrentUser;

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
    }
}
