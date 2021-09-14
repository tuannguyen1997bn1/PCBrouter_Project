using ActUtlTypeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace PCBrouter_prj.ViewModel
{
    public class ControlManualViewModel : BaseViewModel
    {

        #region Defination
        public static DispatcherTimer TimerCheckErrors;
        public static string flag_KnifeSelect = "knife1";
        private ActUtlType plc;
        private UserControlKteam.ControlManual ctrManual;
        private string _ErrorCodeX;
        public string ErrorCodeX
        {
            get { return _ErrorCodeX; }
            set
            {
                _ErrorCodeX = value;
                OnPropertyChanged("ErrorCodeX");
            }
        }
        private string _ErrorCodeY;
        public string ErrorCodeY
        {
            get { return _ErrorCodeY; }
            set
            {
                _ErrorCodeY = value;
                OnPropertyChanged("ErrorCodeY");
            }
        }
        private string _ErrorCodeZ1;
        public string ErrorCodeZ1
        {
            get { return _ErrorCodeZ1; }
            set
            {
                _ErrorCodeZ1 = value;
                OnPropertyChanged("ErrorCodeZ1");
            }
        }
        private string _ErrorCodeZ2;
        public string ErrorCodeZ2
        {
            get { return _ErrorCodeZ2; }
            set
            {
                _ErrorCodeZ2 = value;
                OnPropertyChanged("ErrorCodeZ2");
            }
        }
        private string _SpeedValueX;
        public string SpeedValueX
        {
            get { return _SpeedValueX; }
            set
            {
                _SpeedValueX = value;
                OnPropertyChanged("SpeedValueX");
            }
        }
        private string _SpeedValueY;
        public string SpeedValueY
        {
            get { return _SpeedValueY; }
            set
            {
                _SpeedValueY = value;
                OnPropertyChanged("SpeedValueY");
            }
        }
        private string _SpeedValueZ1;
        public string SpeedValueZ1
        {
            get { return _SpeedValueZ1; }
            set
            {
                _SpeedValueZ1 = value;
                OnPropertyChanged("SpeedValueZ1");
            }
        }
        private string _SpeedValueZ2;
        public string SpeedValueZ2
        {
            get { return _SpeedValueZ2; }
            set
            {
                _SpeedValueZ2 = value;
                OnPropertyChanged("SpeedValueZ2");
            }
        }
        private string _PositionNoX;
        public string PositionNoX
        {
            get { return _PositionNoX; }
            set
            {
                _PositionNoX = value;
                OnPropertyChanged("PositionNoX");
            }
        }
        private string _PositionNoY;
        public string PositionNoY
        {
            get { return _PositionNoY; }
            set
            {
                _PositionNoY = value;
                OnPropertyChanged("PositionNoY");
            }
        }
        private string _PositionNoZ1;
        public string PositionNoZ1
        {
            get { return _PositionNoZ1; }
            set
            {
                _PositionNoZ1 = value;
                OnPropertyChanged("PositionNoZ1");
            }
        }
        private string _PositionNoZ2;
        public string PositionNoZ2
        {
            get { return _PositionNoZ2; }
            set
            {
                _PositionNoZ2 = value;
                OnPropertyChanged("PositionNoZ2");
            }
        }
        private string _PositionValX;
        public string PositionValX
        {
            get { return _PositionValX; }
            set
            {
                _PositionValX = value;
                OnPropertyChanged("PositionValX");
            }
        }
        private string _PositionValY;
        public string PositionValY
        {
            get { return _PositionValY; }
            set
            {
                _PositionValY = value;
                OnPropertyChanged("PositionValY");
            }
        }
        private string _PositionValZ1;
        public string PositionValZ1
        {
            get { return _PositionValZ1; }
            set
            {
                _PositionValZ1 = value;
                OnPropertyChanged("PositionValZ1");
            }
        }
        private string _PositionValZ2;
        public string PositionValZ2
        {
            get { return _PositionValZ2; }
            set
            {
                _PositionValZ2 = value;
                OnPropertyChanged("PositionValZ2");
            }
        }
        #endregion
        public ICommand LoadedManualUCCommand { get; set; }
        public ICommand ClosingManualUCCommand { get; set; }
        public ICommand SetSpeedCommand { get; set; }
        public ICommand ReverseUpCommand { get; set; }
        public ICommand ReverseDownCommand { get; set; }
        public ICommand ForwardUpCommand { get; set; }
        public ICommand ForwardDownCommand { get; set; }
        public ICommand ResetCommand { get; set; }
        public ICommand HomeCommand { get; set; }
        public ICommand ExcuteNoCommand { get; set; }
        public ICommand KnifeSelectCommand { get; set; }
        public ICommand RunKnifeCommand { get; set; }
        public ICommand Knife_Speed1_Command { get; set; }
        public ICommand Knife_Speed2_Command { get; set; }
        public ICommand Knife_Speed3_Command { get; set; }
        public ICommand Brake1_Command { get; set; }
        public ICommand Brake2_Command { get; set; }
        public ControlManualViewModel()
        {
            ClosingManualUCCommand = new RelayCommand<UserControlKteam.ControlManual>((p) => { return true; }, (p) =>
            {
                LoginViewModel.IsLogin = false;
            });
            LoadedManualUCCommand = new RelayCommand<UserControlKteam.ControlManual>((p) => { return true; }, (p) =>
            {
                plc = MainViewModel.plc;
                ctrManual = p;
                TimerCheckErrors = new DispatcherTimer();
                TimerCheckErrors.Interval = new TimeSpan(0, 0, 0, 0, 250);
                TimerCheckErrors.Tick += TimerCheckErrors_Tick;
                TimerCheckErrors.Start();
            });
            KnifeSelectCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                if (p.ToString() == "rad_btn_knife1")
                {
                    flag_KnifeSelect = "knife1";
                }
                else if (p.ToString() == "rad_btn_knife2")
                {
                    flag_KnifeSelect = "knife2";
                }
            });
            Brake1_Command = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                int m115;
                plc.GetDevice("M115", out m115);
                if (m115 == 0) 
                {
                    plc.SetDevice("M115", 1);
                }
                else
                {
                    plc.SetDevice("M115", 0);
                }
            });
            Brake2_Command = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                int m116;
                plc.GetDevice("M116", out m116);
                if (m116 == 0)
                {
                    plc.SetDevice("M116", 1);
                }
                else
                {
                    plc.SetDevice("M116", 0);
                }
            });
            RunKnifeCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                if (flag_KnifeSelect == "knife1")
                {
                    int m107;
                    plc.GetDevice("M107", out m107);
                    if (m107 == 0)
                    {
                        plc.SetDevice("M107", 1);
                    }   
                    else
                    {
                        plc.SetDevice("M107", 0);
                    }    
                }   
                if (flag_KnifeSelect == "knife2")
                {
                    int m111;
                    plc.GetDevice("M111", out m111);
                    if (m111 == 0)
                    {
                        plc.SetDevice("M111", 1);
                    }
                    else
                    {
                        plc.SetDevice("M111", 0);
                    }
                }    
            });
            Knife_Speed1_Command = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                if (flag_KnifeSelect == "knife1")
                {
                    int m108;
                    plc.GetDevice("M108", out m108);
                    if (m108 == 0)
                    {
                        plc.SetDevice("M108", 1);
                    }
                    else
                    {
                        plc.SetDevice("M108", 0);
                    }
                }
                if (flag_KnifeSelect == "knife2")
                {
                    int m112;
                    plc.GetDevice("M112", out m112);
                    if (m112 == 0)
                    {
                        plc.SetDevice("M112", 1);
                    }
                    else
                    {
                        plc.SetDevice("M112", 0);
                    }
                }
            });
            Knife_Speed2_Command = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                if (flag_KnifeSelect == "knife1")
                {
                    int m109;
                    plc.GetDevice("M109", out m109);
                    if (m109 == 0)
                    {
                        plc.SetDevice("M109", 1);
                    }
                    else
                    {
                        plc.SetDevice("M109", 0);
                    }
                }
                if (flag_KnifeSelect == "knife2")
                {
                    int m113;
                    plc.GetDevice("M113", out m113);
                    if (m113 == 0)
                    {
                        plc.SetDevice("M113", 1);
                    }
                    else
                    {
                        plc.SetDevice("M113", 0);
                    }
                }
            });
            Knife_Speed3_Command = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                if (flag_KnifeSelect == "knife1")
                {
                    int m110;
                    plc.GetDevice("M110", out m110);
                    if (m110 == 0)
                    {
                        plc.SetDevice("M110", 1);
                    }
                    else
                    {
                        plc.SetDevice("M110", 0);
                    }
                }
                if (flag_KnifeSelect == "knife2")
                {
                    int m114;
                    plc.GetDevice("M114", out m114);
                    if (m114 == 0)
                    {
                        plc.SetDevice("M114", 1);
                    }
                    else
                    {
                        plc.SetDevice("M114", 0);
                    }
                }
            });
            SetSpeedCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                if (p.ToString() == "btn_SetSpeedX")
                {
                    SpeedSet(SpeedValueX,"D200","D201");
                }  
                else if (p.ToString() == "btn_SetSpeedY")
                {
                    SpeedSet(SpeedValueY, "D220", "D221");
                }
                else if (p.ToString() == "btn_SetSpeedZ1")
                {
                    SpeedSet(SpeedValueZ1, "D240", "D241");
                }
                else if (p.ToString() == "btn_SetSpeedZ2")
                {
                    SpeedSet(SpeedValueZ2, "D260", "D261");
                }
            });
            ForwardUpCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                if (p.ToString() == "btn_ForwardX")
                {
                    if (Convert.ToInt32(SpeedValueX) > 0)
                    {
                        plc.SetDevice("M155", 1);
                    }
                }
                else if (p.ToString() == "btn_ForwardY")
                {
                    if (Convert.ToInt32(SpeedValueY) > 0)
                    {
                        plc.SetDevice("M165", 1);
                    }
                }
                else if (p.ToString() == "btn_ForwardZ1")
                {
                    if (Convert.ToInt32(SpeedValueZ1) > 0)
                    {
                        plc.SetDevice("M175", 1);
                    }
                }
                else if (p.ToString() == "btn_ForwardZ2")
                {
                    if (Convert.ToInt32(SpeedValueZ2) > 0)
                    {
                        plc.SetDevice("M185", 1);
                    }
                }
            });
            ForwardDownCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                if (p.ToString() == "btn_ForwardX")
                {
                    plc.SetDevice("M155", 0);
                }
                else if (p.ToString() == "btn_ForwardY")
                {
                    plc.SetDevice("M165", 0);
                }
                else if (p.ToString() == "btn_ForwardZ1")
                {
                    plc.SetDevice("M175", 0);
                }
                else if (p.ToString() == "btn_ForwardZ2")
                {
                    plc.SetDevice("M185", 0);
                }
            });
            ReverseUpCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                if (p.ToString() == "btn_ReverseX")
                {
                    if (Convert.ToInt32(SpeedValueX) > 0)
                    {
                        plc.SetDevice("M156", 1);
                    }
                }
                else if (p.ToString() == "btn_ReverseY")
                {
                    if (Convert.ToInt32(SpeedValueY) > 0)
                    {
                        plc.SetDevice("M166", 1);
                    }
                }
                else if (p.ToString() == "btn_ReverseZ1")
                {
                    if (Convert.ToInt32(SpeedValueZ1) > 0)
                    {
                        plc.SetDevice("M176", 1);
                    }
                }
                else if (p.ToString() == "btn_ReverseZ2")
                {
                    if (Convert.ToInt32(SpeedValueZ2) > 0)
                    {
                        plc.SetDevice("M186", 1);
                    }
                }
            });
            ReverseDownCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                if (p.ToString() == "btn_ReverseX")
                {
                    plc.SetDevice("M156", 0);
                }
                else if (p.ToString() == "btn_ReverseY")
                {
                    plc.SetDevice("M166", 0);
                }
                else if (p.ToString() == "btn_ReverseZ1")
                {
                    plc.SetDevice("M176", 0);
                }
                else if (p.ToString() == "btn_ReverseZ2")
                {
                    plc.SetDevice("M186", 0);
                }
            });
            ResetCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                if (p.ToString() == "btn_ResetX")
                {
                    plc.SetDevice("M154", 1);
                    Thread.Sleep(100);
                    plc.SetDevice("M154", 0);
                }
                else if (p.ToString() == "btn_ResetY")
                {
                    plc.SetDevice("M164", 1);
                    Thread.Sleep(100);
                    plc.SetDevice("M164", 0);
                }
                else if (p.ToString() == "btn_ResetZ1")
                {
                    plc.SetDevice("M174", 1);
                    Thread.Sleep(100);
                    plc.SetDevice("M174", 0);
                }
                else if (p.ToString() == "btn_ResetZ2")
                {
                    plc.SetDevice("M184", 1);
                    Thread.Sleep(100);
                    plc.SetDevice("M184", 0);
                }
            });
            HomeCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                if (p.ToString() == "btn_HomeX")
                {
                    plc.SetDevice("M152", 1);
                    Thread.Sleep(100);
                    plc.SetDevice("M152", 0);
                }
                else if (p.ToString() == "btn_HomeY")
                {
                    plc.SetDevice("M162", 1);
                    Thread.Sleep(100);
                    plc.SetDevice("M162", 0);
                }
                else if (p.ToString() == "btn_HomeZ1")
                {
                    plc.SetDevice("M172", 1);
                    Thread.Sleep(100);
                    plc.SetDevice("M172", 0);
                }
                else if (p.ToString() == "btn_HomeZ2")
                {
                    plc.SetDevice("M182", 1);
                    Thread.Sleep(100);
                    plc.SetDevice("M182", 0);
                }
            });
            ExcuteNoCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                if (p.ToString() == "btn_ExcuteNoX")
                {
                    PositionSet(PositionNoX, "D208", "M157");
                }
                else if (p.ToString() == "btn_ExcuteNoY")
                {
                    PositionSet(PositionNoY, "D228", "M167");
                }
                else if (p.ToString() == "btn_ExcuteNoZ1")
                {
                    PositionSet(PositionNoZ1, "D248", "M177");
                }
                else if (p.ToString() == "btn_ExcuteNoZ2")
                {
                    PositionSet(PositionNoZ2, "D268", "M187");
                }
            });
        }
        private void CheckBrake()
        {
            int m115;
            plc.GetDevice("M115", out m115);
            if (m115 == 0)
            {
                ctrManual.Dispatcher.Invoke(() =>
                {
                    ctrManual.badged_brake1.Badge = "OFF";
                    ctrManual.badged_brake1.BadgeColorZoneMode = MaterialDesignThemes.Wpf.ColorZoneMode.Dark;
                });
            }
            else
            {
                ctrManual.Dispatcher.Invoke(() =>
                {
                    ctrManual.badged_brake1.Badge = "ON";
                    ctrManual.badged_brake1.BadgeColorZoneMode = MaterialDesignThemes.Wpf.ColorZoneMode.Standard;
                });
            }
            int m116;
            plc.GetDevice("M116", out m116);
            if (m116 == 0)
            {
                ctrManual.Dispatcher.Invoke(() =>
                {
                    ctrManual.badged_brake2.Badge = "OFF";
                    ctrManual.badged_brake2.BadgeColorZoneMode = MaterialDesignThemes.Wpf.ColorZoneMode.Dark;
                });
            }
            else
            {
                ctrManual.Dispatcher.Invoke(() =>
                {
                    ctrManual.badged_brake2.Badge = "ON";
                    ctrManual.badged_brake2.BadgeColorZoneMode = MaterialDesignThemes.Wpf.ColorZoneMode.Standard;
                });
            }
        }
        private void TimerCheckErrors_Tick(object sender, EventArgs e)
        {
            ErrorCheck();
            CheckBrake();
        }
        public void ErrorCheck()
        {
            int errX, errY, errZ1, errZ2;
            plc.GetDevice("D20",out errX);
            plc.GetDevice("D50", out errY);
            plc.GetDevice("D80", out errZ1);
            plc.GetDevice("D120", out errZ2);
            ErrorCodeX = errX.ToString();
            ErrorCodeY = errY.ToString();
            ErrorCodeZ1 = errZ1.ToString();
            ErrorCodeZ2 = errZ2.ToString();
        }

        private void SpeedSet(string speedVal, string buffer1, string buffer2)
        {
            int speed;
            bool canConvert = int.TryParse(speedVal, out speed);
            if (canConvert == true)
            {
                plc.SetDevice(buffer1, speed % 65536); // thanh ghi Dxxx
                plc.SetDevice(buffer2, speed / 65536); // thanh ghi Dxxx
            }
            else
            {
                MessageBox.Show("Nhap sai toc do, xin nhap lai!");
            }
        }
        private void PositionSet(string position, string buffer, string bit)
        {
            int pos;
            bool canConvert = int.TryParse(position, out pos);
            if (canConvert == true)
            {
                plc.SetDevice(buffer, pos); // thanh ghi Dxxx
                Thread.Sleep(100);
                plc.SetDevice(bit, 1); // bit thực thi
                Thread.Sleep(100);
                plc.SetDevice(bit, 0);
            }
            else
            {
                MessageBox.Show("Nhap sai vi tri, xin nhap lai!");
            }
        }
    }
}
