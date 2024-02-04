namespace Trucks.DataProcessor
{
    using System.ComponentModel.DataAnnotations;
	using System.Text;
	using Castle.Core.Internal;
	using Data;
	using Newtonsoft.Json;
	using Trucks.Data.Models;
	using Trucks.Data.Models.Enums;
	using Trucks.DataProcessor.ImportDto;
	using Trucks.Utilities;

	public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedDespatcher
            = "Successfully imported despatcher - {0} with {1} trucks.";

        private const string SuccessfullyImportedClient
            = "Successfully imported client - {0} with {1} trucks.";

        public static string ImportDespatcher(TrucksContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();

            XMLhelper helper = new XMLhelper();

            ImportDespatchersDto[] despatchersDtos = helper.Deserialize<ImportDespatchersDto[]>(xmlString, "Despatchers");

            List<Despatcher> despatchers = new List<Despatcher>();

            foreach (ImportDespatchersDto despatcherDto in despatchersDtos)
            {
                if(!IsValid(despatcherDto) || despatcherDto.Position.IsNullOrEmpty())
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Despatcher despatcher = new Despatcher()
                {
                    Name = despatcherDto.Name,
                    Position = despatcherDto.Position,
                };

                foreach (var truckDto in despatcherDto.Trucks)
                {
                    if (!IsValid(truckDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }
                    Truck truck = new Truck()
                    {
                        RegistrationNumber = truckDto.RegistrationNumber,
                        VinNumber = truckDto.VinNumber,
                        TankCapacity = truckDto.TankCapacity,
                        CargoCapacity = truckDto.CargoCapacity,
                        CategoryType = (CategoryType) truckDto.CategoryType,
                        MakeType = (MakeType) truckDto.MakeType,
                    };
                    despatcher.Trucks.Add(truck);
                }
                despatchers.Add(despatcher);
                var count = despatchers.Count();
                sb.AppendLine(string.Format(SuccessfullyImportedDespatcher, despatcher.Name, despatcher.Trucks.Count));
            }
            context.Despatchers.AddRange(despatchers);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }
        public static string ImportClient(TrucksContext context, string jsonString)
        {
            ImportClientDto[] clientDtos = JsonConvert.DeserializeObject<ImportClientDto[]>(jsonString);

            List<Client> clients = new List<Client>();

            StringBuilder sb = new StringBuilder();

            int[] truckIds = context.Trucks.Select(t => t.Id).ToArray();

            foreach(var clientDto in clientDtos)
            {
                if (!IsValid(clientDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if(clientDto.Type == "usual")
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Client client = new Client()
                {
                    Name = clientDto.Name,
                    Nationality = clientDto.Nationality,
                    Type = clientDto.Type,
                };

                foreach(var truckId in clientDto.TrucksIds.Distinct())
                {
                    if (!truckIds.Contains(truckId))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }
                    ClientTruck clientTruck = new ClientTruck()
                    {
                        TruckId = truckId,

                    };
                    client.ClientsTrucks.Add(clientTruck);
                }
                clients.Add(client);
                sb.AppendLine(string.Format(SuccessfullyImportedClient, client.Name, client.ClientsTrucks.Count));
               
            }
            context.Clients.AddRange(clients);
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