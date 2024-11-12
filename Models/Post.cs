using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace LoginPopup.Models
{
    public class Post
    {
        public string GetPreview(int length = 100)
        {
            if (string.IsNullOrEmpty(Content))
                return string.Empty;

            if (Content.Length <= length)
                return Content;
            else
                return Content.Substring(0, length) + "...";
        }

        // DO NOT TRY TO SET THIS VARIABLE
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public ObjectId PostId { get; set; }


        [BsonElement("post_id"), BsonRepresentation(BsonType.Int32)]
        public int Id { get; set; }

        [BsonElement("user"), BsonRepresentation(BsonType.String)]
        public string? User { get; set; }

        [BsonElement("title"), BsonRepresentation(BsonType.String)]
        public string Title { get; set; }

        [BsonElement("content"), BsonRepresentation(BsonType.String)]
        public string Content { get; set; }

        [BsonElement("datetime"), BsonRepresentation(BsonType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string GetTimeAgo()
        {
            TimeSpan timeSinceCreated = DateTime.Now - CreatedAt;

            if (timeSinceCreated.TotalMinutes < 1)
                return $"{(int)timeSinceCreated.TotalSeconds} seconds ago";
            else if (timeSinceCreated.TotalHours < 1)
                return $"{(int)timeSinceCreated.TotalMinutes} minutes ago";
            else if (timeSinceCreated.TotalDays < 1)
                return $"{(int)timeSinceCreated.TotalHours} hours ago";
            else if (timeSinceCreated.TotalDays < 7)
                return $"{(int)timeSinceCreated.TotalDays} days ago";
            else if (timeSinceCreated.TotalDays < 30)
                return $"{(int)(timeSinceCreated.TotalDays / 7)} weeks ago";
            else if (timeSinceCreated.TotalDays < 365)
                return $"{(int)(timeSinceCreated.TotalDays / 30)} months ago";
            else
                return $"{(int)(timeSinceCreated.TotalDays / 365)} years ago";
        }


        [BsonElement("score"), BsonRepresentation(BsonType.Int32)]
        public int Score { get; set; } = 0;

        [BsonElement("subreddit"), BsonRepresentation(BsonType.String)]
        public string SubredditName { get; set; }

        [BsonElement("tag"), BsonRepresentation(BsonType.String)]
        public string Tag { get; set; }

        /* Add when username is made available to the controller */
        //[BsonElement("user"), BsonRepresentation(BsonType.String)]
        //public string UserName { get; set; }
    }
}
