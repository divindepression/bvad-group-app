namespace BvadGroupApi.Dtos
{
    public record MyProfileDto(
        Guid UserId,
        string Username,
        string Email,
        string FirstName,
        string LastName,
        string FullName,
        string Role,
        string? PhotoUrl,
        EmployeeDto? Employee
    );
}