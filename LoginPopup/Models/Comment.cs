using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace LoginPopup.Models
{
    public class Comment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; } // Unique identifier for the comment

        [BsonElement("postId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? PostId { get; set; } // References the unique ObjectId of the related post

        [BsonElement("user")]
        public string User { get; set; }

        [BsonElement("content")]
        public string Content { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [BsonElement("replies")]
        public List<Comment> Replies { get; set; } = new List<Comment>(); //Nested Replies

        [BsonElement("parentCommentId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId? ParentCommentId { get; set; } // ID of the parent comment, null if top-level
    }
}