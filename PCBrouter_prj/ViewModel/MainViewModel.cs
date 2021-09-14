using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using MaterialDesignColors;
using ActUtlTypeLib;
using PCBrouter_prj.ViewModel;
using System.Threading;
using System.Windows.Threading;
using PCBrouter_prj.UserControlKteam;

namespace PCBrouter_prj.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public static MainWindow mwd;
        public static int iretCon;
        public static ActUtlType plc = new ActUtlType();
        public static DispatcherTimer TimerCheckStatus;
        public ICommand LoadedWindowCommand { get; set; }
        public ICommand ClosingWindowCommand { get; set; }
        public ICommand ModeCommand { get; set; }
        public ICommand ServoOnCommand { get; set; }


        private object _UCview;
        public object UCview
        {
            get { return _UCview; }
            set
            {
                _UCview = value;
                OnPropertyChanged();
            }
        }
       
        public MainViewModel()
        {
            ServoOnCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                int m120;
                plc.GetDevice("M120",out m120);
                if (m120 == 1)
                {
                    plc.SetDevice("M120", 0);
                }   
                else
                {
                    plc.SetDevice("M120", 1);
                }    
            });
            ClosingWindowCommand = new RelayCommand<MainWindow>((p) => { return true; }, (p) =>
            {
                if (CheckStatusThread != null)
                {
                    CheckStatusThread.Abort();
                }
            });
            ModeCommand = new RelayCommand<object>((p) => { return true; }, (p)=>
            {
                if (p.ToString() == "ManualModeParameter")
                {
                    UCview = new ControlManualViewModel();
                    plc.SetDevice("M105", 0);
                }    
                else if (p.ToString() == "AutoModeParameter")
                {
                    UCview = new ControlAutoViewModel();
                    plc.SetDevice("M105", 1);
                }    
            });
            LoadedWindowCommand = new RelayCommand<MainWindow>((p) => { return true; }, (p) => {
                mwd = p;
                Connection();
                UCview = new ControlAutoViewModel();
                StartCheckStatusThread();
            });
        }
        public Thread CheckStatusThread;
        public void StartCheckStatusThread()
        {
            if (CheckStatusThread != null)
            {
                CheckStatusThread.Abort();
            }
            CheckStatusThread = new Thread(new ThreadStart(ThreadCheckExecution));
            CheckStatusThread.IsBackground = true;
            CheckStatusThread.Start();
        }
        public void ServoOnCheck()
        {
            int m120;
            plc.GetDevice("M120", out m120);
            if (m120 == 1) // m100 == 1
            {
                mwd.Dispatcher.Invoke(() =>
                {
                    mwd.badgedUI.Badge = "ON"; // đây đúng k
                    mwd.badgedUI.BadgeColorZoneMode = MaterialDesignThemes.Wpf.ColorZoneMode.Standard; //
                });
            }
            else
            {
                mwd.Dispatcher.Invoke(() =>
                {
                    mwd.badgedUI.Badge = "OFF"; // đây đúng k
                    mwd.badgedUI.BadgeColorZoneMode = MaterialDesignThemes.Wpf.ColorZoneMode.Dark; //
                });
            }
        }
        public void ModeSelectionCheck()
        {
            if (ControlAutoViewModel.autoFlag == true)
            {
                mwd.Dispatcher.Invoke(() =>
                {
                    mwd.badgedUI.IsEnabled = false;
                    mwd.rad_btn_Auto.IsEnabled = false;
                    mwd.rad_btn_Manual.IsEnabled = false;
                });
            }
            else
            {
                mwd.Dispatcher.Invoke(() =>
                {
                    mwd.badgedUI.IsEnabled = true;
                    mwd.rad_btn_Auto.IsEnabled = true;
                    mwd.rad_btn_Manual.IsEnabled = true;
                });
            }
        }
        public void SignalPanelCheck()
        {
            string BitAddress = "Y80\nX80\nX8C\nX8D\nX8E\nX8F\nX88\nX89\nX8A\nX8B";
            MaterialDesignThemes.Wpf.PackIcon[] UIicons = new MaterialDesignThemes.Wpf.PackIcon[10] { mwd.ico_PLC_Ready, mwd.ico_QD75_Ready, mwd.ico_AXISX_Busy, mwd.ico_AXISY_Busy, mwd.ico_AXISZ1_Busy, mwd.ico_AXISZ2_Busy, mwd.ico_AXISX_Error, mwd.ico_AXISY_Error, mwd.ico_AXISZ1_Error, mwd.ico_AXISZ2_Error };
            Checkgroup(BitAddress, UIicons);
        }
        public void ThreadCheckExecution()
        {
            while(true)
            {
                EmergencyCheck();
                ModeSelectionCheck();
                ServoOnCheck();
                SignalPanelCheck();
            }    
        }
        public void EmergencyCheck()
        {
            int m0;
            plc.GetDevice("M0", out m0);
            if (m0 == 1)
            {
                Application.Current.Shutdown();
            }    
        }
        public void Connection()
        {
            plc.ActLogicalStationNumber = 1;
            iretCon = plc.Open();
            if (iretCon == 0)
            {
                MessageBox.Show("Connected", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Failed Connection", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void Checkgroup(string bit, MaterialDesignThemes.Wpf.PackIcon[] icons)
        {
            int iReturnCode = 99;
            short[] lpshDeviceValue = new short[10];
            try
            {
                //The ReadDeviceRandom2 method is executed.
                iReturnCode = plc.ReadDeviceRandom2(bit, 10, out lpshDeviceValue[0]);
            }
            catch
            {
                //MessageBox.Show(exception.Message);
                for (int i = 0; i < 10; i++)
                {
                    icons[i].Background = Brushes.Red;
                    icons[i].Kind = MaterialDesignThemes.Wpf.PackIconKind.CloseNetwork;
                }
                return;
            }
            finally
            {
                mwd.Dispatcher.Invoke(() => 
                {
                    if (iReturnCode == 0)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            if (lpshDeviceValue[i] == 1)
                            {
                                if (i <= 1)
                                {
                                    icons[i].Background = Brushes.Lime;
                                    icons[i].Kind = MaterialDesignThemes.Wpf.PackIconKind.LanConnect;
                                }    
                                else if (i < 6 && i > 1)
                                {
                                    icons[i].Background = Brushes.Lime;
                                    icons[i].Kind = MaterialDesignThemes.Wpf.PackIconKind.Wifi;
                                }    
                                else
                                {
                                    icons[i].Background = Brushes.Red;
                                    icons[i].Kind = MaterialDesignThemes.Wpf.PackIconKind.Alert;
                                }    
                            }
                            else
                            {
                                if (i <= 1)
                                {
                                    icons[i].Background = Brushes.Transparent;
                                    icons[i].Kind = MaterialDesignThemes.Wpf.PackIconKind.LanDisconnect;
                                }
                                else if (i < 6 && i > 1)
                                {
                                    icons[i].Background = Brushes.Transparent;
                                    icons[i].Kind = MaterialDesignThemes.Wpf.PackIconKind.WifiOff;
                                }
                                else
                                {
                                    icons[i].Background = Brushes.Transparent;
                                    icons[i].Kind = MaterialDesignThemes.Wpf.PackIconKind.CheckCircle;
                                }
                            }
                        }
                    }
                });
            }
        }
    }
}
