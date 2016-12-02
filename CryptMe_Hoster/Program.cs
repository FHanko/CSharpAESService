using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using CryptMe_Service;

namespace CryptMe_Hoster
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ServiceHost Host = new ServiceHost(typeof(CryptMe)))
            {
                Host.Open();
                Console.WriteLine("CryptMe is ready to encrypt data input.");
                Console.ReadLine();
                Host.Close();
            }
        }
    }
}
