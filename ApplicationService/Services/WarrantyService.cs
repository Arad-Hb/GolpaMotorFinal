using Application.Services;
using DataAccess.Mappers;
using DataAccess.Services;
using DomainModel.Models;
using DomainModel.ViewModels.Warranty;
using Framework.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ApplicationService.Services
{
    public class WarrantyService : IWarrantyService
    {
        private const int MaxCardsPerRequest = 10;
        private const int RegisterAttemptWindowSeconds = 60;

        private readonly IWarrantyCardRepository cards;
        private readonly IProductRepository products;
        private readonly ICardRegistrationRepository registrations;
        private readonly IUserRepository users;
        private readonly IRewardService rewards;
        private readonly IReportActivityWriter reportActivities;
        private readonly IMemoryCache cache;

        public WarrantyService(
            IWarrantyCardRepository cards,
            IProductRepository products,
            ICardRegistrationRepository registrations,
            IUserRepository users,
            IRewardService rewards,
            IReportActivityWriter reportActivities,
            IMemoryCache cache)
        {
            this.cards = cards;
            this.products = products;
            this.registrations = registrations;
            this.users = users;
            this.rewards = rewards;
            this.reportActivities = reportActivities;
            this.cache = cache;
        }

        public async Task<(List<WarrantyCardListItem> Items, int PageIndex, int PageCount, int RecordCount)> SearchCards(WarrantyCardSearchModel sm)
        {
            sm ??= new WarrantyCardSearchModel();
            var search = await cards.SearchAsync(sm);
            var pageSize = sm.PageSize <= 0 ? 10 : sm.PageSize;
            var pageCount = pageSize <= 0 ? 1 : (int)Math.Ceiling(search.Total / (double)pageSize);
            return (search.Items, sm.PageIndex, pageCount, search.Total);
        }

        public async Task<OperationResult> GenerateCodes(long productId, int count, int validityMonths = 12)
        {
            var op = new OperationResult("GenerateWarrantyCodes");
            if (productId <= 0 || count <= 0)
                return op.ToFailed("محصول و تعداد معتبر نیست.");
            if (count > 200)
                count = 200;

            var created = 0;
            for (var i = 0; i < count; i++)
            {
                string serial;
                do
                {
                    serial = WarrantyCodeGenerator.Serial();
                }
                while (await cards.SerialExistsAsync(serial));

                await cards.AddAsync(WarrantyCardMapper.ToEntity(
                    productId, serial, WarrantyCodeGenerator.ScratchedCode(), validityMonths));
                created++;
            }

            await cards.SaveAsync();
            return op.ToSuccess($"{created} کد گارانتی تولید شد.");
        }

        public async Task<WarrantyImportResult> ImportCards(long productId, IReadOnlyList<WarrantyCardImportItem> items)
        {
            var result = new WarrantyImportResult();
            if (productId <= 0 || !await products.Exists(productId))
            {
                result.Message = "محصول معتبر نیست.";
                return result;
            }

            items ??= Array.Empty<WarrantyCardImportItem>();
            var existing = await cards.GetSerialsAsync();
            var batchSerials = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var list = new List<WarrantyCard>();

            foreach (var item in items)
            {
                var serial = item.SerialNumber?.Trim();
                var code = item.ScratchedCode?.Trim();

                if (string.IsNullOrWhiteSpace(serial) && string.IsNullOrWhiteSpace(code) && !item.ValidityMonths.HasValue)
                {
                    result.Empty++;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(serial) || string.IsNullOrWhiteSpace(code))
                {
                    result.Empty++;
                    continue;
                }

                var months = item.ValidityMonths ?? 12;
                if (months <= 0 || months > 60)
                {
                    result.Invalid++;
                    continue;
                }

                if (existing.Contains(serial) || batchSerials.Contains(serial))
                {
                    result.Duplicate++;
                    continue;
                }

                batchSerials.Add(serial);
                list.Add(WarrantyCardMapper.ToEntity(productId, serial, code, months));
            }

            if (list.Count > 0)
            {
                cards.AddRange(list);
                await cards.SaveAsync();
            }

            result.Inserted = list.Count;
            result.Success = true;
            result.Message =
                $"{result.Inserted} کارت درج شد، {result.Duplicate} تکراری، {result.Empty} خالی و {result.Invalid} نامعتبر نادیده گرفته شد.";
            return result;
        }

        public async Task<(bool Ok, string Message)> CheckScratchCode(string? code)
        {
            var scratchedCode = code?.Trim();
            if (string.IsNullOrWhiteSpace(scratchedCode))
                return (false, "لطفاً رمز را وارد کنید.");

            var (card, isAmbiguous) = await registrations.GetByScratchedCodeAsync(scratchedCode);
            if (isAmbiguous || card == null)
                return (false, "این رمز در سامانه وجود ندارد.");

            if (card.Product == null)
                return (false, "محصول مرتبط با این کارت یافت نشد.");

            if (await registrations.IsRegisteredAsync(card.WarrantyCardID))
                return (false, "این کارت قبلاً ثبت شده است.");

            return (true, "رمز معتبر است.");
        }

        public async Task<RegisterCardsResult> RegisterCards(RegisterCardsRequest request)
        {
            request ??= new RegisterCardsRequest();

            var phone = IranianMobileNumber.Normalize(request.CustomerPhoneNumber);
            if (phone == null)
                return Fail("شماره موبایل معتبر نیست.");
            request.CustomerPhoneNumber = phone;

            var codes = (request.Codes ?? new List<string>())
                .Select((code, index) => (Code: code?.Trim(), Row: index + 1))
                .Where(x => !string.IsNullOrWhiteSpace(x.Code))
                .ToList();

            if (codes.Count == 0)
                return Fail("رمز اجباری است.");

            if (codes.Count > MaxCardsPerRequest)
                return Fail("حداکثر ۱۰ کارت گارانتی در هر درخواست قابل ثبت است.");

            var validCards = new List<WarrantyCard>();
            var invalidCards = new List<string>();
            var seenCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in codes)
            {
                var scratchedCode = item.Code!;

                if (!seenCodes.Add(scratchedCode))
                {
                    invalidCards.Add($"ردیف {item.Row}: این رمز تکراری است.");
                    continue;
                }

                var (card, isAmbiguous) = await registrations.GetByScratchedCodeAsync(scratchedCode);

                if (isAmbiguous || card == null)
                {
                    invalidCards.Add($"ردیف {item.Row}: رمز وارد شده معتبر نیست.");
                    continue;
                }

                if (card.Product == null)
                {
                    invalidCards.Add($"ردیف {item.Row}: محصول مرتبط با این کارت یافت نشد.");
                    continue;
                }

                var alreadyRegistered = await registrations.IsRegisteredAsync(card.WarrantyCardID);
                if (alreadyRegistered)
                {
                    invalidCards.Add($"ردیف {item.Row}: این کارت قبلاً ثبت شده است.");
                    continue;
                }

                validCards.Add(card);
            }

            if (invalidCards.Count > 0)
            {
                return Fail(
                    validCards.Count == 0
                        ? "اطلاعات وارد شده همه کارت‌ها نامعتبر است."
                        : "هیچ کارتی ثبت نشد. لطفاً رمزهای نامعتبر را اصلاح کنید.",
                    failedLines: invalidCards);
            }

            if (IsRateLimited(request.RateLimitKey, out var retryAfterSeconds))
            {
                return Fail(
                    $"لطفاً {retryAfterSeconds} ثانیه صبر کنید و سپس دوباره تلاش کنید.",
                    retryAfterSeconds);
            }

            var user = await users.GetByPhone(phone);
            if (user == null)
            {
                var created = await users.CreateCustomer(phone, request.FirstName, request.LastName);
                if (!created.Success)
                    return Fail(string.IsNullOrWhiteSpace(created.Message) ? "ایجاد کاربر ناموفق بود." : created.Message);

                user = await users.GetByPhone(phone);
                if (user == null)
                    return Fail("ایجاد کاربر ناموفق بود.");
            }

            await users.EnsureCustomerRole(user.Id);

            if (request.CustomerTypeId.HasValue)
            {
                var alreadyLinked = await registrations.IsCardAlreadyRegisteredByUserAsync(
                    request.CustomerTypeId.Value, user.Id);

                if (!alreadyLinked)
                {
                    await registrations.AddUserCustomerType(new UserCustomerType
                    {
                        UserID = user.Id,
                        CustomerTypeID = request.CustomerTypeId.Value
                    });
                }
            }

            foreach (var card in validCards)
            {
                var occurredAtUtc = DateTime.UtcNow;
                var registration = new CardRegistration
                {
                    WarrantyCardID = card.WarrantyCardID,
                    UserID = user.Id,
                    SerialNumber = card.SerialNumber,
                    ScratchedCode = card.ScratchedCode,
                    CustomerPhoneNumber = phone,
                    CreatedAt = occurredAtUtc,
                    EarnedPionts = card.Product.ProductPoint,
                    IsApproved = true
                };
                await registrations.AddRegistration(registration);

                card.IsRegistered = true;

                var pointTransaction = new PointTransaction
                {
                    UserID = user.Id,
                    PointsAmount = card.Product.ProductPoint,
                    PointTransactionDate = occurredAtUtc,
                    Description = $"ثبت کارت گارانتی {card.SerialNumber}"
                };
                await registrations.AddTransaction(pointTransaction);
                await reportActivities.AddCardRegisteredAsync(
                    user.Id, card, registration, pointTransaction);
            }

            try
            {
                await registrations.SaveChangesAsync();
                await reportActivities.FinalizeSourceKeysAsync();
            }
            catch (DbUpdateException)
            {
                return Fail("ثبت کارت انجام نشد. ممکن است یکی از کارت‌ها قبلاً ثبت شده باشد.");
            }

            RememberAttempt(request.RateLimitKey);
            await rewards.RefreshEligibility(user.Id);

            return new RegisterCardsResult
            {
                Success = true,
                Message = $"{validCards.Count} کارت با موفقیت ثبت شد.",
                SavedCount = validCards.Count
            };
        }

        private bool IsRateLimited(string key, out int retryAfterSeconds)
        {
            retryAfterSeconds = 0;
            if (string.IsNullOrWhiteSpace(key))
                key = "warranty-reg:unknown";

            var now = DateTime.UtcNow;
            if (!cache.TryGetValue(key, out DateTime lastAttemptUtc))
                return false;

            var remaining = RegisterAttemptWindowSeconds - (int)(now - lastAttemptUtc).TotalSeconds;
            if (remaining <= 0)
                return false;

            retryAfterSeconds = remaining;
            return true;
        }

        private void RememberAttempt(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                key = "warranty-reg:unknown";

            cache.Set(key, DateTime.UtcNow, TimeSpan.FromSeconds(RegisterAttemptWindowSeconds));
        }

        private static RegisterCardsResult Fail(string message, int? retryAfterSeconds = null, List<string>? failedLines = null)
        {
            return new RegisterCardsResult
            {
                Success = false,
                Message = message,
                RetryAfterSeconds = retryAfterSeconds,
                FailedLines = failedLines ?? new List<string>()
            };
        }
    }
}
