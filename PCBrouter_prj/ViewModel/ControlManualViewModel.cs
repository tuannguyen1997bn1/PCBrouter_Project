using ActUtlTypeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace PCBrouter_prj.ViewModel
{
    public class ControlManualViewModel : BaseViewModel
    {
        #region VARIABLE-BIDING DEFINATION
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

        #region COMMANDS DEFINATION
        public ICommand LoadedManualUCCommand { get; set; }
        public ICommand ClosedManualUCCommand { get; set; }
        public ICommand SetSpeedCommand { get; set; }
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
        public ICommand ForwardCommand { get; set; }
        public ICommand ReverseCommand { get; set; }
        #endregion

        #region CONTRUCTOR AND COMMANDS'S EXECUTION
        public ControlManualViewModel()
        {
            ReverseCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                if (p.ToString() == "btn_ReverseX")
                {
                    plc.GetDevice("M156", out int m156);
                    if (m156 == 0)
                    {
                        if (int.Parse(SpeedValueX) != 0)
                        {
                            plc.SetDevice("M156", 1);
                        }    
                    }
                    else
                    {
                        plc.SetDevice("M156", 0);
                    }

                }
                else if (p.ToString() == "btn_ReverseY")
                {
                    plc.GetDevice("M166", out int m166);
                    if (m166 == 0)
                    {
                        if (int.Parse(SpeedValueY) != 0)
                            plc.SetDevice("M166", 1);
                    }
                    else
                    {
                        plc.SetDevice("M166", 0);
                    }
                }
                else if (p.ToString() == "btn_ReverseZ1")
                {

                    plc.GetDevice("M176", out int m176);
                    if (m176 == 0)
                    {
                        if (int.Parse(SpeedValueZ1) != 0)
                            plc.SetDevice("M176", 1);
                    }
                    else
                    {
                        plc.SetDevice("M176", 0);
                    }
                }
                else if (p.ToString() == "btn_ReverseZ2")
                {

                    plc.GetDevice("M186", out int m186);
                    if (m186 == 0)
                    {
                        if (int.Parse(SpeedValueZ2) != 0)
                            plc.SetDevice("M186", 1);
                    }
                    else
                    {
                        plc.SetDevice("M186", 0);
                    }
                }
            });
            ForwardCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                
                if (p.ToString() == "btn_ForwardX")
                {
                    plc.GetDevice("M155", out int m155);
                    if (m155 == 0 )
                    {
                        if (int.Parse(SpeedValueX) != 0)
                            plc.SetDevice("M155", 1);
                    }    
                    else
                    {
                        plc.SetDevice("M155", 0);
                    }    

                }
                else if (p.ToString() == "btn_ForwardY")
                {
                    plc.GetDevice("M165", out int m165);
                    if (m165 == 0)
                    {
                        if (int.Parse(SpeedValueY) != 0)
                            plc.SetDevice("M165", 1);
                    }
                    else
                    {
                        plc.SetDevice("M165", 0);
                    }
                }
                else if (p.ToString() == "btn_ForwardZ1")
                {

                    plc.GetDevice("M175", out int m175);
                    if (m175 == 0)
                    {
                        if (int.Parse(SpeedValueZ1) != 0)
                            plc.SetDevice("M175", 1);
                    }
                    else
                    {
                        plc.SetDevice("M175", 0);
                    }
                }
                else if (p.ToString() == "btn_ForwardZ2")
                {

                    plc.GetDevice("M175", out int m185);
                    if (m185 == 0)
                    {
                        if (int.Parse(SpeedValueZ2) != 0)
                            plc.SetDevice("M185", 1);
                    }
                    else
                    {
                        plc.SetDevice("M185", 0);
                    }
                }
            });
            ClosedManualUCCommand = new RelayCommand<UserControlKteam.ControlManual>((p) => { return true; }, (p) =>
            {
                LoginViewModel.IsLogin = false;
                TimerCheckErrors.Stop();
            });
            LoadedManualUCCommand = new RelayCommand<UserControlKteam.ControlManual>((p) => { return true; }, (p) =>
            {
                plc = MainViewModel.plc;
                ctrManual = p;
                BrakeCheck();
                TimerCheckErrors = new DispatcherTimer();
                TimerCheckErrors.Interval = new TimeSpan(0, 0, 0, 0, 300);
                TimerCheckErrors.Tick += TimerCheckErrors_Tick;
                TimerCheckErrors.Start();
                plc.SetDevice("M105", 0);
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
                BrakeCheck();
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
                BrakeCheck();
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
        #endregion

        #region SUPPORT METHODS
        public void BrakeCheck()
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
            BrakeCheck();
        }
        public void ErrorCheck()
        {
            short[] errors = new short[4];
            plc.ReadDeviceRandom2("D20\nD50\nD80\nD120", 4, out errors[0]);
            ErrorCodeX = errors[0].ToString();
            ErrorCodeY = errors[1].ToString();
            ErrorCodeZ1 = errors[2].ToString();
            ErrorCodeZ2 = errors[3].ToString();
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
        #endregion
    }
}
