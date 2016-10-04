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
        public List<string> ActorsNames { get; set; }
        public ScenarioCreatorWindow Generator { get; set; }
        private string _selectedName;
        private float _probability;
        private string _animationsPath = MainWindow.ProjectPath + "\\Assets\\Resources\\Animations\\";
        private static int _id;

        public AddActionWindow()
        {
            _selectedName = "";
            InitializeComponent();
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
                    PrepareActorsTreeView(_selectedName);
                    save_button.IsEnabled = CheckSaveButtonStatus();
                }
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

        private void cancel_button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void save_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Action action = new Action();
                action.Name = _selectedName;
                action.Probability = _probability;
                action.Index = _id;
                _id++;
                foreach (TreeViewItem actorItem in Actors_TreeView.Items)
                {
                    Actor actor = new Actor();
                    actor.Name = actorItem.Header.ToString();
                    if (actorItem.Items.Count != 0)
                    {
                        if (action.Name.Equals("Walk") || action.Name.Equals("Run"))
                        {
                            actor.MocapId = "";
                            if (PreviousLevel != null)
                            {
                                SetPreviousId(actorItem.Items[0] as TreeViewItem, ref actor);
                            }
                        }
                        else
                        {
                            ComboBox mocapIdCb = actorItem.Items[0] as ComboBox;
                            actor.MocapId = mocapIdCb.SelectedItem as string;
                            if (PreviousLevel != null)
                            {
                                SetPreviousId(actorItem.Items[1] as TreeViewItem, ref actor);
                            }
                        }
                    }
                    action.Actors.Add(actor);
                }
                Generator.AddNewActionToLevel(action);
                Close();
            }
            catch (ScenarioException ScException)
            {
                MessageBox.Show(ScException.Message);
            }
        }

        private void probability_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _probability = (float)probability_Slider.Value / 100.0f;
            probability_textBlock.Text = _probability.ToString();
            save_button.IsEnabled = CheckSaveButtonStatus();
        }

        private void PrepareActorsTreeView(string actionName)
        {
            Actors_TreeView.Items.Clear();

            for (int i = 0; i < ActorsNames.Count; i++)
            {
                TreeViewItem actorItem = new TreeViewItem();
                actorItem.IsExpanded = true;
                actorItem.Header = string.Format("{0}", ActorsNames[i]);

                if (!(_selectedName.Equals("Walk") || _selectedName.Equals("Run")))
                {
                    ComboBox mocapIdComboBox = new ComboBox();
                    mocapIdComboBox.IsEditable = true;
                    mocapIdComboBox.IsReadOnly = true;
                    mocapIdComboBox.Text = "Choose mocap index";
                    mocapIdComboBox.ItemsSource = GetAllMocapId(actionName);
                    actorItem.Items.Add(mocapIdComboBox);
                }

                if (PreviousLevel != null)
                {
                    HashSet<int> prevId = GetPreviousId(actorItem.Header.ToString());
                    TreeViewItem prevItem = new TreeViewItem();
                    prevItem.IsExpanded = true;
                    prevItem.Header = "Previous action/s";
                    foreach (int id in prevId)
                    {
                        CheckBox checkId = new CheckBox();
                        checkId.Name = string.Format("fako_{0}", id.ToString());
                        checkId.Content = GetActionName(id);
                        prevItem.Items.Add(checkId);
                    }
                    actorItem.Items.Add(prevItem);
                }
                Actors_TreeView.Items.Add(actorItem);
            }
        }

        private HashSet<int> GetPreviousId(string actorName)
        {
            HashSet<int> previousId = new HashSet<int>();
            foreach (Action action in PreviousLevel.Actions)
            {
                foreach (Actor actor in action.Actors)
                {
                    if (actor.Name.Equals(actorName))
                    {
                        previousId.Add(action.Index);
                    }
                }
            }
            return previousId;
        }

        private void Actors_TreeView_Initialized(object sender, EventArgs e)
        {
            Actors_TreeView.Items.Add(string.Format("Choose animation and probablity."));
        }

        private void SetPreviousId(TreeViewItem prevItem, ref Actor actor)
        {
            List<int> previousId = new List<int>();
            foreach (CheckBox prevId in prevItem.Items)
            {
                if ((bool)prevId.IsChecked)
                {
                    string[] index = prevId.Name.Split('_');
                    previousId.Add(int.Parse(index[1]));
                }
            }
            if (previousId.Count == 0)
            {
                throw new ScenarioException("Choose at least one previous action.");
            }
            actor.PreviousActivitiesIndexes = previousId.ToArray();
        }

        private bool CheckSaveButtonStatus()
        {
            if (_selectedName.Equals("") || _probability == 0.0f)
            {
                return false;
            }
            return true;
        }

        private string GetActionName(int id)
        {
            foreach (Action action in PreviousLevel.Actions )
            {
                if (action.Index == id)
                {
                    return action.Name;
                }
            }
            return null;
        }
    }
}
