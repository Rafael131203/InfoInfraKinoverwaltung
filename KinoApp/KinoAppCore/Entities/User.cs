namespace KinoAppCore.Entities;
public sealed class User
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Username { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string? RolesCsv { get; private set; }

    private User() { }
    public User(string username, string passwordHash, string? rolesCsv = null)
    {
        Username = username; PasswordHash = passwordHash; RolesCsv = rolesCsv;
    }
}
