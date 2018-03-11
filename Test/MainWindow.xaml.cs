﻿using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
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
using System.Collections.Specialized;
using System.ComponentModel;
using ClientClassLib;
using FirstFloor.ModernUI.Presentation;
using ServerData;

namespace Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        public Client client;
        List<string> SubscribedEvents = new List<string>();
        public Login LoginFrm;
        private ModernTab Kurse;

        public MainWindow()
        {
            InitializeComponent();

            GlobalMethods.ErrorMessageCallback ErrorCallback = Fehler_Ausgabe;
            GlobalMethods.UpdateFormCallback UpdateFormCallback = UpdateChat;

            client = new Client(ErrorCallback, UpdateFormCallback);

            client.Connect(PacketHandler.GetIPAddress(), 4444); //Connect to Server on IP and POrt 4444

            LoginFrm = new Login();
            LoginFrm.Show();
            LoginFrm.Loaded += LoginFrm_Loaded;
            LoginFrm.GotFocus += LoginFrm_GotFocus;
            LoginFrm.Closing += LoginFrmOnClosing;
            IsEnabled = false;
            GotFocus += OnGotFocus;
            
        }

        //Callback Delegates+++++++++++++++++++++++++++
        private void Fehler_Ausgabe(string s)
        {
            MessageBox.Show(s);
        }
        private void UpdateChat(Packet p)
        {
            //Chat im aktiven Fenster neu laden ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        }

        private void LoginFrmOnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            Close();
        }

        private void OnGotFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            Button cmd_refresh = UIHelper.FindVisualChildByName<Button>(this, "cmd_refresh");
            if (cmd_refresh != null && !SubscribedEvents.Contains("cmd_refresh"))
            {
                cmd_refresh.Click += Cmd_Refresh_Click;
                SubscribedEvents.Add("cmd_refresh");
            }

            Button cmd_save = UIHelper.FindVisualChildByName<Button>(this, "cmd_save");
            if (cmd_save != null && !SubscribedEvents.Contains("cmd_save"))
            {
                cmd_save.Click += cmd_save_Click;
                SubscribedEvents.Add("cmd_save");
            }
        
    }

    private void cmd_save_Click(object sender, RoutedEventArgs e)
        {
            Pages.Settings.Kurswahl kw = UIHelper.FindVisualParent<Pages.Settings.Kurswahl>((Button)sender);
            try
            {
                List<string> k = kw.GetChecked();
                if (k.Count == 0) throw new Exception("Bitte mindestens einen Kurs auswählen.");
                ListDictionary data = new ListDictionary
                {
                    {"K_ID",k}
                };
                Packet kursUpdate = client.SendAndWaitForResponse(PacketType.KursUpdate, data);
                if (!kursUpdate.Success) throw new Exception(kursUpdate.MessageString);
                Reload_Kurse();
                throw new Exception("Erfolgreich gespeichert");
            }
            catch (Exception ex)
            {
                kw.lbl_Kurswahl_Error.Text = ex.Message;
            }
        }

        private void Cmd_Refresh_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            Pages.Settings.Kurswahl kw = UIHelper.FindVisualParent<Pages.Settings.Kurswahl>((Button)sender);
            try
            {
                Packet kurse = client.SendAndWaitForResponse(PacketType.GetVerfügbareKurse);
                Packet getKurse = client.SendAndWaitForResponse(PacketType.GetKurseVonSchüler);
                if (kurse.Success && getKurse.Success)
                {
                    kw.UpdateKurse(kurse.Data,getKurse.Data);
                }
                else
                {
                    throw new Exception (kurse.MessageString+" "+getKurse.MessageString);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        

        void LoginFrm_Loaded(object sender, RoutedEventArgs e)
        {
            LoginFrm.Focus();
        }

        void LoginFrm_GotFocus(object sender, RoutedEventArgs e)
        {

            Button cmd_SchülerLogin = UIHelper.FindVisualChildByName<Button>(LoginFrm, "cmd_SchülerLogin");
            if (cmd_SchülerLogin != null && !SubscribedEvents.Contains("cmd_SchülerLogin_Click"))
            {
                cmd_SchülerLogin.Click += cmd_SchülerLogin_Click;
                SubscribedEvents.Add("cmd_SchülerLogin_Click");
            }

            Button cmd_SchülerRegi = UIHelper.FindVisualChildByName<Button>(LoginFrm, "cmd_AbsendenRegistrierungSchueler");
            if (cmd_SchülerRegi != null && !SubscribedEvents.Contains("cmd_SchülerRegi"))
            {
                cmd_SchülerRegi.Click += cmd_SchülerRegi_Click;
                SubscribedEvents.Add("cmd_SchülerRegi");
            }

            Button cmd_LehrerLogin = UIHelper.FindVisualChildByName<Button>(LoginFrm, "cmd_LehrerLogin");
            if (cmd_LehrerLogin != null && !SubscribedEvents.Contains("cmd_LehrerLogin"))
            {
                cmd_LehrerLogin.Click += cmd_LehrerLogin_Click;
                SubscribedEvents.Add("cmd_LehrerLogin");
            }
            
            Button cmd_LehrerRegi = UIHelper.FindVisualChildByName<Button>(LoginFrm, "cmd_AbsendenRegistrierungLehrer");
            if (cmd_LehrerRegi != null && !SubscribedEvents.Contains("cmd_LehrerRegi"))
            {
                cmd_LehrerRegi.Click += cmd_LehrerRegi_Click;
                SubscribedEvents.Add("cmd_LehrerRegi");
            }
            
            ComboBox cbB_Klasse = UIHelper.FindVisualChildByName<ComboBox>(LoginFrm, "cbB_Klasse");
            if (cbB_Klasse != null && !SubscribedEvents.Contains("cbB_Klasse"))
            {
                cbB_Klasse.DropDownOpened += CbB_Klasse_DropDownOpened;
                SubscribedEvents.Add("cbB_Klasse");
            }
            
        }


        //Lehrer++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        private void cmd_LehrerLogin_Click(object sender, RoutedEventArgs e)
        {
            Pages.Login.Lehrer_Login lehrer_login =
                UIHelper.FindVisualParent<Pages.Login.Lehrer_Login>((Button) sender);
            try
            {
                ListDictionary dataLogin = new ListDictionary{
                    {"email", lehrer_login.txt_Email.Text},
                    {"passwort", lehrer_login.txt_Passwort.Password}
                };
                Packet loginResponse = client.Login(dataLogin, false);

                if (loginResponse.Success)
                {
                    LoginFrm.Closing -= LoginFrmOnClosing; 
                    LoginFrm.Close();
                    IsEnabled = true;
                }
                else
                {
                    lehrer_login.lbl_LehrerLoginError.Text = loginResponse.MessageString;

                }
            }
            catch (Exception ex)
            {
                lehrer_login.lbl_LehrerLoginError.Text = ex.Message;
            }
        }

        private void cmd_LehrerRegi_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            Pages.Login.Lehrer_Register lehrer_regi = UIHelper.FindVisualParent<Pages.Login.Lehrer_Register>((Button)sender);
            try
            {
                //string Name = lehrer_regi.txt_Name.Text;
                //string Vname = lehrer_regi.txt_Vorname.Text;
                //string lehrerCode = lehrer_regi.txt_LehrerCode.Text;
                //string Email = lehrer_regi.txt_Email.Text;
                //string Passwort = lehrer_regi.txt_Passwort.Password;
                //string Anrede = (string)lehrer_regi.cbB_Anrede.SelectedValue;
                //string Titel = (string)lehrer_regi.cbB_Titel.SelectedValue;
                //MessageBox.Show(Vname + Name + Anrede + Email + Passwort + Titel);

                ListDictionary dataRegister = new ListDictionary{
                        {"name", lehrer_regi.txt_Name.Text},
                        {"vname", lehrer_regi.txt_Vorname.Text},
                        {"anrede", (string)lehrer_regi.cbB_Anrede.SelectedValue},
                        {"titel", (string)lehrer_regi.cbB_Titel.SelectedValue},
                        {"email", lehrer_regi.txt_Email.Text},
                        {"passwort", lehrer_regi.txt_Passwort.Password},
                        {"lehrerPasswort", "teachersPassword"}         //Lehrer Passwort abfrage designen+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                    };

                Packet registerResponse = client.Register_Lehrer(dataRegister);
                if (registerResponse.Success)
                {
                    throw new Exception("Registrierung erfolgreich.");
                }
                else
                {
                    throw new Exception(registerResponse.MessageString);
                }
            }
            catch (Exception ex)
            {
                lehrer_regi.lbl_Lehrer_Registrations_Error.Text = ex.Message;
            }
        }

        //Schüler++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        void cmd_SchülerRegi_Click(object sender, RoutedEventArgs e)
        {
            Pages.Login.Schüler_Register schüler_regi = UIHelper.FindVisualParent<Pages.Login.Schüler_Register>((Button)sender);
            try
            {
                //string Name = schüler_regi.txt_Name.Text;
                //string Vname = schüler_regi.txt_Vorname.Text;
                //string Phone = schüler_regi.txt_Telefonnummer.Text;
                //string Email = schüler_regi.txt_Email.Text;
                //string Passwort = schüler_regi.txt_Passwort.Password;
                //string Klasse = Convert.ToString(schüler_regi.cbB_Klasse.SelectedValue);

                ListDictionary dataRegister = new ListDictionary{
                {"name", schüler_regi.txt_Name.Text},
                {"vname", schüler_regi.txt_Vorname.Text},
                {"phone", schüler_regi.txt_Telefonnummer.Text},
                {"klasse",Convert.ToString(schüler_regi.cbB_Klasse.SelectedValue)},
                {"email", schüler_regi.txt_Email.Text},
                {"passwort", schüler_regi.txt_Passwort.Password}
            };

                Packet registerResponse = client.Register_Schüler(dataRegister);
                if (registerResponse.Success)
                {
                    throw new Exception("Registrierung erfolgreich.");
                }
                else
                {
                    throw new Exception(registerResponse.MessageString);
                }
            }
            catch (Exception ex)
            {
                schüler_regi.lbl_Schüler_Registrations_Error.Text = ex.Message;
            }
        }

        void CbB_Klasse_DropDownOpened(object sender, EventArgs e)
        {
                Packet klassen = client.SendAndWaitForResponse(PacketType.Klassenwahl);
                if (klassen.Success)
                {
                    ComboBox cb = (ComboBox) sender;

                    List<string> lst_data = (List<string>) klassen.Data["Kl_Name"];
                    cb.Items.Clear();

                    foreach (string s in lst_data)
                    {
                        cb.Items.Add(s);
                    }
                }
                else
                {
                    MessageBox.Show(
                        "Klassen konnten nicht geladen werden:\n" + klassen.MessageString);
                }
            
        }

        void cmd_SchülerLogin_Click(object sender, RoutedEventArgs e)
        {
            Pages.Login.Schüler_Login schüler_login = UIHelper.FindVisualParent<Pages.Login.Schüler_Login>((Button)sender);
            try
            {
                ListDictionary dataLogin = new ListDictionary{
                    {"email", schüler_login.txt_Email.Text},
                    {"passwort", schüler_login.txt_Passwort.Password}
                };

                Packet loginResponse = client.Login(dataLogin, true);
                if (loginResponse.Success)

                {
                    LoginFrm.Closing -= LoginFrmOnClosing; 
                    LoginFrm.Close();
                    IsEnabled = true;
                    Reload_Kurse();
                }
                else
                {
                    throw new Exception(loginResponse.MessageString);

                }
            }
            catch (Exception ex)
            {
                schüler_login.lbl_SchülerLoginError.Text = ex.Message;
            }
        }
        //-------------------------------------------------------------------------------

        void Reload_Kurse()
        {
            try
            {
                if (Kurse == null)
                {
                    Kurse = UIHelper.FindVisualChildByName<ModernTab>(this, "mt_Kurse");
                }
                Packet GetKurse = client.SendAndWaitForResponse(PacketType.GetKurseVonSchüler);
                if (GetKurse.Success)
                {
                    foreach (var Link in Kurse.Links.Where(L => L.Source.OriginalString != "Pages/Home.xaml").ToList()) Kurse.Links.Remove(Link);

                    if (((List<string>)GetKurse.Data["K_Name"]).Count != 0)
                    {
                        foreach (string Kurs in (List<string>)GetKurse.Data["K_Name"])
                        {
                            Kurse.Links.Add(new Link
                            {
                                DisplayName = Kurs,
                                Source = new Uri("Pages/Home.xaml?Kurs=" + Kurs, UriKind.Relative)
                            });
                        }
                    }
                }
                else
                {
                    throw new Exception(GetKurse.MessageString);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
