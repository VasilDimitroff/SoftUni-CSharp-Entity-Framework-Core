namespace VaporStore.DataProcessor
{
	using System;
    using System.Linq;
    using Data;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;

    public static class Serializer
	{
		public static string ExportGamesByGenres(VaporStoreDbContext context, string[] genreNames)
		{
			var genres = context.Genres
				.Where(genre => genreNames.Contains(genre.Name))
				.Select(genre => new
                {
					Id = genre.Id,
					Genre = genre.Name,
					Games = genre.Games.Where(game => game.Purchases.Any()).Select(game => new
                    {
						Id = game.Id,
						Title = game.Name,
						Developer = game.Developer.Name,
						Tags = string.Join(", ", game.GameTags.Select(gt => gt.Tag.Name)),
						Players = game.Purchases.Count()
                    })
					.OrderByDescending(game => game.Players)
					.ThenBy(game => game.Id)
					.ToArray(),
					TotalPlayers = genre.Games.Sum(game => game.Purchases.Count)
                })
				.OrderByDescending(genre => genre.TotalPlayers)
				.ThenBy(genre => genre.Id)
				.ToArray();

			var json = JsonConvert.SerializeObject(genres, Formatting.Indented);

			return json;
		}

		public static string ExportUserPurchasesByType(VaporStoreDbContext context, string storeType)
		{
			return null;	
		}
	}
}