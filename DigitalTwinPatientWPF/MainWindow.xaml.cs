using DigitalTwinPatientWPF.Database;
using DigitalTwinPatientWPF.Helpers;
using DigitalTwinPatientWPF.Statics;
using System;
using System.Linq;
using System.Windows;

namespace DigitalTwinPatientWPF
{
    public partial class MainWindow : Window
    {
        private DigitalTwinPatientDBTestEntities _db = new DigitalTwinPatientDBTestEntities();
        private MessageHelper _mh = new MessageHelper();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string email = EmailEnter.Text;
                string password = PasswordEnter.Password;

                var hashedPassword = PasswordHasher.Hash(password);

                var doctor = _db.Doctor.Where(d => d.Email == email && d.Password == hashedPassword).FirstOrDefault();

                if (doctor == null)
                {
                    _mh.ShowError("Неправильная почта или пароль");
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
