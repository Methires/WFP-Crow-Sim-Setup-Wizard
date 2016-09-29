using System.IO;
using System.Windows;

namespace CrowdSimSetupWizard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool StartUnity { get; set; } 
        public MainWindow()
        {
            InitializeComponent();

            if (!UnityProjectExists())
            {
                var Result = MessageBox.Show("Unity project appears to be missinig from working directory!", "Unity project missing", MessageBoxButton.OK, MessageBoxImage.Error);
                if (Result == MessageBoxResult.OK)
                {
                    Close();
                }
            }
            else
            {
                WizardWindow wizard = new WizardWindow();
                wizard.ShowDialog();
                Close();
            }
            
        }

        private bool UnityProjectExists()
        {
            return Directory.Exists(WizardWindow.Project);
        }
    }
}
