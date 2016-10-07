using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CrowdSimSetupWizard
{
    public partial class ChooseActorsForAction : Window
    {
        public List<string> ActorsNames { get; set; }
        public Level PreviousLevel { get; set; }
        public ScenarioCreatorWindow GeneratorMainWindow { get; set; }

        public ChooseActorsForAction()
        {
            InitializeComponent();     
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Next_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<string> chosenActors = new List<string>();
                foreach (CheckBox item in Actors_TreeView.Items)
                {
                    if ((bool)item.IsChecked)
                    {
                        chosenActors.Add(item.Content.ToString());
                    }
                }
                if (chosenActors.Count == 0)
                {
                    throw new ScenarioException("Choose at least one actor from list.");
                }
                AddActionWindow actionWindow = new AddActionWindow();
                actionWindow.ActorsNames = chosenActors;
                actionWindow.PreviousLevel = PreviousLevel;
                actionWindow.Generator = GeneratorMainWindow;
                Hide();
                actionWindow.ShowDialog();
                Close();

            }
            catch (ScenarioException ScEx)
            {
                MessageBox.Show(ScEx.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void PrepareActorsList()
        {
            foreach (string actor in ActorsNames)
            {
                CheckBox cb = new CheckBox();
                cb.Content = actor;
                Actors_TreeView.Items.Add(cb);
            }
        }
    }
}
