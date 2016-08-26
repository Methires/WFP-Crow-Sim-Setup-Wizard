using System;
using System.Windows;
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
        public struct ConfigData
        {
            public string SceneName;
            public int DayTime;
            public int WeatherConditions;
            public string Models;
            public int MaxPeople;
            public bool Tracking;
            public string ScenarioFilePath;
            public int Length;
            public int Repeats;
            public int Instances;
            public string ResultsPath;
            public string VisualFileExtension;
            public int VisualResultsType;
            public string AnnotationsFileExtension;
            public int AnnotationsType;
        }

        private static ConfigData _data;
        private List<AnimationFile> _animations;
        private List<ModelFile> _models;
        private string _path;
        private StringBuilder _modelsFilter;

        public WizardWindow()
        {
            _path = AppDomain.CurrentDomain.BaseDirectory;
            _modelsFilter = new StringBuilder();
            InitializeComponent();

            _data.Tracking = false;
            _data.VisualFileExtension = "|PNG";
            _data.AnnotationsFileExtension = "|TXT";
            _data.AnnotationsType = 1;
        }

        private void Choose_Scenario_Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.Filter = "XML Files (*.xml)|*.xml";
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string fileName = dlg.FileName;
                _data.ScenarioFilePath = fileName;
                Scenario_File_Path.Text = fileName;
                File_Contents.Text = File.ReadAllText(fileName);
                ScenarioPage.CanSelectNextPage = true;
            }
        }

        private void Length_Value_Picker_Spinned(object sender, Xceed.Wpf.Toolkit.SpinEventArgs e)
        {
            _data.Length = (int)Length_Value_Picker.Value;
        }

        private void Tracking_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)Tracking_CheckBox.IsChecked)
            {
                _data.Tracking = true;
                Choose_Scenario_Button.IsEnabled = false;
                Instances_Value_Picker.IsReadOnly = true;
                Length_Value_Picker.IsReadOnly = false;
                Repeats_Header.Text = "Repeat session:";
                ScenarioPage.CanSelectNextPage = true;
            }
            else
            {
                _data.Tracking = false;
                Choose_Scenario_Button.IsEnabled = true;
                Instances_Value_Picker.IsReadOnly = false;
                Length_Value_Picker.IsReadOnly = true;
                Repeats_Header.Text = "Play scenario:";
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
        }

        private void Crowd_Size_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _data.MaxPeople = (int)Crowd_Size_Slider.Value;
            if (Crowd_Number_Presentation != null)
            {
                Crowd_Number_Presentation.Text = _data.MaxPeople.ToString();
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

        private void Scene_List_Checked(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(Square))
            {
                _data.SceneName = "Crowd Simulator Prototype";
            }
            else if (sender.Equals(Obstacles))
            {
                _data.SceneName = "Scene with obstacles";
            }
            else if (sender.Equals(City))
            {
                _data.SceneName = "City like scene";
            }
        }

        private void Results_Type_Checked(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(Type_Raw))
            {
                _data.VisualResultsType = 1;
            }
            else if (sender.Equals(Type_Sequences))
            {
                _data.VisualResultsType = 2;
            }
            else if (sender.Equals(Type_Sequences_Boxes))
            {
                _data.VisualResultsType = 3;
            }
        }

        private void Visual_File_Type_Click(object sender, RoutedEventArgs e)
        {
            //NO YET IMPLEMENTED IN UNITY PROJECT
        }

        private void Annotations_File_Type_Click(object sender, RoutedEventArgs e)
        {
            //NO YET IMPLEMENTED IN UNITY PROJECT
        }

        private void Choose_Results_Button_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new FolderBrowserDialog();
            DialogResult result = dlg.ShowDialog();
            if(result == System.Windows.Forms.DialogResult.OK)
            {
                _data.ResultsPath = dlg.SelectedPath;
                Results_Path.Text = _data.ResultsPath;
                ResultsPage.CanFinish = true;
            }
        }

        private void Animations_List_Initialized(object sender, EventArgs e)
        {
            GetAnimationsList();
        }

        private void GetAnimationsList()
        {
            if (_animations != null)
            {
                _animations.Clear();
            }
            string animationsPath = _path + "Unity-CrowdSim-Prototype\\Assets\\Resources\\Animations\\";
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
            Animations_List.ItemsSource = _animations;
        }

        private void Add_Action_Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.Filter = "FBX Files (*.fbx)|*.fbx";
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string destPath = _path + "Unity-CrowdSim-Prototype\\Assets\\Resources\\Animations\\" + Path.GetFileName(dlg.FileName);
                File.Copy(dlg.FileName, destPath, true);
                GetAnimationsList();
            }
        }

        private void Models_List_Initialized(object sender, EventArgs e)
        {
            GetModelsList();
        }

        private void GetModelsList()
        {
            if (_models != null)
            {
                _models.Clear();
            }
            string modelsPath = _path + "Unity-CrowdSim-Prototype\\Assets\\Characters\\";
            string[] modelsFiles = Directory.GetFiles(modelsPath, "*.fbx", SearchOption.AllDirectories);
            _models = new List<ModelFile>();
            for (int i = 0; i < modelsFiles.Length; i++)
            {
                _models.Add(new ModelFile
                {
                    ModelName = Path.GetFileNameWithoutExtension(modelsFiles[i]),
                    ModelPath = modelsFiles[i],
                    CheckBoxName = Path.GetFileNameWithoutExtension(modelsFiles[i]) + "_Checkbox"
                });
            }
            Models_List.ItemsSource = _models;
        }

        private void Add_Model_Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.Filter = "FBX Files (*.fbx)|*.fbx";
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string destPath = _path + "Unity-CrowdSim-Prototype\\Assets\\Characters\\" + Path.GetFileName(dlg.FileName);
                File.Copy(dlg.FileName, destPath, true);
                GetModelsList();
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

        protected override void OnClosing(CancelEventArgs e)
        {
            bool wasCodeClosed = new StackTrace().GetFrames().FirstOrDefault(x => x.GetMethod() == typeof(Window).GetMethod("Close")) != null;
            if (wasCodeClosed)
            {
                _data.Models = _modelsFilter.ToString();
                XmlDocument config = new XmlDocument();
                config.Load(_path + "Unity-CrowdSim-Prototype\\Assets\\config.xml");
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
                    }
                    else if (configElement.ChildNodes[i].Name == "simulation")
                    {
                        configElement.ChildNodes[i].Attributes[0].Value = _data.Tracking.ToString();
                        configElement.ChildNodes[i].Attributes[1].Value = _data.Tracking? "" :_data.ScenarioFilePath.ToString();                     
                        configElement.ChildNodes[i].Attributes[2].Value = _data.Length.ToString();
                        configElement.ChildNodes[i].Attributes[3].Value = _data.Repeats.ToString();
                        configElement.ChildNodes[i].Attributes[4].Value = _data.Instances.ToString();
                    }
                    else if (configElement.ChildNodes[i].Name == "results")
                    {
                        configElement.ChildNodes[i].Attributes[0].Value = _data.ResultsPath.ToString();
                        configElement.ChildNodes[i].Attributes[1].Value = _data.VisualFileExtension.ToString();
                        configElement.ChildNodes[i].Attributes[2].Value = _data.VisualResultsType.ToString();
                        configElement.ChildNodes[i].Attributes[3].Value = _data.AnnotationsFileExtension.ToString();
                        configElement.ChildNodes[i].Attributes[4].Value = _data.AnnotationsType.ToString();
                    }
                }
                config.Save(_path + "Unity-CrowdSim-Prototype\\Assets\\config.xml");
            }
            base.OnClosing(e);
        }
    }
}
