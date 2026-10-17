using LibraryLab.Application.Contracts;
using LibraryLab.Application.Services;
using LibraryLab.Domain.Entities;
using LibraryLab.Domain.Enums;

namespace LibraryLab.Application.Exercises;

// TODO(STUDENT): Este contrato pertenece al ejercicio Factory Method. No está implementado por diseño.
public interface ILoanFactoryExercise
{
    Loan Create(CreateLoanCommand command, ResourceKind resourceKind);
}

public sealed class LoanFactoryExercise : ILoanFactoryExercise
{
    public Loan Create(CreateLoanCommand command, ResourceKind resourceKind)
    {
        return new Loan(
            Guid.NewGuid(),
            command.UserId,
            command.ResourceId,
            resourceKind,
            DateTimeOffset.UtcNow,
            command.DueAt
        );
    }
}


// TODO(STUDENT): Este contrato pertenece al ejercicio Builder. No está implementado por diseño.
public interface ILoanReceiptBuilderExercise
{
    LoanReceipt Build(Guid loanId, Guid resourceId, DateTimeOffset dueAt);
}

public sealed class LoanReceiptBuilderExercise : ILoanReceiptBuilderExercise
{
    public LoanReceipt Build(Guid loanId, Guid resourceId, DateTimeOffset dueAt)
    {
        return new LoanReceipt(loanId, resourceId, dueAt);
    }
}

public sealed record LegacyCatalogRecord(string InventoryCode, string DisplayName, string Category);
public sealed record CatalogImport(Guid Id, string InventoryCode, string Name, string Category);

// TODO(STUDENT): Este contrato pertenece al ejercicio Adapter. No está implementado por diseño.
public interface ILegacyCatalogAdapterExercise
{
    CatalogImport Adapt(LegacyCatalogRecord record);
}

public sealed class LegacyCatalogAdapterExercise : ILegacyCatalogAdapterExercise
{
    public CatalogImport Adapt(LegacyCatalogRecord record)
    {
        return new CatalogImport(
            Guid.NewGuid(),
            record.InventoryCode,
            record.DisplayName,
            record.Category
        );
    }
}

public sealed record LoanReceipt(Guid LoanId, Guid ResourceId, DateTimeOffset DueAt);
public sealed record LoanNotice(Guid LoanId, string Recipient);

public interface ILoanNoticeSender
{
    Task SendAsync(LoanNotice notice, CancellationToken cancellationToken);
}

public interface ILoanNoticeAudit
{
    Task RecordAsync(LoanNotice notice, CancellationToken cancellationToken);
}

// TODO(STUDENT): Este contrato pertenece al ejercicio Decorator. No está implementado por diseño.
public sealed class LoanNoticeDecoratorExercise(ILoanNoticeSender sender, ILoanNoticeAudit audit) : ILoanNoticeSender
{
    public async Task SendAsync(LoanNotice notice, CancellationToken cancellationToken)
    {
        await sender.SendAsync(notice, cancellationToken);
        await audit.RecordAsync(notice, cancellationToken);
    }
}


public sealed record MemberOverview(Guid UserId, string UserName, IReadOnlyList<LoanReceipt> ActiveLoans);

// TODO(STUDENT): Este contrato pertenece al ejercicio Facade. No está implementado por diseño.
public interface IMemberOverviewFacadeExercise
{
    Task<MemberOverview> GetAsync(Guid userId, CancellationToken cancellationToken);
}


public sealed class MemberOverviewFacadeExercise(
    ILibraryUserRepository users,
    ILoanRepository loans,
    ILibraryResourceRepository resources) : IMemberOverviewFacadeExercise
{
    public async Task<MemberOverview> GetAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await users.FindAsync(userId, cancellationToken);

        if (user is null)
        {
            throw new ApplicationNotFoundException(
                $"No se encontró el usuario con identificador {userId}.");
        }

        var allLoans = await loans.ListAsync(cancellationToken);
        var activeLoans = new List<LoanReceipt>();

        foreach (var loan in allLoans)
        {
            if (loan.UserId != userId || loan.Status != LoanStatus.Active)
            {
                continue;
            }

            var resource = await resources.FindAsync(
                loan.ResourceId,
                cancellationToken);

            if (resource is not null)
            {
                activeLoans.Add(new LoanReceipt(
                    loan.Id,
                    loan.ResourceId,
                    loan.DueAt));
            }
        }

        return new MemberOverview(
            user.Id,
            user.Name,
            activeLoans);
    }
}

