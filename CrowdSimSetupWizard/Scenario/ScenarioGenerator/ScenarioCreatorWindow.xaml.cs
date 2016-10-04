using System;
using System.Collections.Generic;
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
        }

        private void Remove_Level_Click(object sender, RoutedEventArgs e)
        {
            _levels.Remove(_levels.Last());
            if (_levels.Count == 0)
            {
                Remove_Level.IsEnabled = false;
            }
            UpdateTreeView();
        }

        private void UpdateTreeView()
        {
            Scenario_TreeView.Items.Clear();
            foreach (Level level in _levels)
            {
                TreeViewItem tvLevel = new TreeViewItem();
                tvLevel.Header = string.Format("Level: {0}", level.Index.ToString());
                tvLevel.IsExpanded = true;
                foreach (Action action in level.Actions)
                {
                    TreeViewItem tvAction = new TreeViewItem();
                    tvAction.Header = string.Format("{0} | Probability: {1}", action.Name, action.Probability);
                    tvLevel.Items.Add(tvAction);
                }

                Scenario_TreeView.Items.Add(tvLevel);
            }
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
                }
                else
                {
                    Add_Action_Button.IsEnabled = false;
                }
            }
            else
            {
                Add_Action_Button.IsEnabled = false;
            }
        }

        private void Add_Action_Button_Click(object sender, RoutedEventArgs e)
        {
            //DefineActors numberWindow = new DefineActors();
            //AddActionWindow actionWindow = new AddActionWindow();
            //actionWindow.PreviousLevel = GetPreviousLevel(_selectedItem);
            //numberWindow.ActionWindow = actionWindow;
            //numberWindow.ShowDialog();       
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

        }

        private void Edit_Action_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
