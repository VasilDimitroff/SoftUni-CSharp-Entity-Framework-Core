using Instagraph.Data;
using Instagraph.DataProcessor.DtoModels;
using Instagraph.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Instagraph.DataProcessor
{

    public class Deserializer
    {
        private static string successMessage = "Successfully imported {0}.";
        private static string errorMessage = "Error: Invalid data.";

        public static string ImportPictures(InstagraphContext context, string jsonString)
        {
            var pictures = JsonConvert.DeserializeObject<ImportPictureModel[]>(jsonString);

            var sb = new StringBuilder();

            foreach (var pic in pictures)
            {
                if (!IsValid(pic))
                {
                    sb.AppendLine(errorMessage);
                    continue;
                }

                var targetPath = context.Pictures.FirstOrDefault(x => x.Path == pic.Path);

                if (targetPath != null)
                {
                    sb.AppendLine(errorMessage);
                    continue;
                }

                var picture = new Picture()
                {
                    Path = pic.Path,
                    Size = pic.Size
                };

                context.Pictures.Add(picture);
                context.SaveChanges();
                sb.AppendLine($"Successfully imported Picture {picture.Path}.");
            }

            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportUsers(InstagraphContext context, string jsonString)
        {
            var users = JsonConvert.DeserializeObject<ImportUserModel[]>(jsonString);

            var sb = new StringBuilder();

            foreach (var userModel in users)
            {
                if (!IsValid(userModel))
                {
                    sb.AppendLine(errorMessage);
                    continue;
                }

                var picture = context.Pictures.FirstOrDefault(x => x.Path == userModel.ProfilePicture);
                var targetUser = context.Users.FirstOrDefault(x => x.Username == userModel.Username);

                if (picture == null || targetUser != null)
                {
                    sb.AppendLine(errorMessage);
                    continue;
                }

                var user = new User
                {
                    Username = userModel.Username,
                    Password = userModel.Password,
                    ProfilePicture = picture
                };

                context.Users.Add(user);
                sb.AppendLine($"Successfully imported User {user.Username}.");
            }

            context.SaveChanges();
            return sb.ToString().TrimEnd();
        }

        public static string ImportFollowers(InstagraphContext context, string jsonString)
        {
            var userFollowers = JsonConvert.DeserializeObject<ImportFollowerModel[]>(jsonString);

            var sb = new StringBuilder();

            foreach (var userFollower in userFollowers)
            {
                if (!IsValid(userFollower))
                {
                    sb.AppendLine(errorMessage);
                    continue;
                }

                var targetUser = context.Users.FirstOrDefault(x => x.Username == userFollower.User);
                var targetFollower = context.Users.FirstOrDefault(x => x.Username == userFollower.Follower); 

                if (targetUser == null || targetFollower == null || targetFollower.Username == targetUser.Username)
                {
                    sb.AppendLine(errorMessage);
                    continue;
                }

                if (targetUser.Followers.Any(u => u.Follower.Username == targetFollower.Username))
                {
                    sb.AppendLine(errorMessage);
                    continue;
                }

                var userFollowerModel = new UserFollower
                {
                    User = targetUser,
                    Follower = targetFollower
                };

                context.UsersFollowers.Add(userFollowerModel);
                context.SaveChanges();
                sb.AppendLine($"Successfully imported Follower {targetFollower.Username} to User {targetUser.Username}.");
            }

            return sb.ToString().TrimEnd();
        }

        public static string ImportPosts(InstagraphContext context, string xmlString)
        {
            return null;
        }

        public static string ImportComments(InstagraphContext context, string xmlString)
        {
            return null;
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
