using System.Collections.Generic;
using System.Windows;
using System.Linq;

namespace LigerShark.WebJobsVs.Dialog
{
    public partial class ProjectSelector2 : Window
    {
        private IEnumerable<string> _projectNames;

        public ProjectSelector2(IEnumerable<string> projectNames)
        {
            InitializeComponent();

            _projectNames = projectNames;
            
            if (projectNames.Any())
            {
                names.ItemsSource = _projectNames;
                names.SelectedIndex = 0;
            }
            else
            {
                names.IsEnabled = false;
                btnOk.IsEnabled = false;
            }
        }

        public string SelectedProjectName { get; set; }
        public string Schedule { get; set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SelectedProjectName = names.SelectedItem.ToString();
            Schedule = continuous.IsChecked.Value ? "continuous" : "triggered";
            DialogResult = true;
        }
    }
}
