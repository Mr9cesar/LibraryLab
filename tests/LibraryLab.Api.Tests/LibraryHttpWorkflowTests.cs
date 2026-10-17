using System.Net;
using System.Net.Http.Json;
using LibraryLab.Application.Services;
using LibraryLab.Domain.Enums;

namespace LibraryLab.Api.Tests;

public sealed class LibraryHttpWorkflowTests
{
    [Fact]
    public async Task Registers_users_books_and_equipment_and_exposes_them_over_http()
    {
        await using var factory = new LibraryLabApiFactory();
        using var client = factory.CreateApiClient();

        var user = await PostCreatedAsync<UserView>(client, "/api/users",
            new { name = "  Ana  ", email = "ANA@librarylab.local" });
        var book = await PostCreatedAsync<BookView>(client, "/api/books",
            new { isbn = "978-958-00-0010-1", title = "El libro de prueba", author = "Autora de prueba" });
        var equipment = await PostCreatedAsync<EquipmentView>(client, "/api/equipment",
            new { inventoryCode = "EQ-HTTP-001", name = "Tablet de prueba", type = EquipmentType.Tablet });

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("Ana", user.Name);
        Assert.Equal("ana@librarylab.local", user.Email);
        Assert.Equal(LibraryUserStatus.Active, user.Status);
        Assert.NotEqual(Guid.Empty, book.Id);
        Assert.Equal("978-958-00-0010-1", book.Isbn);
        Assert.Equal("El libro de prueba", book.Title);
        Assert.Equal("Autora de prueba", book.Author);
        Assert.True(book.IsAvailable);
        Assert.NotEqual(Guid.Empty, equipment.Id);
        Assert.Equal("EQ-HTTP-001", equipment.InventoryCode);
        Assert.Equal(EquipmentType.Tablet, equipment.Type);
        Assert.True(equipment.IsAvailable);

        Assert.Equal(user, Assert.Single(Assert.IsType<UserView[]>(await client.GetFromJsonAsync<UserView[]>("/api/users"))));
        Assert.Equal(book, Assert.Single(Assert.IsType<BookView[]>(await client.GetFromJsonAsync<BookView[]>("/api/books"))));
        Assert.Equal(equipment, Assert.Single(Assert.IsType<EquipmentView[]>(await client.GetFromJsonAsync<EquipmentView[]>("/api/equipment"))));
        Assert.Equal(book, await client.GetFromJsonAsync<BookView>($"/api/books/{book.Id}"));
        Assert.Equal(equipment, await client.GetFromJsonAsync<EquipmentView>($"/api/equipment/{equipment.Id}"));
        Assert.Equal(3, factory.State.SaveCalls);
    }

    [Theory]
    [InlineData(false, ResourceKind.Book)]
    [InlineData(true, ResourceKind.Equipment)]
    public async Task Loan_and_return_change_availability_and_reject_repeated_operations(
        bool useEquipment, ResourceKind expectedKind)
    {
        await using var factory = new LibraryLabApiFactory();
        using var client = factory.CreateApiClient();
        var user = await PostCreatedAsync<UserView>(client, "/api/users",
            new { name = "Luis", email = "luis@librarylab.local" });
        var resourceId = await CreateResourceAsync(client, useEquipment);
        var dueAt = DateTimeOffset.UtcNow.AddDays(3);
        var command = new { userId = user.Id, resourceId, dueAt };

        var loan = await PostCreatedAsync<LoanView>(client, "/api/loans", command);
        Assert.Equal(user.Id, loan.UserId);
        Assert.Equal(resourceId, loan.ResourceId);
        Assert.Equal(expectedKind, loan.ResourceKind);
        Assert.Equal(dueAt, loan.DueAt);
        Assert.Equal(LoanStatus.Active, loan.Status);
        Assert.Null(loan.ReturnedAt);
        Assert.False(loan.IsOverdue);
        await AssertAvailabilityAsync(client, useEquipment, resourceId, false);

        using var duplicateLoan = await client.PostAsJsonAsync("/api/loans", command);
        Assert.Equal(HttpStatusCode.Conflict, duplicateLoan.StatusCode);
        Assert.Equal(loan, await client.GetFromJsonAsync<LoanView>($"/api/loans/{loan.Id}"));
        Assert.Equal(loan, Assert.Single(Assert.IsType<LoanView[]>(await client.GetFromJsonAsync<LoanView[]>("/api/loans"))));

        using var returnResponse = await client.PostAsJsonAsync($"/api/loans/{loan.Id}/return", new { });
        Assert.Equal(HttpStatusCode.OK, returnResponse.StatusCode);
        var returnedLoan = Assert.IsType<LoanView>(await returnResponse.Content.ReadFromJsonAsync<LoanView>());
        Assert.Equal(loan.Id, returnedLoan.Id);
        Assert.Equal(LoanStatus.Returned, returnedLoan.Status);
        Assert.NotNull(returnedLoan.ReturnedAt);
        await AssertAvailabilityAsync(client, useEquipment, resourceId, true);

        using var repeatedReturn = await client.PostAsJsonAsync($"/api/loans/{loan.Id}/return", new { });
        Assert.Equal(HttpStatusCode.Conflict, repeatedReturn.StatusCode);
        Assert.Equal(returnedLoan, await client.GetFromJsonAsync<LoanView>($"/api/loans/{loan.Id}"));
        Assert.Single(factory.State.Loans);
        Assert.Equal(4, factory.State.SaveCalls);
        Assert.Equal(2, factory.State.CommittedTransactions);
        Assert.Equal(factory.State.BegunTransactions, factory.State.DisposedTransactions);
    }

