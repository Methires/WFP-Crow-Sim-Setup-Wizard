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
    /// <summary>
    /// Interaction logic for ChooseActionsWindow.xaml
    /// </summary>
    public partial class ChooseActionsWindow : Window
    {
        public ChooseActionsWindow()
        {
            InitializeComponent();
        }

        private void Next_Step_Button_Click(object sender, RoutedEventArgs e)
        {
            ChooseScenarioWindow scenarioWindow = new ChooseScenarioWindow();
            scenarioWindow.Show();
            Close();
        }

        private void Prev_Step_Button_Click(object sender, RoutedEventArgs e)
        {
            ChooseModelsWindow modelsWindow = new ChooseModelsWindow();
            modelsWindow.Show();
            Close();
        }
    }
}
