namespace LibraryLab.Application.Contracts;

public sealed class ConcurrencyConflictException(string message, Exception? innerException = null)
    : Exception(message, innerException);
