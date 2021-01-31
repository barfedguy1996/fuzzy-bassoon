using System;
using System.Runtime.InteropServices;

namespace ConsoleApp1
{
    class Program
    {
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool CopyFile(String sourceFile, String detinationFile, bool failifExists);
       
        static void Main(string[] args)
        {
            Console.WriteLine("Enter source file path: ");
            String sourceFile = Console.ReadLine();
            Console.WriteLine("Enter destination file path:");
            String destinationFile = Console.ReadLine();
            if (CopyFile(sourceFile, destinationFile,true))
            {
                Console.WriteLine(sourceFile + " copied to " + destinationFile);
            }
            else
            {
                Console.WriteLine("Failed to copy " + sourceFile);
            }
        }
    }
}
