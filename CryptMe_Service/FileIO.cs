using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptMe_Service
{
    class FileIO
    {
        public static void WriteFile(byte[] buffer, string name)
        {
            using (FileStream FS = new FileStream(name, FileMode.Create))
            using (BinaryWriter BR = new BinaryWriter(FS))
            {
                BR.Write(buffer);
            }
        }

        async public static void WriteFileAsync(string text, string name)
        {
            using (FileStream FS = new FileStream(name, FileMode.Create))
            using (StreamWriter SR = new StreamWriter(FS))
            {
                await SR.WriteAsync(text);
            }
        }

        public static byte[] ReadFile(string filename)
        {
            int len = (int)(new FileInfo(filename).Length);
            byte[] buffer = new byte[len];
            using (FileStream FS = new FileStream(filename, FileMode.Open))
            using (BinaryReader BR = new BinaryReader(FS))
            {
                buffer = BR.ReadBytes(len);
            }
            return buffer;
        }

        public static string ReadFileString(string filename)
        {
            string ret;
            using (FileStream FS = new FileStream(filename, FileMode.Open))
            using (StreamReader SR = new StreamReader(FS))
            {
                ret = SR.ReadToEnd();
            }
            return ret;
        }
    }
}
