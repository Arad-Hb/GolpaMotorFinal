using DataAccess.Repositories;
using DomainModel.Models;
using DomainModel.ViewModels.Reports;
using Framework.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GolpaMotorFinal.Tests;

public class UnifiedReportTests
{
    [Fact]
    public async Task Product_summary_counts_cards_distinct_customers_points_and_requests()
    {
        await using var db = CreateDb();
        var user = NewUser("u1");
        var product = new Product
        {
            ProductName = "محصول",
            ProductPoint = 100,
            IsAvailable = true,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-10)
        };
        var first = NewCard(product, "S1", true);
        var second = NewCard(product, "S2", true);
        var free = NewCard(product, "S3", false);
        db.AddRange(user, product, first, second, free);
        await db.SaveChangesAsync();

        db.CardRegistrations.AddRange(
            NewRegistration(user, first, 100),
            NewRegistration(user, second, 100));
        var pending = new RewardDeliveryStatus { Title = RewardStatusTitles.Pending };
        var catalog = new RewardCatalog
        {
            Title = "جایزه",
            RequiredPoints = 100,
            IsActive = true
        };
        db.AddRange(pending, catalog);
        await db.SaveChangesAsync();
        db.RewardRequests.Add(new RewardRequest
        {
            UserID = user.Id,
            RewardCatalogID = catalog.RewardCatalogID,
            RewardDeliveryStatusID = pending.RewardDeliveryStatusID,
            RequestDate = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var page = await new ReportRepository(db).SearchProductWarranty(
            new ProductWarrantyReportSearchModel { PageSize = 50 });

        var row = Assert.Single(page.Items);
        Assert.Equal(3, row.TotalCards);
        Assert.Equal(2, row.RegisteredCards);
        Assert.Equal(1, row.UnregisteredCards);
        Assert.Equal(1, row.UniqueCustomers);
        Assert.Equal(200, row.AwardedPoints);
        Assert.Equal(1, row.CustomersWithRewardRequest);
        Assert.Equal(1, row.RewardRequestCount);
    }

    [Fact]
    public async Task Activity_writer_records_running_balance_for_registration()
    {
        await using var db = CreateDb();
        var user = NewUser("u2");
        var product = new Product
        {
            ProductName = "محصول",
            ProductPoint = 75,
            IsAvailable = true
        };
        var card = NewCard(product, "S4", true);
        db.AddRange(user, product, card);
        db.PointTransactions.Add(new PointTransaction
        {
            UserID = user.Id,
            PointsAmount = 25,
            PointTransactionDate = DateTime.UtcNow.AddDays(-1)
        });
        await db.SaveChangesAsync();

        var registration = NewRegistration(user, card, 75);
        var transaction = new PointTransaction
        {
            UserID = user.Id,
            PointsAmount = 75,
            PointTransactionDate = registration.CreatedAt
        };
        db.Add(registration);
        db.Add(transaction);

        await new ReportActivityWriter(db).AddCardRegisteredAsync(
            user.Id, card, registration, transaction);
        await db.SaveChangesAsync();

        var log = Assert.Single(db.ReportActivityLogs);
        Assert.Equal(ReportActivityTypes.CardRegistered, log.ActivityType);
        Assert.Equal(75, log.PointsDelta);
        Assert.Equal(100, log.TotalEarnedPoints);
        Assert.Equal(100, log.RemainedPoints);
        Assert.Equal(25, transaction.PointsBeforeTransaction);
        Assert.Equal(100, transaction.PointsAfterTransaction);
        Assert.Equal(card.WarrantyCardID, log.WarrantyCardID);
        Assert.NotNull(log.CardRegistrationID);
        Assert.NotNull(log.PointTransactionID);
    }

    [Fact]
    public async Task Activity_query_applies_entity_filter_and_stable_pagination()
    {
        await using var db = CreateDb();
        var user = NewUser("u3");
        db.Users.Add(user);
        await db.SaveChangesAsync();

        for (var i = 0; i < 3; i++)
        {
            db.ReportActivityLogs.Add(new ReportActivityLog
            {
                SourceKey = $"test:{i}",
                ActivityType = ReportActivityTypes.RewardRequested,
                OccurredAtUtc = DateTime.UtcNow.AddMinutes(i),
                UserID = user.Id,
                StatusTitle = RewardStatusTitles.Pending
            });
        }
        await db.SaveChangesAsync();

        var page = await new ReportRepository(db).SearchActivities(
            new ReportActivitySearchModel
            {
                UserID = user.Id,
                ActivityType = ReportActivityTypes.RewardRequested,
                PageIndex = 1,
                PageSize = 2
            });

        Assert.Equal(3, page.RecordCount);
        Assert.Equal(2, page.PageCount);
        Assert.Single(page.Items);
    }

    [Fact]
    public void Jalali_range_is_half_open_and_displays_in_iran_time()
    {
        var from = "1405/07/04".JalaliStartOfDayUtc();
        var to = "1405/07/04".JalaliEndExclusiveUtc();

        Assert.NotNull(from);
        Assert.NotNull(to);
        Assert.Equal(TimeSpan.FromDays(1), to - from);
        Assert.Equal("1405/07/04", from!.Value.ToIranTime().ToPersianDate());
    }

    [Fact]
    [Trait("Category", "LocalSqlServer")]
    public async Task Product_card_details_query_executes_on_configured_sql_server_when_enabled()
    {
        if (Environment.GetEnvironmentVariable("GOLPA_SQL_INTEGRATION") != "1")
            return;

        var options = new DbContextOptionsBuilder<GolpaMotorDbContext>()
            .UseSqlServer(
                "Data Source=.;Initial Catalog=GolpaMotorFinal;Integrated Security=True;Trust Server Certificate=True")
            .Options;
        await using var db = new GolpaMotorDbContext(options);
        var productId = await db.Products
            .Where(x => !x.IsDeleted && x.WarrantyCards.Any())
            .Select(x => x.ProductID)
            .FirstAsync();

        var page = await new ReportRepository(db).SearchProductCards(
            new ReportActivitySearchModel
            {
                ProductID = productId,
                PageSize = 50
            });

        Assert.True(page.RecordCount > 0);
        Assert.NotEmpty(page.Items);
        Assert.All(page.Items, x => Assert.Equal(productId, x.ProductID));
    }

    private static GolpaMotorDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<GolpaMotorDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        var db = new GolpaMotorDbContext(options);
        db.Database.OpenConnection();
        db.Database.EnsureCreated();
        return db;
    }

    private static ApplicationUser NewUser(string id) => new()
    {
        Id = id,
        UserName = id,
        PhoneNumber = $"09{id.GetHashCode():000000000}",
        FirstName = "کاربر",
        IsActive = true
    };

    private static WarrantyCard NewCard(Product product, string serial, bool registered) => new()
    {
        Product = product,
        SerialNumber = serial,
        ScratchedCode = $"CODE-{serial}",
        ValidityMonths = 12,
        IsRegistered = registered,
        IssuedAtUtc = DateTime.UtcNow.AddDays(-2),
        ProductAssignedAtUtc = DateTime.UtcNow.AddDays(-2)
    };

    private static CardRegistration NewRegistration(
        ApplicationUser user,
        WarrantyCard card,
        int points) => new()
    {
        User = user,
        UserID = user.Id,
        WarrantyCard = card,
        WarrantyCardID = card.WarrantyCardID,
        SerialNumber = card.SerialNumber,
        ScratchedCode = card.ScratchedCode,
        CustomerPhoneNumber = user.PhoneNumber!,
        CreatedAt = DateTime.UtcNow,
        EarnedPionts = points,
        IsApproved = true
    };
}
