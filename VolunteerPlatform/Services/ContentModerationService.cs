namespace VolunteerPlatform.Services;

public enum ModerationResult
{
    Clean,       // se publică
    NeedsReview, // conținut suspect, merge la moderator
    Blocked,     // blocat complet, nu se salvează
}

public class ModerationReport
{
    public ModerationResult Result { get; init; }
    public string? Reason { get; init; }
    public List<string> MatchedTerms { get; init; } = [];
}

public interface IContentModerationService
{
    ModerationReport Evaluate(string title, string description, DateTime startsAt);
}

public class ContentModerationService : IContentModerationService
{
    private static readonly HashSet<string> _blockedTerms = new(StringComparer.OrdinalIgnoreCase)
    {
        // droguri / substanțe ilegale
        "droguri", "drog", "heroina", "heroină", "cocaina", "cocaină",
        "amfetamina", "amfetamină", "metamfetamina", "metamfetamină",
        "canabis", "cannabis", "marijuana", "marihuana", "haș", "has", "hash",
        "ecstasy", "mdma", "lsd", "ketamina", "ketamină", "fentanil",
        "substanță ilegală", "substante ilegale",

        // activități ilegale
        "trafic", "traficant", "contraband", "contrabandă",
        "furt", "fura", "furăm", "hoti", "hoți",
        "jaf", "jefuim", "jaful",
        "inșelătorie", "înșelătorie", "frauda", "fraudă", "escrocherie",
        "spălare de bani", "spalare bani",
        "arme", "pistol", "cuțit", "cutit", "bomba", "bombă",
        "exploziv", "explozibili",
        "ucide", "omor", "asasinat",

        // conținut sexual explicit
        "sex", "sexual", "masaj erotic", "escortă", "escorta",
        "prostituție", "prostituție", "proxenetism",
        "pornografie", "pornografic", "adult", "xxx",
        "nuditate", "explicit",

        // hărțuire / ură
        "hărțuire", "hartuire", "abuz", "violenta", "violență",
        "amenintare", "amenințare", "intimidare", "santaj", "șantaj",
        "discriminare", "rasism", "rasist", "xenofob", "xenofobie",
    };

    private static readonly HashSet<string> _suspectTerms = new(StringComparer.OrdinalIgnoreCase)
    {
        // bani / financiar
        "bani cash", "cash", "plată cash", "plata cash",
        "transfer bancar", "crypto", "bitcoin", "ethereum",
        "împrumut", "imprumut", "credit", "avans",
        "recompensă", "recompensa", "platit", "plătit", "salariu", "platesc",
        "comision", "câștig", "castig", "profit", "bonus",

        // alcool / tutun
        "alcool", "bere", "vin", "vodca", "vodcă", "whisky", "whiskey",
        "țuică", "tuica", "rachiu", "coniac", "sampanie", "șampanie",
        "bautura", "băutură", "bar", "pub", "club", "discoteca", "discotecă",
        "petrecere", "party", "chef",
        "tigari", "țigări", "tutun", "fumat", "trabuc",
        "vaping", "vape", "narghilea",

        // noapte / ore nesigure
        "noaptea", "noapte", "miezul noptii", "miezul nopții",
        "tarziu", "târziu", "dupa miezul noptii", "după miezul nopții",
        "ora 22", "ora 23", "ora 00", "ora 01", "ora 02", "ora 03",
        "ora 04", "ora 05", "22:00", "23:00", "00:00", "01:00",
        "02:00", "03:00", "04:00", "05:00",
        "weekend noapte", "vineri seara", "sambata seara", "sâmbătă seara",

        // locații izolate / risc
        "pădure", "padure", "izolat", "izolată", "izolata",
        "abandonat", "abandonată", "parasit", "părăsit", "paragina", "parăgină",
        "subsol", "pivnita", "pivniță", "depozit", "garaj",
        "fara martori", "fără martori", "singur", "singură",
        "discret", "discreta", "discretă", "confidential", "confidențial",
        "secret", "secretă", "privat", "privată",

        // date personale
        "buletin", "carte de identitate", "pașaport", "pasaport",
        "cod numeric", "cnp", "cont bancar", "iban", "card bancar",
        "parola", "parolă", "pin", "cod secret",

        // medicamente / substanțe controlate
        "medicamente", "pastile", "reteta", "rețetă",
        "tranchilizant", "somnifer", "sedativ", "antidepresiv",
        "morfina", "morfină", "opiu", "codeine", "codeina", "tramadol",

        // alte riscuri
        "minor", "copil mic", "copii mici", "fara adulti", "fără adulți",
        "singur acasa", "singur acasă", "usa descuiata", "ușa descuiată",
        "cheie sub pres", "cheie ascunsa", "cheie ascunsă",
        "urgent urgent", "foarte urgent", "extrem de urgent",
        "nimeni nu stie", "nimeni nu știe", "fara sa stie", "fără să știe",
    };

    public ModerationReport Evaluate(string title, string description, DateTime startsAt)
    {
        var text = $"{title} {description}".ToLowerInvariant();

        var hour = startsAt.Hour;
        if (hour < 6 || hour >= 21)
        {
            return new ModerationReport
            {
                Result = ModerationResult.NeedsReview,
                Reason = $"Ora de start ({startsAt:HH:mm}) este în afara intervalului permis (06:00–21:00).",
            };
        }

        var blockedMatches = FindMatches(text, _blockedTerms);
        if (blockedMatches.Count > 0)
        {
            return new ModerationReport
            {
                Result = ModerationResult.Blocked,
                Reason = "Conținutul conține termeni interzisi și nu poate fi publicat.",
                MatchedTerms = blockedMatches,
            };
        }

        var suspectMatches = FindMatches(text, _suspectTerms);
        if (suspectMatches.Count > 0)
        {
            return new ModerationReport
            {
                Result = ModerationResult.NeedsReview,
                Reason = "Conținutul conține termeni care necesită verificare manuală.",
                MatchedTerms = suspectMatches,
            };
        }

        return new ModerationReport { Result = ModerationResult.Clean };
    }

    private static List<string> FindMatches(string text, HashSet<string> terms)
    {
        var found = new List<string>();

        foreach (var term in terms)
        {
            if (text.Contains(term, StringComparison.OrdinalIgnoreCase))
                found.Add(term);
        }

        return found;
    }
}