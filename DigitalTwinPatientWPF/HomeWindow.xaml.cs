using DigitalTwinPatientWPF.Database;
using DigitalTwinPatientWPF.Helpers;
using DigitalTwinPatientWPF.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DigitalTwinPatientWPF
{
    public partial class HomeWindow : Window
    {
        private DigitalTwinPatientDBTestEntities _db = new DigitalTwinPatientDBTestEntities();
        private MessageHelper _mh = new MessageHelper();
        private Doctor _currentDoctor;
        private List<PatientShortDto> _allPatients;

        public HomeWindow(Doctor doctor)
        {
            InitializeComponent();
            _currentDoctor = doctor;
            LoadDoctorInfo();
            Prescription();
        }

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

            FrequenceIdComboBox.ItemsSource = _db.Frequency.ToList();
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
                    Quantity = decimal.Parse(quantity),
                    DoseUnitId = (int)doseUnitId,
                    FrequencyId = (int)frequenceId,
                    DurationInDays = int.Parse(durationInDays),
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
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentSession.CurrentUser = null;
            new MainWindow().Show();
            this.Close();
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
    }

    public class PatientShortDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
    }
}