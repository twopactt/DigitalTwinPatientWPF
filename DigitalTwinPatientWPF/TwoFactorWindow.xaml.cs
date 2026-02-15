using DigitalTwinPatientWPF.Helpers;
using System.Windows;

namespace DigitalTwinPatientWPF
{
    public partial class TwoFactorWindow : Window
    {
        private readonly MessageHelper _mh = new MessageHelper();
        private readonly string _code;

        public bool IsConfirm { get; private set; }

        public TwoFactorWindow(string code)
        {
            InitializeComponent();
            _code = code;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (CodeBox.Text == _code)
            {
                IsConfirm = true;
                this.Close();
            }
            else
            {
                _mh.ShowError("Неверный код");
            }
        }
    }
}
