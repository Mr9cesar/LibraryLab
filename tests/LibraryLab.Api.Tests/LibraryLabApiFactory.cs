using LibraryLab.Application.Contracts;
using LibraryLab.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace LibraryLab.Api.Tests;

// Estos repositorios prueban HTTP, validación y casos de uso reales. No prueban MySQL,
// restricciones relacionales, rollback ni concurrencia entre conexiones de base de datos.
internal sealed class LibraryLabApiFactory : WebApplicationFactory<Program>
{
    public ApiTestState State { get; } = new();

    public HttpClient CreateApiClient() => CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false
    });

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:InitializeOnStartup"] = "false"
            }));
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ILibraryUserRepository>();
            services.RemoveAll<ILibraryResourceRepository>();
            services.RemoveAll<ILoanRepository>();
            services.RemoveAll<IUnitOfWork>();
            services.AddSingleton<ILibraryUserRepository>(new ApiTestUserRepository(State));
            services.AddSingleton<ILibraryResourceRepository>(new ApiTestResourceRepository(State));
            services.AddSingleton<ILoanRepository>(new ApiTestLoanRepository(State));
            services.AddSingleton<IUnitOfWork>(new ApiTestUnitOfWork(State));
        });
    }
}

internal sealed class ApiTestState
{
    public Dictionary<Guid, LibraryUser> Users { get; } = [];
    public Dictionary<Guid, LibraryResource> Resources { get; } = [];
    public Dictionary<Guid, Loan> Loans { get; } = [];
    public int SaveCalls { get; set; }
    public int BegunTransactions { get; set; }
    public int CommittedTransactions { get; set; }
    public int DisposedTransactions { get; set; }
}

internal sealed class ApiTestUserRepository(ApiTestState state) : ILibraryUserRepository
{
    public Task AddAsync(LibraryUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        state.Users.Add(user.Id, user);
        return Task.CompletedTask;
    }

    public Task<LibraryUser?> FindAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(state.Users.GetValueOrDefault(id));

    public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        Task.FromResult(state.Users.Values.Any(user => user.Email == normalizedEmail));

    public Task<IReadOnlyList<LibraryUser>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<LibraryUser>>(state.Users.Values.OrderBy(user => user.Name).ToArray());
}

internal sealed class ApiTestResourceRepository(ApiTestState state) : ILibraryResourceRepository
{
    public Task AddAsync(LibraryResource resource, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        state.Resources.Add(resource.Id, resource);
        return Task.CompletedTask;
    }

    public Task<LibraryResource?> FindAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(state.Resources.GetValueOrDefault(id));

    public Task<Book?> FindBookAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(state.Resources.GetValueOrDefault(id) as Book);

    public Task<Equipment?> FindEquipmentAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(state.Resources.GetValueOrDefault(id) as Equipment);

    public Task<IReadOnlyList<Book>> ListBooksAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Book>>(state.Resources.Values.OfType<Book>().OrderBy(book => book.Name).ToArray());

    public Task<IReadOnlyList<Equipment>> ListEquipmentAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Equipment>>(state.Resources.Values.OfType<Equipment>()
            .OrderBy(equipment => equipment.Name).ToArray());
}

internal sealed class ApiTestLoanRepository(ApiTestState state) : ILoanRepository
{
    public Task AddAsync(Loan loan, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        state.Loans.Add(loan.Id, loan);
        return Task.CompletedTask;
    }

    public Task<Loan?> FindAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(state.Loans.GetValueOrDefault(id));

    public Task<IReadOnlyList<Loan>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Loan>>(state.Loans.Values.OrderBy(loan => loan.BorrowedAt).ToArray());
}

// Registra que los casos de uso guardan y confirman su trabajo. No emula transacciones SQL.
internal sealed class ApiTestUnitOfWork(ApiTestState state) : IUnitOfWork
{
    public Task<IApplicationTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        state.BegunTransactions++;
        return Task.FromResult<IApplicationTransaction>(new ApiTestTransaction(state));
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        state.SaveCalls++;
        return Task.CompletedTask;
    }

    private sealed class ApiTestTransaction(ApiTestState state) : IApplicationTransaction
    {
        public Task CommitAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            state.CommittedTransactions++;
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            state.DisposedTransactions++;
            return ValueTask.CompletedTask;
        }
    }
}
