# Android Data Processor Service

**Version:** 1.0.0  
**Last Updated:** 2025-06-17

---

## Overview

**Android Data Processor Service** is a Windows Service that receives, processes, and stores data sent from Android devices. Incoming HTTP requests are validated, logged, and their data is persisted to a SQL Server database. This service is designed for reliability, scalability, and ease of integration with data collection apps.

---

## Features

- Listens for HTTP requests from Android devices
- Validates device identity and payload structure
- Logs operations, errors, and exceptions to file
- Stores incoming data in a SQL Server database
- Robust error handling with clear logging
- Configurable port, database, and logging options

---

## Requirements

- **Operating System:** Windows 10, 11, or Windows Server 2016+
- **.NET Runtime:** .NET 6.0 or higher
- **Database:** SQL Server (local or accessible on the network)
- **Administrator Rights:** Required for installation and service management

---

## Installation

Please refer to [INSTALLATION.md](https://github.com/devznsh/service/blob/main/INSTALLTION.md) for a complete, step-by-step installation and configuration guide.

## Usage

1. **Start the Service**  
   Use the Services app (`services.msc`) or run:
   ```powershell
   & 'C:\Windows\System32\sc.exe' start AndroidDataProcessorService
   ```

2. **Send Data**  
   - Send HTTP POST requests to the configured port (default: 8080).
   - Ensure your requests include the required headers and payload structure as specified in your API documentation.

3. **Monitor Logs**  
   - Log files are created in the specified directory (see `appsettings.json`).
   - Review logs for incoming requests, errors, and service activity.

---

## Configuration

- Edit `appsettings.json` (located alongside the executable) to set:
  - SQL Server connection string
  - Listening port
  - Log file location and log level

---

## Troubleshooting

- **Service won’t start:**  
  - Check Windows Event Viewer (`eventvwr.msc`) and log files for errors.
  - Ensure correct permissions and valid configuration.

- **Port conflicts:**  
  - Change the listening port in `appsettings.json` if 8080 is already in use.

- **Database issues:**  
  - Confirm SQL Server is running and accessible.
  - Verify your connection string and credentials.

---

## Uninstallation

To stop and remove the service, run:
```powershell
& 'C:\Windows\System32\sc.exe' stop AndroidDataProcessorService
& 'C:\Windows\System32\sc.exe' delete AndroidDataProcessorService
```

