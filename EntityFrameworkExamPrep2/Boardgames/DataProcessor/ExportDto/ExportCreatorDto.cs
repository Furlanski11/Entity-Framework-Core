using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Boardgames.DataProcessor.ExportDto
{
	[XmlType("Creator")]
	public class ExportCreatorDto
	{

        [XmlElement("CreatorName")]
        public string CreatorName { get; set; }

		[XmlAttribute("BoardgamesCount")]
		public int BoardgamesCount { get; set; }


		[XmlArray("Boardgames")]
        public ExportBoardgameDto[] Boardgames { get; set; }
    }

    [XmlType("Boardgame")]
    public class ExportBoardgameDto
    {
        [XmlElement("BoardgameName")]
        public string BoardgameName { get; set; }

        [XmlElement("BoardgameYearPublished")]
        public int BoardgameYearPublished { get; set; }
    }
}
