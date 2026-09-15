using System.Security.Cryptography;
using System.Text;

namespace LibraryManagement.AppServices.Security;

/// <summary>Hashes and verifies account passwords.</summary>
public interface IPasswordHasher
{
    (string Hash, string Salt) Hash(string password);

    bool Verify(string password, string hash, string salt);
}

/// <summary>
/// PBKDF2-SHA256 hashing built on the .NET base class library, so the project needs no
/// third-party hashing dependency.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public (string Hash, string Salt) Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Derive(password, salt);

        return (Convert.ToBase64String(key), Convert.ToBase64String(salt));
    }

    public bool Verify(string password, string hash, string salt)
    {
        if (string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(hash) ||
            string.IsNullOrWhiteSpace(salt))
        {
            return false;
        }

        byte[] saltBytes;
        byte[] expected;

        try
        {
            saltBytes = Convert.FromBase64String(salt);
            expected = Convert.FromBase64String(hash);
        }
        catch (FormatException)
        {
            // A stored value that is not valid Base64 can never match.
            return false;
        }

        // Fixed-time comparison keeps the check free of timing side channels.
        return CryptographicOperations.FixedTimeEquals(Derive(password, saltBytes), expected);
    }

    private static byte[] Derive(string password, byte[] salt)
        => Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, Iterations, Algorithm, KeySize);
}
