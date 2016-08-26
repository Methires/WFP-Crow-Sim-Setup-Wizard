using System;
using System.Windows;

namespace CrowdSimSetupWizard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            WizardWindow wizard = new WizardWindow();
            wizard.ShowDialog();
            string project = AppDomain.CurrentDomain.BaseDirectory + "Unity-CrowdSim-Prototype";
            string command = string.Format(" -batchmode -projectPath {0} -executeMethod Preparer.PrepareSimulation", project);
            System.Diagnostics.Process.Start("C:\\Program Files\\Unity\\Editor\\Unity.exe", command);
            Close();
        }
    }
}
