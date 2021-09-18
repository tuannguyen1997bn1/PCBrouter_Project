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

namespace PCBrouter_prj.ViewModel
{

    public class ControlAutoViewModel : BaseViewModel
    {
        #region DEFINATION

        IEnumerable<ModelList> dataSource;
        public static bool flagCal = true;
        public static bool autoFlag = false;
        public static int sumPos_X;
        public static int sumPos_Y;
        public static int sumShape_X;
        public static int sumShape_Y;
        public static int cuttingLength;
        public static int cuttingGroove;
        public static int shapeDistance_X;
        public static int shapeDistance_Y;
        public static int Z1_Auto_Val = 0; // khoảng lệch giữa Z1 cắt và điểm đo dao
        public static int Z2_Auto_Val = 0;
        public static int DistanceDefault_Z = 404648;
        public static int[,] arrPos_X;
        public static int[,] arrPos_Y;
        

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
                    //ModelSelected = SelectedItems.Model.ToString();
                    //XvalSelected = SelectedItems.Xval.ToString();
                    //YvalSelected = SelectedItems.Yval.ToString();
                    //PCBsumSelected = SelectedItems.PCBnum.ToString();
                    ctrAuto.Dispatcher.Invoke(() => { 
                        ctrAuto.btn_LoadModel.IsEnabled = true;
                        ctrAuto.btn_Run.IsEnabled = true; // test
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
            LoadedAutoUCCommand = new RelayCommand<UserControlKteam.ControlAuto>((p) => { return true; }, (p) =>
            {
                plc = MainViewModel.plc;
                ctrAuto = p;
                plc.SetDevice("M103", 1);
                Thread.Sleep(50);
                plc.SetDevice("M103", 0);

                CheckState(ctrAuto);
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
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    autoFlag = true;
                });
                plc.SetDevice("M105", 1);
                //StartAutoThread();
                StartTestThread();
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
                StopModeEnable();
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
                plc.SetDevice("M2", 0);
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
                ModelSelected = SelectedItems.Model.ToString();
                XvalSelected = SelectedItems.Xval.ToString();
                YvalSelected = SelectedItems.Yval.ToString();
                PCBsumSelected = SelectedItems.PCBnum.ToString();

                //// tham số tọa độ chạy
                dataSource= DataProvider.Ins.DB.ModelLists.Where(u => u.Id == SelectedItems.Id).AsEnumerable();
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


                // set dao
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
            plc.GetDevice("M1", out int m1);
            if (ctrAuto.btn_Run.IsEnabled == false && m1 == 1)
            {
                //MessageBox.Show("Sai trình tự vận hành ( chạy tự động )!");
                //plc.SetDevice("M1", 0);
                //plc.SetDevice("M103", 1);
                //Thread.Sleep(100);
                //plc.SetDevice("M103", 0);
            }
            if (ctrAuto.btn_Run.IsEnabled == true && m1 == 1)
            {
                plc.SetDevice("M1", 0);
                m1 = 0;
                try
                {
                    RunModeEnable();
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        autoFlag = true;
                    });
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
                    plc.SetDevice("M105", 1);
                    
                    autoFlag = true;
                    StartTestThread();
                    //StartAutoThread();
                    plc.SetDevice("M1", 0);
                }
                catch
                {

                }
            }
            
        }
        private void TimerStopAuto_Tick(object sender, EventArgs e)
        {
            plc.GetDevice("M2", out int m2);
            if (ctrAuto.btn_Stop.IsEnabled == true && m2 == 1)
            {
                m2 = 0;
                plc.SetDevice("M2", 0);
                try
                {
                    StopModeEnable();
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

                    autoFlag = false;
                    plc.SetDevice("M2", 0);
                }
                catch
                {

                }
            }
            else if (ctrAuto.btn_Stop.IsEnabled == false && m2 == 1)
            {
                //MessageBox.Show("Sai trình tự vận hành ( dừng tự động )!");
                //plc.SetDevice("M2", 0);
                //plc.SetDevice("M103", 1);
                //Thread.Sleep(100);
                //plc.SetDevice("M103", 0);
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
        //private int[,] ExecutePosValue(string StrPos, string StringDistance)
        //{
        //    // text coordinate cắt ngang : 4185034/-1914115,4331691/-2040651,4568255/-1799060,4713902/-1914115,4860559/-2040651,5097123/-1799060,5242770/-1914115
        //    // text coordinate cắt dọc : 4486561/-2051348,4234720/-1987582,4430244/-1899653,4621751/-1810799,4488143/-1746814,4370443/-1746814,4234720/-1681958,4430244/-1594029,4621751/-1505175,4488143/-1441190,4370445/-1441190,4234720/-1376334,4430244/-1288405,4621751/-1199551,4488143/-1135566,4370443/-1135566,4234720/-1070710,4430244/-982781,4621751/-893927,4370443/-829942
        //    string[] arrPosPin = ExecuteStringModel(StrPos); // toa do 1 (string)
        //    string[] arrPosPinRowCol = ExecuteStringModel(StringDistance); // toa do 2 (string)
        //    int sumPinVal = SumPosCalculate(StrPos)[0];
        //    int sumPinsStr = SumPosCalculate(StrPos)[1];
        //    int[,] intPosPin = new int[sumPinVal, 2];
        //    int j = 0;
        //    for (int i = 0; i < sumPinsStr; i++)
        //    {
        //        int rs;
        //        bool canParse = int.TryParse(arrPosPin[i], out rs);
        //        if (canParse == true)
        //        {
        //            intPosPin[j, 0] = rs;
        //            intPosPin[j, 1] = int.Parse(arrPosPinRowCol[i]);
        //        }
        //        else if (arrPosPin[i] != null)
        //        {
        //            string[] temp = ExecuteStringModel2(arrPosPin[i]);
        //            for (int u = 0; u < temp.Length; u++)
        //            {
        //                if (temp[u] != null)
        //                {
        //                    intPosPin[j, 0] = int.Parse(temp[u]);
        //                    intPosPin[j, 1] = int.Parse(arrPosPinRowCol[i]);
        //                    if (u < (temp.Length - 1))
        //                    {
        //                        j++;
        //                    }
        //                }
        //            }
        //        }
        //        j++;
        //    }
        //    return intPosPin;
        //}
        //private string[] ExecuteStringModel2(string arrPosPin)
        //{
        //    if (arrPosPin[arrPosPin.Length - 1] != 47)
        //    {
        //        arrPosPin = arrPosPin + "/";
        //    }
        //    int len = arrPosPin.Length;
        //    int sumPin = 0;
        //    for (int i = 0; i < len; i++)
        //    {
        //        if (arrPosPin[i] == 47)
        //        {
        //            sumPin++;
        //        }
        //    }
        //    string[] rs = new string[sumPin];
        //    if (len > 0)
        //    {
        //        int count = 0;
        //        int arrLen = 0;
        //        for (int i = 0; i < len; i++)
        //        {
        //            if (arrPosPin[i] == 47)
        //            {
        //                int index1 = i - count;
        //                string strTemp = arrPosPin.Substring(index1, count);
        //                if (strTemp != null)
        //                {
        //                    rs[arrLen] = strTemp;
        //                    count = 0;
        //                    arrLen++;
        //                }
        //            }
        //            else
        //            {
        //                count++;
        //            }
        //        }
        //    }
        //    return rs;
        //}
        //private string[] ExecuteStringModel1(string StrPos)
        //{
        //    if (StrPos[StrPos.Length - 1] != 44)
        //    {
        //        StrPos = StrPos + ",";
        //    }
        //    int len = StrPos.Length;
        //    int sumPinsStr = SumPosCalculate(StrPos)[1];
        //    string[] arrPosPin = new string[sumPinsStr];
        //    if (len > 0)
        //    {
        //        int count = 0;
        //        int arrLen = 0;
        //        for (int i = 0; i < len; i++)
        //        {
        //            if (StrPos[i] == 44)
        //            {
        //                int index1 = i - count;
        //                string  strTemp = StrPos.Substring(index1, count);
        //                if (strTemp != null)
        //                {
        //                    arrPosPin[arrLen] = strTemp;
        //                    count = 0;
        //                    arrLen++;
        //                }
        //            }
        //            else
        //            {
        //                count++;
        //            }
        //        }
        //    }
        //    return arrPosPin;
        //}
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
            ExecutionThread = new Thread(new ThreadStart(ExecutionMethod));
            ExecutionThread.Priority = ThreadPriority.Lowest;
            ExecutionThread.IsBackground = true;
            ExecutionThread.Start();
        }
        public void StartTestThread()
        {
            if (ExecutionThread != null)
            {
                ExecutionThread.Abort();
            }
            ExecutionThread = new Thread(new ThreadStart(ExecutionTest));
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
            int flagStep1 = 0;
            int flagStep2 = 0;
            int flagStep3 = 0;
            short[] z1Val = new short[2];
            short[] z2Val = new short[2];
            try
            {
                ctrAuto.Dispatcher.Invoke(() =>
                {
                    ctrAuto.btn_LoadModel.IsEnabled = false;
                    //ctrAuto.btn_Run.IsEnabled = false; 
                });
                while ((flagStep1 == 0 || flagStep2 == 0)&& flagStep3 == 0 && flagCal == true)
                {
                    short[] bits = new short[2] { 1, 1};
                    plc.ReadDeviceRandom2("M9\nM10", 2, out bits[0]);
                    if (bits[0] == 0 && flagStep1 == 0)
                    {
                        plc.ReadDeviceRandom2("D60\nD61", 2, out z1Val[0]);
                        flagStep1 = 1;
                    }
                    if (bits[1] == 0 && flagStep2 == 0)
                    {
                        plc.ReadDeviceRandom2("D90\nD91", 2, out z2Val[0]);
                        flagStep2 = 1;
                    }
                }
                while (flagStep1 == 1 && flagStep2 == 1 && flagStep3 == 0 &&flagCal == true)
                {
                    Z1_Auto_Val = Convert.ToInt32((z1Val[1] << 16) + z1Val[0]) + DistanceDefault_Z;
                    Z2_Auto_Val = Convert.ToInt32((z2Val[1] << 16) + z2Val[0]) + DistanceDefault_Z;
                    flagStep3 = 1;
                }
                while (flagStep1 == 1 && flagStep2 == 1 && flagStep3 == 1 && flagCal == true)
                {
                    short[] currentVal = new short[4];
                    plc.ReadDeviceRandom2("D0\nD30\nD60\nD90", 4, out currentVal[0]);
                    if (currentVal[0] == 0 && currentVal[1] == 0 && currentVal[2] == 0 && currentVal[3] == 0)
                    {
                        plc.SetDevice("M105", 1);
                        plc.SetDevice("M7", 0);
                        flagStep1 = 0;
                        flagStep2 = 0;
                        flagStep3 = 0;
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
                    //int[] z1Val_new = new int[2] { -(Z1_Auto_Val % 65536), -(Z1_Auto_Val / 65536) };
                    //int[] z2Val_new = new int[2] { -(Z2_Auto_Val % 65536), -(Z2_Auto_Val / 65536) };
                    //plc.SetDevice("D1200", z1Val_new[0]);
                    //plc.SetDevice("D1201", z1Val_new[1]);
                    //plc.SetDevice("D1300", z2Val_new[0]);
                    //plc.SetDevice("D1301", z2Val_new[1]);
                    if (Z1_Auto_Val != 0 && Z2_Auto_Val != 0)
                    {
                        ctrAuto.btn_Run.IsEnabled = true;
                        MessageBox.Show("Z1val = " + Z1_Auto_Val.ToString() + "\n" + "Z2val = " + Z2_Auto_Val.ToString());
                    }    
                    else
                    {
                        //ctrAuto.btn_Run.IsEnabled = false;
                        MessageBox.Show("Set Dao Lỗi !! ");
                    }
                    TimerSetKnife.IsEnabled = true;
                    TimerSetKnife.Start();
                    ctrAuto.btn_LoadModel.IsEnabled = true;
                });
                if (KnifeThread != null)
                {
                    KnifeThread.Abort();
                }
            }
              
        }
        #endregion

        #region AUTO EXECUTION

        public void ExecutionTest()
        {
            // d1000 : X
            // D1100 : Y

            // D1200 : Z1 cắt
            // D1300 : Z2 cắt 

            // D1400 : Z1 chờ
            // D1500 : Z2 chờ

            // D1600 : Y tịnh tiến
            // D1700 : X tịnh tiến
            try
            {
                //int[] z1Val_new = new int[4] { (-Z1_Auto_Val - 70000 % 65536), (-Z1_Auto_Val - 800000 / 65536), (-Z1_Auto_Val - 800000 - 500000) % 65536, (-Z1_Auto_Val - 800000 - 500000) / 65536 };
                //int[] z2Val_new = new int[4] { (-Z2_Auto_Val % 65536), (-Z2_Auto_Val / 65536), (-Z2_Auto_Val - 500000) % 65536, (-Z2_Auto_Val - 500000) / 65536 };
                int[,] arr = new int[4, 2] { { 4185034, 1906844 }, { 4185034, 1599318 }, { 4185034, 1295099 }, { 4185034, 989970 } }; // đã nhân -1 với y
                int total = arr.Length / 2;
                while (autoFlag == true)
                {
                    int flag1 = 0;
                    int flag2 = 0;
                    bool flagWait = false;
                    int i = 0;
                    
                    while (flag1 == 0 && flag2 == 0 && autoFlag == true)
                    {
                        plc.GetDevice("M1666", out int m1666);
                        if (m1666 == 1)
                        {
                            plc.SetDevice("M1777", 1);
                            flag1 = 1;
                        }
                    }
                    while (flag1 == 1 && flag2 == 0 && autoFlag == true)
                    {

                        if (i <= 4)
                        {
                            plc.GetDevice("M1777", out int m1777);
                            if (m1777 == 1)
                            {
                                m1777 = 0;
                                plc.SetDevice("M1777", 0);
                                if (i == 4)
                                {
                                    
                                    flag2 = 1;
                                    flagWait = false;
                                }
                                if (i < 4)
                                {

                                    plc.SetDevice("D1000", arr[i, 0] % 65536);//X
                                    plc.SetDevice("D1001", arr[i, 0] / 65536);

                                    plc.SetDevice("D1100", arr[i, 1] % 65536);//Y1
                                    plc.SetDevice("D1101", arr[i, 1] / 65536);

                                    plc.SetDevice("D1600", (arr[i, 1] - 35880) % 65536);// Y2 tịnh tiến
                                    plc.SetDevice("D1601", (arr[i, 1] - 35880) / 65536);

                                    plc.SetDevice("D1400", 500000 % 65536); // Z1 chờ
                                    plc.SetDevice("D1401", 500000 / 65536);

                                    //plc.SetDevice("D1300", z2Val_new[2]); // Z2 chờ
                                    //plc.SetDevice("D1301", z2Val_new[3]);

                                    plc.SetDevice("D1200",( 500000 + 200000 )% 65536); // Z1 cắt
                                    plc.SetDevice("D1201", (500000 + 200000 )/ 65536);

                                    //plc.SetDevice("D1300", z2Val_new[0]); // Z2 cắt
                                    //plc.SetDevice("D1301", z2Val_new[1]);

                                    Thread.Sleep(300);
                                    flagWait = true;
                                    plc.SetDevice("M200", 1);

                                }
                                while (flagWait == true && flag1 == 1 && flag2 == 0 && autoFlag == true)
                                {
                                    short[] Bits = new short[16];
                                    plc.ReadDeviceRandom2("D0\nD1\nD30\nD31\nD60\nD61\nD90\nD91\nD1000\nD1001\nD1610\nD1611\nD1410\nD1411\nD1510\nD1511", 16, out Bits[0]);
                                    if (Bits[0] == Bits[8]
                                        && Bits[1] == Bits[9]
                                        && Bits[2] == Bits[10]
                                        && Bits[3] == Bits[11]
                                        && Bits[4] == Bits[12]
                                        && Bits[5] == Bits[13])
                                    //&& Bits[6] == Bits[14]
                                    //&& Bits[7] == Bits[15])
                                    {
                                        i++;
                                        flagWait = false;
                                    }
                                }
                            }
                        }

                    }
                    while (flag1 == 1 && flag2 == 1 && autoFlag == true)
                    {
                        //short[] Bits = new short[4];
                        //plc.ReadDeviceRandom2("D0\nD1\nD30\nD31", 4, out Bits[0]);
                        //if (Bits[0] == 0 && Bits[1] == 0 && Bits[2] == 0 && Bits[3] == 0)
                        //{

                        //plc.SetDevice("M1555", 1);
                        plc.SetDevice("M1444", 1);
                        Thread.Sleep(50);
                        plc.SetDevice("M1444", 0);
                        plc.SetDevice("M1555", 1);
                        autoFlag = false;
                        i = 0;
                        flag1 = 0;
                        flag2 = 0;
                        flagWait = false;
                        //}
                    }
                } 
            }
            catch (Exception e)
            {
                MessageBox.Show("Error to start Test_Execution thread: " + e.ToString());
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
       //D1000: X
       //D1100 : Y

       //D1200 : Z1 cắt
       //D1300: Z2 cắt

       //D1400: Z1 chờ
       //D1500: Z2 chờ

       //D1600: Y tịnh tiến
       //D1700 : X tịnh tiến
            try
            {
                int totalPos_X = sumPos_X * sumShape_X;
                int totalPos_Y = sumPos_Y * sumShape_Y;

                while (autoFlag == true)
                {
                    ExecuteUpDown();
                    int flag1 = 0;
                    int flag2 = 0;
                    int flag3 = 0;
                    int i = 0;
                    int j = 0;
                    bool flagWait = false;
                    while (flag1 == 0 && flag2 == 0 && flag3 == 0 && autoFlag == true)
                    {
                        plc.GetDevice("M1666", out int m1666);
                        if (m1666 == 1)
                        {
                            plc.SetDevice("M1777", 1);
                            flag1 = 1;
                        }
                    }
                    while (flag1 == 1 && flag2 == 0 && flag3 == 0 && autoFlag == true) // === YX === tịnh tiến ngang
                    {
                        if (sumPos_Y > 0 && i <= sumPos_Y)
                        {
                            plc.GetDevice("M1777", out int m1777);
                            if (m1777 == 1)
                            {
                                plc.SetDevice("M1777", 0);
                                if (i == totalPos_Y)
                                {
                                    plc.SetDevice("M1444", 1);
                                    Thread.Sleep(50);
                                    plc.SetDevice("M1444", 0);
                                    plc.SetDevice("M1888", 1);
                                    flag2 = 1;
                                    flagWait = false;
                                }
                                if (i < totalPos_Y)
                                {
                                    ExecutePos_Y(i);// X + Y1 + Y2
                                    Thread.Sleep(300);
                                    plc.SetDevice("M200", 1); // bit tịnh tiến ngang (theo Y)
                                    flagWait = true;

                                }
                                while (flagWait == true && flag1 == 1 && flag2 == 0 && flag3 == 0 && autoFlag == true)
                                {
                                    short[] Bits = new short[16];
                                    plc.ReadDeviceRandom2("D0\nD1\nD30\nD31\nD60\nD61\nD90\nD91\nD1000\nD1001\nD1610\nD1611\nD1410\nD1411\nD1510\nD1511", 16, out Bits[0]);
                                    if (Bits[0] == Bits[8]
                                        && Bits[1] == Bits[9]
                                        && Bits[2] == Bits[10]
                                        && Bits[3] == Bits[11]
                                        && Bits[4] == Bits[12]
                                        && Bits[5] == Bits[13]
                                        && Bits[6] == Bits[14]
                                        && Bits[7] == Bits[15])
                                    {
                                        i++;
                                        flagWait = false;
                                    }
                                }
                            }
                        }
                    }
                    while (flag1 == 1 && flag2 == 1 && flag3 == 0 && autoFlag == true) // === XY === tịnh tiến dọc
                    {
                        if (sumPos_X > 0 && j <= sumPos_X)
                        {
                            plc.GetDevice("M1888", out int m1888);
                            if (m1888 == 1)
                            {
                                plc.SetDevice("M1888", 0);
                                m1888 = 0;
                                if (j == totalPos_X)
                                {
                                    plc.SetDevice("M1555", 1);
                                    Thread.Sleep(50);
                                    plc.SetDevice("M1555", 0);
                                    flag3 = 1;
                                    flagWait = false;
                                }
                                if (j < totalPos_X)
                                {
                                    ExecutePos_X(j); // X1 +X2 + Y
                                    Thread.Sleep(300);
                                    plc.SetDevice("M250", 1); // bit tịnh tiến dọc (theo X)
                                    flagWait = true;
                                }
                                while (flagWait == true && flag1 == 1 && flag2 == 1 && flag3 == 0 && autoFlag == true)
                                {
                                    short[] Bits = new short[16];
                                    plc.ReadDeviceRandom2("D0\nD1\nD30\nD31\nD60\nD61\nD90\nD91\nD1700\nD1701\nD1110\nD1111\nD1410\nD1411\nD1510\nD1511", 16, out Bits[0]);
                                    if (Bits[0] == Bits[8]
                                        && Bits[1] == Bits[9]
                                        && Bits[2] == Bits[10]
                                        && Bits[3] == Bits[11]
                                        && Bits[4] == Bits[12]
                                        && Bits[5] == Bits[13]
                                        && Bits[6] == Bits[14]
                                        && Bits[7] == Bits[15])
                                    {
                                        j++;
                                        flagWait = false;
                                    }
                                }
                            }
                        }
                    }
                    while (flag1 == 1 && flag2 == 1 && flag3 == 1 && autoFlag == true)
                    {
                        short[] Bits = new short[4];
                        plc.ReadDeviceRandom2("D0\nD1\nD30\nD31", 4, out Bits[0]);
                        if (Bits[0] == 0 && Bits[1] == 0 && Bits[2] == 0 && Bits[3] == 0)
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
                if (ExecutionThread != null)
                {
                    ExecutionThread.Abort();
                }
            }
        }
        public void ExecuteUpDown()
        {
            if (Z1_Auto_Val != 0 && Z2_Auto_Val != 0)
            {
                int[] z1Val_new = new int[4] { (-Z1_Auto_Val % 65536), (-Z1_Auto_Val / 65536), (-Z1_Auto_Val - DistanceDefault_Z) % 65536, (-Z1_Auto_Val - DistanceDefault_Z) / 65536 };
                int[] z2Val_new = new int[4] { (-Z2_Auto_Val % 65536), (-Z2_Auto_Val / 65536), (-Z2_Auto_Val - DistanceDefault_Z) % 65536, (-Z2_Auto_Val - DistanceDefault_Z) / 65536 };
                if (z1Val_new[2] > 0 && z1Val_new[3] > 0)
                {
                    plc.SetDevice("D1400", z1Val_new[2]); // Z1 chờ
                    plc.SetDevice("D1401", z1Val_new[3]);

                    plc.SetDevice("D1500", z2Val_new[2]); // Z2 chờ
                    plc.SetDevice("D1501", z2Val_new[3]);

                    plc.SetDevice("D1200", z1Val_new[0]); // Z1 cắt
                    plc.SetDevice("D1201", z1Val_new[1]);

                    plc.SetDevice("D1300", z2Val_new[0]); // Z2 cắt
                    plc.SetDevice("D1301", z2Val_new[1]);
                }    
                else
                {
                    MessageBox.Show("Cài đặt gốc dao lỗi!");
                    autoFlag = false;
                }    
            }    
            else
            {
                MessageBox.Show("Chưa cài đặt gốc dao!");
                autoFlag = false;
            }    
        }
        public void ExecutePos_X(int i)// (20,2) - tịnh tiến dọc
        {
            try
            {
                int Xval = arrPos_X[i % sumPos_X, 1] + (i / sumPos_X) * shapeDistance_X;
                int Yval = arrPos_X[i % sumPos_X, 0];
                
                plc.SetDevice("D800", Xval % 65536); // X1
                plc.SetDevice("D801", Xval / 65536);

                plc.SetDevice("D1700", (Xval + cuttingLength) % 65536);// X2
                plc.SetDevice("D1701", (Xval + cuttingLength) / 65536);

                plc.SetDevice("D1000", Yval % 65536); // Y 
                plc.SetDevice("D1001", Yval / 65536);

            }
            catch (Exception)
            {
                throw;
            }
        }
        public void ExecutePos_Y(int i) // (7,2) - tịnh tiến ngang
        {
            try
            {
                int Xval = arrPos_Y[i % sumPos_Y, 0];
                int Yval = arrPos_Y[i % sumPos_Y, 1] + (i / sumPos_Y) * shapeDistance_Y;
                

                plc.SetDevice("D800", Xval % 65536); // X 
                plc.SetDevice("D801", Xval / 65536);

                plc.SetDevice("D1000", -Yval % 65536); // Y1 
                plc.SetDevice("D1001", -Yval / 65536);

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
