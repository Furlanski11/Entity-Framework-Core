namespace Medicines.DataProcessor
{
    using Medicines.Data;
	using Medicines.Data.Models.Enums;
	using Medicines.DataProcessor.ExportDtos;
	using Medicines.Utilities;
	using Microsoft.EntityFrameworkCore;
	using Newtonsoft.Json;

	public class Serializer
    {
        public static string ExportPatientsWithTheirMedicines(MedicinesContext context, string date)
        {
            var patients = context.Patients
                .Where(p => p.PatientsMedicines.Any(pm => pm.Medicine.ProductionDate > Convert.ToDateTime(date)))
                .Select(p => new ExportPatientsDto
                {
                    Name = p.FullName,
                    AgeGroup = p.AgeGroup.ToString(),
                    Medicines = p.PatientsMedicines.Select(pm => new ExportMedicineDto
                    {
                        Category = pm.Medicine.Category.ToString(),
                        Price = pm.Medicine.Price,
                        Producer = pm.Medicine.Producer,
                        BestBefore = pm.Medicine.ExpiryDate.ToString("yyyy-MM-dd")
                    }).OrderByDescending(m => m.BestBefore)
                    .ThenBy(m => m.Price)
                    .ToArray()
                }).OrderByDescending(p => p.Medicines.Count())
                .ThenBy(p => p.Name)
                .ToArray();
            XmlHelper xmlHelper = new XmlHelper();

            return string.Format(xmlHelper.Serialize(patients, "Patients"), Formatting.Indented);
		}

        public static string ExportMedicinesFromDesiredCategoryInNonStopPharmacies(MedicinesContext context, int medicineCategory)
        {
            var medicines = context.Medicines
                .Where(m => m.Category == (Category)medicineCategory && m.Pharmacy.IsNonStop == true)
                 .Select(m => new
                 {
                     Name = m.Name,
                     Price = m.Price.ToString("f2"),
                     Pharmacy = new
                     {
                         Name = m.Pharmacy.Name,
                         PhoneNumber = m.Pharmacy.PhoneNumber
                     }
                 })
                 .OrderBy(m => m.Price)
                 .ThenBy(m => m.Name)
				 .ToArray();
            return JsonConvert.SerializeObject(medicines, Formatting.Indented);
        }
    }
}
