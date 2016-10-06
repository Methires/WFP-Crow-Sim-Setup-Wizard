using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using System;
using System.Xml;

namespace CrowdSimSetupWizard
{
    public partial class ScenarioCreatorWindow : Window
    {
        public List<string> ActorsNames { get; set; }
        public int ActorsNumber { get; set; }
        private List<Level> _levels;
        private TreeViewItem _selectedItem;

        public ScenarioCreatorWindow()
        {
            _levels = new List<Level>();
            InitializeComponent();
        }

        private void Add_Level_Button_Click(object sender, RoutedEventArgs e)
        {
            Level level = new Level(_levels.Count);
            _levels.Add(level);
            Remove_Level.IsEnabled = true;
            UpdateTreeView();
            ValidateChange();
        }

        private void Remove_Level_Click(object sender, RoutedEventArgs e)
        {
            _levels.Remove(_levels.Last());
            if (_levels.Count == 0)
            {
                Remove_Level.IsEnabled = false;
            }
            UpdateTreeView();
            ValidateChange();
        }

        private void UpdateTreeView()
        {
            Scenario_TreeView.Items.Clear();
            foreach (Level level in _levels)
            {
                TreeViewItem tvLevel = new TreeViewItem();
                tvLevel.Name = string.Format("level_id_{0}", level.Index);
                tvLevel.Header = string.Format("Level: {0}", level.Index.ToString());
                tvLevel.IsExpanded = true;
                foreach (Action action in level.Actions)
                {
                    TreeViewItem tvAction = new TreeViewItem();
                    tvAction.Name = string.Format("action_id_{0}", action.Index);
                    tvAction.Header = string.Format("{0} | Probability: {1}", action.Name, action.Probability);
                    tvAction.IsExpanded = true;
                    tvLevel.Items.Add(tvAction);
                    
                    foreach (Actor actor in action.Actors)
                    {
                        TreeViewItem tvActor = new TreeViewItem();
                        tvActor.Header = actor.Name + GetPreviousActionsNames(actor, _levels.IndexOf(level), _levels);
                        tvActor.IsEnabled = false;
                        tvAction.Items.Add(tvActor);
                    }
                }
                Scenario_TreeView.Items.Add(tvLevel);
            }
        }

        private string GetPreviousActionsNames(Actor actor, int id, List<Level> levels)
        {
            string actionNames = "";
            if (id - 1 >= 0)
            {
                actionNames += " Previous: ";
                foreach (Action action in levels[id - 1].Actions)
                {
                    foreach (int prevId in actor.PreviousActivitiesIndexes)
                    {
                        if (action.Index == prevId)
                        {
                            actionNames += string.Format("{0} ", action.Name);
                        }
                    }
                }
            }

            return actionNames;
        }

        private void Scenario_TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeView tree = sender as TreeView;
            _selectedItem = tree.SelectedItem as TreeViewItem;
            if (_selectedItem != null)
            {
                if (_selectedItem.Parent.GetType() == typeof(TreeView))
                {
                    Add_Action_Button.IsEnabled = true;
                    Remove_Action_Button.IsEnabled = false;
                }
                else
                {
                    Add_Action_Button.IsEnabled = false;
                }

                if (_selectedItem.Parent.GetType() == typeof(TreeViewItem))
                {
                    Remove_Action_Button.IsEnabled = true;
                }
            }
            else
            {
                Add_Action_Button.IsEnabled = false;
                Remove_Action_Button.IsEnabled = false;
            }
        }

        private void Add_Action_Button_Click(object sender, RoutedEventArgs e)
        {
            ChooseActorsForAction actorsWindow = new ChooseActorsForAction();
            actorsWindow.ActorsNames = ActorsNames;
            actorsWindow.PreviousLevel = GetPreviousLevel(_selectedItem);
            actorsWindow.GeneratorMainWindow = this;
            actorsWindow.PrepareActorsList();
            actorsWindow.ShowDialog();
        }

        private Level GetPreviousLevel(TreeViewItem _selectedItem)
        {
            string[] header = _selectedItem.Header.ToString().Split(':');
            int index = int.Parse(header[header.Length - 1]);
            if (index - 1 >= 0)
            {
                return _levels[index - 1];
            }
            return null;
        }

        private void Remove_Action_Button_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem parent = _selectedItem.Parent as TreeViewItem;
            string[] actionItemName = _selectedItem.Name.Split('_');
            string[] levelItemName = parent.Name.Split('_');
            int actionIndex = int.Parse(actionItemName[2]);
            int levelIndex = int.Parse(levelItemName[2]);
            Action actionToRemove = new Action();
            foreach (Action action in _levels[levelIndex].Actions)
            {
                if (action.Index == actionIndex)
                {
                    actionToRemove = action;
                    break;
                }
            }
            _levels[levelIndex].Actions.Remove(actionToRemove);
            UpdateTreeView();
            ValidateChange();
            Remove_Action_Button.IsEnabled = false;
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void AddNewActionToLevel(Action action)
        {
            string[] header = _selectedItem.Header.ToString().Split(':');
            int index = int.Parse(header[header.Length - 1]);
            _levels[index].Actions.Add(action);
            UpdateTreeView();
            ValidateChange();
        }

        private void ValidateChange()
        {
            ScenarioValidator validator = new ScenarioValidator();
            string status;
            Save_Scenario_Button.IsEnabled = validator.ValidateGeneratedScenario(_levels, ActorsNames, out status);
            Error_Message_TextBlock.Text = status;
            if (status.Equals("Validated."))
            {
                Error_Message_TextBlock.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                Error_Message_TextBlock.Foreground = new SolidColorBrush(Colors.Red);
            }
        }
 
        private void Save_Scenario_Button_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "GeneratedScenario";
            dlg.DefaultExt = ".xml";
            dlg.Filter = "XML Files (*.xml)|*.xml";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                PrepareXmlFile(dlg.FileName);
            }
            Close();
        }

        private void PrepareXmlFile(string fileName)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement scenarioElement = (XmlElement)doc.AppendChild(doc.CreateElement("scenario"));
            foreach (Level level in _levels)
            {
                XmlElement levelElement = (XmlElement)scenarioElement.AppendChild(doc.CreateElement("level"));
                levelElement.SetAttribute("id", level.Index.ToString());
                foreach (Action action in level.Actions)
                {
                    XmlElement actionElement = (XmlElement)levelElement.AppendChild(doc.CreateElement("action"));
                    actionElement.SetAttribute("prob", action.Probability.ToString());
                    actionElement.SetAttribute("name", action.Name);
                    actionElement.SetAttribute("id", action.Index.ToString());
                    foreach (Actor actor in action.Actors)
                    {
                        XmlElement actorElement = (XmlElement)actionElement.AppendChild(doc.CreateElement("actor"));
                        actorElement.SetAttribute("name", actor.Name);
                        actorElement.SetAttribute("mocapId", actor.MocapId);
                        foreach (int prevId in actor.PreviousActivitiesIndexes)
                        {
                            XmlElement prevElement = (XmlElement)actorElement.AppendChild(doc.CreateElement("prev"));
                            prevElement.SetAttribute("id", prevId.ToString());
                        }
                    }
                }
            }
            doc.Save(fileName);
        }
    }
}
