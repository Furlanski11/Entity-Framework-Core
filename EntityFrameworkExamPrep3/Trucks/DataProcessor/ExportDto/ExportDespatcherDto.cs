using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Trucks.Data.Models;

namespace Trucks.DataProcessor.ExportDto
{
	[XmlType("Despatcher")]
	public class ExportDespatcherDto
	{
		[XmlAttribute("TrucksCount")]
		public int TrucksCount { get; set; }

		[XmlElement("DespatcherName")]
		public string DespatcherName { get; set; } = null!;


		[XmlArray("Trucks")]
		public ExportTruckDto[] Trucks { get; set; } = null!;
	}

	[XmlType("Truck")]
	public class ExportTruckDto
	{
		[XmlElement("RegistrationNumber")]
		public string RegistrationNumber { get; set; } = null!;

		[XmlElement("Make")]
		public string Make { get; set; } = null!;
    }
}

