using System;
using System.Windows;
using System.IO;
using System.Windows.Forms;

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

        public static ConfigData Data;

        public WizardWindow()
        {
            InitializeComponent();

            Data.Tracking = false;
            Data.VisualFileExtension = "PNG|";
            Data.AnnotationsFileExtension = "TXT|";
            Data.AnnotationsType = 1;
        }

        private void Choose_Scenario_Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.Filter = "XML Files (*.xml)|*.xml";
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string fileName = dlg.FileName;
                Data.ScenarioFilePath = fileName;
                Scenario_File_Path.Text = fileName;
                File_Contents.Text = File.ReadAllText(fileName);
                ScenarioPage.CanSelectNextPage = true;
            }
        }

        private void Length_Value_Picker_Spinned(object sender, Xceed.Wpf.Toolkit.SpinEventArgs e)
        {
            Data.Length = (int)Length_Value_Picker.Value;
        }

        private void Tracking_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)Tracking_CheckBox.IsChecked)
            {
                Data.Tracking = true;
                Choose_Scenario_Button.IsEnabled = false;
                Instances_Value_Picker.IsReadOnly = true;
                Length_Value_Picker.IsReadOnly = false;
                Repeats_Header.Text = "Repeat session:";
                ScenarioPage.CanSelectNextPage = true;
            }
            else
            {
                Data.Tracking = false;
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
                Data.Length = (int)Length_Value_Picker.Value;
            }
            else if (sender.Equals(Instances_Value_Picker))
            {
                Data.Instances = (int)Instances_Value_Picker.Value;
            }
            else if (sender.Equals(Repeats_Value_Picker))
            {
                Data.Repeats = (int)Repeats_Value_Picker.Value;
            }
        }

        private void Crowd_Size_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Data.MaxPeople = (int)Crowd_Size_Slider.Value;
            if (Crowd_Number_Presentation != null)
            {
                Crowd_Number_Presentation.Text = Data.MaxPeople.ToString();
            }
        }

        private void Weather_Conditions_Checked(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(Sun))
            {
                Data.WeatherConditions = 1;
            }
            else if (sender.Equals(Rain))
            {
                Data.WeatherConditions = 2;
            }
            else if (sender.Equals(Snow))
            {
                Data.WeatherConditions = 3;
            }
            else if (sender.Equals(Overcast))
            {
                Data.WeatherConditions = 4;
            }
            else if (sender.Equals(Fog))
            {
                Data.WeatherConditions = 5;
            }
        }

        private void Day_Time_Checked(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(Morning))
            {
                Data.DayTime = 1;
            }
            else if (sender.Equals(Noon))
            {
                Data.DayTime = 2;
            }
            else if (sender.Equals(Afternoon))
            {
                Data.DayTime = 3;
            }
        }

        private void Scene_List_Checked(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(Square))
            {
                Data.SceneName = "Crowd Simulator Prototype";
            }
            else if (sender.Equals(Obstacles))
            {
                Data.SceneName = "Scene with obstacles";
            }
            else if (sender.Equals(City))
            {
                Data.SceneName = "City like scene";
            }
        }

        private void Results_Type_Checked(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(Type_Raw))
            {
                Data.VisualResultsType = 1;
            }
            else if (sender.Equals(Type_Sequences))
            {
                Data.VisualResultsType = 2;
            }
            else if (sender.Equals(Type_Sequences_Boxes))
            {
                Data.VisualResultsType = 3;
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
                Data.ResultsPath = dlg.SelectedPath;
                Results_Path.Text = Data.ResultsPath;
                ResultsPage.CanFinish = true;
            }
        }
    }
}
