﻿using System;
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
using MaterialDesignThemes.Wpf;

namespace PCBrouter_prj.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public static MainWindow mwd;
        public Thread CheckStatusThread;
        public static int iretCon;
        public static ActUtlType plc = new ActUtlType();
        public static DispatcherTimer TimerCheckStatus;
        public ICommand LoadedWindowCommand { get; set; }
        public ICommand ClosingWindowCommand { get; set; }
        public ICommand ModeCommand { get; set; }
        public ICommand ServoOnCommand { get; set; }
        public int SevoOnflag { get; set; }
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
                    AccessManualMode();
                }    
                else if (p.ToString() == "AutoModeParameter")
                {
                    UCview = new ControlAutoViewModel();
                    plc.SetDevice("M105", 1);
                }
                SwitchModeCheck();
            });
            LoadedWindowCommand = new RelayCommand<MainWindow>((p) => { return true; }, (p) => {
                mwd = p;
                Connection();
                UCview = new ControlAutoViewModel();
                plc.SetDevice("M105", 1);
                StartCheckStatusThread();
            });
        }
        public void Connection()
        {
            plc.ActLogicalStationNumber = 2;
            iretCon = plc.Open();
            if (iretCon == 0)
            {
                MessageBox.Show("Đã kết nối tới PLC", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Không thể kết nối tới PLC", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        public void AccessManualMode()
        {
            LoginWindow loginWD = new LoginWindow();
            loginWD.ShowDialog();
            if (LoginViewModel.IsLogin == true)
            {
                UCview = new ControlManualViewModel();
                plc.SetDevice("M105", 0);
            }
        }
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
        public void ThreadCheckExecution()
        {
            while (true)
            {
                ModeSelection_Emergency_Check();
                ServoOnCheck();
                SignalPanelCheck();
            }
        }
        public void ServoOnCheck()
        {
            int iretSvOn = plc.GetDevice("M120", out int m120);
            if (iretSvOn == 0)
            {
                if (m120 == 1)
                {
                    mwd.Dispatcher.Invoke(() =>
                    {
                        SevoOnflag = 1;
                        mwd.badgedUI.Badge = "ON";
                        mwd.badgedUI.BadgeColorZoneMode = MaterialDesignThemes.Wpf.ColorZoneMode.Standard; 
                    });
                }
                else
                {
                    mwd.Dispatcher.Invoke(() =>
                    {
                        SevoOnflag = 0;
                        mwd.badgedUI.Badge = "OFF";
                        mwd.badgedUI.BadgeColorZoneMode = MaterialDesignThemes.Wpf.ColorZoneMode.Dark; 
                    });
                }
            }
            else
            {
                mwd.Dispatcher.Invoke(() =>
                {
                    mwd.badgedUI.IsEnabled = false;
                    mwd.badgedUI.Badge = "ERROR";
                    mwd.badgedUI.BadgeColorZoneMode = MaterialDesignThemes.Wpf.ColorZoneMode.Dark; 
                });
            }
        }
        public void SwitchModeCheck()
        {
            if (LoginViewModel.IsLogin == false)
            {
                mwd.Dispatcher.Invoke(() =>
                {
                    mwd.rad_btn_Auto.IsChecked = true;
                    mwd.rad_btn_Auto.Tag = true;
                    mwd.rad_btn_Manual.IsEnabled = false;
                    mwd.rad_btn_Manual.Tag = false;
                });
            }
        }
        public void ModeSelection_Emergency_Check()
        {
            int iretEmr = plc.GetDevice("M0", out int m0);
            if (iretEmr == 0)
            {
                //if (m0 == 1)
                //{
                //    Application.Current.Shutdown();
                //}
                if (ControlAutoViewModel.autoFlag == true || ControlAutoViewModel.flagCal == true)
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
            else
            {
                mwd.Dispatcher.Invoke(() =>
                {
                    mwd.badgedUI.IsEnabled = false;
                    mwd.rad_btn_Auto.IsEnabled = false;
                    mwd.rad_btn_Manual.IsEnabled = false;
                });
            }
        }
        public void SignalPanelCheck()
        {
            string BitAddress = "Y80\nY81\nX8C\nX8D\nX8E\nX8F\nX88\nX89\nX8A\nX8B";
            PackIcon[] UIicons = new PackIcon[10] { mwd.ico_PLC_Ready, mwd.ico_QD75_Ready, mwd.ico_AXISX_Busy, mwd.ico_AXISY_Busy, mwd.ico_AXISZ1_Busy, mwd.ico_AXISZ2_Busy, mwd.ico_AXISX_Error, mwd.ico_AXISY_Error, mwd.ico_AXISZ1_Error, mwd.ico_AXISZ2_Error };
            Checkgroup(BitAddress, UIicons);
        }
        private void Checkgroup(string bit, MaterialDesignThemes.Wpf.PackIcon[] icons)
        {
            int iReturnCode = 99;
            short[] lpshDeviceValue = new short[10];
            try
            {
                iReturnCode = plc.ReadDeviceRandom2(bit, 10, out lpshDeviceValue[0]);
            }
            catch
            {
                return;
            }
            finally
            {
                
                    if (iReturnCode == 0)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            if (lpshDeviceValue[i] == 1)
                            {
                                if (i <= 1)
                                {
                                    mwd.Dispatcher.Invoke(() =>
                                    {
                                        icons[i].Background = Brushes.Lime;
                                        icons[i].Kind = MaterialDesignThemes.Wpf.PackIconKind.LanConnect;
                                    });
                                }    
                                else if (i < 6 && i > 1)
                                {
                                    mwd.Dispatcher.Invoke(() =>
                                    {
                                        icons[i].Background = Brushes.Lime;
                                        icons[i].Kind = MaterialDesignThemes.Wpf.PackIconKind.Wifi;
                                    });
                                    
                                }    
                                else
                                {
                                    mwd.Dispatcher.Invoke(() =>
                                    {
                                        icons[i].Background = Brushes.Red;
                                        icons[i].Kind = MaterialDesignThemes.Wpf.PackIconKind.Alert;
                                    });
                                }    
                            }
                            else
                            {
                                if (i <= 1)
                                {
                                    mwd.Dispatcher.Invoke(() =>
                                    {
                                        icons[i].Background = Brushes.Transparent;
                                        icons[i].Kind = MaterialDesignThemes.Wpf.PackIconKind.LanDisconnect;
                                    });
                                }
                                else if (i < 6 && i > 1)
                                {
                                    mwd.Dispatcher.Invoke(() =>
                                    {
                                        icons[i].Background = Brushes.Transparent;
                                        icons[i].Kind = MaterialDesignThemes.Wpf.PackIconKind.WifiOff;
                                    });
                                }
                                else
                                {
                                    mwd.Dispatcher.Invoke(() =>
                                    {
                                        icons[i].Background = Brushes.Transparent;
                                        icons[i].Kind = MaterialDesignThemes.Wpf.PackIconKind.CheckCircle;
                                    });
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            mwd.Dispatcher.Invoke(() =>
                            {
                                icons[i].Background = Brushes.Red;
                                icons[i].Kind = MaterialDesignThemes.Wpf.PackIconKind.CloseNetwork;
                            });
                        }
                    }
            }
        }
    }
}
