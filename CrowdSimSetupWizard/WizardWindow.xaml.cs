using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Drawing.Imaging;
using Microsoft.Win32;

namespace CrowdSimSetupWizard
{
    /// <summary>
    /// Interaction logic for WizardWindow.xaml
    /// </summary>
    public partial class WizardWindow : Window
    { 
        private struct ConfigData
        {
            //Scene:
            public string SceneName;
            public int DayTime;
            public int WeatherConditions;
            //Crowd
            public string Models;
            public int MaxPeople;
            public string CrowdActions;
            //Scenario
            public bool Tracking;
            public string ScenarioFilePath;
            public int Length;
            public int Repeats;
            public int Instances;
            //Results
            public string ResultsPath;
            public bool BoundingBoxes;
            public int ScreenWidth;
            public int ScreenHeight;
            public int FrameRate;
            public int BufferSize;
            //Mode
            public string Mode;
            //Generation
            public int SceneSize;
        }

        private void InitializeData()
        {
            _data.SceneName = "";
            _data.DayTime = 0;
            _data.WeatherConditions = 0;
            //Crowd
            _data.Models = "";
            _data.MaxPeople = (int)Crowd_Size_Slider.Value;
            _data.CrowdActions = "";
            //Scenario
            _data.Tracking = false;
            _data.ScenarioFilePath = "";
            _data.Length = (int)Length_Value_Picker.Value;
            _data.Repeats = (int)Repeats_Value_Picker.Value;
            _data.Instances = (int)Instances_Value_Picker.Value;          
            //Results
            _data.ResultsPath = "";
            _data.BoundingBoxes = false;
            _data.ScreenWidth = (int)Width_Value_Picker.Value;
            _data.ScreenHeight = (int)Height_Value_Picker.Value;
            _data.FrameRate = (int)Framerate_Value_Picker.Value;
            _data.BufferSize = (int)Buffer_Size_Value_Picker.Value;
            //Mode
            _data.Mode = "simulation";
            //Generation
            _data.SceneSize = (int)Scene_Size_Value_Picker.Value;
        }

        private static ConfigData _data;
        private List<AnimationFile> _animations;
        private List<ModelFile> _models;
        private List<SceneFile> _scenes;
        private string _path = AppDomain.CurrentDomain.BaseDirectory;
        private string _unityPath = "C:\\Program Files\\Unity\\Editor\\Unity.exe";
        //private string _project = AppDomain.CurrentDomain.BaseDirectory + "Unity-CrowdSim-Prototype";
        public static string Project = AppDomain.CurrentDomain.BaseDirectory + "UnityCrowdSimAndGenerator";
        private StringBuilder _modelsFilter;
        private StringBuilder _actionsFilter;
        private BitmapImage _noImage;
        private bool _unityKilled = false;
        private string _screenshotDirectory = "D:/Screenshots";
        private DirectoryInfo _screenshotDirInfo;
        private BackgroundWorker _bw;
        private bool _simulationComplete = false;
        private DateTime _simulationStart;



        public WizardWindow()
        {
            InitializeComponent();
            InitializeData();
            FindUnity();
            _data.Tracking = false;
            _data.BoundingBoxes = true;
            _noImage = GetNoImage();
            
        }
        
        //private const string keyBase = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths";
        private void FindUnity()
        {
            //string fileName = "Unity.exe";

            //RegistryKey localMachine = Registry.LocalMachine;
            //RegistryKey fileKey = localMachine.OpenSubKey(string.Format(@"{0}\\{1}", keyBase, fileName));
            //object result = null;
            //if (fileKey != null)
            //{
            //    result = fileKey.GetValue(string.Empty);
            //}
            //fileKey.Close();
            //string u = (string)result;
            //_unityPath = u;
        }

        private void Scene_List_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.RadioButton action = sender as System.Windows.Controls.RadioButton;
            string sceneName = action.Content.ToString();
            _data.SceneName = sceneName;
            LoadScenePreviewImage(sceneName);
        }

        private void LoadScenePreviewImage(string sceneName)
        {
            string scenePreviewImagePath = GetScenePreviewPath(sceneName);
            if (scenePreviewImagePath != null)
            {
                Uri scenePreviewImageUri = new Uri(scenePreviewImagePath);
                BitmapImage previewImage = new BitmapImage(scenePreviewImageUri);
                Scene_Preview_Image.Stretch = System.Windows.Media.Stretch.Fill;
                Scene_Preview_Image.Source = previewImage;
            }
            else
            {
                Scene_Preview_Image.Source = _noImage;
            }            
        }

