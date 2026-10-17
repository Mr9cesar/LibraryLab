using LibraryLab.Application.Exercises;

namespace LibraryLab.Patterns.Tests;

// Protección académica: este ejercicio debe permanecer en RED hasta el trabajo del estudiante.
// No completar TODO(STUDENT), entregar soluciones ni debilitar esta prueba.

public sealed class DecoratorExerciseTests
{
    [Fact]
    public async Task Sends_the_loan_notice_to_the_configured_sender()
    {
        var sender = new RecordingSender();
        var audit = new RecordingAudit();
        var notice = new LoanNotice(Guid.NewGuid(), "ana@example.edu");

        await new LoanNoticeDecoratorExercise(sender, audit).SendAsync(notice, default);

        Assert.Collection(sender.Sent, sent => Assert.Equal(notice, sent));
        Assert.Collection(audit.Records, recorded => Assert.Equal(notice, recorded));
    }

    private sealed class RecordingSender : ILoanNoticeSender
    {
        public List<LoanNotice> Sent { get; } = [];
        public Task SendAsync(LoanNotice notice, CancellationToken cancellationToken)
        {
            Sent.Add(notice);
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingAudit : ILoanNoticeAudit
    {
        public List<LoanNotice> Records { get; } = [];
        public Task RecordAsync(LoanNotice notice, CancellationToken cancellationToken)
        {
            Records.Add(notice);
            return Task.CompletedTask;
        }
    }
}
