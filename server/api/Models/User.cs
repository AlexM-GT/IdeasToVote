namespace IdeasToVote.Api.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Idea> Ideas { get; set; } = new List<Idea>();
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
