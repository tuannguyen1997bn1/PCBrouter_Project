using ActUtlTypeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PCBrouter_prj.ViewModel
{
    public class ControlManualViewModel : BaseViewModel
    {
        #region Defination
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
        public ICommand SetSpeedCommand { get; set; }
        public ICommand ReverseUpCommand { get; set; }
        public ICommand ReverseDownCommand { get; set; }
        public ICommand ForwardUpCommand { get; set; }
        public ICommand ForwardDownCommand { get; set; }
        public ICommand ResetCommand { get; set; }
        public ICommand HomeCommand { get; set; }
        public ICommand ExcuteNoCommand { get; set; }
        public ICommand ExcuteValCommand { get; set; }
        public ControlManualViewModel()
        {
            LoadedManualUCCommand = new RelayCommand<UserControlKteam.ControlManual>((p) => { return true; }, (p) =>
            {
                plc = MainViewModel.plc;
                ctrManual = p;
            });
            SetSpeedCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                if (p.ToString() == "btn_SetSpeedX")
                {
                   // SpeedSet(SpeedValueX,"");
                }  
                else if (p.ToString() == "btn_SetSpeedY")
                {
                    //SpeedSet(SpeedValueY, "");
                }
                else if (p.ToString() == "btn_SetSpeedZ1")
                {
                   //SpeedSet(SpeedValueZ1, "");
                }
                else if (p.ToString() == "btn_SetSpeedZ2")
                {
                   // SpeedSet(SpeedValueZ2, "");
                }
            });
            ForwardUpCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                if (p.ToString() == "btn_ForwardX")
                {
                    if (Convert.ToInt32(SpeedValueX) > 0)
                    {
                        plc.SetDevice("M501", 1);
                    }
                }
                else if (p.ToString() == "btn_ForwardY")
                {
                    if (Convert.ToInt32(SpeedValueY) > 0)
                    {
                        plc.SetDevice("M503", 1);
                    }
                }
                else if (p.ToString() == "btn_ForwardZ1")
                {
                    if (Convert.ToInt32(SpeedValueZ1) > 0)
                    {
                        plc.SetDevice("M505", 1);
                    }
                }
                else if (p.ToString() == "btn_ForwardZ2")
                {
                    if (Convert.ToInt32(SpeedValueZ2) > 0)
                    {
                        plc.SetDevice("M507", 1);
                    }
                }
            });
            ForwardDownCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                if (p.ToString() == "btn_ForwardX")
                {
                    plc.SetDevice("M501", 0);
                }
                else if (p.ToString() == "btn_ForwardY")
                {
                    plc.SetDevice("M503", 0);
                }
                else if (p.ToString() == "btn_ForwardZ1")
                {
                    plc.SetDevice("M505", 0);
                }
                else if (p.ToString() == "btn_ForwardZ2")
                {
                    plc.SetDevice("M507", 0);
                }
            });
            ReverseUpCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                if (p.ToString() == "btn_ReverseX")
                {
                    if (Convert.ToInt32(SpeedValueX) > 0)
                    {
                        plc.SetDevice("M502", 1);
                    }
                }
                else if (p.ToString() == "btn_ReverseY")
                {
                    if (Convert.ToInt32(SpeedValueY) > 0)
                    {
                        plc.SetDevice("M504", 1);
                    }
                }
                else if (p.ToString() == "btn_ReverseZ1")
                {
                    if (Convert.ToInt32(SpeedValueZ1) > 0)
                    {
                        plc.SetDevice("M506", 1);
                    }
                }
                else if (p.ToString() == "btn_ReverseZ2")
                {
                    if (Convert.ToInt32(SpeedValueZ2) > 0)
                    {
                        plc.SetDevice("M508", 1);
                    }
                }
            });
            ReverseDownCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                if (p.ToString() == "btn_ReverseX")
                {
                    plc.SetDevice("M502", 0);
                }
                else if (p.ToString() == "btn_ReverseY")
                {
                    plc.SetDevice("M504", 0);
                }
                else if (p.ToString() == "btn_ReverseZ1")
                {
                    plc.SetDevice("M506", 0);
                }
                else if (p.ToString() == "btn_ReverseZ2")
                {
                    plc.SetDevice("M508", 0);
                }
            });
            ResetCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                if (p.ToString() == "btn_ResetX")
                {
                    plc.SetDevice("", 1);
                    Thread.Sleep(100);
                    plc.SetDevice("", 0);
                }
                else if (p.ToString() == "btn_ResetY")
                {
                    plc.SetDevice("", 1);
                    Thread.Sleep(100);
                    plc.SetDevice("", 0);
                }
                else if (p.ToString() == "btn_ResetZ1")
                {
                    plc.SetDevice("", 1);
                    Thread.Sleep(100);
                    plc.SetDevice("", 0);
                }
                else if (p.ToString() == "btn_ResetZ2")
                {
                    plc.SetDevice("", 1);
                    Thread.Sleep(100);
                    plc.SetDevice("", 0);
                }
            });
            HomeCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                if (p.ToString() == "btn_HomeX")
                {
                    plc.SetDevice("", 1);
                    Thread.Sleep(100);
                    plc.SetDevice("", 0);
                }
                else if (p.ToString() == "btn_HomeY")
                {
                    plc.SetDevice("", 1);
                    Thread.Sleep(100);
                    plc.SetDevice("", 0);
                }
                else if (p.ToString() == "btn_HomeZ1")
                {
                    plc.SetDevice("", 1);
                    Thread.Sleep(100);
                    plc.SetDevice("", 0);
                }
                else if (p.ToString() == "btn_HomeZ2")
                {
                    plc.SetDevice("", 1);
                    Thread.Sleep(100);
                    plc.SetDevice("", 0);
                }
            });
            ExcuteNoCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                if (p.ToString() == "btn_ExcuteNoX")
                {
                    PositionSet(PositionNoX, "buffer", "bit");
                }
                else if (p.ToString() == "btn_ExcuteNoY")
                {
                    PositionSet(PositionNoY, "buffer", "bit");
                }
                else if (p.ToString() == "btn_ExcuteNoZ1")
                {
                    PositionSet(PositionNoZ1, "buffer", "bit");
                }
                else if (p.ToString() == "btn_ExcuteNoZ2")
                {
                    PositionSet(PositionNoZ2, "buffer", "bit");
                }
            });
            ExcuteValCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                if (p.ToString() == "btn_ExcuteValX")
                {
                    PositionSet(PositionValX, "buffer", "bit");
                }
                else if (p.ToString() == "btn_ExcuteValY")
                {
                    PositionSet(PositionValY, "buffer", "bit");
                }
                else if (p.ToString() == "btn_ExcuteValZ1")
                {
                    PositionSet(PositionValZ1, "buffer", "bit");
                }
                else if (p.ToString() == "btn_ExcuteValZ2")
                {
                    PositionSet(PositionValZ2, "buffer", "bit");
                }
            });
        }
        private void SpeedSet(string speedVal, string buffer1, string buffer2)
        {
            int speed;
            bool canConvert = int.TryParse(speedVal, out speed);
            if (canConvert == true)
            {
                plc.SetDevice(buffer1, speed); // thanh ghi Dxxx
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
