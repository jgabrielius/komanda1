﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using University_advisor.Tools;

namespace University_advisor
{
    class SqlDriver
    {
        private static SQLiteConnection Connect()
        {
            try
            {
                SQLiteConnection dbConnection = new SQLiteConnection("Data Source=../../Database.sqlite;Version=3;");
                dbConnection.Open();
                return dbConnection;
            }
            catch (SQLiteException e)
            {
                Logger.Log(e.StackTrace);
                return null;
            }
        }

        public static bool Execute(string sql)
        {
            SQLiteConnection dbConnection = Connect();
            if(dbConnection == null)
            {
                Logger.Log("Failed to connect to database. Aborting query");
                return false;
            }
            try
            {
                SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
                command.ExecuteNonQuery();
                Logger.Log("Query executed successfully");
                return true;
            }
            catch (SQLiteException e)
            {
                Logger.Log(e.StackTrace);
                return false;
            }
            dbConnection.Close();
        }

        public static ArrayList Fetch(string sql)
        {
            SQLiteConnection dbConnection = Connect();
            if (dbConnection == null)
            {
                Logger.Log("Failed to connect to database. Aborting query");
                return null;
            }
            try
            {
                ArrayList al = new ArrayList();
                SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Object[] temp = new object[reader.FieldCount];
                    reader.GetValues(temp);
                    al.Add(temp);
                }
                reader.Close();
                dbConnection.Close();
                return al;
            }
            catch (SQLiteException e)
            {
                Logger.Log(e.StackTrace);
                return null;
            }
        }
    }
}
