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
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
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
        public int Likes { get; set; } = 0;     // Add this property
        public int Dislikes { get; set; } = 0;  // Add this property

        public string SubredditName { get; set; }


    }
}