        private string GetScenePreviewPath(string sceneName)
        {
            string scenesPath = Project +" \\Assets\\Scenes\\";
            string[] scenePreviewFiles = Directory.GetFiles(scenesPath, sceneName + ".png", SearchOption.AllDirectories);
            if (scenePreviewFiles.Length != 0)
            {
                return scenePreviewFiles[0];
            }
            else
            {
                return null;//_project + "\\Assets\\Scenes\\noImage.png";
            }             
        }

        private void Day_Time_Checked(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(Morning))
            {
                _data.DayTime = 1;
            }
            else if (sender.Equals(Noon))
            {
                _data.DayTime = 2;
            }
            else if (sender.Equals(Afternoon))
            {
                _data.DayTime = 3;
            }
        }

        private void Weather_Conditions_Checked(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(Sun))
            {
                _data.WeatherConditions = 1;
            }
            else if (sender.Equals(Rain))
            {
                _data.WeatherConditions = 2;
            }
            else if (sender.Equals(Snow))
            {
                _data.WeatherConditions = 3;
            }
            else if (sender.Equals(Overcast))
            {
                _data.WeatherConditions = 4;
            }
            else if (sender.Equals(Fog))
            {
                _data.WeatherConditions = 5;
            }
        }

        private void Crowd_Size_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _data.MaxPeople = (int)Crowd_Size_Slider.Value;
            if (Crowd_Number_Presentation != null)
            {
                Crowd_Number_Presentation.Text = _data.MaxPeople.ToString();
            }
        }

        private void Models_List_Initialized(object sender, EventArgs e)
        {
            GetModelsList();
        }

        private void LoadModelPreviewImage(string modelName)
        {
            string modelPreviewImagePath = GetModelPreviewPath(modelName);
            if (modelPreviewImagePath != null)
            {
                Uri modelPreviewImageUri = new Uri(modelPreviewImagePath);
                BitmapImage previewImage = new BitmapImage(modelPreviewImageUri);
                Model_Preview_Image.Source = previewImage;
            }
            else
            {                
                Model_Preview_Image.Source = _noImage;
            }
            
        }

        private BitmapImage GetNoImage()
        {
            System.Drawing.Bitmap bitmap = Properties.Resources.noImage;
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }

        private string GetModelPreviewPath(string modelName)
        {
            string modelsPath = Project + "\\Assets\\Characters\\";
            string[] modelPreviewFiles = Directory.GetFiles(modelsPath, modelName + ".png", SearchOption.AllDirectories);
            if (modelPreviewFiles.Length != 0)
            {
                return modelPreviewFiles[0];
            }
            else
            {
                return null;
            }
        }

        private void Models_List_Selected(object sender, RoutedEventArgs e)
        {
           System.Windows.Controls.ListBox modelsList = sender as System.Windows.Controls.ListBox;
            if ((ModelFile)modelsList.SelectedItem !=null)
            {
                LoadModelPreviewImage(((ModelFile)modelsList.SelectedItem).ModelName);
            }           
        }

        private void Models_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox model = sender as System.Windows.Controls.CheckBox;            
            if ((bool)model.IsChecked)
            {
                _modelsFilter.Append("|" + model.Content);
            }
            else
            {
                _modelsFilter.Replace("|" + model.Content, string.Empty);
            }
        }

        private void Load_Scene_Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.Filter = "FBX Files (*.unity)|*.unity";
            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                string destPath = string.Format("{0}\\Assets\\Scenes\\{1}", Project, Path.GetFileName(dlg.FileName));
                Directory.CreateDirectory(Path.GetDirectoryName(destPath));

                if (!File.Exists(destPath))
                {
                    File.Copy(dlg.FileName, destPath);

                    string previewPath = Path.GetDirectoryName(dlg.FileName) + "\\" + Path.GetFileNameWithoutExtension(dlg.FileName) + ".png";

                    if (File.Exists(previewPath))
                    {
                        string destPreviewpath = Path.GetDirectoryName(destPath) + "\\" + Path.GetFileNameWithoutExtension(destPath) + ".png";
                        File.Copy(previewPath, destPreviewpath);
                    }
                }                                  
                GetScenesList();
            }
        }

        private void Generate_Scene_Button_Click(object sender, RoutedEventArgs e)
        {
            string command = string.Format(" -batchmode -projectPath {0} -executeMethod Preparer.PrepareSimulation", Project);
            _data.Mode = "generation";
            PrepareConfigFile();           
            Process.Start(_unityPath, command);
            System.Action refresh = () => GetScenesList();
            WaitForUnity(SceneBusyIndicator, "Generating scene...", ScenePage, refresh);
            GetScenesList();                               
        }

        private void Add_Model_Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "FBX Files (*.fbx)|*.fbx";
            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                string destPath = string.Format("{0}\\Assets\\Characters\\{1}\\{2}", Project, Path.GetFileNameWithoutExtension(dlg.FileName), Path.GetFileName(dlg.FileName));
                Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                File.Copy(dlg.FileName, destPath);

                string command = string.Format(" -batchmode -projectPath {0} -executeMethod Preparer.CloseAfterImporting", Project);
                Process.Start(_unityPath, command);
                System.Action refresh = () => GetModelsList();
                WaitForUnity(ModelsBusyIndicator, "Importing model...", ModelsPage, refresh);               
            }

        }

        private void GetModelsList()
        {
            if (_models != null)
            {
                _models.Clear();
            }
            _modelsFilter = new StringBuilder();
            string modelsPath = Project + "\\Assets\\Characters\\";
            string[] modelsFiles = Directory.GetFiles(modelsPath, "*.fbx", SearchOption.AllDirectories);
            _models = new List<ModelFile>();
            for (int i = 0; i < modelsFiles.Length; i++)
            {
                _models.Add(new ModelFile
                {
                    ModelName = Path.GetFileNameWithoutExtension(modelsFiles[i]),
                    ModelPath = modelsFiles[i]
                });
            }
            Models_List.ItemsSource = _models;
        }

        private void Animations_List_Initialized(object sender, EventArgs e)
        {
            GetAnimationsList();
        }

        private void Scenes_List_Initialized(object sender, EventArgs e)
        {
            GetScenesList();
        }
         
        private void GetScenesList()
        {
            if (_scenes != null)
            {
                _scenes.Clear();
            }
            //_actionsFilter = new StringBuilder();
            string scenesPath = Project + "\\Assets\\Scenes\\";
            string[] sceneFiles = Directory.GetFiles(scenesPath, "*.unity", SearchOption.AllDirectories);
            for (int i = 0; i < sceneFiles.Length; i++)
            {
                sceneFiles[i] = Path.GetFileNameWithoutExtension(sceneFiles[i]);
            }
            _scenes = new List<SceneFile>();
            for (int i = 0; i < sceneFiles.Length; i++)
            {
                SceneFile newScene = new SceneFile() { FileName = sceneFiles[i], IsFirst = i == 0 };

                if (newScene.IsFirst)
                {
                    LoadScenePreviewImage(newScene.FileName);
                }

                _scenes.Add(newScene);
            }
            Scenes_List.ItemsSource = _scenes;
        }

        private void GetAnimationsList()
        {
            if (_animations != null)
            {
                _animations.Clear();
            }
            _actionsFilter = new StringBuilder();  
            string animationsPath = Project + "\\Assets\\Resources\\Animations\\";
            string[] animationFiles = Directory.GetFiles(animationsPath, "*.fbx", SearchOption.AllDirectories);
            for (int i = 0; i < animationFiles.Length; i++)
            {
                animationFiles[i] = Path.GetFileNameWithoutExtension(animationFiles[i]);
            }
            _animations = new List<AnimationFile>();
            foreach (string file in animationFiles)
            {
                _animations.Add(new AnimationFile() { FileName = file });
            }
            Actions_List.ItemsSource = _animations;
        }

        private void Actions_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox action = sender as System.Windows.Controls.CheckBox;           
            if ((bool)action.IsChecked)
            {
                _actionsFilter.Append("|" + action.Content);
            }
            else
            {
                _actionsFilter.Replace("|" + action.Content, string.Empty);
            }
        }

        private void Add_Action_Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.Filter = "FBX Files (*.fbx)|*.fbx";
            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                int atCounter = Path.GetFileNameWithoutExtension(dlg.FileName).Count(x => x == '@');
                if (atCounter > 0)
                { 
                    string destPath = Project + "\\Assets\\Resources\\Animations\\" + Path.GetFileName(dlg.FileName);
                    File.Copy(dlg.FileName, destPath, true);

                    string command = string.Format(" -batchmode -projectPath {0} -executeMethod Preparer.CloseAfterImporting", Project);
                    Process.Start(_unityPath, command);

                    System.Action refresh = () => GetAnimationsList();
                    WaitForUnity(ActionsBusyIndicator, "Importing action...", ActionsPage, refresh);
                }
                else
                {
                    System.Windows.MessageBox.Show("Animation file has invalid name.\n '@' symbol must separate mocap actor id from action name.","Invalid file name",MessageBoxButton.OK);
                }
                
            }
        }

        private void WaitForUnity(Xceed.Wpf.Toolkit.BusyIndicator busyIndicator, string busyMessage, Xceed.Wpf.Toolkit.WizardPage page, System.Action refershView)
        {            
            busyIndicator.IsBusy = true;
            page.CanSelectNextPage = false;
            busyIndicator.BusyContent = busyMessage;
            SetUnityDependentControlls(false);
            BackgroundWorker unitySpy = new BackgroundWorker();

            unitySpy.DoWork += (o, ea) =>
            {
                bool waitForGeneration = true;
                do
                {
                    var unityProcess = Process.GetProcessesByName("Unity");
                    waitForGeneration = unityProcess.Length > 0;
                }
                while (waitForGeneration);
            };

            unitySpy.RunWorkerCompleted += (o, ea) =>
            {
                busyIndicator.IsBusy = false;
                page.CanSelectNextPage = true;
                refershView();
                SetUnityDependentControlls(true);
            };

            unitySpy.RunWorkerAsync();
        }

        private void SetUnityDependentControlls(bool areEnabled)
        {
            Generate_Scene_Button.IsEnabled = areEnabled;
            Add_Action_Button.IsEnabled = areEnabled;
            Add_Model_Button.IsEnabled = areEnabled;
            
        }

        private void Choose_Scenario_Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.Filter = "XML Files (*.xml)|*.xml";
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string fileName = dlg.FileName;
                ScenarioValidator validator = new ScenarioValidator();
                if (validator.ValidateScenario(fileName))
                {
                    _data.ScenarioFilePath = fileName;
                    Scenario_File_Path.Text = fileName;
                    File_Contents.Text = File.ReadAllText(fileName);
                    ScenarioPage.CanSelectNextPage = true;
                }
                else
                {
                    _data.ScenarioFilePath = "";
                    Scenario_File_Path.Text = "";
                    File_Contents.Text = "";
                    ScenarioPage.CanSelectNextPage = false;
                }
            }
        }

        private void Create_Scenario_Button_Click(object sender, RoutedEventArgs e)
        {
            ScenarioCreatorWindow scenarioWindow = new ScenarioCreatorWindow();
            scenarioWindow.ShowDialog();
        }

        private void Value_Picker_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (sender.Equals(Length_Value_Picker))
            {
                _data.Length = (int)Length_Value_Picker.Value;
            }
            else if (sender.Equals(Instances_Value_Picker))
            {
                _data.Instances = (int)Instances_Value_Picker.Value;
            }
            else if (sender.Equals(Repeats_Value_Picker))
            {
                _data.Repeats = (int)Repeats_Value_Picker.Value;
            }
            else if(sender.Equals(Width_Value_Picker))
            {
                _data.ScreenWidth = (int)Width_Value_Picker.Value;
            }
            else if (sender.Equals(Height_Value_Picker))
            {
                _data.ScreenHeight = (int)Height_Value_Picker.Value;
            }
            else if (sender.Equals(Framerate_Value_Picker))
            {
                _data.FrameRate = (int)Framerate_Value_Picker.Value;
            }
            else if (sender.Equals(Scene_Size_Value_Picker))
            {
                _data.SceneSize = (int)Scene_Size_Value_Picker.Value;
            }
            else if (sender.Equals(Buffer_Size_Value_Picker))
            {
                _data.BufferSize = (int)Buffer_Size_Value_Picker.Value;
            }
        }

        private void Tracking_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)Tracking_CheckBox.IsChecked)
            {
                _data.Tracking = true;
                Choose_Scenario_Button.IsEnabled = false;
                Instances_Value_Picker.IsReadOnly = true;
                Length_Value_Picker.IsReadOnly = false;
                Repeats_Header.Text = "Repeat tracking session:";
                ScenarioPage.CanSelectNextPage = true;
            }
            else
            {
                _data.Tracking = false;
                Choose_Scenario_Button.IsEnabled = true;
                Instances_Value_Picker.IsReadOnly = false;
                Length_Value_Picker.IsReadOnly = true;
                Repeats_Header.Text = "Repeat scenario:";
                if (Scenario_File_Path.Text != "")
                {
                    ScenarioPage.CanSelectNextPage = true;
                }
                else
                {
                    ScenarioPage.CanSelectNextPage = false;
                }
            }
        }

        private void Choose_Results_Button_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new FolderBrowserDialog();
            DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                _data.ResultsPath = dlg.SelectedPath;
                _screenshotDirectory = _data.ResultsPath;
                Results_Path.Text = _data.ResultsPath;

                //ResultsPage.CanFinish = true;
                ResultsPage.CanSelectNextPage = true;
            }
        }

        private void Results_Type_Checked(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(Type_Sequences))
            {
                _data.BoundingBoxes = false;
            }
            else if (sender.Equals(Type_Sequences_Boxes))
            {
                _data.BoundingBoxes = true;
            }
        }

        private void PrepareConfigFile()
        {
            _data.Models = _modelsFilter.ToString();
            _data.CrowdActions = _actionsFilter.ToString();
            string configPath = Project + "\\Assets\\config.xml";
            XmlDocument config = new XmlDocument();
            config.Load(configPath);
            XmlElement configElement = config.DocumentElement;
            for (int i = 0; i < configElement.ChildNodes.Count; i++)
            {
                if (configElement.ChildNodes[i].Name == "scene")
                {
                    configElement.ChildNodes[i].Attributes[0].Value = _data.SceneName;
                    configElement.ChildNodes[i].Attributes[1].Value = _data.DayTime.ToString();
                    configElement.ChildNodes[i].Attributes[2].Value = _data.WeatherConditions.ToString();
                }
                else if (configElement.ChildNodes[i].Name == "crowd")
                {
                    configElement.ChildNodes[i].Attributes[0].Value = _data.Models;
                    configElement.ChildNodes[i].Attributes[1].Value = _data.MaxPeople.ToString();
                    configElement.ChildNodes[i].Attributes[2].Value = _data.CrowdActions.ToString();
                }
                else if (configElement.ChildNodes[i].Name == "simulation")
                {
                    configElement.ChildNodes[i].Attributes[0].Value = _data.Tracking.ToString();
                    configElement.ChildNodes[i].Attributes[1].Value = _data.Tracking ? "" : _data.ScenarioFilePath.ToString();
                    configElement.ChildNodes[i].Attributes[2].Value = _data.Length.ToString();
                    configElement.ChildNodes[i].Attributes[3].Value = _data.Repeats.ToString();
                    configElement.ChildNodes[i].Attributes[4].Value = _data.Instances.ToString();
                }
                else if (configElement.ChildNodes[i].Name == "results")
                {
                    configElement.ChildNodes[i].Attributes[0].Value = _data.ResultsPath.ToString();
                    configElement.ChildNodes[i].Attributes[1].Value = _data.BoundingBoxes.ToString();
                    configElement.ChildNodes[i].Attributes[2].Value = _data.ScreenWidth.ToString();
                    configElement.ChildNodes[i].Attributes[3].Value = _data.ScreenHeight.ToString();
                    configElement.ChildNodes[i].Attributes[4].Value = _data.FrameRate.ToString();
                    configElement.ChildNodes[i].Attributes[5].Value = _data.BufferSize.ToString();
                }
                else if (configElement.ChildNodes[i].Name == "mode")
                {
                    configElement.ChildNodes[i].Attributes[0].Value = _data.Mode;
                }
                else if (configElement.ChildNodes[i].Name == "generation")
                {
                    configElement.ChildNodes[i].Attributes[0].Value = _data.SceneSize.ToString();
                }
            }
            config.Save(configPath);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            KillUnityProcess();
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {          
            _data.Mode = "simulation";           
            PrepareConfigFile();

            string command = string.Format(" -batchmode -projectPath {0} -executeMethod Preparer.PrepareSimulation", Project);
            Process.Start(_unityPath, command);

            abortSimButton.IsEnabled = true;
            startButton.IsEnabled = false; ;
            showInExplorerButton.IsEnabled = true;
            SummaryPage.CanFinish = false;
            SummaryPage.CanSelectPreviousPage = false;

            _unityKilled = false;
            _simulationComplete = false;

            ResetInfo();
            InitializeUnityMonitor();
            _bw.RunWorkerAsync();
        }

        private void KillUnityProcess()
        {
            Process[] unityProcessess = Process.GetProcessesByName("Unity");
            if (unityProcessess.Length > 0)
            {
                unityProcessess[0].Kill();
                _unityKilled = true;
            }
        }

        private bool IsUnityRunning()
        {
            Process[] unityProcessess = Process.GetProcessesByName("Unity");
            return unityProcessess.Length > 0;
        }

        private void abortSimButton_Click(object sender, RoutedEventArgs e)
        {
            KillUnityProcess();
            _bw.CancelAsync();
            //SimulationEndedInfo();
        }

        private void showInExplorerButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(_screenshotDirectory);
        }

        //public delegate void MonitorDelegate();
        public void MonitorUnity(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;           
            bool waitForGeneration = true;
            do
            {                
                var unityProcess = Process.GetProcessesByName("Unity");
                waitForGeneration = unityProcess.Length > 0;
                System.Threading.Thread.Sleep(500);               
                worker.ReportProgress(0);              
            }
            while (waitForGeneration);
        }

        private void UpdateSimulationInfo(object sender, ProgressChangedEventArgs e)
        {           
            statusValueLabel.Content = "Running...";
            statusValueLabel.Foreground = System.Windows.Media.Brushes.DarkOrange;
            _screenshotDirInfo = EarliestDirectory();
            memValueLabel.Content = GetUnityMemoryUsage() + " MB";

            if (_screenshotDirInfo != null)
            {
                repeatsValueabel.Content = SubdirectoriesCount(_screenshotDirInfo).ToString();
                DirectoryInfo earliestRepeat = EarliestSubdirectory(_screenshotDirInfo);
                framesCountValueLabel.Content = ScreenshotCount(earliestRepeat);                
                GeneratedFilesTextBlock.Text = GetFilesList(earliestRepeat);
            }            
        }

        private int GetUnityMemoryUsage()
        {
            int memUsage = 0;
            Process[] unityProcessess = Process.GetProcessesByName("Unity");
            if (unityProcessess.Length > 0)
            {
                var mem = unityProcessess[0].WorkingSet64;
                memUsage = (int)(mem / 1048576);
            }

            return memUsage;
        }

        private void ResetInfo()
        {
            repeatsValueabel.Content = 0;
            framesCountValueLabel.Content = 0;
            GeneratedFilesTextBlock.Text = string.Empty;
        }

        private void SimulationEndedInfo()
        {
            startButton.IsEnabled = true;
            abortSimButton.IsEnabled = false;
            _simulationComplete = true;
            SummaryPage.CanFinish = true;
            SummaryPage.CanSelectPreviousPage = true;
        }

        private void OnSimulationCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SimulationEndedInfo();
            if (!_unityKilled)
            {
                statusValueLabel.Content = "Finished";
                statusValueLabel.Foreground = System.Windows.Media.Brushes.Green;
            }
            else
            {
                statusValueLabel.Content = "Aborted";
                statusValueLabel.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void InitializeUnityMonitor()
        {          
            _bw = new BackgroundWorker();
            _bw.WorkerReportsProgress = true;
            _simulationStart = DateTime.Now;
            _bw.WorkerSupportsCancellation = true;
            _bw.DoWork += MonitorUnity;
            _bw.ProgressChanged += UpdateSimulationInfo;
            _bw.RunWorkerCompleted += OnSimulationCompleted;
        }

        private DirectoryInfo EarliestDirectory()
        {
            DirectoryInfo dir = new DirectoryInfo(_screenshotDirectory);
            DirectoryInfo[] files = dir.GetDirectories().OrderByDescending(p => p.CreationTime).ToArray();
            if (files.Length > 0)
            {
                if (files[0].CreationTime >= _simulationStart)
                {
                    return files[0];
                }              
            }
            return null;            
        }

        private DirectoryInfo EarliestSubdirectory(DirectoryInfo dir)
        {
            DirectoryInfo[] files = dir.GetDirectories().OrderByDescending(p => p.CreationTime).ToArray();
            return files[0];
        }

        private int SubdirectoriesCount(DirectoryInfo dir)
        {
            return dir.GetDirectories().Count();
        }

        private int ScreenshotCount(DirectoryInfo dir)
        {
            return dir.GetFiles("*.png", SearchOption.AllDirectories).Count();
        }

        private string GetFilesList(DirectoryInfo dir)
        {
            FileInfo[] screenshots = dir.GetFiles("*.png", SearchOption.AllDirectories);
            screenshots = screenshots.OrderByDescending(p => p.CreationTime).ToArray();

            if (screenshots != null && screenshots.FirstOrDefault() != null)
            {
                return screenshots[0].FullName;
            }
            else
            {
                return null;
            }          
        }
    }
}
