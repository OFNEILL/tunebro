using FftSharp;
using NAudio.Wave;
using Newtonsoft.Json;
using ScottPlot;
using System.Numerics;
using TuneBro.Business.Objects;

namespace TuneBro.Business
{
    public class Business
    {
        private static string resultPath = Directory.GetCurrentDirectory() + "/Samples";
        private static string configPath = Directory.GetCurrentDirectory() + "/tunebro-config";

        public Business()
        {
            if (!Directory.Exists(resultPath))
            {
                Directory.CreateDirectory(resultPath);
            }
            if (!Directory.Exists(configPath))
            {
                Directory.CreateDirectory(configPath);
            }
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

        public Settings GetSettings()
        {
            //check if settings file exists, if not create it
            if (!File.Exists(configPath + "/settings.json"))
            {
                File.Create(configPath + "/settings.json").Close();

                var writtenSettings = new Settings { MagnitudeDiff = 2500 };

                //write default settings to file
                File.WriteAllText(configPath + "/settings.json", JsonConvert.SerializeObject(writtenSettings));

                return writtenSettings;
            }

            //read settings from file
            string json = File.ReadAllText(configPath + "/settings.json");

            //convert json to settings object
            Settings settings = JsonConvert.DeserializeObject<Settings>(json);

            return settings;
        }

        public void UpdateSettings(Settings settings)
        {
            //check if settings file exists, if not create it
            if (!File.Exists(configPath + "/settings.json"))
            {
                File.Create(configPath + "/settings.json");
            }

            //convert settings to a json string
            string json = JsonConvert.SerializeObject(settings);

            //update settings
            File.WriteAllText(configPath + "/settings.json", json);

            return;
        }
    }
}
