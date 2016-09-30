using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CrowdSimSetupWizard
{
    /// <summary>
    /// Interaction logic for AddActionWindow.xaml
    /// </summary>
    public partial class AddActionWindow : Window
    {
        public Level PreviousLevel { get; set; }
        public int ActorsNumber { get; set; }
        private string _selectedName;
        private string _selectedMocapId;
        private float _probability;
        private string _animationsPath = WizardWindow.Project + "\\Assets\\Resources\\Animations\\";
        private static int _id;

        public AddActionWindow()
        {
            _selectedName = "";
            InitializeComponent();
            //save_button.IsEnabled = CheckSaveButtonStatus();
        }

        private void action_Name_List_ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            string[] animationFiles = Directory.GetFiles(_animationsPath, "*.fbx", SearchOption.AllDirectories);
            for (int i = 0; i < animationFiles.Length; i++)
            {
                animationFiles[i] = System.IO.Path.GetFileNameWithoutExtension(animationFiles[i]);
            }
            HashSet<string> _actions = new HashSet<string>();
            _actions.Add("Walk");
            _actions.Add("Run");
            foreach (string name in animationFiles)
            {
                string[] animation = name.Split('@');
                _actions.Add(animation[animation.Length - 1]);
            }
            action_Name_List_ComboBox.ItemsSource = _actions;
        }

        private void action_Name_List_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if (cb != null)
            {
                _selectedName = cb.SelectedItem as string;
                if (_selectedName != null)
                {
                    _selectedMocapId = "";
                    PrepareActorsTreeView(_selectedName);
                }
                else
                {
                }
                save_button.IsEnabled = CheckSaveButtonStatus();
            }
        }

        private HashSet<string> GetAllMocapId(string animationName)
        {
            HashSet<string> mocapId = new HashSet<string>();
            string[] animationFiles = Directory.GetFiles(_animationsPath, "*.fbx", SearchOption.AllDirectories);
            for (int i = 0; i < animationFiles.Length; i++)
            {
                animationFiles[i] = System.IO.Path.GetFileNameWithoutExtension(animationFiles[i]);
            }
            foreach (string name in animationFiles)
            {
                string[] animation = name.Split('@');
                if (animation[animation.Length - 1].Equals(animationName))
                {
                    mocapId.Add(animation[0]);
                }
            }
            return mocapId;
        }

        private void action_MocapId_List_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if (cb != null)
            {
                _selectedMocapId = cb.SelectedItem as string;
                //save_button.IsEnabled = CheckSaveButtonStatus();
            }
        }

        private void cancel_button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void save_button_Click(object sender, RoutedEventArgs e)
        {
            Action action = new Action();
            action.Name = _selectedName;
            action.Probability = _probability;
            action.Index = _id;
            _id++;
            foreach (TreeViewItem actorItem in actors_TreeView.Items)
            {
                Actor actor = new Actor();
                actor.Name = actorItem.Header.ToString();
                if (actorItem.Items.Count != 0)
                {
                    if (!(action.Name.Equals("Walk") || action.Name.Equals("Run")))
                    {

                    } 
                }
            }
        }

        private void probability_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _probability = (float)probability_Slider.Value / 100.0f;
            probability_textBlock.Text = _probability.ToString();
            //save_button.IsEnabled = CheckSaveButtonStatus();
        }

        private bool CheckSaveButtonStatus()
        {
            if (_selectedName != "" && _probability > 0.0f)
            {
                if (_selectedName.Equals("Walk") || _selectedName.Equals("Run"))
                {
                    return true;
                }
                else
                {
                    if (_selectedMocapId != "")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void PrepareActorsTreeView(string actionName)
        {
            actors_TreeView.Items.Clear();

            for (int i = 0; i < ActorsNumber; i++)
            {
                TreeViewItem actorItem = new TreeViewItem();
                actorItem.IsExpanded = true;
                actorItem.Header = string.Format("Actor {0}", i);

                if (!(_selectedName.Equals("Walk") || _selectedName.Equals("Run")))
                {
                    TreeViewItem mocapIdItem = new TreeViewItem();
                    ComboBox mocapIdComboBox = new ComboBox();
                    mocapIdComboBox.IsEditable = true;
                    mocapIdComboBox.IsReadOnly = true;
                    mocapIdComboBox.Text = "Choose mocap index";
                    mocapIdComboBox.ItemsSource = GetAllMocapId(actionName);
                    mocapIdItem.Header = mocapIdComboBox;
                    actorItem.Items.Add(mocapIdItem);
                }

                if (PreviousLevel != null)
                {
                    HashSet<int> prevId = GetPreviousActions(actorItem.Header.ToString());
                    TreeViewItem prevItem = new TreeViewItem();
                    prevItem.IsExpanded = true;
                    prevItem.Header = "Previous action/s";
                    foreach (int id in prevId)
                    {
                        CheckBox checkId = new CheckBox();
                        checkId.Name = id.ToString();
                        prevItem.Items.Add(checkId);
                    }
                    actorItem.Items.Add(prevItem);
                }
                actors_TreeView.Items.Add(actorItem);
            }
        }

        private HashSet<int> GetPreviousActions(string actorName)
        {
            HashSet<int> previousId = new HashSet<int>();
            foreach (Action action in PreviousLevel.Actions)
            {
                foreach (Actor actor in action.Actors)
                {
                    if (actor.Name.Equals(actorName))
                    {
                        foreach (int id in actor.PreviousActivitiesIndexes)
                        {
                            previousId.Add(id);
                        } 
                    }
                }
            }
            return previousId;
        }
    }
}
