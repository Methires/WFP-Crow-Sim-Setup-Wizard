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
            WizardWindow wizard = new WizardWindow();
            wizard.ShowDialog();
            Close();
        }
    }
}
