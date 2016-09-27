using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Xml;

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
            //Mode
            public string Mode;
            //Generation
            public int SceneSize;

            public void Initialize()
            {
                SceneName = "";
                DayTime = 0;
                WeatherConditions = 0;
                //Crowd
                Models = "";
                MaxPeople = 0;
                CrowdActions = "";
                //Scenario
                Tracking = false;
                ScenarioFilePath = "";
                Length = 0;
                Repeats = 0;
                Instances = 0;
                //Results
                ResultsPath = "";
                BoundingBoxes = false;
                ScreenWidth = 0;
                ScreenHeight = 0;
                FrameRate = 0;
                //Mode
                Mode = "simulation";
                //Generation
                SceneSize = 0; ;
            }
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
        

        public WizardWindow()
        {
            //_path = AppDomain.CurrentDomain.BaseDirectory;
            InitializeComponent();
            //FindUnity();

            _data.Initialize();

            _data.Tracking = false;
            _data.BoundingBoxes = true;
            
        }

        //private void FindUnity()
        //{
        //    string[] probableDirectories = Directory.GetDirectories(@"C:\Program Files\","Unity",SearchOption.AllDirectories);
        //    foreach (var dir in probableDirectories)
        //    {
        //        Directory.GetFiles(dir, "Unity.exe", SearchOption.AllDirectories);
        //    }
        //}

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
            Uri scenePreviewImageUri = new Uri(scenePreviewImagePath);
            BitmapImage previewImage = new BitmapImage(scenePreviewImageUri);
            Scene_Preview_Image.Stretch = System.Windows.Media.Stretch.Fill;
            Scene_Preview_Image.Source = previewImage;
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
                return  Project + "\\Assets\\Scenes\\noImage.png";
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
            Uri modelPreviewImageUri = new Uri(modelPreviewImagePath);
            BitmapImage previewImage = new BitmapImage(modelPreviewImageUri);
            Model_Preview_Image.Source = previewImage;
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
                return Project + "\\Assets\\Scenes\\noImage.png";
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
            SceneBusyIndicator.IsBusy = true;
            SceneBusyIndicator.BusyContent = "Generating scene...";
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
                SceneBusyIndicator.IsBusy = false;
                GetScenesList();
            };

            unitySpy.RunWorkerAsync();                        
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
                InitializeUnityImport(ModelsBusyIndicator, "Importing model...");
                GetModelsList();
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
                    InitializeUnityImport(ActionsBusyIndicator, "Importing action...");
                    GetAnimationsList();
                }
                else
                {
                    System.Windows.MessageBox.Show("Animation file has invalid name.\n '@' symbol must separate mocap actor id from action name.","Invalid file name",MessageBoxButton.OK);
                }
                
            }
        }

        private void InitializeUnityImport(Xceed.Wpf.Toolkit.BusyIndicator busyIndicator, string busyMessage)
        {
            string command = string.Format(" -batchmode -projectPath {0} -executeMethod Preparer.CloseAfterImporting", Project);
            Process.Start(_unityPath, command);

            busyIndicator.IsBusy = true;
            busyIndicator.BusyContent = busyMessage;
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
            };

            unitySpy.RunWorkerAsync();
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
                Results_Path.Text = _data.ResultsPath;
                ResultsPage.CanFinish = true;
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
            bool wasCodeClosed = new StackTrace().GetFrames().FirstOrDefault(x => x.GetMethod() == typeof(Window).GetMethod("Close")) != null;
            if (wasCodeClosed)
            {
                _data.Mode = "simulation";
                PrepareConfigFile();
                string command = string.Format(" -batchmode -projectPath {0} -executeMethod Preparer.PrepareSimulation", Project);
                Process.Start(_unityPath, command);
            }
            base.OnClosing(e);
        }
    }
}
