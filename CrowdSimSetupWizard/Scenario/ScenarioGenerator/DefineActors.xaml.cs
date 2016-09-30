using System.Windows;

namespace CrowdSimSetupWizard
{
    public partial class DefineActors : Window
    {
        public AddActionWindow ActionWindow { get; set; } 
        private int _actorNumber;

        public DefineActors()
        {
            _actorNumber = 1;
            InitializeComponent();
        }

        private void Actor_Number_Picker_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _actorNumber = (int)Actor_Number_Picker.Value;
        }

        private void Next_Button_Click(object sender, RoutedEventArgs e)
        {
            ActionWindow.ActorsNumber = _actorNumber;
            ActionWindow.Show();
            Close();
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
