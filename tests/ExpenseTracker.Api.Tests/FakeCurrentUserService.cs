using ExpenseTracker.Api.Interface;

namespace ExpenseTracker.Api.Tests;

internal sealed class FakeCurrentUserService : ICurrentUserService
{
    public FakeCurrentUserService(int userId = 1) => UserId = userId;
    public int UserId { get; }
}