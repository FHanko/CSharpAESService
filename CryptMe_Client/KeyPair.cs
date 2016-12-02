using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptMe_Client
{
    public class KeyPair
    {
        public byte[] CurrentKey { get; set; }
        public byte[] CurrentIV { get; set; }
    }
}
