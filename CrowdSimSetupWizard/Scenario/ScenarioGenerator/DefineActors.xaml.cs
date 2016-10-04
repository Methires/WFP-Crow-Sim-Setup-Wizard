using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Linq;

namespace CrowdSimSetupWizard
{
    public partial class DefineActors : Window
    {
        private int _actorsNumber;
        private bool _customNames;

        public DefineActors()
        {
            _actorsNumber = 1;
            _customNames = false;
            InitializeComponent();
        }

        private void Actor_Number_Picker_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            int previousValue = _actorsNumber;
            _actorsNumber = (int)Actor_Number_Picker.Value;
            if (_customNames)
            {
                if (previousValue > _actorsNumber)
                {
                    Actor_Names_TreeView.Items.Remove(Actor_Names_TreeView.Items[Actor_Names_TreeView.Items.Count - 1]);
                }
                else if ( previousValue < _actorsNumber)
                {
                    InitializeActorsNamesList();
                }
            }
        }

        private void Next_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<string> actorsNames;
                if (_customNames)
                {
                    actorsNames = new List<string>();
                    foreach (TreeViewItem item in Actor_Names_TreeView.Items)
                    {
                        TextBox itemTB = item.Header as TextBox;
                        if (itemTB.Text.Equals(""))
                        {
                            throw new ScenarioException("Empty string cannot be an actor's name.");
                        }
                        else
                        {
                            actorsNames.Add(itemTB.Text);
                        }
                        if (actorsNames.Count != actorsNames.Distinct().Count())
                        {
                            throw new ScenarioException("Each actor must have a unique name.");
                        }
                    }
                }
                else
                {
                    actorsNames = GetDefaultNames();
                }
                ScenarioCreatorWindow generator = new ScenarioCreatorWindow();
                generator.ActorsNumber = _actorsNumber;
                generator.ActorsNames = actorsNames;
                Hide();
                generator.ShowDialog();
                Close();
            }
            catch (ScenarioException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Custom_Names_Checkbox_Click(object sender, RoutedEventArgs e)
        {
            _customNames = Actor_Names_TreeView.IsEnabled = Actor_Names_ScrollView.IsEnabled = (bool)Custom_Names_Checkbox.IsChecked;
            if (_customNames)
            {
                InitializeActorsNamesList();
                if (Actor_Names_TreeView.Items.Count > _actorsNumber)
                {
                    do
                    {
                        Actor_Names_TreeView.Items.Remove(Actor_Names_TreeView.Items[Actor_Names_TreeView.Items.Count - 1]);
                    }
                    while (Actor_Names_TreeView.Items.Count != _actorsNumber);
                }
            }
        }

        private void InitializeActorsNamesList()
        {
            for (int i = Actor_Names_TreeView.Items.Count; i < _actorsNumber; i++)
            {
                TextBox actorTextBox = new TextBox();
                actorTextBox.Text = string.Format("Actor{0}", i);
                actorTextBox.MaxLines = actorTextBox.MinLines = 1;
                actorTextBox.MaxLength = 20;
                TreeViewItem item = new TreeViewItem();
                item.Header = actorTextBox;
                Actor_Names_TreeView.Items.Add(item);
            }
        }

        private List<string> GetDefaultNames()
        {
            List<string> names = new List<string>();
            for (int i = 0; i < _actorsNumber; i++)
            {
                names.Add(string.Format("Actor{0}", i));
            }

            return names;
        }
    }
}
