namespace MusicHub
{
    using System;
    using System.Linq;
    using System.Text;
    using Data;
    using Initializer;

    public class StartUp
    {
        public static void Main(string[] args)
        {
            MusicHubDbContext context = 
                new MusicHubDbContext();

           DbInitializer.ResetDatabase(context);

           //example usage
           var result = ExportSongsAboveDuration(context, 4);
           Console.WriteLine(result);  
        }

        public static string ExportAlbumsInfo(MusicHubDbContext context, int producerId)
        {

            var albums = context.Albums.Where(a => a.ProducerId == producerId)
                .ToList()
                .Select(album => new
                {
                    Name = album.Name,
                    ReleaseDate = album.ReleaseDate,
                    ProducerName = album.Producer.Name,
                    TotalPrice = album.Songs.Sum(song => song.Price),
                    Songs = album.Songs.Select(song => new
                    {
                        SongName = song.Name,
                        Price = song.Price,
                        Writer = song.Writer.Name
                    })
                    .OrderByDescending(song => song.SongName)
                    .ThenBy(song => song.Writer)
                })
                .OrderByDescending(album => album.TotalPrice);
                

            var sb = new StringBuilder();

            foreach (var album in albums)
            {
                sb.AppendLine($"-AlbumName: {album.Name}")
                    .AppendLine($"-ReleaseDate: {album.ReleaseDate.ToString("MM/dd/yyyy")}")
                    .AppendLine($"-ProducerName: {album.ProducerName}")
                    .AppendLine($"-Songs:");

                int counter = 1;

                foreach (var song in album.Songs)
                {
                    sb.AppendLine($"---#{counter++}")
                        .AppendLine($"---SongName: {song.SongName}")
                        .AppendLine($"---Price: {song.Price:f2}")
                        .AppendLine($"---Writer: {song.Writer}");
                }

                sb.AppendLine($"-AlbumPrice: {album.TotalPrice:f2}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string ExportSongsAboveDuration(MusicHubDbContext context, int duration)
        {
            var songs = context.Songs
                .ToList()
                .Where(song => song.Duration.TotalSeconds > duration)
                .Select(song => new
                {
                    Name = song.Name,
                    PerformerName = song.SongPerformers
                        .Select(sp => sp.Performer.FirstName + " " + sp.Performer.LastName)
                        .FirstOrDefault(),
                    WriterName = song.Writer.Name,
                    Producer = song.Album.Producer.Name,
                    Duration = song.Duration
                })
                .OrderBy(song => song.Name)
                .ThenBy(song => song.WriterName)
                .ThenBy(song => song.PerformerName);


            var sb = new StringBuilder();
            int counter = 1;

            foreach (var song in songs)
            {
                sb.AppendLine($"-Song #{counter++}")
                    .AppendLine($"---SongName: {song.Name}")
                    .AppendLine($"---Writer: {song.WriterName}")
                    .AppendLine($"---Performer: {song.PerformerName}")
                    .AppendLine($"---AlbumProducer: {song.Producer}")
                    .AppendLine($"---Duration: {song.Duration:c}");
            }

            return sb.ToString().TrimEnd();
        }
    }
}
