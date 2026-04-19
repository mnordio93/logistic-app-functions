namespace LogisticApp.Functions.Services;

public interface IVeconLoginService
{
    Task<bool> LoginAsync(CancellationToken ct = default);
}
