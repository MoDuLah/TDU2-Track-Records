using System.Windows;
using TDU2_Track_Records.Properties;

namespace TDU2_Track_Records
{
    public partial class ObjectivesWindow : Window
    {
        readonly string connectionString = Settings.Default.connectionString;

        public ObjectivesWindow()
        {
            InitializeComponent();
        }
    }
}
