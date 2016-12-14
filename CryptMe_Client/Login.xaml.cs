using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CM = CryptMe_Client.CryptMe;

namespace CryptMe_Client
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public static string currentUser;
        public static CM.CryptMeClient CMC;
        public Login()
        {
            InitializeComponent();
            CMC = new CM.CryptMeClient("BasicHttpBinding_ICryptMe");
            btn_Login.Click += (o, e) => ClickLogin(tbx_lName.Text, tbx_lPassword.Password);
            btn_Register.Click += async (o, e) => await CMC.RegisterUserAsync(tbx_lName.Text, tbx_lPassword.Password);
        }

        async void ClickLogin(string uname, string upwd)
        {
            //using (MD5 md5 = MD5.Create())
            {
                if (!(await CMC.CheckUserAsync(uname, upwd)))
                {
                    MessageBox.Show("Invalid User");
                }
                else
                {
                    currentUser = uname;
                    MainWindow mw = new MainWindow();
                    mw.Show();
                    this.Close();
                }
            }
        }
    }
}
