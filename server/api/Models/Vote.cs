namespace IdeasToVote.Api.Models;

public class Vote
{
    public int Id { get; set; }
    public int IdeaId { get; set; }
    public int UserId { get; set; }
    public int Value { get; set; }
    public DateTime CreatedAt { get; set; }

    public Idea? Idea { get; set; }
    public User? User { get; set; }
}
