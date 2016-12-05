using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CryptMe_Service
{
    [ServiceContract]
    public interface ICryptMe
    {
        [OperationContract]
        Task<byte[]> CryptAES(string plainText, byte[] Key, byte[] IV);
        [OperationContract]
        Task<string> DecryptAES(byte[] cipherText, byte[] Key, byte[] IV);
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CryptMe : ICryptMe
    {
        async public Task<byte[]> CryptAES(string plainText, byte[] Key, byte[] IV)
        {
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

        async public Task<string> DecryptAES(byte[] cipherText, byte[] Key, byte[] IV)
        {
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
