using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using NAudio.Wave;
using FftSharp;
using System.Drawing;
using System.Windows.Media;
using NAudio.Dmo;
using ScottPlot.Interactivity.UserActionResponses;

namespace TuneBro
{
    public partial class LiveStreamViewer : System.Windows.Window
    {
        private WaveInEvent? Wave;
        private readonly double[] AudioValues;
        private readonly double[] FftValues;
        private readonly int SampleRate = 44100;
        private readonly int BitDepth = 16;
        private readonly int ChannelCount = 1;
        private readonly int BufferMilliseconds = 20;
        private readonly DispatcherTimer timer;
        private long colourNum = 0;
        private long prevColourNum = 0;

        public LiveStreamViewer()
        {
            InitializeComponent();

            AudioValues = new double[SampleRate * BufferMilliseconds / 1000];
            double[] paddedAudio = Pad.ZeroPad(AudioValues);
            double[] filter = FftSharp.Filter.LowPass(paddedAudio, SampleRate, 10000);
            filter = FftSharp.Filter.LowPass(filter, SampleRate, 5000);

            System.Numerics.Complex[] fftComplex = FftSharp.FFT.Forward(filter);

            double[] fftMag = FftSharp.FFT.Magnitude(fftComplex);

            FftValues = new double[fftMag.Length];

            double fftPeriod = FftSharp.FFT.FrequencyResolution(SampleRate, fftMag.Length);

            wpfPlot.Plot.Add.Signal(FftValues, 1.0 / fftPeriod);
            wpfPlot.Plot.YLabel("Spectral Power");
            wpfPlot.Plot.XLabel("Frequency (kHz)");
            wpfPlot.Refresh();

            timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void ViewerLoaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                var caps = WaveIn.GetCapabilities(i);
                DeviceComboBox.Items.Add(caps.ProductName);
            }
        }

        private void DeviceComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Wave is not null)
            {
                Wave.StopRecording();
                Wave.Dispose();

                for (int i = 0; i < AudioValues.Length; i++)
                    AudioValues[i] = 0;
                wpfPlot.Plot.Axes.AutoScale();
            }

            if (DeviceComboBox.SelectedIndex == -1)
                return;

            Wave = new WaveInEvent()
            {
                DeviceNumber = DeviceComboBox.SelectedIndex,
                WaveFormat = new WaveFormat(SampleRate, BitDepth, ChannelCount),
                BufferMilliseconds = BufferMilliseconds
            };

            Wave.DataAvailable += WaveIn_DataAvailable;
            Wave.StartRecording();

            wpfPlot.Plot.Title(DeviceComboBox.SelectedItem.ToString());
        }

        private void WaveIn_DataAvailable(object? sender, WaveInEventArgs e)
        {
            for (int i = 0; i < e.Buffer.Length / 2; i++)
            {
                AudioValues[i] = BitConverter.ToInt16(e.Buffer, i * 2);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            double[] paddedAudio = Pad.ZeroPad(AudioValues);
            double[] filter = FftSharp.Filter.LowPass(paddedAudio, SampleRate, 10000000);
            filter = FftSharp.Filter.LowPass(filter, SampleRate, 5000);

            System.Numerics.Complex[] fftComplex = FftSharp.FFT.Forward(filter);
            double[] fftMag = FftSharp.FFT.Power(fftComplex);

            colourNum = (long)Math.Floor((fftMag.Max()));

            //ensure positive
            if (colourNum < 0) colourNum *= -1;

            colourNum = colourNum * 50;

            if (colourNum > 0)
            {
                if((colourNum - prevColourNum) > 2500 || (prevColourNum - colourNum) > 2500)
                {
                    //convert number to hex
                    string hexValue = colourNum.ToString("X");

                    ColourLabel.Text = $"Colour: {hexValue}";

                    //get colour from hex
                    System.Drawing.Color colour = System.Drawing.ColorTranslator.FromHtml("#" + hexValue);
                    ColourSquare.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromArgb(colour.A, colour.R, colour.G, colour.B));
                    ColourSquare.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(colour.A, colour.R, colour.G, colour.B));

                    //update background colour
                    if (colour.R != 0 && colour.G != 0 && colour.B != 0)
                    {
                        //window colour
                        this.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(colour.A, colour.R, colour.G, colour.B));

                        //graph colour
                        ScottPlot.PlotStyle style = new ScottPlot.PlotStyle();
                        style.FigureBackgroundColor = new ScottPlot.Color(hexValue);
                        style.DataBackgroundColor = new ScottPlot.Color(hexValue);
                        wpfPlot.Plot.SetStyle(style);
                    }
                }
                ColourLabel.Text = $"Colour: {colourNum}";
            }

            Array.Copy(fftMag, FftValues, fftMag.Length);

            //get volume in DB
            double volume = 20 * Math.Log10(fftMag.Max());

            // Find frequency peak
            int peakIndex = fftMag.ToList().IndexOf(fftMag.Max());
            double fftPeriod = FFT.FrequencyResolution(SampleRate, fftMag.Length);
            double peakFrequency = fftPeriod * peakIndex;

            PeakFrequencyLabel.Text = $"Peak Frequency: {peakFrequency:N0} Hz";

            // Auto-scale the plot Y axis limits
            double fftPeakMag = fftMag.Max();

            var limits = wpfPlot.Plot.Axes.GetLimits();
            double plotYMax = limits.Top;

            wpfPlot.Plot.Axes.AutoScaleX();
            wpfPlot.Plot.Axes.AutoScaleExpandY();

            wpfPlot.Plot.GetImage(500, 500);


            // Refresh plot
            wpfPlot.Refresh();
        }
    }
}
