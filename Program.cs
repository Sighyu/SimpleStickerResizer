using System;
using System.IO;
using System.Diagnostics;

namespace SimpleStickerResizer
{
    internal static class Program
    {
        private static readonly string[] SupportedFormats = { ".png", ".jpg", ".jpeg", ".gif", ".webp" };

        static void Main()
        {
            Console.Write("Enter the path to the stickers: ");
            string inputDir = Console.ReadLine();

            if (!Directory.Exists(inputDir))
            {
                Console.WriteLine("Error: Directory does not exist.");
                return;
            }

            string outputDir = Path.Combine(inputDir, "resized");
            Directory.CreateDirectory(outputDir);

            foreach (string file in Directory.GetFiles(inputDir, "*.*", SearchOption.TopDirectoryOnly))
            {
                string ext = Path.GetExtension(file).ToLower();
                if (Array.Exists(SupportedFormats, e => e == ext))
                {
                    string outputFile = Path.Combine(outputDir, Path.GetFileName(file));

                    if (ext == ".webp" && IsAnimated(file))
                    {
                        outputFile = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(file) + ".gif");
                        Console.WriteLine($"Converting animated WebP: {file} to GIF...");
                        RunFFmpeg($"-i \"{file}\" -vf scale=160:160 \"{outputFile}\"");
                    }
                    else
                    {
                        Console.WriteLine($"Resizing image: {file}");
                        RunFFmpeg($"-i \"{file}\" -vf scale=160:160 \"{outputFile}\"");
                    }
                }
            }

            Console.WriteLine("Job Done!");
        }

        private static void RunFFmpeg(string args)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            process?.WaitForExit();
        }

        private static bool IsAnimated(string filePath)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "ffprobe",
                Arguments = $"\"{filePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            string errorOutput = process?.StandardError.ReadToEnd() ?? string.Empty;
            process?.WaitForExit();

            return !errorOutput.Contains("Duration: N/A");
        }
    }
}
