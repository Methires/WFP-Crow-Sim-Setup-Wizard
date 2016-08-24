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
    /// Interaction logic for ConfigureResultsWindow.xaml
    /// </summary>
    public partial class ConfigureResultsWindow : Window
    {
        public ConfigureResultsWindow()
        {
            InitializeComponent();
        }

        private void Prev_Step_Button_Click(object sender, RoutedEventArgs e)
        {
            ChooseScenarioWindow scenarioWindow = new ChooseScenarioWindow();
            scenarioWindow.Show();
            Close();
        }

        private void Run_Button_Click(object sender, RoutedEventArgs e)
        {


        }
    }
}
