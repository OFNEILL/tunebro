using NAudio.Wave;
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
using TuneBro.Business;

namespace Tunebro
{
    /// <summary>
    /// Interaction logic for TuneLight.xaml
    /// </summary>
    public partial class TuneLight : Window
    {
        public static string workingPath = "";

        private static Window window;
        public TuneLight(string path)
        {
            workingPath = path;
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

        private void RunAudio(object sender, RoutedEventArgs e)
        {
            var window = new Window();
            var canvas = new Canvas();
            using (var reader = new AudioFileReader(workingPath))
            {
                float f = 0.0f;
                float max = 0.0f;
                int mid = 100;
                int yScale = 100;
                int xPos = 0;
                int read = 0;

                long samples = reader.Length / (reader.WaveFormat.Channels * reader.WaveFormat.BitsPerSample / 8);
                int batch = (int)Math.Max(40, samples / 4000); // waveform <= 4000 pixels in width
                float[] buffer = new float[batch];

                while ((read = reader.Read(buffer, 0, batch)) == batch)
                {
                    for (int n = 0; n < read; n++)
                    {
                        max = Math.Max(Math.Abs(buffer[n]), max);
                    }
                    var line = new Line();

                    //set xPos in rleation to point in buffer (time)
                    line.X1 = xPos; 
                    line.X2 = xPos;

                    //set yPos in relation to volume of buffer (amplitude)
                    line.Y1 = mid + (max * yScale);
                    line.Y2 = mid - (max * yScale);
                    line.StrokeThickness = 0.1;
                    line.Stroke = Brushes.Black;
                    canvas.Children.Add(line);
                    max = 0;
                    xPos++;
                }
                canvas.Width = xPos;
                canvas.Height = mid * 2;
            }
            window.Height = 260;
            var scrollViewer = new ScrollViewer();
            scrollViewer.Content = canvas;
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            window.Content = scrollViewer;
            window.ShowDialog();
        }
    }
}
