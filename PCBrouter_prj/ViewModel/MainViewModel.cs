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
        public static ControlAutoViewModel ctr1;
        public static ActUtlType plc = new ActUtlType();
        public static DispatcherTimer TimerCheckStatus;
        public ICommand LoadedWindowCommand { get; set; }
        public ICommand ClosingWindowCommand { get; set; }
        public ICommand ModeCommand { get; set; }
        public ICommand ServoOnCommand { get; set; }


        private Object _UCview;
        public Object UCview
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
                ctr1 = new ControlAutoViewModel();
                UCview = new ControlAutoViewModel();
                TimerCheckStatus = new DispatcherTimer();
                TimerCheckStatus.Interval = new TimeSpan(0, 0, 0, 0, 250);
                TimerCheckStatus.Tick += TimerCheckStatus_Tick;
                TimerCheckStatus.Start();
                StartThread1();
            });
        }
        private void TimerCheckStatus_Tick(object sender, EventArgs e)
        {
            EmergencyCheck();
            ServoOnCheck();
            //ModeSelectionCheck();
        }
        public Thread ExecutionThread1;
        public void StartThread1()
        {
            if (ExecutionThread1 != null)
            {
                ExecutionThread1.Abort();
            }
            ExecutionThread1 = new Thread(new ThreadStart(ModeSelectionCheck));
            ExecutionThread1.IsBackground = true;
            ExecutionThread1.Start();
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
            while(true)
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
            int iret = plc.Open();
            if (iret == 0)
            {
                MessageBox.Show("Connected", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Failed Connection", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        //public void Operation()
        //{
        //    plc.WriteDeviceRandom("M101\nM102\nM103\nM104\nM105\nM106\nM107\nM108\nM109\nM110", 10, ref ar1[0]);
        //    plc.WriteDeviceRandom("M456\nM50\nM100\nM199\nM300\nM370\nM371", 7, ref ar1[0]);
        //    plc.SetDevice("D200", 0);
        //    plc.SetDevice("D100", 0);
        //}


    }
}
