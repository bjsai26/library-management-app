namespace LibraryManagement.Infrastructure.Enums;

/// <summary>
/// Drives the role checks on the controllers. Members can read the catalogue;
/// admins can also change it.
/// </summary>
public enum UserRole
{
    Member = 1,
    Admin = 2
}
