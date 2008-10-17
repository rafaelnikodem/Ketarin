﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

namespace Ketarin
{
    class DbManager
    {
        private static SQLiteConnection m_DbConn;

        public static SQLiteConnection Connection
        {
            get
            {
                if (m_DbConn == null)
                {
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Ketarin\\jobs.db";
                    // Is a special path set in the registry?
                    string regPath = CDBurnerXP.Settings.GetValue("Ketarin", "DatabasePath", "") as string;
                    if (!string.IsNullOrEmpty(regPath) && File.Exists(regPath))
                    {
                        path = regPath;
                    }
                    // Or is there a database file in the startup directory?
                    string localPath = Path.Combine(Application.StartupPath, "jobs.db");
                    if (File.Exists(localPath))
                    {
                        path = localPath;
                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    string connString = string.Format("Data Source={0};Version=3;", path);
                    if (!File.Exists(path))
                    {
                        connString += "New=True;";
                    }
                    m_DbConn = new SQLiteConnection(connString);
                    m_DbConn.Open();
                }
                return m_DbConn;
            }
        }

        public static void LoadOrCreateDatabase()
        {
            using (IDbCommand command = Connection.CreateCommand())
            {
                command.CommandText = @"CREATE TABLE IF NOT EXISTS jobs  
                                        (JobId              INTEGER PRIMARY KEY,
                                         ApplicationName    TEXT,
                                         TargetPath         TEXT,
                                         FixedDownloadUrl   TEXT,
                                         FileHippoId        TEXT,
                                         DateAdded          DATE,
                                         LastUpdated        DATE,
                                         PreviousLocation   TEXT,
                                         DeletePreviousFile INTEGER,
                                         SourceType         INTEGER,
                                         IsEnabled          INTEGER);";
                command.ExecuteNonQuery();
            }

            using (IDbCommand command = Connection.CreateCommand())
            {
                command.CommandText = @"CREATE TABLE IF NOT EXISTS variables  
                                        (JobId              INTEGER,
                                         VariableName       TEXT,
                                         Url                TEXT,
                                         StartText          TEXT,
                                         EndText            TEXT);";
                command.ExecuteNonQuery();
            }
        }


        public static IEnumerable<ApplicationJob> GetJobs()
        {
            IDbCommand command = Connection.CreateCommand();
            command.CommandText = "SELECT * FROM jobs ORDER BY ApplicationName";

            using (IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    ApplicationJob job = new ApplicationJob();
                    job.Hydrate(reader);
                    yield return job;
                }
            }
        }
    }
}
