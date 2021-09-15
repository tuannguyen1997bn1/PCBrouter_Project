//using ActUtlTypeLib;
using PCBrouter_prj.Model;
using System;
using System.Linq;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System.Threading;
using ActUtlTypeLib;
using System.Windows.Threading;
using PCBrouter_prj.ViewModel;
using PCBrouter_prj.UserControlKteam;

namespace PCBrouter_prj.ViewModel
{

    public class ControlAutoViewModel : BaseViewModel
    {
        private ActUtlType plc;
        public static bool autoFlag = false;
        public static int C_sumPosXY;
        public static int R_sumPosYX;
        public static int C_sumXY;
        public static int R_sumYX;
        public static int[,] C_arrPosXY;
        public static int[,] R_arrPosYX;
        public static DispatcherTimer TimerStartAuto;
        public static DispatcherTimer TimerStopAuto;
        private UserControlKteam.ControlAuto ctrAuto;
        public ICommand RunCommand { get; set; }
        public ICommand StopCommand { get; set; }
        public ICommand ResetCommand { get; set; }
        public ICommand HomeCommand { get; set; }
        public ICommand LoadModelCommand { get; set; }
        public ICommand LoadedAutoUCCommand { get; set; }
        private ObservableCollection<ModelList> _ListData;
        public ObservableCollection<ModelList> ListData
        {
            get => _ListData;
            set
            {
                _ListData = value;
                OnPropertyChanged();
            }
        }
        private ModelList _SelectedItems;
        public ModelList SelectedItems
        {
            get { return _SelectedItems; }
            set
            {
                _SelectedItems = value;
                OnPropertyChanged("SelectedItems");
            }
        }
        private string _ModelSelected;
        public string ModelSelected
        {
            get { return _ModelSelected; }
            set
            {
                _ModelSelected = value;
                OnPropertyChanged("ModelSelected");
            }
        }
        private string _XvalSelected;
        public string XvalSelected
        {
            get { return _XvalSelected; }
            set
            {
                _XvalSelected = value;
                OnPropertyChanged("XvalSelected");
            }
        }
        private string _YvalSelected;
        public string YvalSelected
        {
            get { return _YvalSelected; }
            set
            {
                _YvalSelected = value;
                OnPropertyChanged("YvalSelected");
            }
        }
        private string _PCBsumSelected;
        public string PCBsumSelected
        {
            get { return _PCBsumSelected; }
            set
            {
                _PCBsumSelected = value;
                OnPropertyChanged("PCBsumSelected");
            }
        }
        
        public ControlAutoViewModel()
        {
            LoadedAutoUCCommand = new RelayCommand<UserControlKteam.ControlAuto>((p) => { return true; }, (p) =>
            {
                plc = MainViewModel.plc;
                ctrAuto = p;
                CheckState(ctrAuto);
                TimerStartAuto = new DispatcherTimer();
                TimerStartAuto.Interval = new TimeSpan(0, 0, 0, 0, 100);
                TimerStopAuto = new DispatcherTimer();
                TimerStopAuto.Interval = new TimeSpan(0, 0, 0, 0, 100);
                TimerStartAuto.Tick += TimerStartAuto_Tick;
                TimerStopAuto.Tick += TimerStopAuto_Tick;
                TimerStartAuto.IsEnabled = true;
                TimerStartAuto.Start();
                ImportData();
            });
            RunCommand = new RelayCommand<System.Windows.Controls.Button>((p) => { return true; }, (p) =>
            {
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    autoFlag = true;
                });
                //StartThread();
                StartTest();
                ctrAuto.Dispatcher.Invoke(() =>
                {
                    ctrAuto.btn_Run.IsEnabled = false;
                    ctrAuto.btn_Stop.IsEnabled = true;
                    ctrAuto.btn_Reset.IsEnabled = false;
                    ctrAuto.btn_Home.IsEnabled = false;
                    ctrAuto.btn_LoadModel.IsEnabled = false;
                    ctrAuto.grid_dataBox.IsEnabled = false;
                    ctrAuto.grid_tableDB.IsEnabled = false;
                });
                    plc.SetDevice("M101", 1);
                    Thread.Sleep(100);
                    plc.SetDevice("M101", 0);
            });
            StopCommand = new RelayCommand<System.Windows.Controls.Button>((p) => { return true; }, (p) =>
            {
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    autoFlag = false;
                });
                
