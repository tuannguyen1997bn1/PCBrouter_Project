using System;
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
    public class ControlBarLogin : BaseViewModel
    {
        MainViewModel mvm = new MainViewModel();
        #region commands
        public ICommand CloseWindowCommand1 { get; set; }
        public ICommand MaximizeWindowCommand1 { get; set; }
        public ICommand MinimizeWindowCommand1 { get; set; }
        public ICommand MouseMoveWindowCommand1 { get; set; }
        #endregion

        public ControlBarLogin()
        {
            CloseWindowCommand1 = new RelayCommand<UserControl>((p) => { return p == null ? false : true; }, (p) => 
            {
                FrameworkElement window = GetWindowParent(p);
                var w = window as Window;
                if (w != null)
                {
                    w.Close();
                }
            });
            MaximizeWindowCommand1 = new RelayCommand<UserControl>((p) => { return p == null ? false : true; }, (p) =>
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
            }
            );
            MinimizeWindowCommand1 = new RelayCommand<UserControl>((p) => { return p == null ? false : true; }, (p) =>
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
            }
            );
            MouseMoveWindowCommand1 = new RelayCommand<UserControl>((p) => { return p == null ? false : true; }, (p) =>
            {                
                FrameworkElement window = GetWindowParent(p);
                var w = window as Window;
                if (w != null)
                {
                    w.DragMove();
                }
            }
           );
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
