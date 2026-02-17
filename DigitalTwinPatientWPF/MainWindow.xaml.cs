using DigitalTwinPatientWPF.Database;
using DigitalTwinPatientWPF.Helpers;
using DigitalTwinPatientWPF.Statics;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace DigitalTwinPatientWPF
{
    public partial class MainWindow : Window
    {
        private DigitalTwinPatientDBTestOneEntities _db = new DigitalTwinPatientDBTestOneEntities();
        private MessageHelper _mh = new MessageHelper();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = LoginEnter.Text;
                string password = PasswordEnter.Password;

                if (!Regex.IsMatch(login, @"^[a-zA-Z]\d{7}$"))
                {
                    _mh.ShowError("Логин должен быть в формате: буква + 7 цифр");
                    return;
                }
                if (password.Length < 10)
                {
                    _mh.ShowError("Пароль должен быть минимум 10 символов");
                    return;
                }

                var doctor = _db.Doctor.Where(d => d.Login == login && d.Password == password).FirstOrDefault();

                if (doctor == null)
                {
                    _mh.ShowError("Неправильная почта или пароль");
                    return;
                }

                string code = new Random().Next(100000, 999999).ToString();
                _mh.ShowInfo($"Код двухфакторной аутентификации: {code}");

                TwoFactorWindow twoFactorWindow = new TwoFactorWindow(code);
                twoFactorWindow.Owner = this;
                twoFactorWindow.ShowDialog();

                if (!twoFactorWindow.IsConfirm)
                {
                    return;
                }

                CurrentSession.CurrentUser = doctor;

                new HomeWindow(doctor).Show();
                this.Close();
            }
            catch (Exception ex)
            {
                _mh.ShowError(ex.Message);
            }
        }
    }
}
