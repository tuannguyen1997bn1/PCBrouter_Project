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
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace PCBrouter_prj.ViewModel
{

    public class ControlAutoViewModel : BaseViewModel
    {
        #region DEFINATION

        IEnumerable<ModelList> dataSource;
        public static bool flagCal = true; // cờ đo dao
        public static bool autoFlag = false; // cờ tự đôngj

        public static int posSum_X = 0; // tổng số điểm cắt
        public static int posSum_Y = 0; // để xác định số cụm cắt (1 hoặc 2)

        public static int sumPos_X; // tổng số điểm chạy cắt tịnh tiến theo chiều X
        public static int sumPos_Y; // tổng số điểm chạy cắt tịnh tiến theo chiều Y

        public static int sumShape_X; // số biên dạng trong 1 PCB cắt tịnh tiến theo chiều X(2)
        public static int sumShape_Y; // số biên dạng trong 1 PCB cắt tịnh tiến theo chiều Y(4)

        public static int cuttingLength; // độ dài cắt  
        public static int cuttingGroove; // khoảng cách giữa 2 đường cắt

        public static int shapeDistance_X; // khoảng cách giữa các biên dạng ( theo X - cụm 20 điểm )
        public static int shapeDistance_Y; // khoảng cách giữa các biên dạng ( theo Y - cụm 7 điểm )

        public static int Z1_Auto_Val = 0; // khoảng lệch giữa Z1 cắt và điểm đo dao Z1
        public static int Z2_Auto_Val = 0; // khoảng lệch giữa Z1 cắt và điểm đo dao Z2

        public static int PiecePCB_distance_X = 2198000; // khoảng cách giữa 2 PCB ngoài và 2 PCB trong - theo X
        public static int PiecePCB_distance_Y = 1000; // offset giữa 2 PCB ngoài và trong - theo Y

        public static int DistanceDefault_Z1 = 477100; // độ dài cố định giữa điểm đo dao và điểm cắt Z1
        public static int DistanceDefault_Z2 = 479000; // độ dài cố định giữa điểm đo dao và điểm cắt Z2

        public static int[,] arrPos_X; // tọa độ các điểm bắt đầu cắt tịnh tiến theo chiều X (20;2)
        public static int[,] arrPos_Y; // tọa độ các điểm bắt đầu cắt tịnh tiến theo chiều Y (7;2)
        

        private ActUtlType plc;
        public static DispatcherTimer TimerStartAuto;
        public static DispatcherTimer TimerStopAuto;
        public static DispatcherTimer TimerSetKnife;
        public Thread ExecutionThread;
        public Thread KnifeThread;
        private ControlAuto ctrAuto;
        public Window wd;

        public ICommand RunCommand { get; set; }
        public ICommand StopCommand { get; set; }
        public ICommand ResetCommand { get; set; }
        public ICommand HomeCommand { get; set; }
        public ICommand LoadModelCommand { get; set; }
        public ICommand LoadedAutoUCCommand { get; set; }
        public ICommand CheckPosSumCommand { get; set; }

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
                if (SelectedItems != null )
                {
                    ctrAuto.Dispatcher.Invoke(() => { 
                        ctrAuto.btn_LoadModel.IsEnabled = true;
                    });
                }    
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
        private string _CalculatedZ1_Knife;
        public string CalculatedZ1_Knife
        {
            get { return _CalculatedZ1_Knife; }
            set
            {
                _CalculatedZ1_Knife = value;
                OnPropertyChanged("CalculatedZ1_Knife");
            }
        }
        private string _CalculatedZ2_Knife;
        public string CalculatedZ2_Knife
        {
            get { return _CalculatedZ2_Knife; }
            set
            {
                _CalculatedZ2_Knife = value;
                OnPropertyChanged("CalculatedZ2_Knife");
            }
        }
        #endregion

        #region CONTRUCTOR AND COMMAND
        public ControlAutoViewModel()
        {
            CheckPosSumCommand = new RelayCommand<object>((p) => { return true; }, (p) =>
            {
                if (dataSource != null)
                {
                    if (p.ToString() == "Dual_Zone")
                    {
                        posSum_X = sumPos_X * sumShape_X * 2;
                        posSum_Y = sumPos_Y * sumShape_Y * 2;
                    }
                    if (p.ToString() == "Single_Zone")
                    {
                        posSum_X = sumPos_X * sumShape_X;
                        posSum_Y = sumPos_Y * sumShape_Y;
                    }
                }    
                else
                {
                    MessageBox.Show("Xin hãy chọn lại Model và Load Data lại!");
                }    
                   
            });
            LoadedAutoUCCommand = new RelayCommand<UserControlKteam.ControlAuto>((p) => { return true; }, (p) =>
            {
                plc = MainViewModel.plc;
                ctrAuto = p;
                plc.SetDevice("M120", 1);
                plc.SetDevice("M103", 1);
                Thread.Sleep(50);
                plc.SetDevice("M103", 0);
                CheckState(ctrAuto);
                autoFlag = false;
                flagCal = false;
                TimerStartAuto = new DispatcherTimer();
                TimerStartAuto.Interval = new TimeSpan(0, 0, 0, 0, 50);
                TimerStopAuto = new DispatcherTimer();
                TimerStopAuto.Interval = new TimeSpan(0, 0, 0, 0, 50);
                TimerSetKnife = new DispatcherTimer();
                TimerSetKnife.Interval = new TimeSpan(0, 0, 0, 0, 50);

                TimerStartAuto.Tick += TimerStartAuto_Tick1;
                TimerStopAuto.Tick += TimerStopAuto_Tick;
                TimerSetKnife.Tick += TimerSetKnife_Tick;

                TimerStartAuto.IsEnabled = true;
                TimerStartAuto.Start();
                TimerSetKnife.IsEnabled = true;
                TimerSetKnife.Start();

                ImportData();
                plc.SetDevice("M104", 1);
                Thread.Sleep(50);
                plc.SetDevice("M104", 0);
            });
            RunCommand = new RelayCommand<System.Windows.Controls.Button>((p) => { return true; }, (p) =>
            {
                DialogResult result = System.Windows.Forms.MessageBox.Show("Bạn có muốn bắt đầu chu trình tự động không, hãy đảm bảo đã làm đủ các bước set up và kiểm tra kĩ thông tin?", "Xác nhận", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    if (Z1_Auto_Val != 0 && Z2_Auto_Val != 0 && MainViewModel.ServoOnBit == true)
                    {
                        InvokeUI(() =>
                        {
                            autoFlag = true;
                        });
                        plc.SetDevice("M105", 1);
                        StartAutoThread();
                        StopModeEnable();
                        plc.SetDevice("M101", 1);
                        Thread.Sleep(100);
                        plc.SetDevice("M101", 0);
                    }    
                    else if (Z1_Auto_Val == 0 || Z2_Auto_Val == 0)
                    {
                        MessageBox.Show("Chưa set dao!");
                    }   
                    else
                    {
                        MessageBox.Show("Chưa bật servo on!");
                    }    
                }
                else 
                {
                    return;
                }
                
            });
            StopCommand = new RelayCommand<System.Windows.Controls.Button>((p) => { return true; }, (p) =>
            {
                InvokeUI(() =>
                {
                    autoFlag = false;
                    flagCal = false;
                });
                plc.SetDevice("M2", 0);
                plc.SetDevice("M120", 0);
                RunModeEnable();
                plc.SetDevice("M102", 1);
                Thread.Sleep(100);
                plc.SetDevice("M102", 0);
            });
            ResetCommand = new RelayCommand<System.Windows.Controls.Button>((p) => { return true; }, (p) =>
            {
                plc.SetDevice("M105", 1);
                plc.SetDevice("M103", 1);
                Thread.Sleep(100);
                plc.SetDevice("M103", 0);
                autoFlag = false;
                flagCal = false;
                if (KnifeThread != null)
                {
                    KnifeThread.Abort();
                }
            });
            HomeCommand = new RelayCommand<System.Windows.Controls.Button>((p) => { return true; }, (p) =>
            {
                plc.SetDevice("M104", 1);
                Thread.Sleep(100);
                plc.SetDevice("M104", 0);
            });
            LoadModelCommand = new RelayCommand<System.Windows.Controls.Button>((p) => { return true; }, (p) =>
            {
                LoadDataForAuto();
            });
        }
        private void LoadDataForAuto()
        {
            if (SelectedItems != null && ctrAuto.btn_LoadModel.IsEnabled == true)
            {

                // tham số hiển thị
                if (SelectedItems != null)
                {
                    ModelSelected = SelectedItems.Model.ToString();
                    XvalSelected = SelectedItems.Xval.ToString();
                    YvalSelected = SelectedItems.Yval.ToString();
                    PCBsumSelected = SelectedItems.PCBnum.ToString();
                }    

                //// tham số tọa độ chạy
                dataSource = DataProvider.Ins.DB.ModelLists.Where(u => u.Id == SelectedItems.Id).AsEnumerable();
                arrPos_X = ArrayPositionExecute(dataSource.LastOrDefault().C_pos_X); // 20,2
                arrPos_Y = ArrayPositionExecute(dataSource.LastOrDefault().R_pos_Y); // 7,2
                sumPos_X = intSumPosCalculate(dataSource.LastOrDefault().C_pos_X); // 20
                sumPos_Y = intSumPosCalculate(dataSource.LastOrDefault().R_pos_Y); // 7
                sumShape_X = dataSource.LastOrDefault().C_shape_num__X; //2
                sumShape_Y = dataSource.LastOrDefault().R_shape_num_Y; //4
                cuttingGroove = dataSource.LastOrDefault().Cutting_groove; //6978
                cuttingLength = dataSource.LastOrDefault().Cutting_length; //32889
                shapeDistance_X = dataSource.LastOrDefault().C_distance_X; //528868
                shapeDistance_Y = dataSource.LastOrDefault().R_distance_Y; //305624

                // mặc định cắt cả 4 PCB
                posSum_X = sumPos_X * sumShape_X * 2; // 80
                posSum_Y = sumPos_Y * sumShape_Y * 2; // 56

                //set dao
                plc.SetDevice("M105", 0);
                plc.SetDevice("M2222", 1);
                Thread.Sleep(50);
                plc.SetDevice("M2222", 0);
                flagCal = true;
                StartKnifeThread();
            }
        }
        private void TimerSetKnife_Tick(object sender, EventArgs e)
        {
            plc.GetDevice("M7", out int m7);
            if (m7 == 1 && ctrAuto.btn_LoadModel.IsEnabled == true)
            {
                flagCal = true;
                TimerSetKnife.IsEnabled = false;
                TimerSetKnife.Stop();
                LoadDataForAuto();
            }
        }
        private void TimerStartAuto_Tick1(object sender, EventArgs e)
        {
            plc.GetDevice("M0", out int m0);
            if ( m0 == 0)
            {
                InvokeUI(() =>
                {
                    System.Windows.Application.Current.Shutdown();
                });
            }
            plc.GetDevice("M1", out int m1);
            if (ctrAuto.btn_Run.IsEnabled == true && Z1_Auto_Val != 0 && Z2_Auto_Val != 0 && m1 == 1 && MainViewModel.ServoOnBit == true)
            {
                plc.SetDevice("M1", 0);
                m1 = 0;
                try
                {
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        autoFlag = true;
                    });
                    plc.SetDevice("M105", 1);
                    autoFlag = true;
                    StartAutoThread();
                    plc.SetDevice("M1", 0);
                    RunModeEnable();
                }
                catch
                {

                }
            }
            if ( m1 == 1 && MainViewModel.ServoOnBit == false)
            {
                MessageBox.Show("Chương trình chưa sẵn sàng chạy ( Servo Off )!");
            } 
            if( (Z1_Auto_Val == 0 || Z2_Auto_Val == 0 ) && m1 == 1 )
            {
                MessageBox.Show("Chương trình chưa sẵn sàng chạy ( Chưa set dao hoặc set dao lỗi )!");
            }

        }
        private void TimerStopAuto_Tick(object sender, EventArgs e)
        {
            plc.GetDevice("M0", out int m0);
            if (m0 == 0)
            {
                InvokeUI(() =>
                {
                    System.Windows.Application.Current.Shutdown();
                });
            }
            plc.GetDevice("M2", out int m2);
            if ( m2 == 1)
            {
                m2 = 0;
                plc.SetDevice("M2", 0);
                try
                {
                    StopModeEnable();
                    plc.SetDevice("M120", 0);
                    flagCal = false;
                    autoFlag = false;
                    plc.SetDevice("M2", 0);
                }
                catch
                {

                }
            }
        }
        #endregion

        #region SUPPORT METHODS
        public void CheckState(ControlAuto user)
        {
            try
            {
                int iretTest = plc.GetDevice("Y80", out int bitTest);
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
                            user.rad_btn_dual.IsEnabled = false;
                            user.rad_btn_single.IsEnabled = false;
                        }
                        user.btn_Home.IsEnabled = true;
                        user.btn_LoadModel.IsEnabled = false;
                        user.btn_Stop.IsEnabled = false;
                        user.btn_Reset.IsEnabled = true;
                        user.grid_dataBox.IsEnabled = true;
                        user.grid_tableDB.IsEnabled = true;
                    });
                }
                else if (autoFlag == true && iretTest == 0)
                {
                    user.Dispatcher.Invoke(() =>
                    {
                        user.rad_btn_dual.IsEnabled = false;
                        user.rad_btn_single.IsEnabled = false;
                        user.btn_Run.IsEnabled = false;
                        user.btn_Stop.IsEnabled = true;
                        user.btn_Reset.IsEnabled = true;
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
                        user.rad_btn_dual.IsEnabled = false;
                        user.rad_btn_single.IsEnabled = false;
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
        private int intSumPosCalculate(string StrPos)
        {
            if (StrPos[StrPos.Length - 1] != 44)
            {
                StrPos = StrPos + ",";
            }
            int len = StrPos.Length;
            int rs = 0;
            for (int i = 0; i < len; i++)
            {
                if (StrPos[i] == 44)
                {
                   rs++;
                }
                
            }
            return rs;
        }
        public int[] StringExecute(string StrCoor)
        {
            if (StrCoor[StrCoor.Length - 1] != 47)
            {
                StrCoor = StrCoor + "/";
            }
            int len = StrCoor.Length;
            int[] coor = new int[2];
            if (len > 0)
            {
                int count = 0;
                int arrLen = 0;
                for (int i = 0; i < len; i++)
                {
                    if (StrCoor[i] == 47)
                    {
                        int index1 = i - count;
                        bool canParse = int.TryParse(StrCoor.Substring(index1, count), out int rs);
                        if (canParse == true)
                        {
                            coor[arrLen] = rs;
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
            return coor;
        }
        public int[,] ArrayPositionExecute(string StrPos)
        {
            // text coordinate cắt ngang : 4185034/-1914115,4331691/-2040651,4568255/-1799060,4713902/-1914115,4860559/-2040651,5097123/-1799060,5242770/-1914115
            // text coordinate cắt dọc : 4486561/-2051348,4234720/-1987582,4430244/-1899653,4621751/-1810799,4488143/-1746814,4370443/-1746814,4234720/-1681958,4430244/-1594029,4621751/-1505175,4488143/-1441190,4370445/-1441190,4234720/-1376334,4430244/-1288405,4621751/-1199551,4488143/-1135566,4370443/-1135566,4234720/-1070710,4430244/-982781,4621751/-893927,4370443/-829942

            if (StrPos[StrPos.Length - 1] != 44)
            {
                StrPos = StrPos + ",";
            }
            int len = StrPos.Length;
            int sumPin = intSumPosCalculate(StrPos);
            int[,] arrInt = new int[sumPin, 2];
            if (len>0)
            {
                int count = 0;
                int arrLen = 0;
                for (int i = 0; i < len; i++)
                {
                    if (StrPos[i] == 44)
                    {
                        int index1 = i - count;
                        string strTemp = StrPos.Substring(index1, count);
                        if (strTemp != null)
                        {
                            //arrString[arrLen] = strTemp;
                            arrInt[arrLen,0] = StringExecute(strTemp)[0];
                            arrInt[arrLen,1] = StringExecute(strTemp)[1];
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
            return arrInt;
        }
        public void InvokeUI(Action a)
        {
            //System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(new System.Windows.Forms.MethodInvoker(a));
            System.Windows.Application.Current.Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(a));
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
        public void StartAutoThread()
        {
            if (ExecutionThread != null)
            {
                ExecutionThread.Abort();
            }
            ExecutionThread = new Thread(new ThreadStart(ExecutionMethod));//ExecutionMethod -- test chạy dọc trước
            ExecutionThread.Priority = ThreadPriority.Lowest;
            ExecutionThread.IsBackground = true;
            ExecutionThread.Start();
        }
        public void StartKnifeThread()
        {
            if (KnifeThread != null)
            {
                KnifeThread.Abort();
            }
            KnifeThread = new Thread(new ThreadStart(KnifeCalculate));
            KnifeThread.Priority = ThreadPriority.Lowest;
            KnifeThread.IsBackground = true;
            KnifeThread.Start();
        }
        public void KnifeCalculate()
        {
            short[] z1Val = new short[2];
            short[] z2Val = new short[2];
            try
            {
                ctrAuto.Dispatcher.Invoke(() =>
                {
                    TimerSetKnife.IsEnabled = false;
                    TimerSetKnife.Stop();
                    ctrAuto.rad_btn_dual.IsEnabled = false;
                    ctrAuto.rad_btn_single.IsEnabled = false;
                    ctrAuto.btn_Stop.IsEnabled = true;
                    ctrAuto.btn_Reset.IsEnabled = false;
                    ctrAuto.btn_Home.IsEnabled = false;
                    ctrAuto.btn_LoadModel.IsEnabled = false;
                    ctrAuto.grid_dataBox.IsEnabled = false;
                    ctrAuto.grid_tableDB.IsEnabled = false;
                });
                ctrAuto.Dispatcher.Invoke(() =>
                {
                    ctrAuto.btn_LoadModel.IsEnabled = false;
                    ctrAuto.btn_Run.IsEnabled = false;
                });
                Thread.Sleep(1000);
                while (flagCal == true)
                {
                    short[] Bits = new short[4];
                    plc.ReadDeviceRandom2("D0\nD30\nD60\nD90", 4, out Bits[0]);
                    if (Math.Abs(Bits[0]) < 100 && Math.Abs(Bits[1]) < 100 && Math.Abs(Bits[2]) < 100 && Math.Abs(Bits[3]) < 100)
                    {
                        plc.GetDevice("D1250", out int z11);
                        plc.GetDevice("D1251", out int z12);
                        plc.GetDevice("D1350", out int z21);
                        plc.GetDevice("D1351", out int z22);
                        Z1_Auto_Val = z11 + (z12*65536) - DistanceDefault_Z1;
                        Z2_Auto_Val = z21 + (z22*65536) - DistanceDefault_Z2;
                        ExecuteUpDown();
                        plc.SetDevice("M105", 1);
                        plc.SetDevice("M7", 0);
                        flagCal = false;
                    }    
                }    
            }
            catch (Exception e)
            {
                MessageBox.Show("Knife Calculate Error : " + e.ToString());
            }
            finally
            {

                plc.SetDevice("M105", 1);
                plc.SetDevice("M7", 0);
                ctrAuto.Dispatcher.Invoke(() =>
                {
                    double tempDouble1 = Math.Truncate(Convert.ToDouble(Z1_Auto_Val) / 10000 * 100) / 100;
                    double tempDouble2 = Math.Truncate(Convert.ToDouble(Z2_Auto_Val) / 10000 * 100) / 100;
                    CalculatedZ1_Knife = Convert.ToString(tempDouble1) + " mm";
                    CalculatedZ2_Knife = Convert.ToString(tempDouble2) + " mm";
                    if (Z1_Auto_Val != 0 && Z2_Auto_Val != 0)
                    {
                        ctrAuto.btn_Run.IsEnabled = true;
                        ctrAuto.rad_btn_dual.IsEnabled = true;
                        ctrAuto.rad_btn_single.IsEnabled = true;
                        MessageBox.Show("Z1val = " + Z1_Auto_Val.ToString() + "\n" + "Z2val = " + Z2_Auto_Val.ToString());
                    }    
                    else
                    {
                        ctrAuto.btn_Run.IsEnabled = false;
                        ctrAuto.rad_btn_dual.IsEnabled = false;
                        ctrAuto.rad_btn_single.IsEnabled = false;
                        MessageBox.Show("Set Dao Lỗi !! ");
                    }
                    TimerSetKnife.IsEnabled = true;
                    TimerSetKnife.Start();
                    ctrAuto.btn_Stop.IsEnabled = false;
                    ctrAuto.btn_Reset.IsEnabled = true;
                    ctrAuto.btn_Home.IsEnabled = true;
                    ctrAuto.btn_LoadModel.IsEnabled = true;
                    ctrAuto.grid_dataBox.IsEnabled = true;
                    ctrAuto.grid_tableDB.IsEnabled = true;
                });
                if (KnifeThread != null)
                {
                    KnifeThread.Abort();
                }
            }
              
        }
        #endregion

        #region AUTO EXECUTION

        public void ExecutionMethod()
        {
            try
            {
                ctrAuto.Dispatcher.Invoke(() =>
                {
                    ctrAuto.btn_Run.IsEnabled = false;
                    ctrAuto.btn_Stop.IsEnabled = true;
                    ctrAuto.btn_Reset.IsEnabled = true;
                    ctrAuto.btn_Home.IsEnabled = false;
                    ctrAuto.btn_LoadModel.IsEnabled = false;
                    ctrAuto.grid_dataBox.IsEnabled = false;
                    ctrAuto.grid_tableDB.IsEnabled = true;
                });
                ExecuteUpDown();
                while (autoFlag == true)
                {
                    //ExecuteUpDown();
                    int totalPos_X = sumPos_X * sumShape_X;
                    int totalPos_Y = sumPos_Y * sumShape_Y;
                    int flag1 = 0;
                    int flag2 = 0;
                    int flag3 = 0;
                    int i = 0;
                    int j = 0;
                    //int i = 28; // chỉ chạy cụm trong
                    //int j = 40;
                    bool flagWait = false;
                    // bắt đầu chu trình
                    while (flag1 == 0 && flag2 == 0 && flag3 == 0  && autoFlag == true)
                    {
                        plc.GetDevice("M1666", out int m1666);
                        if (m1666 == 1)
                        {
                            plc.SetDevice("M1777", 1);
                            flag1 = 1;
                        }
                    }
                    // chạy điểm cắt
                    while (flag1 == 1 && flag2 == 0 && flag3 == 0  && autoFlag == true) // === YX === tịnh tiến ngang
                    {
                                        // so sánh với x2 - d1800, y1 - d1110, z1 - d1410, z2 - d1510
                                        // (end)x2/y1 <===== x2/y2
                                        //                     ^
                                        //                    | |
                                        //                    | |
                                        //     *x1/y1 =====> x1/y2
                        if (posSum_Y > 0 && i <= posSum_Y)
                        {
                            plc.GetDevice("M1777", out int m1777);
                            if (m1777 == 1)
                            {
                                plc.SetDevice("M1777", 0);
                                m1777 = 0;
                                if (i < totalPos_Y) // cụm ngoài
                                {
                                    ExecutePos_Y(i,1);
                                    plc.SetDevice("M200", 1); // bit tịnh tiến ngang (theo Y)
                                    flagWait = true;

                                }
                                else if ( i >= totalPos_Y && i < posSum_Y) // cụm trong
                                {
                                    ExecutePos_Y(i, 2);
                                    plc.SetDevice("M200", 1); // bit tịnh tiến ngang (theo Y)
                                    flagWait = true;
                                }
                                if (i == posSum_Y)
                                {
                                    plc.SetDevice("M1444", 1);
                                    plc.SetDevice("M1999", 1);
                                    flag2 = 1;
                                    flagWait = false;
                                }
                                while (flagWait == true && flag1 == 1 && flag2 == 0 && flag3 == 0 && autoFlag == true)
                                {
                                    short[] Bits = new short[16];
                                    plc.ReadDeviceRandom2("D0\nD1\nD30\nD31\nD60\nD61\nD90\nD91\nD1800\nD1801\nD1110\nD1111\nD1410\nD1411\nD1510\nD1511", 16, out Bits[0]);
                                    // X2 : D1800 // Y1 : D1110 // Z1 chờ : D1410 // Z2 chờ : D1510
                                    if (Bits[0] == Bits[8]
                                        && Bits[1] == Bits[9]
                                        && Bits[2] == Bits[10]
                                        && Bits[3] == Bits[11]
                                        && Bits[4] == Bits[12]
                                        && Bits[5] == Bits[13]
                                        && Bits[6] == Bits[14]
                                        && Bits[7] == Bits[15])
                                    {
                                        plc.SetDevice("M200", 0);
                                        plc.SetDevice("M201", 0);
                                        plc.SetDevice("M202", 0);
                                        plc.SetDevice("M203", 0);
                                        plc.SetDevice("M204", 0);
                                        plc.SetDevice("M205", 0);
                                        i++;
                                        flagWait = false;
                                    }
                                }
                            }
                        }
                    }
                    while (flag1 == 1 && flag2 == 1 && flag3 == 0  && autoFlag == true) // === XY === tịnh tiến dọc
                    {
                        // so sánh với x1-d1000, y2-d1910, z1-d1410, z2-d1510
                        // (end)x1/y2      *x1/y1
                        //      ^           | |
                        //     | |          | |
                        //     | |           v
                        //    x2/y2 <===== x2/y1
                        if (posSum_X > 0 && j <= posSum_X)
                        {
                            plc.GetDevice("M1999", out int m1999);
                            if (m1999 == 1)
                            {
                                plc.SetDevice("M1999", 0);
                                m1999 = 0;
                                if (j < totalPos_X) // cụm ngoài
                                {
                                    
                                    ExecutePos_X(j,1);
                                    plc.SetDevice("M250", 1); // bit tịnh tiến dọc (theo X)
                                    flagWait = true;
                                }
                                else if ( j >= totalPos_X && j < posSum_X) // cụm trong
                                {
                                    
                                    ExecutePos_X(j, 2);
                                    plc.SetDevice("M250", 1); // bit tịnh tiến dọc (theo X)
                                    flagWait = true;
                                }
                                if (j == sumPos_X)
                                {
                                    plc.SetDevice("M1444", 1);
                                    plc.SetDevice("M1555", 1);
                                    flag3 = 1;
                                    flagWait = false;
                                }
                                while (flagWait == true && flag1 == 1 && flag2 == 1 && flag3 == 0 && autoFlag == true)
                                {
                                    short[] Bits = new short[16];
                                    plc.ReadDeviceRandom2("D0\nD1\nD30\nD31\nD60\nD61\nD90\nD91\nD2810\nD2811\nD2910\nD2911\nD2410\nD2411\nD2510\nD2511", 16, out Bits[0]);
                                    // X1 : D2810 // Y2 : D2910 // Z1 chờ : D2410 // Z2 chờ : D2510
                                    if (Bits[0] == Bits[8]
                                        && Bits[1] == Bits[9]
                                        && Bits[2] == Bits[10]
                                        && Bits[3] == Bits[11]
                                        && Bits[4] == Bits[12]
                                        && Bits[5] == Bits[13]
                                        && Bits[6] == Bits[14]
                                        && Bits[7] == Bits[15])
                                    {
                                        plc.SetDevice("M250",0);
                                        plc.SetDevice("M251", 0);
                                        plc.SetDevice("M252", 0);
                                        plc.SetDevice("M253", 0);
                                        plc.SetDevice("M254", 0);
                                        plc.SetDevice("M255", 0);
                                        j++;
                                        flagWait = false;
                                    }
                                }
                            }
                        }
                    }
                    // kết thúc chu trình
                    while (flag1 == 1 && flag2 == 1 && flag3 == 1 && autoFlag == true)
                    {
                        short[] Bits = new short[4];
                        plc.ReadDeviceRandom2("D0\nD1\nD30\nD31", 4, out Bits[0]);
                        if (Math.Abs(Bits[0]) <100  && Math.Abs(Bits[1]) < 100 && Math.Abs(Bits[2]) < 100 && Math.Abs(Bits[3]) < 100)
                        {
                            flagWait = false;
                            autoFlag = false;
                            i = 0;
                            j = 0;
                            flag1 = 0;
                            flag2 = 0;
                            flag3 = 0;

                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error to start Auto_Execution thread: " + e.ToString());
            }
            finally
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
                if (ExecutionThread != null)
                {
                    ExecutionThread.Abort();
                }
            }
        }
        public void ExecuteUpDown()
        {
            plc.GetDevice("D1250", out int z11);
            plc.GetDevice("D1251", out int z12);
            plc.GetDevice("D1350", out int z21);
            plc.GetDevice("D1351", out int z22);
            Z1_Auto_Val = z11 + (z12 * 65536) - DistanceDefault_Z1;
            Z2_Auto_Val = z21 + (z22 * 65536) - DistanceDefault_Z2;
            if (Z1_Auto_Val != 0 && Z2_Auto_Val != 0)
            {
                int[] z1Val_new = new int[4] { (-Z1_Auto_Val % 65536), (-Z1_Auto_Val / 65536), (-Z1_Auto_Val - 130000) % 65536, (-Z1_Auto_Val - 130000) / 65536 };
                int[] z2Val_new = new int[4] { (-Z2_Auto_Val % 65536), (-Z2_Auto_Val / 65536), (-Z2_Auto_Val - 130000) % 65536, (-Z2_Auto_Val - 130000) / 65536 };

                plc.SetDevice("D1400", z1Val_new[2]); // Z1 chờ
                plc.SetDevice("D1401", z1Val_new[3]); 

                plc.SetDevice("D1500", z2Val_new[2]); // Z2 chờ
                plc.SetDevice("D1501", z2Val_new[3]); 

                plc.SetDevice("D1200", z1Val_new[0]); // Z1 cắt ( đã cộng với  - DistanceDefault_Z )
                plc.SetDevice("D1201", z1Val_new[1]); 

                plc.SetDevice("D1300", z2Val_new[0]); // Z2 cắt ( đã cộng với  - DistanceDefault_Z )
                plc.SetDevice("D1301", z2Val_new[1]); 

            }
            else
            {
                MessageBox.Show("Chưa cài đặt gốc dao!");
                autoFlag = false;
            }
        }
        public void ExecutePos_X(int i, int RowPCBnum)// (20,2) - tịnh tiến dọc
        {
            try
            {
                int totalPos_X = sumPos_X * sumShape_X;
                            // so sánh với x1-d1000, y2-d1910, z1-d1410, z2-d1510
                            // (end)x1/y2      *x1/y1
                            //      ^           | |
                            //     | |          | |
                            //     | |           v
                            //    x2/y2 <===== x2/y1

                int Xval = 0;
                int Yval = 0;
                if(RowPCBnum == 1)
                {
                    Xval = arrPos_X[(i % totalPos_X) % sumPos_X, 0] + ((i % totalPos_X) / sumPos_X) * shapeDistance_X ;
                    Yval = arrPos_X[(i % totalPos_X) % sumPos_X, 1];
                }    
                else if (RowPCBnum  == 2)
                {
                    Xval = arrPos_X[(i % totalPos_X) % sumPos_X, 0] + ((i % totalPos_X) / sumPos_X) * shapeDistance_X - PiecePCB_distance_X; // PiecePCB_distance_X = 2197000
                    Yval = arrPos_X[(i % totalPos_X) % sumPos_X, 1] - PiecePCB_distance_Y;
                }    
                plc.SetDevice("D1000", Xval % 65536); // X1
                plc.SetDevice("D1001", Xval / 65536);

                plc.SetDevice("D1700", (Xval + cuttingLength) % 65536);// X2
                plc.SetDevice("D1701", (Xval + cuttingLength) / 65536);

                plc.SetDevice("D1100", -Yval % 65536); // Y1 
                plc.SetDevice("D1101", -Yval / 65536);

                plc.SetDevice("D1800", Xval % 65536); // x3
                plc.SetDevice("D1801", Xval / 65536);

                plc.SetDevice("D1900", (-Yval + cuttingGroove) % 65536); // Y2
                plc.SetDevice("D1901", (-Yval + cuttingGroove)/ 65536);

            }
            catch (Exception)
            {
                throw;
            }
        }
        public void ExecutePos_Y(int i, int RowPCBnum) // (7,2) - tịnh tiến ngang
        {
            try
            {
                int totalPos_Y = sumPos_Y * sumShape_Y;
                            // so sánh với x2 - d1800, y1 - d1110, z1 - d1410, z2 - d1510
                            // (end)x2/y1 <===== x2/y2
                            //                     ^
                            //                    | |
                            //                    | |
                            //     *x1/y1 =====> x1/y2

                int Xval = 0;
                int Yval = 0;
                if (RowPCBnum == 1)
                {

                    Xval = arrPos_Y[(i % totalPos_Y) % sumPos_Y, 0];
                    Yval = arrPos_Y[(i % totalPos_Y) % sumPos_Y, 1] + (i / sumPos_Y) * shapeDistance_Y;
                }
                else if (RowPCBnum == 2)
                {
                    Xval = arrPos_Y[(i % totalPos_Y) % sumPos_Y, 0] - PiecePCB_distance_X;
                    Yval = arrPos_Y[(i % totalPos_Y) % sumPos_Y, 1] + ((i - totalPos_Y) / sumPos_Y) * shapeDistance_Y - PiecePCB_distance_Y;
                    
                }    
                
                plc.SetDevice("D1000", Xval % 65536); // X1 
                plc.SetDevice("D1001", Xval / 65536);

                plc.SetDevice("D1800", (Xval - cuttingGroove) % 65536); // X2
                plc.SetDevice("D1801", (Xval - cuttingGroove) / 65536);

                plc.SetDevice("D1100", -Yval % 65536); // Y1 
                plc.SetDevice("D1101", -Yval / 65536);

                plc.SetDevice("D1900", -Yval % 65536); // Y1 
                plc.SetDevice("D1901", -Yval / 65536);

                plc.SetDevice("D1600", (-Yval - cuttingLength) % 65536);// Y2
                plc.SetDevice("D1601", (-Yval - cuttingLength) / 65536);
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        #endregion
    }
}
