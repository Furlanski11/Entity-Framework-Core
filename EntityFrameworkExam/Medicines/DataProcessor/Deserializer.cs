namespace Medicines.DataProcessor
{
    using Medicines.Data;
	using Medicines.Data.Models;
	using Medicines.Data.Models.Enums;
	using Medicines.DataProcessor.ImportDtos;
	using Medicines.Utilities;
	using Newtonsoft.Json;
	using System.ComponentModel.DataAnnotations;
	using System.Globalization;
	using System.Text;

	public class Deserializer
    {
        private const string ErrorMessage = "Invalid Data!";
        private const string SuccessfullyImportedPharmacy = "Successfully imported pharmacy - {0} with {1} medicines.";
        private const string SuccessfullyImportedPatient = "Successfully imported patient - {0} with {1} medicines.";

        public static string ImportPatients(MedicinesContext context, string jsonString)
        {
            ImportPatientsDto[] patientsDtos = JsonConvert.DeserializeObject<ImportPatientsDto[]>(jsonString);

            StringBuilder sb = new StringBuilder();
            List<Patient> patients = new List<Patient>();
            foreach(var patientDto in patientsDtos)
            {
                if (!IsValid(patientDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

               Patient patient = new Patient()
               {
                   FullName = patientDto.FullName,
                   AgeGroup = (AgeGroup)patientDto.AgeGroup,
                   Gender = (Gender)patientDto.Gender,
                   
               };

                foreach(var medicineDto in patientDto.Medicines)
                {
                    if (patient.PatientsMedicines.Select(pm => pm.MedicineId).Any(m => m == medicineDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }
                    patient.PatientsMedicines.Add(new PatientMedicine
                    {
                        MedicineId = medicineDto
                    });
                }
                sb.AppendLine(string.Format(SuccessfullyImportedPatient, patient.FullName, patient.PatientsMedicines.Count));
            }
            context.Patients.AddRange(patients);
            context.SaveChanges();
            return sb.ToString().TrimEnd();
        }

        public static string ImportPharmacies(MedicinesContext context, string xmlString)
        {
			XmlHelper xmlHelper = new XmlHelper();
            StringBuilder sb = new StringBuilder();

            ImportPharmaciesDto[] pharmaciesDtos = xmlHelper.Deserialize<ImportPharmaciesDto[]>(xmlString, "Pharmacies");
            List<Pharmacy> pharmacies = new();
            foreach(var pharmacyDto in  pharmaciesDtos)
            {
                if (!IsValid(pharmacyDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
                if(pharmacyDto.IsNonStop != "true" && pharmacyDto.IsNonStop != "false")
                {
					sb.AppendLine(ErrorMessage);
					continue;
				}

                Pharmacy pharmacy = new Pharmacy()
                {
                    IsNonStop = bool.Parse(pharmacyDto.IsNonStop),
                    Name = pharmacyDto.Name,
                    PhoneNumber = pharmacyDto.PhoneNumber,
                };
                foreach(var medicineDto in pharmacyDto.Medicines.Distinct())
                {
                    if(pharmacy.Medicines.Any(m => m.Name == medicineDto.Name && m.Producer == medicineDto.Producer))
                    {
						sb.AppendLine(ErrorMessage);
						continue;
					}
                    if (!IsValid(medicineDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }
                    if(Convert.ToDateTime(medicineDto.ProductionDate) >= Convert.ToDateTime(medicineDto.ExpiryDate))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    DateTime tempProdDate = Convert.ToDateTime(medicineDto.ProductionDate, CultureInfo.InvariantCulture);
					DateTime tempExpDate = Convert.ToDateTime(medicineDto.ExpiryDate, CultureInfo.InvariantCulture);
                    
                    Medicine medicine = new Medicine()
                    {
                        Category = (Category)medicineDto.Category,
                        Name = medicineDto.Name,
                        Price = medicineDto.Price,
                        ProductionDate = DateTime.Parse(tempProdDate.ToString("yyyy-MM-dd")),
                        ExpiryDate = DateTime.Parse(tempExpDate.ToString("yyyy-MM-dd")),
                        Producer = medicineDto.Producer,
                    };
                    pharmacy.Medicines.Add(medicine);
                }
                pharmacies.Add(pharmacy);
                sb.AppendLine(string.Format(SuccessfullyImportedPharmacy, pharmacy.Name, pharmacy.Medicines.Count));
            }
            context.Pharmacies.AddRange();
            context.SaveChanges();
            return sb.ToString().TrimEnd();          
		}

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}
