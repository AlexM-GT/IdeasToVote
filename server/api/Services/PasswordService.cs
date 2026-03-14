using System.Security.Cryptography;

namespace IdeasToVote.Api.Services;

public class PasswordService : IPasswordService
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 10000;

    public (string Hash, string Salt) HashPassword(string password)
    {
        using var salt = new Rfc2898DeriveBytes(password, SaltSize, Iterations, HashAlgorithmName.SHA256);
        var saltBytes = salt.Salt;
        var hash = salt.GetBytes(KeySize);

        var saltString = Convert.ToBase64String(saltBytes);
        var hashString = Convert.ToBase64String(hash);

        return (hashString, saltString);
    }

    public bool VerifyPassword(string password, string hash, string salt)
    {
        try
        {
            var saltBytes = Convert.FromBase64String(salt);
            var hashBytes = Convert.FromBase64String(hash);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256);
            var computedHash = pbkdf2.GetBytes(KeySize);

            return CryptographicOperations.FixedTimeEquals(computedHash, hashBytes);
        }
        catch
        {
            return false;
        }
    }
}
