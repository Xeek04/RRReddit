using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LoginPopup.Models
{
    public class Comment
    {
        [BsonElement("user"), BsonRepresentation(BsonType.String)]
        public string Username { get; set; }

        [BsonElement("post_id"), BsonRepresentation(BsonType.Int32)]
        public int PostID { get; set; }

        [BsonElement("comm_id"), BsonRepresentation(BsonType.Int32)]
        public int CommentID { get; set; }

        [BsonElement("content"), BsonRepresentation(BsonType.String)]
        public string Content { get; set; }

        [BsonElement("reply_id"), BsonRepresentation(BsonType.Int32)]
        public int ReplyID { get; set; } = 0;

        [BsonElement("datetime"), BsonRepresentation(BsonType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
