﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PCBrouter_prj.ViewModel
{
    public class ControlBarViewModel : BaseViewModel
    {
        MainViewModel mvm = new MainViewModel();
        #region commands
        public ICommand CloseWindowCommand { get; set; }
        public ICommand MaximizeWindowCommand { get; set; }
        public ICommand MinimizeWindowCommand { get; set; }
        public ICommand MouseMoveWindowCommand { get; set; }
        #endregion

        public  ControlBarViewModel()
        {
            CloseWindowCommand = new RelayCommand<UserControl>((p) => { return p == null ? false : true; }, (p) => {
                FrameworkElement window = GetWindowParent(p);
                var w = window as MainWindow;// WinDow cungx dudowjc
                if (w != null)
                {
                    MessageBoxResult answer1;
                    answer1 = MessageBox.Show("Bạn có muốn thoát khỏi chương trình không ?", "Notification", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                    if (answer1 == MessageBoxResult.OK)
                    {
                        Application.Current.Shutdown();
                        //w.Close(); // nếu có nhiều window thì mới dùng close;
                    }
                }
            });
            MaximizeWindowCommand = new RelayCommand<UserControl>((p) => { return p == null ? false : true; }, (p) =>
            {
                FrameworkElement window = GetWindowParent(p);
                var w = window as Window;
                if (w != null)
                {
                    if (w.WindowState != WindowState.Maximized)
                        w.WindowState = WindowState.Maximized;
                    else
                        w.WindowState = WindowState.Normal;
                }
            });
            MinimizeWindowCommand = new RelayCommand<UserControl>((p) => { return p == null ? false : true; }, (p) =>
            {
                FrameworkElement window = GetWindowParent(p);
                var w = window as Window;
                if (w != null)
                {
                    if (w.WindowState != WindowState.Minimized)
                        w.WindowState = WindowState.Minimized;
                    else
                        w.WindowState = WindowState.Maximized;
                }
            });
            MouseMoveWindowCommand = new RelayCommand<UserControl>((p) => { return p == null ? false : true; }, (p) =>
            {                
                FrameworkElement window = GetWindowParent(p);
                var w = window as Window;
                if (w != null)
                {
                    w.DragMove();
                }
            });
        }
        FrameworkElement GetWindowParent(UserControl p)
        {
            FrameworkElement parent = p;
            while (parent.Parent != null)
            {
                parent = parent.Parent as FrameworkElement;              
            }
            return parent;
        }
    }
}
