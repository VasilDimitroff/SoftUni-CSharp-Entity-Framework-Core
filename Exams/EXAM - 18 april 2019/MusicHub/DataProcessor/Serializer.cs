namespace MusicHub.DataProcessor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using Data;
    using Microsoft.EntityFrameworkCore;
    using MusicHub.DataProcessor.ExportDtos;
    using Newtonsoft.Json;

    public class Serializer
    {
        public static string ExportAlbumsInfo(MusicHubDbContext context, int producerId)
        {
            var albums = context.Albums.OrderByDescending(x => x.Songs.Sum(s => s.Price)).Where(album => album.Producer.Id == producerId)
                .Select(album => new
                {
                    AlbumName = album.Name,
                    ReleaseDate = album.ReleaseDate.ToString("MM/dd/yyyy"),
                    ProducerName = album.Producer.Name,
                    Songs = album.Songs.Select(song => new
                    {
                        SongName = song.Name,
                        Price = song.Price.ToString("f2"),
                        Writer = song.Writer.Name
                    })
                    .OrderByDescending(song => song.SongName)
                    .ThenBy(song => song.Writer)
                    .ToArray(),
                    AlbumPrice = album.Songs.Sum(s => s.Price).ToString("f2"),
                })
                .ToArray();
            ;
            var json = JsonConvert.SerializeObject(albums, Newtonsoft.Json.Formatting.Indented);

            return json;
        }

        public static string ExportSongsAboveDuration(MusicHubDbContext context, int duration)
        {
            var songs = context.Songs.Where(song => song.Duration.TotalSeconds > duration)
                .Select(song => new ExportSongModel
                {
                    SongName = song.Name,
                    Writer = song.Writer.Name,
                    Performer = song.SongPerformers.Select(sp => sp.Performer.FirstName + " " + sp.Performer.LastName).FirstOrDefault(),
                    AlbumProducer = song.Album.Producer.Name,
                    Duration = song.Duration.ToString("c")
                })
                .OrderBy(song => song.SongName)
                .ThenBy(song => song.Writer)
                .ThenBy(song => song.Performer)
                .ToArray();

            StringBuilder sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(String.Empty, String.Empty);

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportSongModel[]), new XmlRootAttribute("Songs"));

            using (StringWriter writer = new StringWriter(sb))
            {
                xmlSerializer.Serialize(writer, songs, namespaces);
            }

            return sb.ToString().TrimEnd();
        }
    }
}