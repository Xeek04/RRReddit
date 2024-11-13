using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RRReddit.Models
{
    public class DatabaseUser
    {
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("username"), BsonRepresentation(BsonType.String)]
        public string? UserName { get; set; }

        [BsonElement("password"), BsonRepresentation(BsonType.String)]
        public string? Password { get; set; }

        [BsonElement("subreddits"), BsonRepresentation(BsonType.String)]
        public string? Subreddits { get; set; }

        [BsonElement("upvotes")]
        public List<ObjectId> Upvotes { get; set; }

        [BsonElement("downvotes")]
        public List<ObjectId> Downvotes { get; set; }

        [BsonElement("bookmarks")]
        public List<ObjectId> Bookmarks { get; set; }

    }

    //class to handle incoming request for the controllers that handle updating subreddits
    public class UpdateSubredditRequest
    {
        public string UpdatedSubreddits { get; set; }
    }
}