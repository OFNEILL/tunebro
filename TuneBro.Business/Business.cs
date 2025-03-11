using FftSharp;
using NAudio.Wave;
using ScottPlot;
using System.Numerics;

namespace TuneBro.Business
{
    public class Business
    {
        private static string resultPath = @"C:\Users\ASUS-PC\Desktop\Samples\";
        public string Init(string path)
        {
            //copy file to working directory
            string workingPath = HandleFileCopy(path);

            //extract audio reader from file
            AudioFileReader reader = ExtractAudioReader(workingPath);
            float maxVolume = ExtractMaxVolume(reader);

            Console.WriteLine(maxVolume.ToString());

            return workingPath;
        }

        private string HandleFileCopy(string path)
        {
            //copy file to new location from path
            Console.WriteLine("Copying File");

            var tempPath = Directory.CreateTempSubdirectory("tunebro-handler-").FullName + Guid.NewGuid().ToString();
            File.Copy(path, tempPath);

            return tempPath;
        }

        private AudioFileReader ExtractAudioReader(string path)
        {
            return new AudioFileReader(path);
        }

        private double[] ExtractBuffer(AudioFileReader reader)
        {
            float[] buffer = new float[reader.WaveFormat.SampleRate];
            int samplesRead;
            List<double> bufferList = new List<double>();
            while ((samplesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < samplesRead; i++)
                {
                    bufferList.Add(buffer[i]);
                }
            }

            double[] bufferOut = bufferList.ToArray();

            //check if buffer is power of 2
            if (buffer.Length != Math.Pow(2, Math.Ceiling(Math.Log(buffer.Length, 2))))
            {
                bufferOut = MakeBufferPO2(bufferOut);
            }

            return bufferOut;
        }

        private double[] MakeBufferPO2(double[] buffer)
        {
            int length = buffer.Length;
            int newLength = (int)Math.Pow(2, Math.Ceiling(Math.Log(length, 2)));
            double[] newBuffer = new double[newLength];
            Array.Copy(buffer, newBuffer, length);
            return newBuffer;
        }

        private float ExtractMaxVolume(AudioFileReader reader)
        {
            float[] buffer = new float[reader.WaveFormat.SampleRate];
            int samplesRead;
            float maxVolume = 0;

            while ((samplesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < samplesRead; i++)
                {
                    if (buffer[i] > maxVolume)
                    {
                        maxVolume = buffer[i];
                    }
                }
            }

            return maxVolume;
        }

        public string GenerateFFTWave(string path)
        {
            string pathOut = "";

            AudioFileReader reader = ExtractAudioReader(path);
            double[] signal = ExtractBuffer(reader);

            // Shape the signal using a Hanning window
            FftSharp.Windows.Hanning window = new FftSharp.Windows.Hanning();
            window.ApplyInPlace(signal);

            // Calculate the FFT as an array of complex numbers
            System.Numerics.Complex[] spectrum = FftSharp.FFT.Forward(signal);

            // or get the magnitude (units sqrd) or power (dB) as double[] 
            double[] frequency = FftSharp.FFT.Magnitude(spectrum);
            double[] power = FftSharp.FFT.Power(spectrum);

            // plot the sample audio
            Plot plt = new Plot();

            plt.Add.Signal(signal, reader.WaveFormat.SampleRate / 1000.0);
            plt.YLabel("Amplitude");
            plt.XLabel("Time (ms)");

            pathOut = $"{resultPath}{Guid.NewGuid()}.png";
            plt.Save(pathOut, 1000, 1000);

            return pathOut;
        }

        public string GenerateFFTFilterGraph(string path)
        {
            string pathOut = "";

            AudioFileReader reader = ExtractAudioReader(path);
            double[] signal = ExtractBuffer(reader);
            double[] lowSignal = FftSharp.Filter.LowPass(signal, reader.WaveFormat.SampleRate, 2000);

            // Shape the signal using a Hanning window
            FftSharp.Windows.Hanning window = new FftSharp.Windows.Hanning();
            window.ApplyInPlace(signal);
            window.ApplyInPlace(lowSignal);

            // Calculate the FFT as an array of complex numbers
            System.Numerics.Complex[] spectrum = FftSharp.FFT.Forward(signal);
            Complex[] lowSpectrum = FftSharp.FFT.Forward(lowSignal);

            // or get the magnitude (units sqrd) or power (dB) as double[] 
            double[] power = FftSharp.FFT.Power(spectrum);
            double[] lowPower = FftSharp.FFT.Power(lowSpectrum);

            double[] frequency = FftSharp.FFT.FrequencyScale(power.Length, reader.WaveFormat.SampleRate);
            double[] lowFrequency = FftSharp.FFT.FrequencyScale(lowPower.Length, reader.WaveFormat.SampleRate);


            // plot the sample audio
            Plot plt = new Plot();

            plt.Add.ScatterLine(frequency, power);
            //plt.Add.ScatterLine(lowFrequency, lowPower);
            //plt.Add.ScatterLine();
            plt.YLabel("Power (dB)");
            plt.XLabel("Frequency (Hz)");

            pathOut = $"{resultPath}{Guid.NewGuid()}.png";
            plt.Save(pathOut, 500, 200);

            return pathOut;
        }
    }
}
