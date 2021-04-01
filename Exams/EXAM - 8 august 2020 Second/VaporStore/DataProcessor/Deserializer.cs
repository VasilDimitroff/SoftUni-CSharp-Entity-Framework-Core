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
			var gamesDto = JsonConvert.DeserializeObject<ImportGameWithTagsModel[]>(jsonString);
			var sb = new StringBuilder();

            foreach (var gameDto in gamesDto)
            {
                if (!IsValid(gameDto) || gameDto.Tags.Length < 1)
                {
					sb.AppendLine("Invalid Data");
					continue;
                }

				var developer = context.Developers.FirstOrDefault(x => x.Name == gameDto.Developer);

                if (developer == null)
                {
					developer = new Developer()
					{
						Name = gameDto.Developer
					};
					context.Developers.Add(developer);
					context.SaveChanges();
				}

				var genre = context.Genres.FirstOrDefault(x => x.Name == gameDto.Genre);

                if (genre == null)
                {
					genre = new Genre
					{
						Name = gameDto.Genre
					};

					context.Genres.Add(genre);
					context.SaveChanges();
                }

				DateTime releaseDate;

				bool isDateParsed = DateTime.TryParseExact(
					gameDto.ReleaseDate,
					"yyyy-MM-dd", 
					CultureInfo.InvariantCulture,
					DateTimeStyles.None, 
					out releaseDate);

                if (!isDateParsed)
                {
					sb.AppendLine("Invalid Data");
					continue;
				}

				var game = new Game
				{
					Name = gameDto.Name,
					Price = gameDto.Price,
					ReleaseDate = releaseDate,
					Developer = developer,
					Genre = genre
				};

                foreach (var tagDto in gameDto.Tags)
                {
                    if (!IsValid(tagDto))
                    {
						sb.AppendLine("Invalid Data");
						continue;
					}

					var tag = context.Tags.FirstOrDefault(x => x.Name == tagDto);

                    if (tag == null)
                    {
						tag = new Tag
						{
							Name = tagDto
						};
                    }

					var gameTag = new GameTag()
					{
						Tag = tag
					};

					game.GameTags.Add(gameTag);
                }

				context.Games.Add(game);		
				context.SaveChanges();

				sb.AppendLine($"Added {game.Name} ({game.Genre.Name}) with {game.GameTags.Count} tags");
            }

			return sb.ToString().TrimEnd();
		}

		public static string ImportUsers(VaporStoreDbContext context, string jsonString)
		{
			var usersDto = JsonConvert.DeserializeObject<ImportUserWithCardsModel[]>(jsonString);

			var sb = new StringBuilder();

            foreach (var userDto in usersDto)
            {
                if (!IsValid(userDto) || !userDto.Cards.Any(c => IsValid(c)))
                {
					sb.AppendLine("Invalid Data");
					continue;
                }

				var user = new User
				{
					FullName = userDto.FullName,
					Username = userDto.Username,
					Email = userDto.Email,
					Age = userDto.Age
				};

                foreach (var cardDto in userDto.Cards)
                {
					CardType cardType;
					bool isCardTypeParsed = Enum.TryParse<CardType>(cardDto.Type, out cardType);

                    if (!isCardTypeParsed)
                    {
						sb.AppendLine("Invalid Data");
						continue;
					}

					var card = new Card
					{
						User = user,
						Cvc = cardDto.CVC,
						Type = cardType,
						Number = cardDto.Number
					};

					user.Cards.Add(card);
                }

				context.Users.Add(user);
				context.SaveChanges();
				sb.AppendLine($"Imported {user.Username} with {user.Cards.Count} cards");
            }

			return sb.ToString().TrimEnd();
		}

		public static string ImportPurchases(VaporStoreDbContext context, string xmlString)
		{
			var serializer = new XmlSerializer(typeof(ImportPurchaseModel[]), new XmlRootAttribute("Purchases"));
			var purchasesDto = (ImportPurchaseModel[])serializer.Deserialize(new StringReader(xmlString));

			var sb = new StringBuilder();

            foreach (var purchDto in purchasesDto)
            {
                if (!IsValid(purchDto))
                {
					sb.AppendLine("Invalid Data");
					continue;
                }

				DateTime date;
				bool isDateParsed = DateTime
					.TryParseExact(
					purchDto.Date,
					"dd/MM/yyyy HH:mm",
					CultureInfo.InvariantCulture,
					DateTimeStyles.None,
					out date);

				PurchaseType purchaseType;
				bool isPurchaseParsed = Enum.TryParse<PurchaseType>(purchDto.Type, out purchaseType);

                if (!isDateParsed || !isPurchaseParsed)
                {
					sb.AppendLine("Invalid Data");
					continue;
				}

				Game currentGame = context.Games.FirstOrDefault(game => game.Name == purchDto.Title);
				//must check if it is null?

				Card currentCard = context.Cards.FirstOrDefault(card => card.Number == purchDto.Card);

				var purchase = new Purchase
				{
					Game = currentGame,
					Type = purchaseType,
					ProductKey = purchDto.Key,
					Card = currentCard,
					Date = date
				};

				User user = context.Users.FirstOrDefault(user => user.Cards.Any(c => c.Number == currentCard.Number));

				context.Purchases.Add(purchase);
				context.SaveChanges();
				sb.AppendLine($"Imported {currentGame.Name} for {user.Username}");
            }

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