using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CM = CryptMe_Client.CryptMe;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace CryptMe_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CM.CryptMeClient CMC;
        enum EncryptMode { Encrypt, Decrypt, Key };
        byte[] CurrentKey;
        byte[] CurrentIV;
        public MainWindow()
        {
            InitializeComponent();
            CMC = new CM.CryptMeClient("BasicHttpBinding_ICryptMe");
            EncBox.Drop += (o, e) => DropFile(o, e, EncryptMode.Encrypt);
            DecBox.Drop += (o, e) => DropFile(o, e, EncryptMode.Decrypt);
            GenerateKeyIV();
        }

        void GenerateKeyIV()
        {
            using (RijndaelManaged myRijndael = new RijndaelManaged())
            {
                myRijndael.GenerateKey();
                myRijndael.GenerateIV();
                CurrentKey = myRijndael.Key;
                CurrentIV = myRijndael.IV;
            }
        }

        void DropFile(object o, DragEventArgs e, EncryptMode em)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                    if (em == EncryptMode.Encrypt)
                    {
                        if (!FileCrypt(file)) MessageBox.Show($"Error while encrypting {file}");
                    }
                    else if (em == EncryptMode.Decrypt)
                    {
                        if (!FileDecrypt(file)) MessageBox.Show($"Error while decrypting {file}");
                    }
                    else if (em == EncryptMode.Key)
                    {
                        if (!LoadKey(file)) MessageBox.Show($"Error while loading {file}");
                    }
            }
        }

        bool LoadKey(string file)
        {
            try
            {
                string json;
                using (FileStream FS = new FileStream(file, FileMode.Open))
                using (StreamReader SR = new StreamReader(FS))
                {
                    json = SR.ReadToEnd();
                }
                List<byte[]> Keys = JsonConvert.DeserializeObject<List<byte[]>>(json);
                CurrentKey = Keys[0];
                CurrentIV = Keys[1];
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("" + e.Message);
                return false;
            }
        }

        bool FileCrypt(string file)
        {
            try
            {
                int len = (int)(new FileInfo(file).Length);
                byte[] buffer = new byte[len];
                using (FileStream FS = new FileStream(file, FileMode.Open))
                using (BinaryReader BR = new BinaryReader(FS))
                {
                    buffer = BR.ReadBytes(len);
                }
                string toBase64 = Convert.ToBase64String(buffer);
                byte[] encrypted = CMC.CryptAES(toBase64, CurrentKey, CurrentIV);
                WriteFile(encrypted, $"{file}.enc");
                List<byte[]> Keys = new List<byte[]>() { CurrentKey, CurrentIV };
                WriteFile(Encoding.Default.GetBytes(JsonConvert.SerializeObject(Keys)), $"{file}.key");
                return true;
            }
            catch(Exception e)
            {
                MessageBox.Show("" + e.Message);
                return false;
            }
        }

        bool FileDecrypt(string file)
        {
            try
            {
                int len = (int)(new FileInfo(file).Length);
                byte[] buffer = new byte[len];
                using (FileStream FS = new FileStream(file, FileMode.Open))
                using (BinaryReader BR = new BinaryReader(FS))
                {
                    buffer = BR.ReadBytes(len);
                }
                string roundtrip = CMC.DecryptAES(buffer, CurrentKey, CurrentIV);
                string filename = file.Split('.').Take(file.Split('.').Count()-1).Aggregate((s,s1) => $"{s}.{s1}");
                WriteFile(Convert.FromBase64String(roundtrip), $"{filename}");
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("" + e.Message);
                return false;
            }
        }

        void WriteFile(byte[] buffer, string name)
        {
            using (FileStream FS = new FileStream(name, FileMode.Create))
            using (BinaryWriter BR = new BinaryWriter(FS))
            {
                BR.Write(buffer);
            }
        }
        void WriteFileToBase64(byte[] buffer, string name)
        {
            string b64 = Convert.ToBase64String(buffer);
            using (FileStream FS = new FileStream(name, FileMode.Create))
            using (StreamWriter BR = new StreamWriter(FS))
            {
                BR.Write(b64);
            }
        }
    }
}
