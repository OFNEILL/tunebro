using Microsoft.Win32;
using NAudio.Wave;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TuneBro.Business;

namespace Tunebro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Audio files (*.mp3;*.wav)|*.mp3;*.wav|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                // Handle the uploaded file (e.g., play it, save it, etc.)
                Business business = new Business();

                //direct to TuneLight.xml window
                ExtractWaveform(business.Init(filePath));
            }
        }

        private void ExtractWaveform(string path)
        {
            var window = new Window();
            var canvas = new Canvas();
            using (var reader = new AudioFileReader(path))
            {
                float f = 0.0f;
                float max = 0.0f;
                int mid = 100; //constant
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
                    Line line = new Line();

                    //set xPos in relation to point in buffer (time)
                    line.X1 = xPos;
                    line.X2 = xPos;

                    //set yPos in relation to volume of buffer (amplitude)
                    line.Y1 = mid + (max * yScale);
                    line.Y2 = mid - (max * yScale);
                    var y1 = line.Y1;
                    var y2 = line.Y2;
                    line.StrokeThickness = 0.1;
                    line.Stroke = Brushes.Black;
                    canvas.Children.Add(line);
                    max = 0;

                    xPos++;
                }

                var fft = FFT(buffer);
                float[] newBuffer = fft.Select(x => (float)x.Magnitude).ToArray();

                while ((read = reader.Read(newBuffer, 0, batch)) == batch)
                {
                    for (int n = 0; n < read; n++)
                    {
                        max = Math.Max(Math.Abs(buffer[n]), max);
                    }
                    Line line = new Line();

                    //set xPos in relation to point in buffer (time)
                    line.X1 = xPos;
                    line.X2 = xPos;

                    //set yPos in relation to volume of buffer (amplitude)
                    line.Y1 = mid + (max * yScale);
                    line.Y2 = mid - (max * yScale);
                    var y1 = line.Y1;
                    var y2 = line.Y2;
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

        private void ExtractFFTWave(string path)
        {
            var window = new Window();
            var canvas = new Canvas();
            using (var reader = new AudioFileReader(path))
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

                    //set xPos in reation to point in buffer (time)
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

        private static Complex[] FFT(float[] buffer)
        {
            int length = buffer.Length;
            Complex[] fft = new Complex[length];

            float[] even = new float[length / 2];
            float[] odd = new float[length / 2];

            for (int i = 0; i < (length / 2); i++)
            {
                even[i] = buffer[2 * i];
                odd[i] = buffer[2 * i + 1];
            }

            Complex[] complexEven = FFT(even);
            Complex[] complexOdd = FFT(odd);

            for (int i = 0; i < length / 2; i++)
            {
                float angle = -2 * MathF.PI * i / length;
                Complex complex = Complex.FromPolarCoordinates(1, angle);

                fft[i] = complexEven[i] + complex * complexOdd[i];
                fft[i + length / 2] = complexEven[i] - complex * complexOdd[i];
            }

            return fft;
        }
    }
}