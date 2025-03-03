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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Tunebro
{
    /// <summary>
    /// Interaction logic for TuneLight.xaml
    /// </summary>
    public partial class TuneLight : Window
    {
        private static Window window;
        public TuneLight()
        {
            InitializeComponent();
        }

        private void Init(object sender, RoutedEventArgs e)
        {
            StartColorFade(Colors.Red, Colors.Pink);
            window = Window.GetWindow(this);
        }

        // Triggered when the button is clicked
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StartColorFade(Colors.Green, Colors.Blue); // Trigger on button click with a color
        }

        // Helper method to start the color fade animation
        private void StartColorFade(Color fromColor, Color toColor)
        {
            // Get the storyboard resource
            Storyboard colorFadeStoryboard = (Storyboard)Resources["PageFade"];

            // Set the From and To values of the ColorAnimation dynamically
            this.Background = new SolidColorBrush(fromColor);
            ColorAnimation colourAnimation = (ColorAnimation)colorFadeStoryboard.Children[0];
            colourAnimation.From = fromColor;
            colourAnimation.To = toColor;

            // Begin the animation of the ColorAnimation within the storyboard
            //colorFadeStoryboard.Begin(window.Resources);
            //this.Background = new SolidColorBrush(toColor);
        }
    }
}
