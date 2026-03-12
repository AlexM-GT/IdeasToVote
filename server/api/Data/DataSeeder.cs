using IdeasToVote.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace IdeasToVote.Api.Data;

public static class DataSeeder
{
    public static void Seed(ApplicationDbContext context)
    {
        if (context.Users.Any())
        {
            return;
        }

        var now = DateTime.UtcNow;

        var users = new List<User>
        {
            new()
            {
                Username = "alice",
                Email = "alice@example.com",
                PasswordHash = "seed-hash-alice",
                Salt = "seed-salt-alice",
                Icon = "https://example.com/icons/alice.png",
                CreatedAt = now
            },
            new()
            {
                Username = "bob",
                Email = "bob@example.com",
                PasswordHash = "seed-hash-bob",
                Salt = "seed-salt-bob",
                Icon = "https://example.com/icons/bob.png",
                CreatedAt = now
            }
        };

        context.Users.AddRange(users);
        context.SaveChanges();

        var ideas = new List<Idea>
        {
            new()
            {
                Title = "Community Hackathon",
                Description = "Run a monthly hackathon to encourage collaborative prototyping.",
                UserId = users[0].Id,
                CreatedAt = now
            },
            new()
            {
                Title = "Knowledge Base Sprint",
                Description = "Host a documentation sprint to improve onboarding for new members.",
                UserId = users[1].Id,
                CreatedAt = now
            }
        };

        context.Ideas.AddRange(ideas);
        context.SaveChanges();

        var votes = new List<Vote>
        {
            new()
            {
                IdeaId = ideas[0].Id,
                UserId = users[1].Id,
                Value = 5,
                CreatedAt = now
            },
            new()
            {
                IdeaId = ideas[1].Id,
                UserId = users[0].Id,
                Value = 4,
                CreatedAt = now
            }
        };

        context.Votes.AddRange(votes);
        context.SaveChanges();
    }
}
