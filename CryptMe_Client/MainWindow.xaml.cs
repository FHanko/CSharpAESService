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
        enum EncryptMode { Encrypt, Decrypt, Key };
        public KeyPair CurrentKeyPair { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            EncBox.Drop += (o, e) => DropFile(o, e, EncryptMode.Encrypt);
            DecBox.Drop += (o, e) => DropFile(o, e, EncryptMode.Decrypt);
            KeyBox.Drop += (o, e) => DropFile(o, e, EncryptMode.Key);
            GenerateKeyIV();
        }

        void GenerateKeyIV()
        {
            using (RijndaelManaged KeyGen = new RijndaelManaged())
            {
                KeyGen.GenerateKey();
                KeyGen.GenerateIV();
                CurrentKeyPair = new KeyPair() { CurrentKey = KeyGen.Key, CurrentIV = KeyGen.IV };
            }
        }

        async void DropFile(object o, DragEventArgs e, EncryptMode em)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                    if (em == EncryptMode.Encrypt)
                    {
                        if (!(await FileCrypt(file))) MessageBox.Show($"Error while encrypting {file}. Check the Error log for more information.");
                    }
                    else if (em == EncryptMode.Decrypt)
                    {
                        if (!(await FileDecrypt(file))) MessageBox.Show($"Error while decrypting {file}. Check the Error log for more information.");
                    }
                    else if (em == EncryptMode.Key)
                    {
                        if (!(LoadKey(file))) MessageBox.Show($"Error while loading {file}. Check the Error log for more information.");
                    }
            }
        }

        bool LoadKey(string file)
        {
            try
            {
                string json = FileIO.ReadFileString(file);
                CurrentKeyPair = JsonConvert.DeserializeObject<KeyPair>(json);
                KeyField.Content = $"Current Key \r\n{Convert.ToBase64String(CurrentKeyPair.CurrentKey)}";
                return true;
            }
            catch (Exception e)
            {
                FileIO.WriteFile(Encoding.Default.GetBytes(e.Message), "Error.txt");
                return false;
            }
        }

        async Task<bool> FileCrypt(string file)
        {
            try
            {
                byte[] buffer = FileIO.ReadFile(file);
                string toBase64 = Convert.ToBase64String(buffer);
                byte[] encrypted = await Login.CMC.CryptAESAsync(toBase64, CurrentKeyPair.CurrentKey, CurrentKeyPair.CurrentIV, Login.currentUser);
                FileIO.WriteFile(encrypted, $"{file}.enc");
                FileIO.WriteFile(Encoding.Default.GetBytes(JsonConvert.SerializeObject(CurrentKeyPair)), $"{file}.key");
                return true;
            }
            catch(Exception e)
            {
                FileIO.WriteFile(Encoding.Default.GetBytes(e.Message), "Error.txt");
                return false;
            }
        }

        async Task<bool> FileDecrypt(string file)
        {
            try
            {
                byte[] buffer = FileIO.ReadFile(file);
                string roundtrip = await Login.CMC.DecryptAESAsync(buffer, CurrentKeyPair.CurrentKey, CurrentKeyPair.CurrentIV, Login.currentUser);
                string filename = file.Split('.').Take(file.Split('.').Count()-1).Aggregate((s,s1) => $"{s}.{s1}");
                FileIO.WriteFile(Convert.FromBase64String(roundtrip), $"{filename}");
                return true;
            }
            catch (Exception e)
            {
                FileIO.WriteFile(Encoding.Default.GetBytes(e.Message), "Error.txt");
                return false;
            }
        }

    }
}
