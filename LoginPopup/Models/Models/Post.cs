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
        [BsonElement("post_id")]
        public int Id { get; set; }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? PostId { get; set; }


        //[BsonElement("post_id"), BsonRepresentation(BsonType.Int32)]
        // public int Id { get; set; }

        [BsonElement("user")]
        public string? User { get; set; }

        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("MovieName")]

        public string MovieName { get; set;}

        [BsonElement("content")]
        public string Content { get; set; }

        [BsonElement("datetime")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedAt { get; set; }

        public string GetTimeAgo()
        {
            if (CreatedAt == default(DateTime) || CreatedAt == DateTime.MinValue)
            {
                return "Unknown time";
            }
            TimeSpan timeSinceCreated = DateTime.Now - CreatedAt;

            if (timeSinceCreated.TotalMinutes < 1)
                return "Just now";
            else if (timeSinceCreated.TotalMinutes < 2)
                return "1 minute ago";
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



        [BsonElement("score")]
        public int Score { get; set; } = 0;
        
        [BsonElement("subreddit")]
        public string SubredditName { get; set; }

        [BsonElement("tag")]
        public string? Tag { get; set; }

        /* Add when username is made available to the controller */
        //[BsonElement("user"), BsonRepresentation(BsonType.String)]
        //public string UserName { get; set; }
    }
}