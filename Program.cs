using System;
using System.Threading;
using System.IO;
using SoundModem;

namespace MorseTrainer // Note: actual namespace depends on the project name.
{
    public class Program
    {
        private static char playChar = '\0';
        private static bool running = true;
        private static Random random = new Random();
        public static void Main(string[] args)
        {
            //Sound thread
            Thread soundThread = new Thread(new ThreadStart(SoundThread));
            soundThread.Name = "Sound";
            soundThread.Start();

            while (running)
            {
                playChar = GetRandomAlphanum();
                while (running)
                {
                    Console.WriteLine("Enter guess");
                    string guess = Console.ReadLine().ToUpper();
                    if (guess.Length != 1)
                    {
                        Console.WriteLine("Quitting");
                        running = false;
                        break;
                    }
                    if (guess[0] == playChar)
                    {
                        Console.WriteLine("Correct");
                        break;
                    }
                    else
                    {
                        if (guess[0] == '?')
                        {
                            Console.WriteLine($"Correct answer was: {playChar}");
                            break;
                        }
                        Console.WriteLine("Try again");
                    }
                }
            }
            soundThread.Join();
        }

        private static void SoundThread()
        {
            //Data input
            MemoryStream readStream = new MemoryStream();
            CW cw = new CW(48000, 24, readStream);
            //Sound output
            MemoryStream writeStream = new MemoryStream();
            IFormat format = new S16LE(writeStream, -1);
            ISink sink = new OpenALSink();
            char lastChar = '\0';
            byte[] byteData = new byte[1];
            while (playChar == '\0')
            {
                Thread.Sleep(100);
            }
            while (running)
            {
                readStream.Position = 0;
                if (playChar != lastChar)
                {
                    byteData[0] = (byte)playChar;
                    readStream.Write(byteData, 0, 1);
                    lastChar = playChar;
                }
                int samplesToWrite = 48000;
                while (cw.GetInput(format))
                {
                    sink.Write(writeStream);
                    samplesToWrite = 48000 - (int)writeStream.Position;
                }
                Tone.WriteSilence(format, samplesToWrite);
                sink.Write(writeStream);
            }
            sink.Close();
        }

        private static char GetRandomAlphanum()
        {
            int randomInt = random.Next(48, 84);
            if (randomInt > 57)
            {
                randomInt += 7;
            }
            return (char)randomInt;
        }
    }
}