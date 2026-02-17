using DigitalTwinPatientWPF.Database;
using DigitalTwinPatientWPF.Statics;
using System;
using System.Linq;
using System.Windows;

namespace DigitalTwinPatientWPF
{
    public partial class PatientCardWindow : Window
    {
        private readonly DigitalTwinPatientDBTestOneEntities _db = new DigitalTwinPatientDBTestOneEntities();
        private Doctor _currentDoctor;
        private readonly int _patientId;
            
        public PatientCardWindow(int patientId)
        {
            InitializeComponent();
            _patientId = patientId;
            _currentDoctor = CurrentSession.CurrentUser;
            LoadOverview();
            LoadHistory();
            LoadPrescriptions();
            LoadMetrics();
            LoadConsultations();
        }

        // ОБЗОР
        private void LoadOverview()
        {
            var patient = _db.Patient
                .Include("Gender")
                .Include("MedicalCard.BloodType")
                .FirstOrDefault(p => p.Id == _patientId);

            if (patient == null) return;

            FullNameText.Text =
                $"{patient.Surname} {patient.Name} {patient.Patronymic}".Trim();

            int age = DateTime.Today.Year - patient.Birthday.Year;
            AgeText.Text = $"Возраст: {age}";

            GenderText.Text = $"Пол: {patient.Gender?.Name}";
            PhoneText.Text = $"Телефон: {patient.Phone}";

            // берём ПОСЛЕДНЮЮ медицинскую карту
            var medicalCard = patient.MedicalCard
                .OrderByDescending(mc => mc.CreatedAt)
                .FirstOrDefault();

            if (medicalCard != null)
            {
                BloodTypeText.Text = $"Группа крови: {medicalCard.BloodType?.Name}";
                HeightWeightText.Text =
                    $"Рост: {medicalCard.Height} см, Вес: {medicalCard.Weight} кг";
            }
            else
            {
                BloodTypeText.Text = "Группа крови: не указана";
                HeightWeightText.Text = "Рост / вес: нет данных";
            }
        }

        // ИСТОРИЯ БОЛЕЗНИ
        private void LoadHistory()
        {
            var history = _db.PatientHistory
                .Where(h => h.PatientId == _patientId)
                .OrderByDescending(h => h.DiagnosisDate)
                .Select(h => new
                {
                    Date = h.DiagnosisDate,
                    Diagnosis = h.Diagnosis.Name,
                    Category = h.Diagnosis.DiagnosisCategory.Name,
                    Status = h.DiagnosisStatus.Name
                })
                .ToList();

            HistoryGrid.ItemsSource = history;
        }

        // НАЗНАЧЕНИЯ
        private void LoadPrescriptions()
        {
            var prescriptions = _db.Prescription
                .Where(p => p.PatientId == _patientId)
                .Select(p => new
                {
                    Medication = p.Medication.Name,
                    Quantity = p.Quantity,
                    DoseUnit = p.DoseUnit.Name,
                    Frequency = p.Frequence.Name,
                    Status = p.PrescriptionStatus.Name
                })
                .ToList()
                .Select(p => new
                {
                    p.Medication,
                    Dose = $"{p.Quantity} {p.DoseUnit}",
                    p.Frequency,
                    p.Status
                })
                .ToList();


            PrescriptionGrid.ItemsSource = prescriptions;
        }

        // ИССЛЕДОВАНИЯ
        private void LoadMetrics()
        {
            var metrics = _db.HealthMetric
                .Where(m => m.PatientId == _patientId)
                .OrderByDescending(m => m.MeasuredDate)
                .Select(m => new
                {
                    Metric = m.MetricType.Name,
                    Value = m.Value,
                    Unit = m.MetricType.UnitOfMetricType.Name,
                    Date = m.MeasuredDate
                })
                .ToList()
                .Select(m => new
                {
                    m.Metric,
                    Value = $"{m.Value} {m.Unit}",
                    m.Date
                })
                .ToList();


            MetricGrid.ItemsSource = metrics;
        }

        // ДОКУМЕНТЫ / КОНСУЛЬТАЦИИ
        private void LoadConsultations()
        {
            var consultations = _db.Consultaion
                .Where(c => c.PatientId == _patientId)
                .OrderByDescending(c => c.DateConsultation)
                .Select(c => new
                {
                    Date = c.DateConsultation,
                    Doctor = c.Doctor.Surname + " " + c.Doctor.Name,
                    Comment = c.Notes
                })
                .ToList();

            ConsultationGrid.ItemsSource = consultations;
        }


        //ща будет блок быстрых действий, там поймешь по названию кликов что куда
        private void AddConsultation_Click(object sender, RoutedEventArgs e)
        {
            var consultation = new Consultaion
            {
                PatientId = _patientId,
                DoctorId = _currentDoctor.Id,
                DateConsultation = DateTime.Now,
                Notes = ""
            };

            _db.Consultaion.Add(consultation);
            _db.SaveChanges();

            MessageBox.Show("Вы записали пациента на прием", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);

            LoadConsultations();
        }

        private void AddPrescription_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Окно назначения лечения будет реализовано позже",
                "Назначение лечения",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void AddResearch_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Окно добавления исследования будет реализовано позже",
                "Исследование",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void EpicrizCard_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Окно создания эпикриза будет реализовано позже",
                "Создание эпикриза",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
