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
    public class LoginViewModel : BaseViewModel
    {
        public static bool KeyClick = false;
        public static bool IsLogin { get; set; }
        public static LoginWindow lg1;
        private string _UserName;
        public string UserName { get => _UserName; set { _UserName = value; OnPropertyChanged(); } }

        private string _Password;
        public string Password { get => _Password; set { _Password = value; OnPropertyChanged(); } }
        public ICommand CloseCommand { get; set; }
        public ICommand LoginCommand { get; set; }
        public ICommand PasswordChangedCommand { get; set; }
        public ICommand FormClosedCommand { get; set; }
        public ICommand KeyEnterCommand { get; set; }
        public ICommand LoadedCommand { get; set; }
        public LoginViewModel()
        {
            IsLogin = false;
            
            LoadedCommand = new RelayCommand<LoginWindow>((p) => { return true; }, (p) => 
            { 
                lg1 = p;
            });
            LoginCommand = new RelayCommand<LoginWindow>((p) => { return true; }, (p) => 
            { 
                Login(p); 
            });
            CloseCommand = new RelayCommand<LoginWindow>((p) => { return true; }, (p) => 
            {
                p.Close();
                IsLogin = false;
            });
            PasswordChangedCommand = new RelayCommand<PasswordBox>((p) => { return true; }, (p) => 
            { 
                Password = p.Password; 
            });
            KeyEnterCommand = new RelayCommand<LoginWindow>((p) => { return true; }, (p) => 
            {
                p.PreviewKeyUp += P_PreviewKeyUp1; ;
                
            });
        }

        private void P_PreviewKeyUp1(object sender, KeyEventArgs e)
        {
                //if (e.Key == Key.Enter)
                //{
                    
                //    Login(lg1);
                //}
                //if (e.Key == Key.Escape)
                //{
                //    IsLogin = false;
                //    lg1.Close();
                //}
            
        }
        void Login(Window p)
        {
            //code dang nhap day
            
            if (p == null)
                return;
            string passEncode = "123456";
            string user = "admin";
            if (Password == passEncode && user == UserName)
            {
                IsLogin = true;
                p.Close();
            }
            else
            {
                MessageBox.Show("Sai tài khoản hoặc mật khẩu!");
            }
        }
    }
}
