using System.ComponentModel;

namespace CalendarSyncService
{
    /// <summary>
    /// Instalator usługi.
    /// </summary>
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        /// <summary>
        /// Konstruktor instalatora usługi.
        /// </summary>
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}
