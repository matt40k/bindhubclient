using System.ComponentModel;
using System.Configuration.Install;

namespace BindHub.Client.Service
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}