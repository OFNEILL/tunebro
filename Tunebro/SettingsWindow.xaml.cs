using System.Windows;
using TuneBro.Business;
using TuneBro.Business.Objects;

namespace TuneBro
{
    public partial class SettingsWindow : Window
    {
        long diff = 0;

        public SettingsWindow(long MagnitudeDiff)
        {
            InitializeComponent();
            diff = MagnitudeDiff;

            MagnitudeDiffTextBox.Text = diff.ToString();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string magnitudeDiffValue = MagnitudeDiffTextBox.Text;

            // Validate the input
            if (long.TryParse(magnitudeDiffValue, out long value))
            {
                TuneBro.Business.Business business = new TuneBro.Business.Business();

                business.UpdateSettings(new Settings { MagnitudeDiff = value });
                MessageBox.Show($"Magnitude Differentiation set to: {value}");
                this.Close(); // Close the settings window
            }
            else
            {
                MessageBox.Show("Invalid input. Please enter a valid number.");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the settings window without saving
            this.Close();
        }
    }
}