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
        
        public static ActUtlType plc = new ActUtlType();
        private DispatcherTimer TimerCheckStatus;
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
            ServoOnCommand = new RelayCommand<System.Windows.Controls.Button>((p) => { return true; }, (p) =>
            {

            });
            ClosingWindowCommand = new RelayCommand<MainWindow>((p) => { return true; }, (p) =>
            {
               
            });
           ModeCommand = new RelayCommand<object>((p) => { return true; }, (p)=>
            {
                if (p.ToString() == "ManualModeParameter")
                {
                    UCview = new ControlManualViewModel();
                }    
                else if (p.ToString() == "AutoModeParameter")
                {
                    UCview = new ControlAutoViewModel();
                }    
            });
            LoadedWindowCommand = new RelayCommand<MainWindow>((p) => { return true; }, (p) => {
                mwd = p;
                Connection();
                UCview = new ControlAutoViewModel();
                TimerCheckStatus = new DispatcherTimer();
                TimerCheckStatus.Interval = new TimeSpan(0, 0, 0, 0, 100);
                TimerCheckStatus.Tick += TimerCheckStatus_Tick;
                TimerCheckStatus.Start();
            });
        }
        private void TimerCheckStatus_Tick(object sender, EventArgs e)
        {
            ServoOnCheck();
            ModeSelectionCheck();
        }
        public void ServoOnCheck()
        {
            int m400 = 0;
            plc.GetDevice("M400", out m400);
            if (m400 == 1) // m100 == 1
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
