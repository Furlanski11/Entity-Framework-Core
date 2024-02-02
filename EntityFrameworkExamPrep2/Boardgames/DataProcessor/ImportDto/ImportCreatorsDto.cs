using Boardgames.Data.Models;
using Boardgames.Data.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Boardgames.DataProcessor.ImportDto
{
	[XmlType("Creator")]
	public class ImportCreatorsDto
	{
        [XmlElement("FirstName")]
		[Required]
		[MinLength(2)]
		[MaxLength(7)]
		public string FirstName { get; set; } = null!;

		[XmlElement("LastName")]
		[Required]
		[MinLength(2)]
		[MaxLength(7)]
		public string LastName { get; set; } = null!;

		[XmlArray("Boardgames")]
		public ImportBoardgameDto[] Boardgames { get; set; } 

	}

	[XmlType("Boardgame")]
	public class ImportBoardgameDto
	{
		[XmlElement("Name")]
		[MinLength(10)]
		[MaxLength(20)]
		[Required]
		public string Name { get; set; } = null!;
		[XmlElement("Rating")]
		[Range(1, 10.00)]
		[Required]
        public double Rating { get; set; }

		[XmlElement("YearPublished")]
		[Range(2018, 2023)]
		[Required]
        public int YearPublished { get; set; }

		[XmlElement("CategoryType")]
		[Required]
		[Range(0, 4)]
        public int CategoryType { get; set; }

		[XmlElement("Mechanics")]
		[Required]
		public string Mechanics { get; set; } = null!;
    }
}
