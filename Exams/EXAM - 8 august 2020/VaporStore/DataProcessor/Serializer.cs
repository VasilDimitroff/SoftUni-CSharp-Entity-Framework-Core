namespace VaporStore.DataProcessor
{
	using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using VaporStore.DataProcessor.Dto.Export;

    public static class Serializer
	{
		public static string ExportGamesByGenres(VaporStoreDbContext context, string[] genreNames)
		{
			var games = context.Genres
				.Include(x => x.Games)
				.ThenInclude(x => x.Purchases)
				.ToArray()
				.Where(genre => genreNames.Contains(genre.Name))
				.Where(genre => genre.Games.Any(g => g.Purchases.Count > 0))
				.Select(genre => new ExportGamesModel
				{
					Id = genre.Id,
					Genre = genre.Name,
					
					Games = genre.Games.Select(game => new ExportGameModel
					{
						Id = game.Id,
						Title = game.Name,
						Developer = game.Developer.Name,
						Tags = string.Join(", ", game.GameTags.Select(gt => gt.Tag.Name)),
						Players = game.Purchases.Count()
					})
					.Where(x => x.Players > 0)
					.OrderByDescending(x => x.Players)
					.ThenBy(x => x.Id)
					.ToArray(),
					TotalPlayers = genre.Games.Sum(g => g.Purchases.Count()),
				})	
				.OrderByDescending(x => x.TotalPlayers)
				.ThenBy(x => x.Id)
				.ToArray();

			var json = JsonConvert.SerializeObject(games, Formatting.Indented);

			return json;
		}

		public static string ExportUserPurchasesByType(VaporStoreDbContext context, string storeType)
		{
			var users = context.Users.Where(user => user.Cards.Any(c => c.Purchases.Count > 0))
				.Include(x => x.Cards)
				.ThenInclude(x => x.Purchases)
				.ThenInclude(x => x.Game)
				.ThenInclude(x => x.Genre)
				.ToArray()
				.Select(user => new ExportUsersModel
				{
					Username = user.Username,
					Purchases = user.Cards.SelectMany(card => card.Purchases).Select( p => new ExportPurchaseModel
                    {
						Card = p.Card.Number,
						Cvc = p.Card.Cvc,
						Date = p.Date,
						Game = new ExportGameDto
                        {
							Title = p.Game.Name,
							Genre = p.Game.Genre.Name,
							Price = p.Game.Price
                        }
					})
					.OrderBy(x => x.Date)
					.ToArray(),
					TotalSpent = user.Cards.Sum(c => c.Purchases.Where(x => x.Game.Genre.Name == storeType).Sum(p => p.Game.Price))
				})
				.OrderByDescending(x => x.TotalSpent)
				.ThenBy(x => x.Username)
				.ToArray();

			StringBuilder sb = new StringBuilder();
			var namespaces = new XmlSerializerNamespaces();
			namespaces.Add(String.Empty, String.Empty);
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportUsersModel[]), new XmlRootAttribute("Users"));
			using (StringWriter writer = new StringWriter(sb))
			{
				xmlSerializer.Serialize(writer, users, namespaces);
			}

			return sb.ToString().TrimEnd();
		}
	}
}