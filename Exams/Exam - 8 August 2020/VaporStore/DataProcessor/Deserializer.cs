namespace VaporStore.DataProcessor
{
	using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using VaporStore.Data.Models;
    using VaporStore.Data.Models.Enums;
    using VaporStore.DataProcessor.Dto.Import;

    public static class Deserializer
	{
		public static string ImportGames(VaporStoreDbContext context, string jsonString)
		{
			var gamesDto = JsonConvert.DeserializeObject<ImportGameDto[]>(jsonString);
			var sb = new StringBuilder();

			var games = new List<Game>();

            foreach (var gameDto in gamesDto)
			{
                if (!IsValid(gameDto) || gameDto.Tags.Count < 1)
                {
					sb.AppendLine("Invalid Data");
					continue;
                }

				var developer = context.Developers.FirstOrDefault(d => d.Name == gameDto.Developer);

                if (developer == null)
                {
					developer = new Developer()
					{
						Name = gameDto.Developer
					};

					context.Developers.Add(developer);
				}

				
				context.SaveChanges();

				Genre genre = context.Genres.FirstOrDefault(d => d.Name == gameDto.Genre);

                if (genre == null)
                {
					genre = new Genre()
					{
						Name = gameDto.Genre
					};

					context.Genres.Add(genre);
				}

				var game = new Game()
				{
					Name = gameDto.Name,
					Price = gameDto.Price,
					ReleaseDate = DateTime.ParseExact(gameDto.ReleaseDate, "yyyy-MM-dd", CultureInfo.InvariantCulture),
					Developer = developer,
					Genre = genre,
				};

				foreach (var tagDto in gameDto.Tags)
				{
					var currentTag = context.Tags.FirstOrDefault(x => x.Name == tagDto);

                    if (currentTag == null)
                    {
						currentTag = new Tag
						{
							Name = tagDto
						};

						context.Tags.Add(currentTag);
					}

					game.GameTags.Add(new GameTag
					{
						Tag = currentTag
					});
				}	

				sb.AppendLine($"Added {game.Name} ({game.Genre.Name}) with {game.GameTags.Count} tags");
				games.Add(game);
            }

			context.Games.AddRange(games);
			context.SaveChanges();

			return sb.ToString().TrimEnd();
		}

		public static string ImportUsers(VaporStoreDbContext context, string jsonString)
		{
			var usersDto = JsonConvert.DeserializeObject<ImportUserDto[]>(jsonString);
			var sb = new StringBuilder();

			var users = new List<User>();

            foreach (var userDto in usersDto)
            {
                if (!IsValid(userDto))
                {
					sb.AppendLine("Invalid Data");
					continue;
                }


				var user = new User()
				{
					FullName = userDto.FullName,
					Username = userDto.Username,
					Email = userDto.Email,
					Age = userDto.Age
				};


				bool isCardValid = true;

                foreach (var cardDto in userDto.Cards)
                {
					if (!IsValid(cardDto))
					{
						sb.AppendLine("Invalid Data");
						isCardValid = false;
						break;
					}

					CardType cardType = Enum.Parse<CardType>(cardDto.Type);

					var card = new Card()
					{
						Number = cardDto.Number,
						Cvc = cardDto.CVC,
						Type = cardType
					};

					user.Cards.Add(card);
				}

                if (!isCardValid)
                {
					continue;
                }

				users.Add(user);
				sb.AppendLine($"Imported {user.Username} with {user.Cards.Count} cards");
            }

			context.Users.AddRange(users);

			context.SaveChanges();

			return sb.ToString().Trim();
		}

		public static string ImportPurchases(VaporStoreDbContext context, string xmlString)
		{
			var serializer =
				new XmlSerializer(typeof(ImportPurchaseDto[]), new XmlRootAttribute("Purchases"));

			var purchasesDto = (ImportPurchaseDto[])serializer.Deserialize(new StringReader(xmlString));

			var sb = new StringBuilder();

			var purchases = new List<Purchase>();

            foreach (var purchaseDto in purchasesDto)
            {
                if (!IsValid(purchaseDto))
                {
					sb.AppendLine("Invalid Data");
					continue;
                }

				DateTime date =
					DateTime.ParseExact(purchaseDto.Date, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);


				PurchaseType type;
				bool isValidType = Enum.TryParse<PurchaseType>(purchaseDto.Type, out type);

				if (!isValidType)
				{
					sb.AppendLine("Invalid Data");
					continue;
				}

				var game = context.Games.FirstOrDefault(game => game.Name == purchaseDto.GameName);

				var card = context.Cards.FirstOrDefault(card => card.Number == purchaseDto.CardNumber);
				
				if (game == null || card == null)
				{
					sb.AppendLine("Invalid Data");
					continue;
				}


				var purchase = new Purchase()
				{
					Game = game,
					Type = type,
					ProductKey = purchaseDto.ProductKey,
					Card = card,
					Date = date	
				};

				purchases.Add(purchase);
				sb.AppendLine($"Imported {purchase.Game.Name} for {purchase.Card.User.Username}");
            }

			context.Purchases.AddRange(purchases);
			context.SaveChanges();

			return sb.ToString().Trim();
		}

		private static bool IsValid(object dto)
		{
			var validationContext = new ValidationContext(dto);
			var validationResult = new List<ValidationResult>();

			return Validator.TryValidateObject(dto, validationContext, validationResult, true);
		}
	}
}