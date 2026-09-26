using Application.Services;
using DataAccess.Mappers;
using DataAccess.Services;
using DomainModel.Models;
using DomainModel.ViewModels.Warranty;
using Framework.Common;
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
        private readonly IMemoryCache cache;

        public WarrantyService(
            IWarrantyCardRepository cards,
            IProductRepository products,
            ICardRegistrationRepository registrations,
            IUserRepository users,
            IRewardService rewards,
            IMemoryCache cache)
        {
            this.cards = cards;
            this.products = products;
            this.registrations = registrations;
            this.users = users;
            this.rewards = rewards;
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

        public async Task<RegisterCardsResult> RegisterCards(RegisterCardsRequest request)
        {
            request ??= new RegisterCardsRequest();

            if (!TryConsumeAttempt(request.RateLimitKey, out var retryAfterSeconds))
            {
                return Fail(
                    $"لطفاً {retryAfterSeconds} ثانیه صبر کنید و سپس دوباره تلاش کنید.",
                    retryAfterSeconds);
            }

            var codes = request.Codes ?? new List<string>();
            if (codes.Count == 0)
                return Fail("اطلاعات کارت‌ها نامعتبر است.");

            if (codes.Count > MaxCardsPerRequest)
                return Fail("حداکثر ۱۰ کارت گارانتی در هر درخواست قابل ثبت است.");

            var validCards = new List<WarrantyCard>();
            var invalidCards = new List<string>();
            var seenCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < codes.Count; i++)
            {
                var scratchedCode = codes[i]?.Trim();

                if (string.IsNullOrWhiteSpace(scratchedCode))
                {
                    invalidCards.Add($"ردیف {i + 1}: رمز وارد نشده است.");
                    continue;
                }

                if (!seenCodes.Add(scratchedCode))
                {
                    invalidCards.Add($"ردیف {i + 1}: این رمز تکراری است.");
                    continue;
                }

                var (card, isAmbiguous) = await registrations.GetByScratchedCodeAsync(scratchedCode);

                if (isAmbiguous || card == null)
                {
                    invalidCards.Add($"ردیف {i + 1}: رمز وارد شده معتبر نیست.");
                    continue;
                }

                if (card.Product == null)
                {
                    invalidCards.Add($"ردیف {i + 1}: محصول مرتبط با این کارت یافت نشد.");
                    continue;
                }

                var alreadyRegistered = await registrations.IsRegisteredAsync(card.WarrantyCardID);
                if (alreadyRegistered)
                {
                    invalidCards.Add($"ردیف {i + 1}: این کارت قبلاً ثبت شده است.");
                    continue;
                }

                validCards.Add(card);
            }

            if (validCards.Count == 0)
            {
                return Fail("اطلاعات وارد شده همه کارت‌ها نامعتبر است.", failedLines: invalidCards);
            }

            var user = await users.GetByPhone(request.CustomerPhoneNumber);
            if (user == null)
            {
                var created = await users.CreateCustomer(
                    request.CustomerPhoneNumber, request.FirstName, request.LastName);
                if (!created.Success)
                    return Fail(string.IsNullOrWhiteSpace(created.Message) ? "ایجاد کاربر ناموفق بود." : created.Message);

                user = await users.GetByPhone(request.CustomerPhoneNumber);
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
                await registrations.AddRegistration(new CardRegistration
                {
                    WarrantyCardID = card.WarrantyCardID,
                    UserID = user.Id,
                    SerialNumber = card.SerialNumber,
                    ScratchedCode = card.ScratchedCode,
                    CustomerPhoneNumber = request.CustomerPhoneNumber,
                    CreatedAt = DateTime.UtcNow
                });

                card.IsRegistered = true;

                await registrations.AddTransaction(new PointTransaction
                {
                    UserID = user.Id,
                    PointsAmount = card.Product.ProductPoint,
                    PointTransactionDate = DateTime.UtcNow,
                    Description = $"ثبت کارت گارانتی {card.SerialNumber}"
                });
            }

            await registrations.SaveChangesAsync();
            await rewards.RefreshEligibility(user.Id);

            var savedCount = validCards.Count;
            var failCount = invalidCards.Count;
            var message = failCount > 0
                ? $"{savedCount} کارت ثبت شد و {failCount} کارت نامعتبر بود."
                : $"{savedCount} کارت با موفقیت ثبت شد.";

            return new RegisterCardsResult
            {
                Success = true,
                Message = message,
                SavedCount = savedCount,
                FailedLines = invalidCards
            };
        }

        private bool TryConsumeAttempt(string key, out int retryAfterSeconds)
        {
            retryAfterSeconds = 0;
            if (string.IsNullOrWhiteSpace(key))
                key = "warranty-reg:unknown";

            var now = DateTime.UtcNow;
            if (cache.TryGetValue(key, out DateTime lastAttemptUtc))
            {
                var elapsed = (int)(now - lastAttemptUtc).TotalSeconds;
                var remaining = RegisterAttemptWindowSeconds - elapsed;
                if (remaining > 0)
                {
                    retryAfterSeconds = remaining;
                    return false;
                }
            }

            cache.Set(key, now, TimeSpan.FromSeconds(RegisterAttemptWindowSeconds));
            return true;
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
