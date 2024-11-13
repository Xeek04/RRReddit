using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using LoginPopup.Models;
using RRReddit.Models;
using RRReddit.Data;
using MongoDB.Bson;
using Microsoft.Extensions.Hosting;

namespace LoginPopup.Controllers
{
    public class CommentController : Controller
    {
        private readonly IMongoCollection<Comment> _comments;
        private readonly IMongoCollection<Post> _postsCollection;

        public CommentController(MongoDatabase mongoDatabase)
        {
            _comments = mongoDatabase.Database.GetCollection<Comment>("comments");
            _postsCollection = mongoDatabase.Database.GetCollection<Post>("posts");
        }

        // POST: Add a new comment to a specific post
        [HttpPost]
        public async Task<IActionResult> AddComment(string postId, string content)
        {
            /* Validate postId and content
            if (postId <= 0 || string.IsNullOrWhiteSpace(content))
            {
                return BadRequest("Invalid post ID or content.");
            }
            */

            if (!ObjectId.TryParse(postId, out _) || string.IsNullOrWhiteSpace(content))
            {
                return BadRequest("Invalid post ID or content.");
            }

            // Retrieve the post using postId
            var post = await _postsCollection.Find(p => p.PostId == postId).FirstOrDefaultAsync();
            if (post == null)
            {
                return NotFound("Post not found");
            }

            var email = HttpContext.Session.GetString("UserEmail");
            var userName = email != null ? email.Split('@')[0] : "Anonymous";

            var comment = new Comment
            {
                //PostId = postId, // Use the manually incremented postId
                PostId = postId, // Set to the ObjectId of the post
                Content = content,
                CreatedAt = DateTime.Now,
                User = userName
            };

            await _comments.InsertOneAsync(comment);
            return RedirectToAction("Details", "Post", new { postId = postId });
        }

        // GET: Retrieve all comments for a specific post
        [HttpGet]
        public async Task<IActionResult> GetComments(string postId)
        {
            if (!ObjectId.TryParse(postId, out ObjectId objectId))
            {
                return BadRequest("Invalid post ID format.");
            }

            var comments = await _comments.Find(c => c.PostId == postId).ToListAsync();
            return Json(comments);
        }

        // POST: Add a reply to a specific comment
        [HttpPost]
        public async Task<IActionResult> AddReply(string commentId, string content)
        {
            if (!ObjectId.TryParse(commentId, out ObjectId commentObjectId) || string.IsNullOrWhiteSpace(content))
            {
                return BadRequest("Invalid comment ID or content.");
            }

            var email = HttpContext.Session.GetString("UserEmail");
            var userName = email != null ? email.Split('@')[0] : "Anonymous";

            var reply = new Comment
            {
                Content = content,
                CreatedAt = DateTime.Now,
                User = userName,
                ParentCommentId = commentObjectId,// Set the ID of the comment being replied to
                Replies = new List<Comment>()
            };

            /* Find the parent comment
            /var filter = Builders<Comment>.Filter.Eq(c => c.Id, new ObjectId(commentId));
            /var parentComment = await _comments.Find(filter).FirstOrDefaultAsync();

            /if (parentComment == null)
            {
                return NotFound("Parent comment not found.");
            }

            // Add the reply to the parent comment's Replies list
            var update = Builders<Comment>.Update.Push(c => c.Replies, reply);
            await _comments.UpdateOneAsync(filter, update);

            // Redirect to the post's details page
            return RedirectToAction("Details", "Post", new { id = parentComment.PostId });
            */
            // Use MongoDB's $push to add the reply to the Replies array of the parent comment
            var filter = Builders<Comment>.Filter.Eq(c => c.Id, commentObjectId);
            var update = Builders<Comment>.Update.Push(c => c.Replies, reply);
            var result = await _comments.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
            {
                return NotFound("Parent comment not found or reply could not be added.");
            }

            // Redirect back to the post's details page
            var parentComment = await _comments.Find(filter).FirstOrDefaultAsync();
            return RedirectToAction("Details", "Post", new { id = parentComment?.PostId });
        }

        //POST: Edit a comment
        [HttpPost]
        public async Task<IActionResult> EditComment(string commentId, string newContent)
        {
            if (!ObjectId.TryParse(commentId, out ObjectId commentObjectId) || string.IsNullOrWhiteSpace(newContent))
            {
                return BadRequest("Invalid comment ID or content.");
            }

            // Find and update the content of the comment with the specified Id
            var filter = Builders<Comment>.Filter.Eq(c => c.Id, commentObjectId);
            var update = Builders<Comment>.Update.Set(c => c.Content, newContent);
            var result = await _comments.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0) return NotFound("Comment not found or could not be updated.");

            var updatedComment = await _comments.Find(filter).FirstOrDefaultAsync();
            return RedirectToAction("Details", "Post", new { id = updatedComment?.PostId });
        }

        //POST : Delete comment
        [HttpPost]
        public async Task<IActionResult> DeleteComment(string commentId)
        {
            if (!ObjectId.TryParse(commentId, out ObjectId commentObjectId))
            {
                return BadRequest("Invalid comment ID.");
            }

            // Find the comment to determine if it’s a top-level comment or a nested reply
            var filter = Builders<Comment>.Filter.Eq(c => c.Id, new ObjectId(commentId));
            var comment = await _comments.Find(filter).FirstOrDefaultAsync();

            if (comment == null) return NotFound("Comment not found.");



            // If it's a top-level comment (no parent), delete it directly from the collection
            if (comment.ParentCommentId == null)
            {
                await _comments.DeleteOneAsync(filter);
            }
            else
            {
                // If it's a nested reply, recursively delete it from the parent's Replies array
                var success = await DeleteNestedComment(comment.ParentCommentId, new ObjectId(commentId));

                if (!success)
                {
                    return NotFound("Parent comment not found or reply could not be deleted.");
                }
            }

            // Redirect back to the post's details page
            return RedirectToAction("Details", "Post", new { id = comment.PostId });
        }

        // Helper method to delete a nested comment from the parent's Replies array
        private async Task<bool> DeleteNestedComment(ObjectId? parentCommentId, ObjectId commentId)
        {
            var parentFilter = Builders<Comment>.Filter.Eq(c => c.Id, parentCommentId);
            var update = Builders<Comment>.Update.PullFilter(c => c.Replies, r => r.Id == commentId);
            var result = await _comments.UpdateOneAsync(parentFilter, update);

            return result.ModifiedCount > 0;
        }

    }
}
