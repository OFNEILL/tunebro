using NAudio.Wave;

namespace TuneBro.Business
{
    public class Business
    {
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
    }
}
