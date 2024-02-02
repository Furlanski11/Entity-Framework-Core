namespace Boardgames.DataProcessor
{
    using System.ComponentModel.DataAnnotations;
	using System.Text;
	using Boardgames.Data;
	using Boardgames.Data.Models;
	using Boardgames.Data.Models.Enums;
	using Boardgames.DataProcessor.ImportDto;
	using Boardgames.Utilities;
	using Newtonsoft.Json;

	public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedCreator
            = "Successfully imported creator – {0} {1} with {2} boardgames.";

        private const string SuccessfullyImportedSeller
            = "Successfully imported seller - {0} with {1} boardgames.";

        public static string ImportCreators(BoardgamesContext context, string xmlString)
        {
           XmlHelper xmlHelper = new XmlHelper();

            ImportCreatorsDto[] importCreatorsDtos = xmlHelper.Deserialize<ImportCreatorsDto[]>(xmlString, "Creators");

            List<Creator> creators = new List<Creator>();

            StringBuilder sb = new StringBuilder();

            foreach (var creatorDto in importCreatorsDtos)
            {
                if (!IsValid(creatorDto))
                {
                    sb.AppendLine(ErrorMessage); 
                    continue;
                }

                Creator creator = new Creator()
                {
                    FirstName = creatorDto.FirstName,
                    LastName = creatorDto.LastName,
                };


                foreach (var boardgame in creatorDto.Boardgames)
                {
					if (!IsValid(boardgame))
					{
						sb.AppendLine(ErrorMessage);
						continue;
					}

					creator.Boardgames.Add(new Boardgame
                    {
                        Name = boardgame.Name,
                        Rating = boardgame.Rating,
                        YearPublished = boardgame.YearPublished,
                        CategoryType = (CategoryType)boardgame.CategoryType,
                        Mechanics = boardgame.Mechanics,
                    });
				}
				creators.Add(creator);
				sb.AppendLine(string.Format(SuccessfullyImportedCreator, creator.FirstName, creator.LastName, creator.Boardgames.Count));
			}
            context.Creators.AddRange(creators);
            context.SaveChanges();
            return sb.ToString().TrimEnd();
        }

        public static string ImportSellers(BoardgamesContext context, string jsonString)
        {
            var sellersDtos = JsonConvert.DeserializeObject<ImportSellersDto[]>(jsonString);

            List<Seller> sellers = new List<Seller>();

            StringBuilder sb = new StringBuilder();

            var uniqueBoardgames = context.Boardgames
                .Select(b => b.Id)
                .ToList();

            foreach (ImportSellersDto sellerDto in sellersDtos)
            {
                if (!IsValid(sellerDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Seller seller = new Seller()
                {
                    Name = sellerDto.Name,
                    Address = sellerDto.Address,
                    Country = sellerDto.Country,
                    Website = sellerDto.Website,
                };

                foreach(var boardgameId in sellerDto.BoardgameIds.Distinct())
                {
                    if (!uniqueBoardgames.Contains(boardgameId))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    BoardgameSeller bgs = new()
                    {
                        Seller = seller,
                        BoardgameId = boardgameId,
                    };
                    seller.BoardgamesSellers.Add(bgs);
                }
                sellers.Add(seller);
                sb.AppendLine(string.Format(SuccessfullyImportedSeller, seller.Name, seller.BoardgamesSellers.Count()));
            }

            context.AddRange(sellers);
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
