using DigitalTwinPatientWPF.Database;
using DigitalTwinPatientWPF.Helpers;
using DigitalTwinPatientWPF.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DigitalTwinPatientWPF
{
    public partial class HomeWindow : Window
    {
        private DigitalTwinPatientDBTestOneEntities _db = new DigitalTwinPatientDBTestOneEntities();
        private MessageHelper _mh = new MessageHelper();
        private Doctor _currentDoctor;
        private List<PatientShortDto> _allPatients;
        private DispatcherTimer _timer = new DispatcherTimer();
        private DateTime _lastActivity = DateTime.Now;

        public HomeWindow(Doctor doctor)
        {
            InitializeComponent();

            _currentDoctor = doctor;
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += CheckInactivity;
            _timer.Start();
            this.MouseMove += UpdateActivity;
            this.KeyDown += UpdateActivity;

            LoadPatients();
            LoadDoctorInfo();
            Prescription();
        }

        // Методы для обновления таймера 10 секунд
        private void UpdateActivity(object sender, EventArgs e)
        {
            _lastActivity = DateTime.Now;
        }

        private void CheckInactivity(object sender, EventArgs e)
        {
            if ((DateTime.Now - _lastActivity).TotalSeconds >= 100)
            {
                _timer.Stop();
                _mh.ShowError("Сессия завершена из-за неактивности");

                CurrentSession.CurrentUser = null;

                new MainWindow().Show();
                this.Close();
            }
        }

        // Раздел с назначениями
        private void Prescription()
        {
            LoadComboPrescription();
        }

        private void LoadDoctorInfo()
        {
            try
            {
                var doctorWithDepartment = _db.Doctor
                    .Include("Department")
                    .FirstOrDefault(d => d.Id == _currentDoctor.Id);

                if (doctorWithDepartment != null)
                {
                    string fullName = $"{doctorWithDepartment.Surname} " +
                                        $"{doctorWithDepartment.Name} " +
                                        $"{doctorWithDepartment.Patronymic}"
                                        .Trim();

                    DoctorFullNameText.Text = fullName;
                    DoctorDepartmentText.Text = doctorWithDepartment.Department.Name;
                }
            }
            catch (Exception ex)
            {
                _mh.ShowError(ex.Message);
            }
        }

        private void LoadComboPrescription()
        {
            _allPatients = _db.Patient
                .Select(p => new PatientShortDto
                {
                    Id = p.Id,
                    FullName = (p.Surname + " " + p.Name +
                               (p.Patronymic != null ? " " + p.Patronymic : "")).Trim()
                })
                .ToList();


            PatientIdComboBox.ItemsSource = _allPatients;
            PatientIdComboBox.DisplayMemberPath = "FullName";
            PatientIdComboBox.SelectedValuePath = "Id";
            PatientIdComboBox.SelectedIndex = -1;

            MedicationIdComboBox.ItemsSource = _db.Medication.ToList();
            MedicationIdComboBox.DisplayMemberPath = "Name";
            MedicationIdComboBox.SelectedValuePath = "Id";
            MedicationIdComboBox.SelectedIndex = 0;

            DoseUnitIdComboBox.ItemsSource = _db.DoseUnit.ToList();
            DoseUnitIdComboBox.DisplayMemberPath = "Name";
            DoseUnitIdComboBox.SelectedValuePath = "Id";
            DoseUnitIdComboBox.SelectedIndex = 0;

            FrequenceIdComboBox.ItemsSource = _db.Frequence.ToList();
            FrequenceIdComboBox.DisplayMemberPath = "Name";
            FrequenceIdComboBox.SelectedValuePath = "Id";
            FrequenceIdComboBox.SelectedIndex = 0;

            InstructionIdComboBox.ItemsSource = _db.Instruction.ToList();
            InstructionIdComboBox.DisplayMemberPath = "Name";
            InstructionIdComboBox.SelectedValuePath = "Id";
            InstructionIdComboBox.SelectedIndex = 0;

            PrescriptionStatusIdComboBox.ItemsSource = _db.PrescriptionStatus.ToList();
            PrescriptionStatusIdComboBox.DisplayMemberPath = "Name";
            PrescriptionStatusIdComboBox.SelectedValuePath = "Id";
            PrescriptionStatusIdComboBox.SelectedIndex = 0;
        }

        private void CreatePrescriptionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var patientId = PatientIdComboBox.SelectedValue;
                var medicationId = MedicationIdComboBox.SelectedValue;
                var quantity = QuantityText.Text;
                var doseUnitId = DoseUnitIdComboBox.SelectedValue;
                var frequenceId = FrequenceIdComboBox.SelectedValue;
                var durationInDays = DurationInDaysText.Text;
                var startDate = StartDateText.Text;
                var endDate = EndDateText.Text;
                var instructionId = InstructionIdComboBox.SelectedValue;
                var prescriptionStatusId = PrescriptionStatusIdComboBox.SelectedValue;

                if (System.String.IsNullOrWhiteSpace(quantity) ||
                    System.String.IsNullOrWhiteSpace(durationInDays) ||
                    System.String.IsNullOrWhiteSpace(startDate))
                {
                    _mh.ShowError("Вы не заполнили текстовые поля");
                    return;
                }

                if (!DateTime.TryParse(startDate, out DateTime startDateTime))
                {
                    _mh.ShowError("Некорректная дата начала");
                    return;
                }

                DateTime? endDateTime = null;
                if (!string.IsNullOrWhiteSpace(endDate))
                {
                    if (DateTime.TryParse(endDate, out DateTime parsedEndDate))
                    {
                        endDateTime = parsedEndDate;
                    }
                    else
                    {
                        _mh.ShowError("Некорректная дата окончания");
                        return;
                    }
                }

                _db.Prescription.Add(new Prescription
                {
                    PatientId = (int)patientId,
                    DoctorId = _currentDoctor.Id,
                    MedicationId = (int)medicationId,
                    Quantity = int.Parse(quantity),
                    DoseUnitId = (int)doseUnitId,
                    FrequenceId = (int)frequenceId,
                    DurationInDay = int.Parse(durationInDays),
                    StartDate = startDateTime,
                    EndDate = endDateTime,
                    InstructionId = (int)instructionId,
                    PrescriptionStatusId = (int)prescriptionStatusId,
                    CreatedAt = DateTime.Now
                });

                _db.SaveChanges();
                _mh.ShowInfo("Назначение создано!");
            }
            catch (Exception ex)
            {
                _mh.ShowError(ex.Message);
            }
        }
        
        private void PatientText_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = PatientText.Text
                .Trim()
                .ToLower();

            if (string.IsNullOrWhiteSpace(text))
            {
                PatientIdComboBox.ItemsSource = _allPatients;
                PatientIdComboBox.IsDropDownOpen = false;
                return;
            }

            var filtered = _allPatients
                .Where(p => p.FullName.ToLower().Contains(text))
                .ToList();


            PatientIdComboBox.ItemsSource = filtered;

            PatientIdComboBox.IsDropDownOpen = filtered.Any();
        }

        private void LoadPatients()
        {
            var today = DateTime.Today;

            var patientsData = _db.Patient
                .Select(p => new
                {
                    p.Id,
                    p.Surname,
                    p.Name,
                    p.Patronymic,
                    p.Birthday,
                    MainDiagnosis = p.PatientHistory
                        .OrderByDescending(ph => ph.DiagnosisDate)
                        .Select(ph => ph.Diagnosis.Name)
                        .FirstOrDefault(),
                    Status = p.PatientHistory
                        .OrderByDescending(h => h.DiagnosisDate)
                        .Select(h => h.DiagnosisStatus.Name)
                        .FirstOrDefault(),
                    LastMetricValue = p.HealthMetric
                        .OrderByDescending(m => m.MeasuredDate)
                        .Select(m => new { m.MetricType.Name, m.Value })
                        .FirstOrDefault(),
                    LastMetricName = p.HealthMetric
                        .OrderByDescending(m => m.MeasuredDate)
                        .Select(m => m.MetricType.Name)
                        .FirstOrDefault()
                })
                .ToList();

            var patients = patientsData
                .Select(p => new PatientViewModel
                {
                    Id = p.Id,
                    FullName = $"{p.Surname} {p.Name} {p.Patronymic ?? ""}",
                    Age = today.Year - p.Birthday.Year - (today.DayOfYear < p.Birthday.DayOfYear ? 1 : 0),
                    MainDiagnosis = p.MainDiagnosis ?? "Не указан",
                    LastMetrics = p.LastMetricValue != null
                        ? $"{p.LastMetricValue.Name}: {p.LastMetricValue.Value}"
                        : "Нет данных",
                    StatusName = p.Status ?? "Не указан",
                    RiskLevel = p.Status == "Хронический"
                        ? "High" 
                        : p.Status == "Подтвержденный" ? "Medium" : "Low" 
                })
                .ToList();

            PatientsGrid.ItemsSource = patients;
        }

        // Метод для поиска по фио, айди и диагнозу
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string search = SearchTextBox.Text.Trim().ToLower();
            int patientId;

            var query = _db.Patient.AsQueryable();

            if (int.TryParse(search, out patientId))
            {
                query = query.Where(p =>  p.Id == patientId);
            }
            else
            {
                query = query.Where(p =>
                    (p.Surname + " " + p.Name + " " + p.Patronymic)
                        .ToLower()
                        .Contains(search)
                    ||
                    p.PatientHistory.Any(h =>
                        h.Diagnosis.Name.ToLower().Contains(search))
                );
            }

            var today = DateTime.Now;

            var patientsData = query
                .Select(p => new
                {
                    p.Id,
                    p.Surname,
                    p.Name,
                    p.Patronymic,
                    p.Birthday,
                    MainDiagnosis = p.PatientHistory
                        .OrderByDescending(h => h.DiagnosisDate)
                        .Select(h => h.Diagnosis.Name)
                        .FirstOrDefault(),
                    Status = p.PatientHistory
                        .OrderByDescending(h => h.DiagnosisDate)
                        .Select(h => h.DiagnosisStatus.Name)
                        .FirstOrDefault(),
                    LastMetricValue = p.HealthMetric
                        .OrderByDescending(m => m.MeasuredDate)
                        .Select(m => new { m.MetricType.Name, m.Value })
                        .FirstOrDefault()
                })
                .ToList();

            var result = patientsData.Select(p => new PatientViewModel
            {
                Id = p.Id,
                FullName = $"{p.Surname} {p.Name} {p.Patronymic}".Trim(),
                Age = today.Year - p.Birthday.Year -
                      (today.DayOfYear < p.Birthday.DayOfYear ? 1 : 0),
                MainDiagnosis = p.MainDiagnosis ?? "Не указан",
                LastMetrics = p.LastMetricValue != null
                    ? $"{p.LastMetricValue.Name}: {p.LastMetricValue.Value}"
                    : "Нет данных",
                StatusName = p.Status ?? "Не указан",
                RiskLevel = p.Status == "Хронический" ? "High" :
                           p.Status == "Подтвержденный" ? "Medium" : "Low"
            }).ToList();

            PatientsGrid.ItemsSource = result;
        }

        private void ResetSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Clear();
            LoadPatients();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentSession.CurrentUser = null;
            new MainWindow().Show();
            this.Close();
        }
    }

    // Пациент в Назначениях
    public class PatientShortDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
    }

    // модель для вывода пациентов в таблицу
    public class PatientViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public int Age { get; set; }
        public string MainDiagnosis { get; set; }
        public string LastMetrics { get; set; }
        public string StatusName { get; set; }
        public string RiskLevel { get; set; }
    }
}