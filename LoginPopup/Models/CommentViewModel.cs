using System.ComponentModel.DataAnnotations;

public class CommentViewModel
{
    public string PostId { get; set; }

    [Required(ErrorMessage = "Content is required.")]
    public string Content { get; set; }
}
