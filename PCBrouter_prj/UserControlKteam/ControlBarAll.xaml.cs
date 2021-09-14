﻿using PCBrouter_prj.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PCBrouter_prj.UserControlKteam
{
    /// <summary>
    /// Interaction logic for ControlBarUC.xaml
    /// </summary>
    public partial class ControlBarAll : UserControl
    {
        public ControlBarLogin Viewmodel { get; set; }

        public ControlBarAll()
        {
            InitializeComponent();
            this.DataContext = Viewmodel = new ControlBarLogin();
        }
    }
}
