using DataAccess.Mappers;
using DataAccess.Repositories;
using DomainModel.Models;
using DomainModel.ViewModels.Reports;
using DomainModel.ViewModels.Reward;
using Framework.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
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
    public async Task Product_cards_are_scoped_to_user_and_include_points_and_validity()
    {
        await using var db = CreateDb();
        var selectedUser = NewUser("cards-user");
        var otherUser = NewUser("other-user");
        var product = new Product
        {
            ProductName = "محصول کارت",
            ProductPoint = 80,
            IsAvailable = true
        };
        var first = NewCard(product, "USER-1", true);
        var second = NewCard(product, "USER-2", true);
        var other = NewCard(product, "OTHER-1", true);
        db.AddRange(selectedUser, otherUser, product, first, second, other);
        await db.SaveChangesAsync();

        db.CardRegistrations.AddRange(
            NewRegistration(selectedUser, first, 80),
            NewRegistration(selectedUser, second, 60),
            NewRegistration(otherUser, other, 80));
        await db.SaveChangesAsync();

        var page = await new ReportRepository(db).SearchProductCards(
            new ReportActivitySearchModel
            {
                UserID = selectedUser.Id,
                PageIndex = 0,
                PageSize = 1
            });

        Assert.Equal(2, page.RecordCount);
        Assert.Equal(2, page.PageCount);
        var item = Assert.Single(page.Items);
        Assert.Equal(selectedUser.Id, item.UserID);
        Assert.True(item.AwardedPoints > 0);
        Assert.NotNull(item.RegisteredAtUtc);
        Assert.NotNull(item.PointsAwardedAtUtc);
        Assert.Equal(12, item.ValidityMonths);
        Assert.NotEqual("شروع‌نشده", item.RemainingValidityText);
    }

    [Fact]
    public async Task Reward_request_returns_profile_error_codes_for_missing_required_information()
    {
        await using var db = CreateDb();
        var user = NewUser("reward-profile-user");
        user.RemainedPoints = 1000;
        var cash = new RewardCatalog
        {
            Title = "پاداش نقدی",
            RequiredPoints = 100,
            IsActive = true,
            IsCashReward = true
        };
        var nonCash = new RewardCatalog
        {
            Title = "پاداش غیرنقدی",
            RequiredPoints = 100,
            IsActive = true,
            IsCashReward = false
        };
        db.AddRange(user, cash, nonCash);
        await db.SaveChangesAsync();

        var repository = new RewardRequestRepository(db, new ReportActivityWriter(db));

        var cashResult = await repository.CreateRequest(user.Id, cash.RewardCatalogID);
        var nonCashResult = await repository.CreateRequest(user.Id, nonCash.RewardCatalogID);

        Assert.False(cashResult.Success);
        Assert.Equal("missing_financial_profile", cashResult.ErrorCode);
        Assert.False(nonCashResult.Success);
        Assert.Equal("missing_shipping_profile", nonCashResult.ErrorCode);
    }

    [Fact]
    public void Reward_catalog_cash_rules_require_amount_and_clear_it_for_non_cash()
    {
        var cashModel = new RewardCatalogAddEditModel
        {
            Title = "نقدی",
            RequiredPoints = 1_000,
            IsCashReward = true
        };
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            cashModel,
            new ValidationContext(cashModel),
            validationResults,
            validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(validationResults, x => x.MemberNames.Contains(nameof(cashModel.CashValue)));

        var catalog = new RewardCatalog { CashValue = 4_000_000, IsCashReward = true };
        RewardCatalogMapper.Apply(catalog, new RewardCatalogAddEditModel
        {
            Title = "غیرنقدی",
            RequiredPoints = 50_000,
            IsCashReward = false,
            CashValue = 4_000_000
        });

        Assert.False(catalog.IsCashReward);
        Assert.Null(catalog.CashValue);
    }

    [Fact]
    public async Task Dashboard_summary_counts_points_and_reward_states()
    {
        await using var db = CreateDb();
        var user = NewUser("dashboard-summary");
        user.TotalEarnedPoints = 1_000;
        user.TotalSettledPoints = 400;
        user.RemainedPoints = 600;
        var deletedUser = NewUser("dashboard-deleted");
        deletedUser.IsDeleted = true;
        deletedUser.TotalEarnedPoints = 9_000;
        var pending = new RewardDeliveryStatus { Title = RewardStatusTitles.Pending };
        var approved = new RewardDeliveryStatus { Title = RewardStatusTitles.Approved };
        var rejected = new RewardDeliveryStatus { Title = RewardStatusTitles.Rejected };
        var reward = new RewardCatalog { Title = "پاداش", RequiredPoints = 100, IsActive = true };
        db.AddRange(user, deletedUser, pending, approved, rejected, reward);
        await db.SaveChangesAsync();

        db.RewardRequests.AddRange(
            NewRewardRequest(user, reward, pending, DateTime.UtcNow, false),
            NewRewardRequest(user, reward, approved, DateTime.UtcNow, false),
            NewRewardRequest(user, reward, rejected, DateTime.UtcNow, true));
        await db.SaveChangesAsync();

        var summary = await new ReportRepository(db).GetAdminDashboardSummary();

        Assert.Equal(1_000, summary.TotalEarnedPoints);
        Assert.Equal(400, summary.TotalSettledPoints);
        Assert.Equal(600, summary.TotalRemainedPoints);
        Assert.Equal(3, summary.TotalRewardRequests);
        Assert.Equal(2, summary.SettledRewardRequests);
        Assert.Equal(1, summary.PendingRewardRequests);
    }

    [Fact]
    public async Task Dashboard_pending_requests_are_filtered_and_newest_first()
    {
        await using var db = CreateDb();
        var user = NewUser("dashboard-pending");
        var pending = new RewardDeliveryStatus { Title = RewardStatusTitles.Pending };
        var approved = new RewardDeliveryStatus { Title = RewardStatusTitles.Approved };
        var reward = new RewardCatalog { Title = "پاداش", RequiredPoints = 100, IsActive = true };
        db.AddRange(user, pending, approved, reward);
        await db.SaveChangesAsync();

        db.RewardRequests.AddRange(
            NewRewardRequest(user, reward, pending, DateTime.UtcNow.AddDays(-2), false),
            NewRewardRequest(user, reward, pending, DateTime.UtcNow.AddDays(-1), false),
            NewRewardRequest(user, reward, pending, DateTime.UtcNow, true),
            NewRewardRequest(user, reward, approved, DateTime.UtcNow.AddHours(1), false));
        await db.SaveChangesAsync();

        var requests = await new ReportRepository(db).GetRecentPendingRewardRequests();

        Assert.Equal(2, requests.Count);
        Assert.True(requests[0].RequestDate >= requests[1].RequestDate);
        Assert.All(requests, x => Assert.Equal(RewardStatusTitles.Pending, x.StatusTitle));
    }

    [Fact]
    public async Task Product_popularity_without_date_filter_is_grouped_and_descending()
    {
        await using var db = CreateDb();
        var user = NewUser("dashboard-products");
        var popular = new Product { ProductName = "محصول محبوب", ProductPoint = 10, IsAvailable = true };
        var other = new Product { ProductName = "محصول دیگر", ProductPoint = 10, IsAvailable = true };
        var cards = new[]
        {
            NewCard(popular, "POPULAR-1", true),
            NewCard(popular, "POPULAR-2", true),
            NewCard(popular, "POPULAR-3", true),
            NewCard(other, "OTHER-1", true)
        };
        db.AddRange(user, popular, other);
        db.AddRange(cards);
        await db.SaveChangesAsync();
        db.CardRegistrations.AddRange(cards.Select(x => NewRegistration(user, x, 10)));
        await db.SaveChangesAsync();

        var rows = await new ReportRepository(db).GetProductPopularity(null, null);

        Assert.Equal(2, rows.Count);
        Assert.Equal("محصول محبوب", rows[0].ProductName);
        Assert.Equal(3, rows[0].Count);
        Assert.Equal(0, rows[0].JalaliYear);
        Assert.Equal(0, rows[0].JalaliMonth);
    }

    [Fact]
    public async Task Warranty_alerts_include_expired_and_thirty_day_cards_only()
    {
        await using var db = CreateDb();
        var user = NewUser("dashboard-expiry");
        var product = new Product { ProductName = "محصول", ProductPoint = 10, IsAvailable = true };
        var expired = NewCard(product, "EXPIRED", true);
        expired.ValidityMonths = 1;
        var soon = NewCard(product, "SOON", true);
        soon.ValidityMonths = 1;
        var far = NewCard(product, "FAR", true);
        far.ValidityMonths = 12;
        var unregistered = NewCard(product, "UNREGISTERED", false);
        unregistered.ValidityMonths = 1;
        db.AddRange(user, product, expired, soon, far, unregistered);
        await db.SaveChangesAsync();

        var expiredRegistration = NewRegistration(user, expired, 10);
        expiredRegistration.CreatedAt = DateTime.UtcNow.AddMonths(-2);
        var soonRegistration = NewRegistration(user, soon, 10);
        soonRegistration.CreatedAt = DateTime.UtcNow.AddMonths(-1).AddDays(15);
        var farRegistration = NewRegistration(user, far, 10);
        var unregisteredRegistration = NewRegistration(user, unregistered, 10);
        unregisteredRegistration.CreatedAt = DateTime.UtcNow.AddMonths(-2);
        db.CardRegistrations.AddRange(
            expiredRegistration,
            soonRegistration,
            farRegistration,
            unregisteredRegistration);
        await db.SaveChangesAsync();

        var alerts = await new ReportRepository(db).GetWarrantyExpiryAlerts(10, 30);

        Assert.Equal(new[] { "EXPIRED", "SOON" }, alerts.Select(x => x.SerialNumber));
        Assert.True(alerts[0].RemainingDays < 0);
        Assert.InRange(alerts[1].RemainingDays, 0, 30);
    }

    [Fact]
    public async Task Dashboard_registrars_include_profile_fields_and_registration_order()
    {
        await using var db = CreateDb();
        var province = new Province { Name = "تهران" };
        var city = new City { Name = "تهران", Province = province };
        var customerType = new CustomerType { Title = "تعمیرکار" };
        var first = NewUser("dashboard-first");
        first.FirstName = "علی";
        first.LastName = "احمدی";
        first.Province = province;
        first.City = city;
        first.UserCustomerTypes.Add(new UserCustomerType
        {
            User = first,
            CustomerType = customerType
        });
        var second = NewUser("dashboard-second");
        var product = new Product { ProductName = "محصول", ProductPoint = 10, IsAvailable = true };
        var cards = new[]
        {
            NewCard(product, "REG-1", true),
            NewCard(product, "REG-2", true),
            NewCard(product, "REG-3", true)
        };
        db.AddRange(first, second, province, city, customerType, product);
        db.AddRange(cards);
        await db.SaveChangesAsync();
        db.CardRegistrations.AddRange(
            NewRegistration(first, cards[0], 10),
            NewRegistration(first, cards[1], 10),
            NewRegistration(second, cards[2], 10));
        await db.SaveChangesAsync();

        var page = await new ReportRepository(db).GetDashboardRegistrars(0, 10);

        Assert.Equal(2, page.RecordCount);
        Assert.Equal(first.Id, page.Items[0].UserID);
        Assert.Equal(2, page.Items[0].RegistrationCount);
        Assert.Equal("تعمیرکار", page.Items[0].JobTitle);
        Assert.Equal("تهران", page.Items[0].Province);
        Assert.Equal("تهران", page.Items[0].City);
        Assert.True(page.Items[0].IsActive);
    }

    [Fact]
    public void Numeric_display_groups_money_and_points_by_three_digits()
    {
        Assert.Equal("4,000,000", 4_000_000.ToGroupedNumber());
        Assert.Equal("50,000", 50_000.ToGroupedNumber());
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

    private static RewardRequest NewRewardRequest(
        ApplicationUser user,
        RewardCatalog reward,
        RewardDeliveryStatus status,
        DateTime requestedAt,
        bool isComplete) => new()
    {
        User = user,
        UserID = user.Id,
        RewardCatalog = reward,
        RewardDeliveryStatus = status,
        RequestDate = requestedAt,
        IsComplete = isComplete
    };
}
