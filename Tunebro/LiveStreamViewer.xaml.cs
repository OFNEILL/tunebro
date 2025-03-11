using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using NAudio.Wave;
using FftSharp;

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

        public LiveStreamViewer()
        {
            InitializeComponent();

            AudioValues = new double[SampleRate * BufferMilliseconds / 1000];
            double[] paddedAudio = Pad.ZeroPad(AudioValues);

            System.Numerics.Complex[] fftComplex = FftSharp.FFT.Forward(paddedAudio);
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
                AudioValues[i] = BitConverter.ToInt16(e.Buffer, i * 2);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            double[] paddedAudio = Pad.ZeroPad(AudioValues);
            System.Numerics.Complex[] fftComplex = FftSharp.FFT.Forward(paddedAudio);
            double[] fftMag = FftSharp.FFT.Power(fftComplex);

            Array.Copy(fftMag, FftValues, fftMag.Length);

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
