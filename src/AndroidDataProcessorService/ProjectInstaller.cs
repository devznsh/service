using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace AndroidDataProcessorService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            // Initialize the designer-generated components
            InitializeComponent();

            // Configure process installer (account to run under)
            this.serviceProcessInstaller1.Account = ServiceAccount.LocalSystem;

            // Configure service installer
            this.serviceInstaller1.ServiceName = "AndroidDataProcessorService";
            this.serviceInstaller1.DisplayName = "Android Data Processor Service";
            this.serviceInstaller1.Description = "Receives and processes data from Android applications and stores it in SQL Server";
            this.serviceInstaller1.StartType = ServiceStartMode.Automatic;
        }

        
    }
}