                ctrAuto.Dispatcher.Invoke(() =>
                {
                    ctrAuto.btn_Run.IsEnabled = true;
                    ctrAuto.btn_Stop.IsEnabled = false;
                    ctrAuto.btn_Reset.IsEnabled = true;
                    ctrAuto.btn_Home.IsEnabled = true;
                    ctrAuto.btn_LoadModel.IsEnabled = true;
                    ctrAuto.grid_dataBox.IsEnabled = true;
                    ctrAuto.grid_tableDB.IsEnabled = true;
                });
                plc.SetDevice("M102", 1);
                Thread.Sleep(100);
                plc.SetDevice("M102", 0);
            });
            ResetCommand = new RelayCommand<System.Windows.Controls.Button>((p) => { return true; }, (p) =>
            {
                plc.SetDevice("M103", 1);
                Thread.Sleep(100);
                plc.SetDevice("M103", 0);
            });
            HomeCommand = new RelayCommand<System.Windows.Controls.Button>((p) => { return true; }, (p) =>
            {
                plc.SetDevice("M104", 1);
                Thread.Sleep(100);
                plc.SetDevice("M104", 0);
            });
            LoadModelCommand = new RelayCommand<System.Windows.Controls.Button>((p) => { return true; }, (p) =>
            {
                
                if (SelectedItems != null)
                {
                    ctrAuto.Dispatcher.Invoke(() =>
                    {
                        ctrAuto.btn_Run.IsEnabled = true;
                    });
                    ModelSelected = SelectedItems.Model.ToString();
                    XvalSelected = SelectedItems.Xval.ToString();
                    YvalSelected = SelectedItems.Yval.ToString();
                    PCBsumSelected = SelectedItems.PCBnum.ToString();
                    var data = DataProvider.Ins.DB.ModelLists.Where(u => u.Id == SelectedItems.Id).AsEnumerable();

                    // chus y phan nay
                    C_arrPosXY = ExecutePosValue(data.LastOrDefault().C_pos_X, data.LastOrDefault().C_distance_Y); // format toa do (X,Y)
                    R_arrPosYX = ExecutePosValue(data.LastOrDefault().R_pos_Y, data.LastOrDefault().R_distance_X); // format toa do (Y,X)
                    C_sumPosXY = SumPosCalculate(data.LastOrDefault().C_pos_X)[0]; // tổng số điểm theo C - 20
                    R_sumPosYX = SumPosCalculate(data.LastOrDefault().R_pos_Y)[0]; // tổng số điểm theo R - 7
                    C_sumXY = SumPosCalculate(data.LastOrDefault().C_pos_X)[1]; // tổng số line/col theo C - 17
                    R_sumYX = SumPosCalculate(data.LastOrDefault().R_pos_Y)[1]; // tổng số line/col theo R - 7
                }
            });
        }
        public void CheckState(ControlAuto user)
        {
            try
            {
                int bitTest;
                int iretTest = plc.GetDevice("Y80", out bitTest);
                if (autoFlag == false && iretTest == 0)
                {
                    user.Dispatcher.Invoke(() =>
                    {
                        if (ModelSelected != null && XvalSelected != null && YvalSelected != null && PCBsumSelected != null)
                        {
                            user.btn_Run.IsEnabled = true;
                        }
                        else
                        {
                            user.btn_Run.IsEnabled = false;
                        }
                        user.btn_Stop.IsEnabled = false;
                        user.btn_Reset.IsEnabled = true;
                        user.btn_Home.IsEnabled = true;
                        user.btn_LoadModel.IsEnabled = true;
                        user.grid_dataBox.IsEnabled = true;
                        user.grid_tableDB.IsEnabled = true;
                    });
                }
                else if (autoFlag == true && iretTest == 0)
                {
                    user.Dispatcher.Invoke(() =>
                    {
                        user.btn_Run.IsEnabled = false;
                        user.btn_Stop.IsEnabled = true;
                        user.btn_Reset.IsEnabled = false;
                        user.btn_Home.IsEnabled = false;
                        user.btn_LoadModel.IsEnabled = false;
                        user.grid_dataBox.IsEnabled = false;
                        user.grid_tableDB.IsEnabled = false;
                    });
                }
                else
                {
                    user.Dispatcher.Invoke(() =>
                    {
                        user.btn_Run.IsEnabled = false;
                        user.btn_Stop.IsEnabled = false;
                        user.btn_Reset.IsEnabled = false;
                        user.btn_Home.IsEnabled = false;
                        user.btn_LoadModel.IsEnabled = false;
                        user.grid_dataBox.IsEnabled = false;
                        user.grid_tableDB.IsEnabled = false;
                    });
                }
            }
            catch
            {
                user.Dispatcher.Invoke(() =>
                {
                    user.btn_Run.IsEnabled = false;
                    user.btn_Stop.IsEnabled = false;
                    user.btn_Reset.IsEnabled = false;
                    user.btn_Home.IsEnabled = false;
                    user.btn_LoadModel.IsEnabled = false;
                    user.grid_dataBox.IsEnabled = false;
                    user.grid_tableDB.IsEnabled = false;
                });
            }
           
        }
        private void TimerStopAuto_Tick(object sender, EventArgs e)
        {
            if (ctrAuto.btn_Stop.IsEnabled == true)
            {
                int bit;
                plc.GetDevice("M1", out bit);
                if (bit == 1)
                {
                    try
                    {
                        ctrAuto.Dispatcher.Invoke(() =>
                        {
                            ctrAuto.btn_Run.IsEnabled = true;
                            ctrAuto.btn_Stop.IsEnabled = false;
                            ctrAuto.btn_Reset.IsEnabled = true;
                            ctrAuto.btn_Home.IsEnabled = true;
                            ctrAuto.btn_LoadModel.IsEnabled = true;
                            ctrAuto.grid_dataBox.IsEnabled = true;
                            ctrAuto.grid_tableDB.IsEnabled = true;
                        });
                        StopModeEnable();
                        autoFlag = false;
                    }
                    catch
                    {

                    }
                }
            }    
        }

        private void TimerStartAuto_Tick(object sender, EventArgs e)
        {
            
            if (ctrAuto.btn_Run.IsEnabled == true)
            {
                int bit = 0;
                //plc.GetDevice("M0", out bit);
                if (bit == 1)
                {
                    try
                    {
                        ctrAuto.Dispatcher.Invoke(() =>
                        {
                            ctrAuto.btn_Run.IsEnabled = false;
                            ctrAuto.btn_Stop.IsEnabled = true;
                            ctrAuto.btn_Reset.IsEnabled = false;
                            ctrAuto.btn_Home.IsEnabled = false;
                            ctrAuto.btn_LoadModel.IsEnabled = false;
                            ctrAuto.grid_dataBox.IsEnabled = false;
                            ctrAuto.grid_tableDB.IsEnabled = false;
                        });
                        RunModeEnable();
                        autoFlag = true;
                        StartTest();
                        //StartThread();
                    }
                    catch
                    {

                    }
                }
            }    
        }
        private void RunModeEnable()
        {
            TimerStartAuto.IsEnabled = false;
            TimerStartAuto.Stop();
            TimerStopAuto.IsEnabled = true;
            TimerStopAuto.Start();
        }
        private void StopModeEnable()
        {
            TimerStopAuto.IsEnabled = false;
            TimerStopAuto.Stop();
            TimerStartAuto.IsEnabled = true;
            TimerStartAuto.Start();
        }
        private int[] SumPosCalculate(string StrPos)
        {
            if (StrPos[StrPos.Length - 1] != 44)
            {
                StrPos = StrPos + ",";
            }
            int len = StrPos.Length;
            int[] rs = new int[2] ;
            for (int i = 0; i < len; i++)
            {
                if (StrPos[i] == 44 || StrPos[i] == 47)
                {
                    rs[0]++;
                    if(StrPos[i] == 44)
                    {
                        rs[1]++;
                    }
                }
            }
            return rs;
        }
        private int[,] ExecutePosValue(string StrPos, string StringDistance)
        {
            string[] arrPosPin = ExecuteStringModel(StrPos); // toa do 1 (string)
            string[] arrPosPinRowCol = ExecuteStringModel(StringDistance); // toa do 2 (string)
            int sumPinVal = SumPosCalculate(StrPos)[0];
            int sumPinsStr = SumPosCalculate(StrPos)[1];
            int[,] intPosPin = new int[sumPinVal, 2];
            int j = 0;
            for (int i = 0; i < sumPinsStr; i++)
            {
                int rs;
                bool canParse = int.TryParse(arrPosPin[i], out rs);
                if (canParse == true)
                {
                    intPosPin[j, 0] = rs;
                    intPosPin[j, 1] = int.Parse(arrPosPinRowCol[i]);
                }
                else if (arrPosPin[i] != null)
                {
                    string[] temp = ExecuteStringModel2(arrPosPin[i]);
                    for (int u = 0; u < temp.Length; u++)
                    {
                        if (temp[u] != null)
                        {
                            intPosPin[j, 0] = int.Parse(temp[u]);
                            intPosPin[j, 1] = int.Parse(arrPosPinRowCol[i]);
                            if (u < (temp.Length - 1))
                            {
                                j++;
                            }
                        }
                    }
                }
                j++;
            }
            return intPosPin;
        }
        private string[] ExecuteStringModel2(string arrPosPin)
        {
            if (arrPosPin[arrPosPin.Length - 1] != 47)
            {
                arrPosPin = arrPosPin + "/";
            }
            int len = arrPosPin.Length;
            int sumPin = 0;
            for (int i = 0; i < len; i++)
            {
                if (arrPosPin[i] == 47)
                {
                    sumPin++;
                }
            }
            string[] rs = new string[sumPin];
            if (len > 0)
            {
                int count = 0;
                int arrLen = 0;
                for (int i = 0; i < len; i++)
                {
                    if (arrPosPin[i] == 47)
                    {
                        int index1 = i - count;
                        string strTemp = arrPosPin.Substring(index1, count);
                        if (strTemp != null)
                        {
                            rs[arrLen] = strTemp;
                            count = 0;
                            arrLen++;
                        }
                    }
                    else
                    {
                        count++;
                    }
                }
            }
            return rs;
        }
        private string[] ExecuteStringModel(string StrPos)
        {
            if (StrPos[StrPos.Length - 1] != 44)
            {
                StrPos = StrPos + ",";
            }
            int len = StrPos.Length;
            int sumPinsStr = SumPosCalculate(StrPos)[1];
            string[] arrPosPin = new string[sumPinsStr];
            if (len > 0)
            {
                int count = 0;
                int arrLen = 0;
                for (int i = 0; i < len; i++)
                {
                    if (StrPos[i] == 44)
                    {
                        int index1 = i - count;
                        string  strTemp = StrPos.Substring(index1, count);
                        if (strTemp != null)
                        {
                            arrPosPin[arrLen] = strTemp;
                            count = 0;
                            arrLen++;
                        }
                    }
                    else
                    {
                        count++;
                    }
                }
            }
            return arrPosPin;
        }
        public void ImportData()
        {
            // gọi biến List data có kiểu như dưới đây
            ListData = new ObservableCollection<ModelList>();

            // gọi biến var của EvenMangerCNCs
            var objectList = DataProvider.Ins.DB.ModelLists;
            int i = 1;
            try
            {
                // check null cho ListData
                if (ListData == null)
                    return;
                else
                {
                    // duyệt từng phần tử của objectlist
                    foreach (var item in objectList)
                    {
                        // khai báo các biến tương ứng với các cột trong table 
                        // và sau đó gán giá trị cho chúng
                        //var Id = DataProvider.Ins.DB.ModelLists.Where(p => p.Id == item.Id);
                        //var model = DataProvider.Ins.DB.ModelLists.Where(p => p.Model == item.Model);
                        //var xval = DataProvider.Ins.DB.ModelLists.Where(p => p.Xval == item.Xval);
                        //var yval = DataProvider.Ins.DB.ModelLists.Where(p => p.Yval == item.Yval);
                        //var pcbnum = DataProvider.Ins.DB.ModelLists.Where(p => p.PCBnum == item.PCBnum);
                        if (item != null)
                        {
                            // nếu item ko bị null thì thực thi bên trong :
                            // khai báo một list kiểu Chart_Model ( 1 list rỗng có cấu trúc tương tự bảng )
                            // sau đó gán giá trị cho từng giá trị của list chart_Model.
                            ModelList chart_Model = new ModelList();
                            chart_Model.Id = i;
                            chart_Model.Model = item.Model;
                            chart_Model.Xval = item.Xval;
                            chart_Model.Yval = item.Yval;
                            chart_Model.PCBnum = item.PCBnum;
                            ListData.Add(chart_Model);
                            i++;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }
        #region AUTO EXECUTION
        // thread test
        public Thread ExecutionThread;
        public void StartThread()
        {
            if (ExecutionThread != null)
            {
                ExecutionThread.Abort();
            }
            ExecutionThread = new Thread(new ThreadStart(ExecutionMethod));
            ExecutionThread.IsBackground = true;
            ExecutionThread.Start();
        }
        public void StartTest()
        {
            if (ExecutionThread != null)
            {
                ExecutionThread.Abort();
            }
            ExecutionThread = new Thread(new ThreadStart(ExecutionTest));
            ExecutionThread.IsBackground = true;
            ExecutionThread.Start();
        }
        public void ExecutionTest()
        {
            // m200 ghi tọa độ + chạy
            // D1000 X
            // D1100 Y
            // bit vào chu trình M1666
            // bit hoàn thành M1777
            // bit ngắt chu trình M1444
            try
            {
                int flag1 = 0;
                int flag2 = 0;
                int i = 0;
                int[,] arr = new int[4, 2] { { 200000, 200000 }, { 500000, 200000 }, { 500000, 500000 }, { 200000, 500000 } };
                int total = arr.Length / 2;
                while (flag1 == 0 && flag2 == 0 && autoFlag == true)
                    {
                        int m1666;
                        plc.GetDevice("M1666", out m1666);
                        if (m1666 == 1)
                        {
                            plc.SetDevice("M1777", 1);
                            flag1 = 1;

                        }
                    }
                while (flag1 == 1 && flag2 == 0 && autoFlag == true)
                    {
                        if (ModelSelected != "" && XvalSelected != "" && YvalSelected != "" && PCBsumSelected != "")
                        {
                            int m1777;
                            plc.GetDevice("M1777", out m1777);
                            if (m1777 == 1)
                            {
                                if (i <= 4)
                                {
                                    plc.SetDevice("M1777", 0);
                                    if (i < 4)
                                    {
                                        plc.SetDevice("D1000", arr[i, 0] % 65536);
                                        plc.SetDevice("D1001", arr[i, 0] / 65536);
                                        plc.SetDevice("D1100", arr[i, 1] % 65536);
                                        plc.SetDevice("D1101", arr[i, 1] / 65536);
                                    }
                                    else if (i == 4)
                                    {
                                        plc.SetDevice("D1000", 0 % 65536);
                                        plc.SetDevice("D1001", 0 / 65536);
                                        plc.SetDevice("D1100", 0 % 65536);
                                        plc.SetDevice("D1101", 0 / 65536);
                                    }
                                    Thread.Sleep(100);
                                    plc.SetDevice("M200", 1);
                                //int flagtemp = 0;
                                    i++;
                                //while (flagtemp == 0)
                                //{
                                //    if (i < 4 )
                                //    {
                                //        short[] currentVal = new short[4];
                                //        plc.ReadDeviceRandom2("D0\nD1\nD30\nD31"/*nD60\nD61\nD90\nD91*/, 4, out currentVal[0]);
                                //        int Zcut = 12000;
                                //        if (currentVal[0] == (arr[i, 0] % 65536) && currentVal[1] == (arr[i, 0] / 65536) && currentVal[2] == (arr[i, 1] % 65536) && currentVal[3] == (arr[i, 1] / 65536))
                                //        {
                                //            plc.SetDevice("D1200", Zcut % 65536);
                                //            plc.SetDevice("D1201", Zcut / 65536);
                                //            Thread.Sleep(50);

                                //            flagtemp = 1;
                                //        }
                                //    }
                                //    else if ( i == 4 )
                                //    {
                                //        flagtemp = 1;
                                //    }
                                    
                                //}
                                // d0 : feed value X
                                // d30 : feed value Y
                                // d60 : feed value Z1
                                // d90 : feed value Z2
                                }
                                else
                                {
                                    flag2 = 1;
                                }
                            }
                        }
                    }
                while (flag1 == 1 && flag2 == 1 && autoFlag == true)
                    {
                        plc.SetDevice("M1444", 1);
                        flag1 = 0;
                        flag2 = 0;
                    }
               
            }
            catch
            {

            }
            finally
            {
                if (ExecutionThread != null)
                {
                    ExecutionThread.Abort();
                }
            }
        }
        public void ExecutionMethod()
        {
            try
            {
                plc = MainViewModel.plc;
                var data = DataProvider.Ins.DB.ModelLists.Where(u => u.Id == SelectedItems.Id).AsEnumerable().LastOrDefault();
                int flag1 = 0;
                int flag2 = 0;
                int flag3 = 0;
                int totalPosXY = C_sumPosXY * data.C_MotionShape_num;
                int totalPosYX = R_sumPosYX * data.R_MotionShape_num;
                while (autoFlag == true)
                {
                    int i = 0;
                    int j = 0;
                    while (flag1 == 0 && flag2 == 0 && flag3 == 0 && autoFlag == true)
                    {
                        int m1666; // bắt đầu chu trình
                        plc.GetDevice("M1666", out m1666);
                        if (m1666 == 1)
                        {
                            plc.SetDevice("M1777", 1);
                            flag1 = 1;
                        }
                    }
                    while (flag1 == 1 && flag2 == 0 && flag3 == 0 && autoFlag == true) // === XY ===
                    {
                        if (ModelSelected != "" && XvalSelected != "" && YvalSelected != "" && PCBsumSelected != "")
                        {
                            int m1777;
                            plc.GetDevice("M1777", out m1777);
                            if (m1777 == 1)
                            {
                                if (i <= totalPosXY)
                                {
                                    plc.SetDevice("M1777", 0);
                                    if (i < totalPosXY)
                                    {
                                        ExecutePosXY(i);
                                    }
                                    else if (i == totalPosXY)
                                    {
                                        plc.SetDevice("D1000", 0 % 65536);
                                        plc.SetDevice("D1001", 0 / 65536);
                                        plc.SetDevice("D1100", 0 % 65536);
                                        plc.SetDevice("D1101", 0 / 65536);
                                    }
                                    Thread.Sleep(100);
                                    plc.SetDevice("M200", 1); // bit chạy ngang
                                    i++;
                                }
                                else
                                {
                                    flag2 = 1;
                                }
                            }
                        }
                    }
                    while (flag1 == 1 && flag2 == 1 && flag3 == 0 && autoFlag == true) // === YX ===
                    {
                        if (ModelSelected != "" && XvalSelected != "" && YvalSelected != "" && PCBsumSelected != "")
                        {
                            int m1777;
                            plc.GetDevice("M1777", out m1777);
                            if (m1777 == 1)
                            {
                                if (j <= totalPosXY)
                                {
                                    plc.SetDevice("M1777", 0);
                                    if (j < totalPosXY)
                                    {
                                        ExecutePosYX(i);
                                    }
                                    else if (j == totalPosXY)
                                    {
                                        plc.SetDevice("D1000", 0 % 65536);
                                        plc.SetDevice("D1001", 0 / 65536);
                                        plc.SetDevice("D1100", 0 % 65536);
                                        plc.SetDevice("D1101", 0 / 65536);
                                    }
                                    Thread.Sleep(100);
                                    plc.SetDevice("M201", 1); // bit chạy dọc
                                    j++;
                                }
                                else
                                {
                                    flag3 = 1;
                                }
                            }
                        }
                    }
                    while (flag1 == 1 && flag2 == 1 && flag3 == 1 && autoFlag == true) // === tắt hút chân không/ final step ===
                    {
                        int d200;
                        int d400;
                        plc.GetDevice("D200", out d200);
                        plc.GetDevice("D400", out d400);
                        if (d200 == 0 && d400 == 0)
                        {
                            plc.SetDevice("M1444", 1);
                            flag1 = 0;
                            flag2 = 0;
                            flag3 = 0;
                            i = 1;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (ExecutionThread != null)
                {
                    ExecutionThread.Abort();
                }
            }
        }
        public void ExecutePosXY(int i)
        {
            try
            {
                int[,] arrPosPinXY = C_arrPosXY;
                var data = DataProvider.Ins.DB.ModelLists.Where(u => u.Id == SelectedItems.Id).AsEnumerable().LastOrDefault();
                int Yval = arrPosPinXY[i, 1];
                int Xval = arrPosPinXY[i, 0] + (i / C_sumPosXY) * int.Parse(data.C_MotionShape_distance_X);
                plc.SetDevice("D800", Xval % 65536); // X value
                plc.SetDevice("D801", Xval / 65536); // X value
                plc.SetDevice("D1000", Yval % 65536); // Y value
                plc.SetDevice("D1001", Yval / 65536); // Y value
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void ExecutePosYX(int i)
        {
            try
            {
                int[,] arrPosPinYX = R_arrPosYX;
                var data = DataProvider.Ins.DB.ModelLists.Where(u => u.Id == SelectedItems.Id).AsEnumerable().LastOrDefault();
                int Yval = arrPosPinYX[i, 0] + (i / R_sumPosYX) * int.Parse(data.C_MotionShape_distance_X);
                int Xval = arrPosPinYX[i, 1];
                plc.SetDevice("D800", Xval % 65536); // X value
                plc.SetDevice("D801", Xval / 65536); // X value
                plc.SetDevice("D1000", Yval % 65536); // Y value
                plc.SetDevice("D1001", Yval / 65536); // Y value
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void InvokeUI(Action a)
        {
            //System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(new System.Windows.Forms.MethodInvoker(a));
            System.Windows.Application.Current.Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(a));
        }
        #endregion
    }
}
