namespace MusicHub.DataProcessor
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
    using MusicHub.Data.Models;
    using MusicHub.Data.Models.Enums;
    using MusicHub.DataProcessor.ImportDtos;
    using Newtonsoft.Json;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data";

        private const string SuccessfullyImportedWriter 
            = "Imported {0}";
        private const string SuccessfullyImportedProducerWithPhone 
            = "Imported {0} with phone: {1} produces {2} albums";
        private const string SuccessfullyImportedProducerWithNoPhone
            = "Imported {0} with no phone number produces {1} albums";
        private const string SuccessfullyImportedSong 
            = "Imported {0} ({1} genre) with duration {2}";
        private const string SuccessfullyImportedPerformer
            = "Imported {0} ({1} songs)";

        public static string ImportWriters(MusicHubDbContext context, string jsonString)
        {
            var writersModels = JsonConvert.DeserializeObject<ImportWriterModel[]>(jsonString);

            var sb = new StringBuilder();

            foreach (var writerModel in writersModels)
            {
                if (!IsValid(writerModel))
                {
                    sb.AppendLine("Invalid data");
                    continue;
                }

                var writer = new Writer()
                {
                    Name = writerModel.Name,
                    Pseudonym = writerModel.Pseudonym
                };

                context.Writers.Add(writer);
                sb.AppendLine($"Imported {writer.Name}");
            }

            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportProducersAlbums(MusicHubDbContext context, string jsonString)
        {
            var producers = JsonConvert.DeserializeObject<ImportProducerWithAlbumsModel[]>(jsonString);

            var sb = new StringBuilder();

            foreach (var producerModel in producers)
            {
                if (!IsValid(producerModel) || !producerModel.Albums.All(a => IsValid(a)))
                {
                    sb.AppendLine("Invalid data");
                    continue;
                }

                var producer = new Producer()
                {
                    Name = producerModel.Name,
                    Pseudonym = producerModel.Pseudonym,
                    PhoneNumber = producerModel.PhoneNumber,
                };

                foreach (var albumModel in producerModel.Albums)
                {
                    DateTime releaseDate;

                    bool isReleaseDateValid = DateTime.TryParseExact(albumModel.ReleaseDate, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out releaseDate);

                    if (!isReleaseDateValid)
                    {
                        sb.AppendLine("Invalid data");
                        continue;
                    }

                    var album = new Album()
                    {
                        Name = albumModel.Name,
                        ReleaseDate = releaseDate
                    };

                    producer.Albums.Add(album);
                }

                context.Producers.Add(producer);

                var phoneInfo = "phone: " + producer.PhoneNumber;

                if (phoneInfo == "phone: ")
                {
                    phoneInfo = "no phone number";
                }

                sb.AppendLine($"Imported {producer.Name} with {phoneInfo} produces {producer.Albums.Count()} albums");
            }

            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportSongs(MusicHubDbContext context, string xmlString)
        {
            var serializer = new XmlSerializer(typeof(ImportSongModel[]), new XmlRootAttribute("Songs"));
            var songsDto = (ImportSongModel[])serializer.Deserialize(new StringReader(xmlString));
            var sb = new StringBuilder();

            foreach (var songDto in songsDto)
            {
                if (!IsValid(songDto))
                {
                    sb.AppendLine("Invalid data");
                    continue;
                }      

                TimeSpan duration;
                bool isDurationParsed =
                    TimeSpan.TryParseExact(songDto.Duration, "c", CultureInfo.InvariantCulture, out duration);

                DateTime createdOn;
                bool isCreatedOnValid = DateTime.TryParseExact(songDto.CreatedOn, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out createdOn);

                Genre genre;
                bool isGenreParsed = Enum.TryParse<Genre>(songDto.Genre, out genre);

                var targetAlbum = context.Albums.FirstOrDefault(x => x.Id == songDto.AlbumId);
                var targetWriter = context.Writers.FirstOrDefault(x => x.Id == songDto.WriterId);


                if (!isDurationParsed || !isCreatedOnValid || !isGenreParsed || targetAlbum == null || targetWriter == null)
                {
                    sb.AppendLine("Invalid data");
                    continue;
                }

                var song = new Song()
                {
                    Name = songDto.Name,
                    Duration = duration,
                    CreatedOn = createdOn,
                    Genre = genre,
                    AlbumId = songDto.AlbumId,
                    WriterId = songDto.WriterId,
                    Price = songDto.Price
                };

                context.Songs.Add(song);
                sb.AppendLine($"Imported {song.Name} ({song.Genre} genre) with duration {song.Duration}");
            }
           context.SaveChanges();
            return sb.ToString().TrimEnd();
        }

        public static string ImportSongPerformers(MusicHubDbContext context, string xmlString)
        {
            var serializer = new XmlSerializer(typeof(ImportPerformerModel[]), new XmlRootAttribute("Performers"));
            var performerModels = (ImportPerformerModel[])serializer.Deserialize(new StringReader(xmlString));
            var sb = new StringBuilder();

            foreach (var performerModel in performerModels)
            {
                if (!IsValid(performerModel))
                {
                    sb.AppendLine("Invalid data");
                    continue;
                }

                var performer = new Performer()
                {
                    FirstName = performerModel.FirstName,
                    LastName = performerModel.LastName,
                    Age = performerModel.Age,
                    NetWorth = performerModel.NetWorth
                };

                bool isSongValid = true;

                foreach (var songModel in performerModel.PerformersSongs)
                {
                    var targetSong = context.Songs.FirstOrDefault(x => x.Id == songModel.Id);

                    if (targetSong == null)
                    {  
                        isSongValid = false;
                        break;
                    }

                    var performerSong = new SongPerformer()
                    {
                        Performer = performer,
                        SongId = targetSong.Id
                    };

                    performer.PerformerSongs.Add(performerSong);
                }

                if (!isSongValid)
                {
                    sb.AppendLine("Invalid data");
                    continue;
                }

                context.Performers.Add(performer);
                sb.AppendLine($"Imported {performer.FirstName} ({performer.PerformerSongs.Count()} songs)");           
            }

            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object obj)
        {
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(obj);
            var validationResult = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResult, true);
            return isValid;
        }
    }
}