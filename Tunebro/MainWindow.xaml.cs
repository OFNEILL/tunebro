using FftSharp;
using Microsoft.Win32;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using ScottPlot;
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
using TuneBro;
using TuneBro.Business;
using Image = System.Windows.Controls.Image;
using Line = System.Windows.Shapes.Line;

namespace Tunebro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CollapsedWaveformButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Audio files (*.mp3;*.wav)|*.mp3;*.wav|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                // Handle the uploaded file (e.g., play it, save it, etc.)
                Business business = new Business();

                //direct to TuneLight.xml window
                //ExtractWaveform(business.Init(filePath));

                //open window detailing waveform
                ExtractWaveform(filePath);
            }
        }

        private void ExtractWaveform(string path)
        {
            var window = new System.Windows.Window();
            //add save button in top left of window
            var saveButton = new Button();
            saveButton.Content = "Save";
            saveButton.Click += (sender, e) =>
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PNG Image|*.png";
                if (saveFileDialog.ShowDialog() == true)
                {
                    var canvas = (Canvas)window.Content;
                    var bitmap = new RenderTargetBitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                    bitmap.Render(canvas);
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    using (var stream = saveFileDialog.OpenFile())
                    {
                        encoder.Save(stream);
                    }
                }
            };
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

                window.Height = 260;
                var scrollViewer = new ScrollViewer();
                scrollViewer.Content = canvas;
                scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                window.Content = scrollViewer;
                window.ShowDialog();
            }
        }

        private void LiveStreamButton_Click(object sender, RoutedEventArgs e)
        {
            //open LiveStreamViewer window
            var window = new LiveStreamViewer();
            window.Show();

            //close current window
            this.Close();
        }
    }
}