namespace IdeasToVote.Api.Models;

public class Idea
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
