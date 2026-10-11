// KeyGen — offline license-key minting tool for St. John Pictures apps.
// Shaun runs this to issue keys to buyers. Never distribute this tool or
// Licensing/LicenseSecrets.cs: anyone holding a secret can mint keys.
//
// Usage:
//   KeyGen --app <id> --email <buyer email> [--months N | --perpetual]
//   KeyGen --selftest
//
// App ids: daypilot, bizpilot, wuse, wuse-business, claimpilot, claimpilot-plus, legal-matters
// Default is 1 month. --perpetual issues a non-expiring key.

using StJohn.Licensing;

if (args.Any(a => a is "--selftest" or "-t"))
{
    return SelfTest.Run() ? 0 : 1;
}

var opts = ParseArgs(args);
if (opts is null)
{
    PrintUsage();
    return 2;
}

var product = ProductFromId(opts.AppId);
var secret = LicenseSecrets.ForProduct(product);
var issued = DateTime.UtcNow;
DateTime? expiry = opts.Perpetual ? null : issued.AddMonths(opts.Months);

var key = LicenseCore.GenerateKey(secret, product, opts.Email, issued, expiry);

Console.WriteLine($"Product : {product} ({opts.AppId})");
Console.WriteLine($"Email   : {opts.Email}");
Console.WriteLine($"Issued  : {issued:yyyy-MM-dd}");
Console.WriteLine($"Expires : {(expiry is null ? "perpetual" : $"{expiry:yyyy-MM-dd}")}");
Console.WriteLine($"Key     : {key}");

// Sanity: the key must verify right here before we hand it out.
if (!LicenseCore.TryParse(key, secret, product, out var info, out var err) || info is null)
{
    Console.Error.WriteLine($"INTERNAL ERROR: freshly minted key did not verify: {err}");
    return 1;
}
if (!LicenseCore.EmailMatches(info, opts.Email))
{
    Console.Error.WriteLine("INTERNAL ERROR: freshly minted key email binding failed.");
    return 1;
}
return 0;

// ----------------------------------------------------------------------

static SealedOpts? ParseArgs(string[] args)
{
    string? app = null, email = null;
    var months = 1;
    var perpetual = false;
    for (var i = 0; i < args.Length; i++)
    {
        switch (args[i])
        {
            case "--app" when i + 1 < args.Length: app = args[++i].ToLowerInvariant(); break;
            case "--email" when i + 1 < args.Length: email = args[++i]; break;
            case "--months" when i + 1 < args.Length && int.TryParse(args[++i], out var m) && m >= 1 && m <= 120:
                months = m; break;
            case "--perpetual": perpetual = true; break;
            default: return null;
        }
    }
    if (app is null || email is null || !email.Contains('@'))
        return null;
    try { ProductFromId(app); }
    catch { return null; }
    return new SealedOpts(app, email, months, perpetual);
}

static ProductId ProductFromId(string appId) => appId switch
{
    "daypilot" => ProductId.DayPilot,
    "bizpilot" => ProductId.BizPilot,
    "wuse" => ProductId.Wuse,
    "wuse-business" or "wusebusiness" => ProductId.WuseBusiness,
    "claimpilot" => ProductId.ClaimPilot,
    "claimpilot-plus" or "bankruptcy-plus" or "plus" => ProductId.ClaimPilotPlus,
    "legal-matters" or "legalmatters" => ProductId.LegalMatters,
    _ => throw new ArgumentOutOfRangeException(nameof(appId)),
};

static void PrintUsage()
{
    Console.WriteLine("KeyGen --app <id> --email <buyer email> [--months N | --perpetual]");
    Console.WriteLine("KeyGen --selftest");
    Console.WriteLine("  ids: daypilot, bizpilot, wuse, wuse-business, claimpilot, claimpilot-plus, legal-matters");
}

sealed record SealedOpts(string AppId, string Email, int Months, bool Perpetual);

/// <summary>Round-trip + tamper tests for every product. No network, no I/O.</summary>
static class SelfTest
{
    public static bool Run()
    {
        var ok = true;
        var products = new[]
        {
            ProductId.DayPilot, ProductId.BizPilot, ProductId.Wuse, ProductId.WuseBusiness,
            ProductId.ClaimPilot, ProductId.ClaimPilotPlus, ProductId.LegalMatters,
        };

        foreach (var product in products)
        {
            var secret = LicenseSecrets.ForProduct(product);
            const string email = "buyer@example.com";

            // 1. Monthly key round-trip.
            var key = LicenseCore.GenerateKey(secret, product, email, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));
            ok &= Check(LicenseCore.TryParse(key, secret, product, out var info, out var err) && info is not null,
                $"{product}: monthly key verifies ({err})");
            ok &= Check(info!.Product == product, $"{product}: product byte round-trips");
            ok &= Check(LicenseCore.EmailMatches(info, email), $"{product}: email binding matches");
            ok &= Check(!LicenseCore.EmailMatches(info, "someone@else.com"), $"{product}: wrong email rejected");
            ok &= Check(!LicenseCore.IsExpired(info), $"{product}: fresh key not expired");
            ok &= Check(!info.IsPerpetual && info.ExpiryUtc is not null, $"{product}: expiry present");

            // 2. Perpetual key round-trip.
            var pkey = LicenseCore.GenerateKey(secret, product, email, DateTime.UtcNow, null);
            ok &= Check(LicenseCore.TryParse(pkey, secret, product, out var pinfo, out _) && pinfo is not null && pinfo.IsPerpetual,
                $"{product}: perpetual key verifies");

            // 3. Expired key parses but reports expired.
            var ekey = LicenseCore.GenerateKey(secret, product, email, DateTime.UtcNow.AddMonths(-2), DateTime.UtcNow.AddMonths(-1));
            ok &= Check(LicenseCore.TryParse(ekey, secret, product, out var einfo, out _) && einfo is not null && LicenseCore.IsExpired(einfo),
                $"{product}: expired key detected");

            // 4. Tampered key rejected (flip one char in the middle group).
            var tampered = key.Substring(0, 7) + (key[7] == 'A' ? 'B' : 'A') + key.Substring(8);
            ok &= Check(!LicenseCore.TryParse(tampered, secret, product, out _, out _),
                $"{product}: tampered key rejected");

            // 5. Wrong product rejected.
            var other = product == ProductId.DayPilot ? ProductId.BizPilot : ProductId.DayPilot;
            ok &= Check(!LicenseCore.TryParse(key, LicenseSecrets.ForProduct(other), other, out _, out _),
                $"{product}: cross-product key rejected");

            // 6. Garbage rejected.
            ok &= Check(!LicenseCore.TryParse("NOT-A-VALID-KEY-1234", secret, product, out _, out _),
                $"{product}: garbage rejected");
            ok &= Check(!LicenseCore.TryParse("", secret, product, out _, out _),
                $"{product}: empty rejected");
        }

        // 7. Key format check.
        var sample = LicenseCore.GenerateKey(LicenseSecrets.DayPilot, ProductId.DayPilot, "a@b.c", DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));
        ok &= Check(sample.Length == 23 && sample.Count(c => c == '-') == 3,
            $"format: 5x4 groups with dashes ({sample})");

        Console.WriteLine(ok ? "SELFTEST: ALL PASS" : "SELFTEST: FAILURES PRESENT");
        return ok;
    }

    private static bool Check(bool cond, string name)
    {
        Console.WriteLine($"  [{(cond ? "PASS" : "FAIL")}] {name}");
        return cond;
    }
}
