using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace AndroidDataProcessorService
{
    public partial class AndroidDataService : ServiceBase
    {
        private string connectionString;
        private string listenerUrl;
        private string logFilePath;
        private DataReceiver dataReceiver;

        public AndroidDataService()
        {
            InitializeComponent();

            this.ServiceName = "AndroidDataProcessorService";
            this.EventLog.Log = "Application";
            this.CanHandlePowerEvent = true;
            this.CanHandleSessionChangeEvent = true;
            this.CanPauseAndContinue = true;
            this.CanShutdown = true;
            this.CanStop = true;
        }

        // Public wrapper methods for Program.cs to use
        public void StartService()
        {
            OnStart(null);
        }

        public void StopService()
        {
            OnStop();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                connectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=AndroidData;Integrated Security=True";
                listenerUrl = "http://localhost:8080/";

                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                string logDirectory = Path.Combine(appDataPath, "AndroidDataProcessor");

                if (!Directory.Exists(logDirectory))
                    Directory.CreateDirectory(logDirectory);

                logFilePath = Path.Combine(logDirectory, "service_log.txt");

                LogToFile($"Android Data Service starting at {DateTime.Now}");
                LogToFile($"Connection string: {MaskConnectionString(connectionString)}");
                LogToFile($"Listener URL: {listenerUrl}");
                LogToFile($"Log file path: {logFilePath}");

                if (!TestDatabaseConnection())
                {
                    LogToFile("Database connection test failed. Service may not function properly.");
                }
                else
                {
                    LogToFile("Database connection test successful.");
                }

                dataReceiver = new DataReceiver(connectionString, listenerUrl, logFilePath);
                dataReceiver.Start();

                LogToFile("Service started successfully");
            }
            catch (Exception ex)
            {
                LogToFile($"Error starting service: {ex.Message}");
                LogToFile(ex.ToString());

                EventLog.WriteEntry($"Error starting service: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        protected override void OnStop()
        {
            try
            {
                LogToFile("Service stop requested");

                if (dataReceiver != null)
                {
                    dataReceiver.Stop();
                    LogToFile("Data receiver stopped");
                }

                LogToFile("Service stopped successfully");
            }
            catch (Exception ex)
            {
                LogToFile($"Error stopping service: {ex.Message}");
                LogToFile(ex.ToString());

                EventLog.WriteEntry($"Error stopping service: {ex.Message}", EventLogEntryType.Error);
            }
        }

        private void LogToFile(string message)
        {
            try
            {
                File.AppendAllText(logFilePath ?? "C:\\Temp\\android_service_error.log",
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\r\n");
            }
            catch
            {
                try
                {
                    File.AppendAllText("C:\\Temp\\android_service_fallback.log",
                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\r\n");
                }
                catch
                {
                    // If all logging fails, we can't do much more
                }
            }
        }

        private string MaskConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return "[null]";

            try
            {
                // Basic masking - hide password if present
                if (connectionString.Contains("Password="))
                {
                    int start = connectionString.IndexOf("Password=") + 9;
                    int end = connectionString.IndexOf(";", start);
                    if (end < 0) end = connectionString.Length;

                    return connectionString.Substring(0, start) + "********" +
                           (end < connectionString.Length ? connectionString.Substring(end) : "");
                }
                return connectionString; // No password to mask
            }
            catch
            {
                return "[masked]";
            }
        }

        private bool TestDatabaseConnection()
        {
            try
            {
                LogToFile("Testing database connection...");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    LogToFile("Database connection opened successfully");

                    using (SqlCommand command = new SqlCommand(
                        "SELECT CASE WHEN OBJECT_ID('AppData', 'U') IS NOT NULL THEN 1 ELSE 0 END",
                        connection))
                    {
                        int tableExists = (int)command.ExecuteScalar();
                        if (tableExists == 0)
                        {
                            LogToFile("ERROR: AppData table does not exist in the database");
                            return false;
                        }
                        LogToFile("AppData table found in database");
                    }

                    connection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                LogToFile($"Database connection test failed: {ex.Message}");
                LogToFile($"Full database connection error: {ex}");
                return false;
            }
        }
    }
}