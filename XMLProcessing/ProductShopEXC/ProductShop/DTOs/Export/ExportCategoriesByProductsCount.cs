using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ProductShop.DTOs.Export
{
	[XmlType("Category")]
	public class ExportCategoriesByProductsCount
	{
		[XmlElement("name")]
		public string name { get; set; }
		[XmlElement("count")]
		public int count { get; set; }
		[XmlElement("averagePrice")]
		public decimal averagePrice { get; set; }
		[XmlElement("totalRevenue")]
		public decimal totalRevenue { get; set; }
	}
}
