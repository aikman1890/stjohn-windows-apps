using System.Text.RegularExpressions;

namespace Wuse.Services;

/// <summary>
/// AI safety layer for wuse. Screens every user request (typed or spoken)
/// against hard refusal categories BEFORE any command runs or the AI is
/// consulted. Refusals are warm, brief, and judgment-free, and always offer
/// a legitimate redirect — never a lecture.
/// </summary>
public static class SafetyGuard
{
    // Each category carries phrase patterns (word-boundary regexes, matched
    // case-insensitively) that indicate a request for wrongdoing. Patterns
    // use action phrasing ("how to", "write me", "help me") to avoid
    // false-positives on innocent mentions ("I have a stomach virus").
    private static readonly (string Category, string[] Patterns)[] RefusalRules =
    {
        ("hacking", new[]
        {
            @"how to hack", @"hack into", @"hack a\b", @"hack my", @"hack the",
            @"hack (an|someones|someone's)", @"break into (a|an|my|the|someones|someone's).{0,20}(account|computer|phone|email|system|server|network|wifi|wi-fi)",
            @"unauthorized access", @"bypass (the )?(password|login|authentication|security|paywall)",
            @"crack (a |the )?password", @"steal (a |the )?(password|credentials|login)",
            @"sql injection", @"\bxss\b", @"buffer overflow", @"zero[\s-]?day",
            @"privilege escalation", @"reverse shell",
        }),
        ("malware", new[]
        {
            @"how to (make|create|build|write).{0,30}(malware|ransomware|virus|trojan|keylogger|rootkit|worm|spyware)",
            @"write (me )?(a )?(malware|ransomware|computer virus|trojan|keylogger|rootkit)",
            @"create (a )?(malware|ransomware|computer virus|trojan|keylogger)",
            @"make (a )?(malware|ransomware|computer virus)",
            @"\bmalware\b.{0,20}(source|code|tutorial|guide)",
            @"ransomware.{0,20}(source|code|tutorial|guide)",
        }),
        ("cyberattacks", new[]
        {
            @"\bddos\b", @"denial of service attack", @"take down (a|the) (website|site|server)",
            @"\bbotnet\b",
        }),
        ("phishing and fraud", new[]
        {
            @"how to phish", @"write (me )?a phishing", @"create (a )?phishing",
            @"spear.?phishing", @"impersonate (a |the )?(bank|microsoft|apple|google|support|irs)",
            @"pretend to be (my )?(bank|microsoft|apple|google|boss)",
            @"fake (login|invoice|website|email).{0,20}(to|for).{0,20}(steal|trick|scam)",
        }),
        ("wrongful access", new[]
        {
            @"spy on (my|someone|someones|someone's)", @"read someone else's (email|messages|texts)",
            @"track (my )?(wife|husband|girlfriend|boyfriend|ex).{0,20}without",
            @"someone's password without",
        }),
        ("weapons and harm", new[]
        {
            @"how to (make|build).{0,30}(a bomb|an explosive|a weapon|a gun)",
            @"hurt (myself|someone|them|him|her)",
        }),
        // Destructive or hostile PC actions wuse must never perform: deleting
        // files, wiping the recycle bin, shutting down / restarting, killing
        // processes, formatting drives. Action-phrased to avoid false
        // positives on innocent mentions.
        ("destructive system actions", new[]
        {
            @"\bdelete\b.{0,30}\b(all )?(my )?(files|documents|photos|pictures)\b",
            @"empty (the )?recycle bin", @"clear (the )?recycle bin",
            @"(shut|turn) (my )?(pc|computer) (down|off)", @"\bshutdown\b",
            @"restart (my )?(pc|computer)", @"reboot (my )?(pc|computer)",
            @"kill (a |the )?process", @"\btaskkill\b", @"stop (a |the )?process",
            @"format (my )?(c:? ?drive|hard drive|disk)",
            @"wipe (my )?(hard drive|disk|pc|computer)",
        }),
    };

    private static readonly RegexOptions RegexOpts =
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

    /// <summary>
    /// Returns true when the input falls in a refusal category.
    /// </summary>
    public static bool IsRefused(string input, out string category)
    {
        category = string.Empty;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        foreach (var (name, patterns) in RefusalRules)
        {
            foreach (var pattern in patterns)
            {
                try
                {
                    if (Regex.IsMatch(input, pattern, RegexOpts))
                    {
                        category = name;
                        return true;
                    }
                }
                catch (RegexParseException)
                {
                    // A bad pattern must never block legitimate use.
                }
            }
        }
        return false;
    }

    /// <summary>
    /// The warm, brief, judgment-free refusal with a legitimate redirect.
    /// </summary>
    public static string WarmRefusal(string category)
    {
        if (category == "destructive system actions")
            return "I can't do that one — deleting files, shutting down, or killing " +
                   "processes is outside what I'm allowed to touch, and I'd hate to " +
                   "break something you need. I'm great at the safe stuff though: opening " +
                   "apps, finding files, reminders, notes, todos, email. What should we do instead?";

        return "I can't help with that one — it's outside what I'm here for, and " +
               "I want to keep things safe and square. But I'm really good at the " +
               "everyday stuff: reminders, notes, todos, lookups, opening apps, all " +
               "right here on your PC. What should we tackle instead?";
    }
}
