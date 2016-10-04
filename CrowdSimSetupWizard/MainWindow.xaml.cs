using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Linq;

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

            if (!FindUnity())
            {
                var Result = MessageBox.Show("Could not find Unity.exe. Please make sure that Unity is installed on your machine.", "Unity.exe missing", MessageBoxButton.OK, MessageBoxImage.Error);
                if (Result == MessageBoxResult.OK)
                {
                    Close();
                }
            }
            
            WizardWindow wizard = new WizardWindow(_unityPath, ProjectPath);
            wizard.ShowDialog();
            Close();                     
        }
        private bool FindUnity()
        {
            var savedPath = Properties.Settings.Default.UnityPath;
            if (File.Exists(savedPath))
            {
                _unityPath = savedPath;
                return true;
            }

            bool unityFound = false;
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            List<string> possibleDirectories = new List<string>();
            foreach (var drive in allDrives)
            {
                if (drive.DriveType == DriveType.Fixed)
                {
                    unityFound = FindUnityAtDir(drive.Name);
                    if (unityFound)
                    {
                        break;
                    }
                }
            }
            return unityFound;
        }

        private bool FindUnityAtDir(string startingDir)
        {
            bool unityFound = false;
            string fileName = "Unity.exe";
            string directory = "Unity";

            string startPath = startingDir;
            List<string> possibleDirectories = SafeFileEnumerator.EnumerateDirectories(startPath, directory, SearchOption.AllDirectories).ToList();

            List<string> possibleFiles;
            foreach (string dir in possibleDirectories)
            {
                possibleFiles = SafeFileEnumerator.EnumerateFiles(dir, fileName, SearchOption.AllDirectories).ToList();
                if (possibleFiles.Count > 0)
                {
                    _unityPath = possibleFiles.FirstOrDefault();
                    Properties.Settings.Default.UnityPath = _unityPath;
                    Properties.Settings.Default.Save();
                    unityFound = true;
                    break;
                }
            }

            return unityFound;
        }

        private bool UnityProjectExists()
        {
            return Directory.Exists(ProjectPath);
        }
    }
}
