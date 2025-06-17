# Android Data Processor Service – Installation Guide

**Version:** 1.0.0  
**Last Updated:** 2025-06-17  

---

## Table of Contents

1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Download & Prepare the Service](#download--prepare-the-service)
4. [Configuration](#configuration)
5. [Installation Steps](#installation-steps)
6. [Starting and Stopping the Service](#starting-and-stopping-the-service)
7. [Verification](#verification)
8. [Uninstallation](#uninstallation)
9. [Known Issues & Exceptions](#known-issues--exceptions)

---

## 1. Overview

**Android Data Processor Service** is a Windows Service that processes incoming data from Android devices and stores it in a SQL Server database.

---

## 2. Prerequisites

- **Operating System:** Windows 10, 11, or Windows Server 2016+
- **.NET Runtime:** .NET 6.0 or higher (ensure it matches your project's target)
- **Database:** SQL Server (local or accessible on the network)
- **Administrator Rights:** Required for installation
- **Network Port:** Default 8080 (can be changed in config)

---

## 3. Download & Prepare the Service

- Ensure you have the built service executable located at:  
  `src\AndroidDataProcessorService\bin\Release\AndroidDataProcessorService.exe`
- (Optional) Copy all contents of the `bin\Release` folder to a dedicated folder such as `C:\AndroidDataProcessorService\` if you want easier management, but this is not required for local installation.

---

## 4. Configuration

1. **Edit the Configuration File**
    - In the `src\AndroidDataProcessorService` directory, open `appsettings.json` or the relevant config file.
    - Update these values:
        - **Database connection string**: Update with your SQL Server details.
        - **HTTP port**: Change if 8080 is in use.
        - **Logging path**: (Optional) Set where logs are stored.

2. **Example connection string:**
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=localhost;Database=AndroidData;User Id=sa;Password=your_password;"
    }
    ```

---

## 5. Installation Steps

### A. Open PowerShell as Administrator

- Click Start, type `PowerShell`, right-click, select **Run as administrator**.

### B. Install the Service

Run this command (all on one line):

```powershell
& 'C:\Windows\System32\sc.exe' create AndroidDataProcessorService binPath= "C:"your path"\src\AndroidDataProcessorService\bin\Release\AndroidDataProcessorService.exe"
```

- This creates the service with the correct path to your executable.

---

## 6. Starting and Stopping the Service

**To Start the Service:**
```powershell
& 'C:\Windows\System32\sc.exe' start AndroidDataProcessorService
```

**To Stop the Service:**
```powershell
& 'C:\Windows\System32\sc.exe' stop AndroidDataProcessorService
```
Or, use the Services app (`services.msc`).

---

## 7. Verification

- Open **Services** (`services.msc`) and confirm the service shows as "Running".
- Check the log file directory for new logs.
- Test the endpoint (e.g. `http://localhost:8080/`) using a browser, Postman, or `curl`.

---

## 8. Uninstallation

**Stop and Remove the Service:**
```powershell
& 'C:\Windows\System32\sc.exe' stop AndroidDataProcessorService
& 'C:\Windows\System32\sc.exe' delete AndroidDataProcessorService
```

---

## 9. Known Issues & Exceptions

### Version: 1.0.0

- **Port Already in Use:**  
  If port 8080 is occupied, change the port in the configuration file and restart the service.
- **Database Connection Errors:**  
  - Ensure SQL Server is running and the connection string is correct.
  - The service must have network access to the database.
- **Permissions Issues:**  
  - PowerShell and all install commands must be run as administrator.
  - The service account must have permission to write logs and connect to SQL Server.
- **Service Fails to Start:**  
  - Check the Windows Event Viewer (`eventvwr.msc`) for detailed errors.
  - Review the log files in the configured log directory.

---

**End of Installation Guide**
