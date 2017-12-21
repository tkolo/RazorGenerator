using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Text;
using RazorGenerator.Core;

namespace RazorGenerator.Standalone
{   
    public class Program
    {
        private const int ProgressWidth = 30;
        
        public static void Main(string[] args)
        {
            var projectDirectory = args[0];            
            
            using (var hostManager = new HostManager(projectDirectory))
            {
                foreach (string arg in args.Skip(1))
                {
                    var inputFilePath = args[1];
                    var outputPath = $"{Path.Combine(Path.GetDirectoryName(inputFilePath), Path.GetFileNameWithoutExtension(inputFilePath))}.generated.cs";
                    var projectRelativePath = inputFilePath.Substring(projectDirectory.Length);
                    var host = hostManager.CreateHost(inputFilePath, projectRelativePath, CodeDomProvider.CreateProvider("C#"), null);
                    host.Error += (o, eventArgs) =>
                    {
                        Console.Error.WriteLine($"In line ({eventArgs.LineNumber}:{eventArgs.ColumnNumber}): {eventArgs.ErrorMessage}");
                        Environment.Exit(1);
                    };
                    host.Progress += (o, eventArgs) =>
                    {
                        //RenderProgressBar(eventArgs.Completed, (uint) (eventArgs.Total * (args.Length - 1)));
                    };

                    var content = host.GenerateCode();
                    var outpuToBytes = ConvertToBytes(content);
                    Console.OpenStandardOutput().Write(outpuToBytes, 0, outpuToBytes.Length);
                }                
                
            }
        }
        
        private static byte[] ConvertToBytes(string content)
        {
            //Get the preamble (byte-order mark) for our encoding
            byte[] preamble = Encoding.UTF8.GetPreamble();
            int preambleLength = preamble.Length;

            byte[] body = Encoding.UTF8.GetBytes(content);

            //Prepend the preamble to body (store result in resized preamble array)
            Array.Resize<byte>(ref preamble, preambleLength + body.Length);
            Array.Copy(body, 0, preamble, preambleLength, body.Length);

            //Return the combined byte array
            return preamble;
        }

        /*private static void RenderProgressBar(uint completed, uint total)
        {
            Console.CursorLeft = 0;
            Console.Write('[');
            int progressWidth = (int) Math.Round((double) completed * ProgressWidth / total);
            for (var i = 0; i < progressWidth; i++)
            {
                Console.Write('=');
            }
            if (progressWidth < ProgressWidth)
                Console.Write('>');
            for (var i = progressWidth + 1; i < ProgressWidth; i++)
            {
                Console.Write(' ');
            }
            Console.Write($"] {(int) Math.Round(completed * 100.0 / total)}%");
        }*/
    }
}