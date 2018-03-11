﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using FirstFloor.ModernUI.Windows.Controls;
using ServerData;

namespace Test.Pages.Login
{
    /// <summary>
    /// Interaction logic for Schüler_Login.xaml
    /// </summary>
    public partial class Lehrer_Login : UserControl
    {
        private MainWindow mw;
        public Lehrer_Login()
        {
            InitializeComponent();
            Loaded += (sender, args) =>
            {
                mw = UIHelper.FindVisualParent<Test.Login>(this).mw;
                cmd_LehrerLogin.Click += CmdLehrerLoginOnClick;
            };
        }

        private void CmdLehrerLoginOnClick(object o, RoutedEventArgs routedEventArgs)
        {
            try
            {
                ListDictionary dataLogin = new ListDictionary{
                    {"email", txt_Email.Text},
                    {"passwort", txt_Passwort.Password}
                };
                Packet loginResponse = mw.client.Login(dataLogin, false);

                if (loginResponse.Success)
                {
                    mw.LoginFrm.Closing -= mw.LoginFrmOnClosing; 
                    mw.LoginFrm.Close();
                    mw.IsEnabled = true;
                    mw.hasRights = true;
                }
                else
                {
                    lbl_LehrerLoginError.Text = loginResponse.MessageString;
                }
            }
            catch (Exception ex)
            {
                lbl_LehrerLoginError.Text = ex.Message;
            }
        }
    }
}