    [Fact]
    public async Task Duplicate_normalized_user_email_returns_conflict_without_adding_a_user()
    {
        await using var factory = new LibraryLabApiFactory();
        using var client = factory.CreateApiClient();
        await PostCreatedAsync<UserView>(client, "/api/users",
            new { name = "Ana", email = "ana@librarylab.local" });

        using var response = await client.PostAsJsonAsync("/api/users",
            new { name = "Otra persona", email = "ANA@LIBRARYLAB.LOCAL" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Single(factory.State.Users);
        Assert.Equal(1, factory.State.SaveCalls);
    }

    [Theory]
    [InlineData("books", false)]
    [InlineData("equipment", false)]
    [InlineData("loans", false)]
    [InlineData("loans", true)]
    public async Task Unknown_resource_or_loan_returns_not_found(string route, bool returnLoan)
    {
        await using var factory = new LibraryLabApiFactory();
        using var client = factory.CreateApiClient();
        var url = $"/api/{route}/{Guid.NewGuid()}";
        using var response = returnLoan
            ? await client.PostAsJsonAsync(url + "/return", new { })
            : await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(0, factory.State.SaveCalls);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Loan_with_unknown_user_or_resource_returns_not_found(bool unknownUser)
    {
        await using var factory = new LibraryLabApiFactory();
        using var client = factory.CreateApiClient();
        var user = await PostCreatedAsync<UserView>(client, "/api/users",
            new { name = "Ana", email = "ana@librarylab.local" });
        var resourceId = await CreateResourceAsync(client, false);

        using var response = await client.PostAsJsonAsync("/api/loans", new
        {
            userId = unknownUser ? Guid.NewGuid() : user.Id,
            resourceId = unknownUser ? resourceId : Guid.NewGuid(),
            dueAt = DateTimeOffset.UtcNow.AddDays(2)
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Empty(factory.State.Loans);
        await AssertAvailabilityAsync(client, false, resourceId, true);
        Assert.Equal(0, factory.State.CommittedTransactions);
    }

    [Theory]
    [InlineData("empty")]
    [InlineData("userId")]
    [InlineData("resourceId")]
    [InlineData("missingDueAt")]
    [InlineData("pastDueAt")]
    public async Task Invalid_loan_request_is_rejected_before_entering_a_transaction(string invalidField)
    {
        await using var factory = new LibraryLabApiFactory();
        using var client = factory.CreateApiClient();
        var request = new Dictionary<string, object>();
        if (invalidField != "empty")
        {
            request["userId"] = invalidField == "userId" ? Guid.Empty : Guid.NewGuid();
            request["resourceId"] = invalidField == "resourceId" ? Guid.Empty : Guid.NewGuid();
            if (invalidField != "missingDueAt")
                request["dueAt"] = DateTimeOffset.UtcNow.AddDays(invalidField == "pastDueAt" ? -1 : 2);
        }

        using var response = await client.PostAsJsonAsync("/api/loans", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, factory.State.BegunTransactions);
        Assert.Equal(0, factory.State.SaveCalls);
        Assert.Empty(factory.State.Loans);
    }

    [Theory]
    [InlineData("/api/users")]
    [InlineData("/api/equipment")]
    public async Task Missing_registration_fields_return_bad_request(string route)
    {
        await using var factory = new LibraryLabApiFactory();
        using var client = factory.CreateApiClient();

        using var response = await client.PostAsJsonAsync(route, new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, factory.State.SaveCalls);
    }

    private static async Task<T> PostCreatedAsync<T>(HttpClient client, string path, object body)
    {
        using var response = await client.PostAsJsonAsync(path, body);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        return Assert.IsType<T>(await response.Content.ReadFromJsonAsync<T>());
    }

    private static async Task<Guid> CreateResourceAsync(HttpClient client, bool useEquipment)
    {
        if (useEquipment)
        {
            var equipment = await PostCreatedAsync<EquipmentView>(client, "/api/equipment",
                new { inventoryCode = "EQ-LOAN-001", name = "Portátil", type = EquipmentType.Laptop });
            return equipment.Id;
        }

        var book = await PostCreatedAsync<BookView>(client, "/api/books",
            new { isbn = "978-958-00-0020-1", title = "Libro para préstamo", author = "Autora" });
        return book.Id;
    }

    private static async Task AssertAvailabilityAsync(HttpClient client, bool useEquipment, Guid id, bool expected)
    {
        if (useEquipment)
        {
            var resource = Assert.IsType<EquipmentView>(await client.GetFromJsonAsync<EquipmentView>($"/api/equipment/{id}"));
            Assert.Equal(expected, resource.IsAvailable);
        }
        else
        {
            var resource = Assert.IsType<BookView>(await client.GetFromJsonAsync<BookView>($"/api/books/{id}"));
            Assert.Equal(expected, resource.IsAvailable);
        }
    }
}
