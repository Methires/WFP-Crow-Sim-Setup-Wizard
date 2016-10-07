using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Linq;
using System.ComponentModel;

namespace CrowdSimSetupWizard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool StartUnity { get; set; } 
        private string _unityPath;
        public static string ProjectPath = AppDomain.CurrentDomain.BaseDirectory + "UnityCrowdSimAndGenerator";

        private bool _unityProjectExists = false;
        private bool _unityFound = false;

        public MainWindow()
        {
            InitializeComponent();         
            WizardWindow wizard = new WizardWindow(_unityPath, ProjectPath);
            wizard.ShowDialog();            
            Close();                     
        }       
    }
}
