using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CryptMe_Service
{
    [ServiceContract]
    public interface ICryptMe
    {
        [OperationContract]
        Task<bool> CheckUser(string uname, string upwd);
        [OperationContract]
        Task<bool> RegisterUser(string uname, string upwd);
        [OperationContract]
        Task<byte[]> CryptAES(string plainText, byte[] Key, byte[] IV, string user);
        [OperationContract]
        Task<string> DecryptAES(byte[] cipherText, byte[] Key, byte[] IV, string user);
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CryptMe : ICryptMe
    {
        const bool AllowRegistration = true;
        public List<User> Userlist = new List<User>();
        public async Task<bool> CheckUser(string uname, string upwd)
        {
            return await Task.Factory.StartNew(() =>
            {
                bool valid;
                if (Userlist.Count == 0)
                {
                    string json = FileIO.ReadFileString("user.json");
                    Userlist = JsonConvert.DeserializeObject<List<User>>(json);
                    valid = CheckUser(uname, upwd).Result;
                }
                else
                {
                    valid = Userlist.Any(u => u.Username == uname && u.Password == upwd);
                }
                Log($"User {uname} is trying to login. SUCCESS = {valid.ToString()}");
                return valid;
            });
        }

        public async Task<bool> RegisterUser(string uname, string upwd)
        {
            if (AllowRegistration)
            {
                return await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        Userlist.Add(new User() { Username = uname, Password = upwd });
                        string json = JsonConvert.SerializeObject(Userlist);
                        FileIO.WriteFileAsync(json, "user.json");
                        Log($"New User Registration; User {uname}");
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                });
            }
            else return false;
        }

        async public void Log(string tolog)
        {
            Console.WriteLine(tolog);
            await Task.Run(() => FileIO.WriteFileAsync(tolog, "Log.txt")); 
        }

        async public Task<byte[]> CryptAES(string plainText, byte[] Key, byte[] IV, string user)
        {
            Log($"New encryption request");
            return await Task<byte[]>.Factory.StartNew(() =>
            {
                byte[] encrypted;
                using (RijndaelManaged rijAlg = new RijndaelManaged())
                {
                    rijAlg.Key = Key;
                    rijAlg.IV = IV;

                    ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(plainText);
                            }
                            encrypted = msEncrypt.ToArray();
                        }
                    }
                }
                return encrypted;
            });
        }

        async public Task<string> DecryptAES(byte[] cipherText, byte[] Key, byte[] IV, string user)
        {
            Log($"New decryption request");
            return await Task<string>.Factory.StartNew(() =>
            {
                string plaintext = null;

                using (RijndaelManaged rijAlg = new RijndaelManaged())
                {
                    rijAlg.Key = Key;
                    rijAlg.IV = IV;

                    ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                plaintext = srDecrypt.ReadToEnd();
                            }
                        }
                    }

                }
                return plaintext;
            });
        }
    }
}
