using System;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AndroidDataProcessorService
{
    public class DataReceiver
    {
        private readonly string connectionString;
        private readonly string listenerUrl;
        private readonly string logFilePath;
        private HttpListener listener;
        private bool isRunning;

        public DataReceiver(string connString, string url, string logPath)
        {
            connectionString = connString;
            listenerUrl = url;
            logFilePath = logPath;
        }

        public void Start()
        {
            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add(listenerUrl);
                listener.Start();

                LogMessage($"HTTP listener started at {listenerUrl}");

                isRunning = true;
                Task.Run(() => ListenForRequests());

                LogMessage("Data receiver started successfully");
            }
            catch (Exception ex)
            {
                LogMessage($"Error starting data receiver: {ex.Message}");
                LogMessage(ex.ToString());
                throw;
            }
        }

        public void Stop()
        {
            try
            {
                isRunning = false;

                if (listener != null && listener.IsListening)
                {
                    listener.Stop();
                    LogMessage("HTTP listener stopped");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error stopping data receiver: {ex.Message}");
            }
        }

        private async Task ListenForRequests()
        {
            while (isRunning)
            {
                try
                {
                    HttpListenerContext context = await listener.GetContextAsync();
                    ProcessRequest(context);
                }
                catch (Exception ex)
                {
                    if (isRunning) // Only log if we're still supposed to be running
                    {
                        LogMessage($"Error in HTTP listener: {ex.Message}");
                    }
                }
            }
        }

        private async void ProcessRequest(HttpListenerContext context)
        {
            string deviceId = null;
            string requestBody = null;

            try
            {
                // Get device ID from headers
                deviceId = context.Request.Headers["Device-ID"];
                if (string.IsNullOrEmpty(deviceId))
                {
                    LogMessage("Error: Missing Device-ID header");
                    SendResponse(context, 400, "Missing required Device-ID header");
                    return;
                }

                // Read request body
                using (StreamReader reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                if (string.IsNullOrEmpty(requestBody))
                {
                    LogMessage($"Error: Empty request body from device {deviceId}");
                    SendResponse(context, 400, "Request body cannot be empty");
                    return;
                }

                LogMessage($"Received data from device {deviceId}: {requestBody.Substring(0, Math.Min(100, requestBody.Length))}...");

                // Store data in database
                bool success = await SaveDataToSqlDatabase(deviceId, requestBody);

                if (success)
                {
                    SendResponse(context, 200, "Data received successfully");
                }
                else
                {
                    SendResponse(context, 500, "Error processing data - unable to store in SQL Server");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error processing request: {ex.Message}");
                LogMessage($"Full exception: {ex}");

                try
                {
                    SendResponse(context, 500, "Internal server error");
                }
                catch
                {
                    // If we can't even send a response, just log it
                    LogMessage("Failed to send error response to client");
                }
            }
        }

        private async Task<bool> SaveDataToSqlDatabase(string deviceId, string dataPayload)
        {
            try
            {
                LogMessage($"Attempting database connection with string: {MaskConnectionString(connectionString)}");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        LogMessage("Opening SQL connection...");
                        await connection.OpenAsync();
                        LogMessage("SQL connection opened successfully");

                        string sql = @"
                            INSERT INTO AppData (DeviceID, DataPayload, ReceivedTime)
                            VALUES (@DeviceID, @DataPayload, GETDATE())
                        ";

                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@DeviceID", deviceId);
                            command.Parameters.AddWithValue("@DataPayload", dataPayload);

                            LogMessage("Executing SQL command...");
                            await command.ExecuteNonQueryAsync();
                            LogMessage("SQL command executed successfully");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"SQL error detail: {ex}");
                        return false;
                    }
                }

                LogMessage($"Successfully saved data from device {deviceId} to SQL database");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage($"SQL Database error: {ex.Message}");
                LogMessage($"Full exception details: {ex}");
                return false;
            }
        }

        private void SendResponse(HttpListenerContext context, int statusCode, string message)
        {
            try
            {
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";

                string responseJson = $"{{\"status\": {statusCode}, \"message\": \"{message}\"}}";
                byte[] buffer = Encoding.UTF8.GetBytes(responseJson);

                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close();

                LogMessage($"Sent response: {statusCode} - {message}");
            }
            catch (Exception ex)
            {
                LogMessage($"Error sending response: {ex.Message}");
            }
        }

        private void LogMessage(string message)
        {
            try
            {
                File.AppendAllText(logFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\r\n");
            }
            catch
            {
                // If logging fails, we can't do much about it
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
    }
}