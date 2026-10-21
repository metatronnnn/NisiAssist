namespace NisiAssist;

/// <summary>Tiny English/Thai switch for overlay text. Language is set from the config.</summary>
public static class Loc
{
    /// <summary>What the user selected in settings.</summary>
    public static bool WantThai { get; set; }

    /// <summary>Set by Fonts: true only when the Thai font has really been loaded.</summary>
    public static System.Func<bool>? ThaiReady { get; set; }

    /// <summary>Thai is used only when selected AND the Thai font is ready; otherwise everything falls back to English (no "=" boxes).</summary>
    public static bool Thai => WantThai && (ThaiReady?.Invoke() ?? false);

    private static string T(string en, string th) => Thai ? th : en;

    public static string PassHeader(int pass, bool active) =>
        active ? T($"NISI PASS {pass}", $"แลก NISI รอบ {pass}") : T($"NISI PASS {pass} - next", $"แลก NISI รอบ {pass} - เร็วๆ นี้");
    public static string Left(double s) => T($"{s:0.0}s", $"{s:0.0} วิ");
    public static string InSeconds(double s) => T($"in {s:0.0}s", $"อีก {s:0.0} วิ");
    public static string Pass(string nisi, string who) => T($"PASS {nisi} -> {who}", $"ส่ง {nisi} -> {who}");
    public static string Get(string nisi, string who) => T($"GET {nisi} <- {who}", $"รับ {nisi} <- {who}");
    public static string PartnerNotFound => T("(partner not found)", "(ไม่พบผู้รับ)");
    public static string HolderNotFound => T("(holder not found)", "(ไม่พบคนถือ)");
    public static string NoVerdict => T("No verdict debuff on you (?)", "ไม่มีดีบัพ verdict บนตัวคุณ (?)");
    public static string Holding(string nisi, float s) => T($"Holding {nisi}: {s:0.0}s left", $"ถือ {nisi}: เหลือ {s:0.0} วิ");
    public static string PartyLine(string name, string held, string need) => T($"{name}  hold: {held}  need: {need}", $"{name}  ถือ: {held}  ต้องรับ: {need}");

    public static string Water => T("WATER", "น้ำ");
    public static string Lightning => T("LIGHTNING", "สายฟ้า");

    public static string WormholeTitle => "WORMHOLE";
    public static string PuddleOrder => T("Puddles: #1 = 5,6  |  #2 = 7,8  |  #3 = 1,2  |  3,4 none", "ก้อนน้ำ: #1 = 5,6  |  #2 = 7,8  |  #3 = 1,2  |  3,4 ไม่รับ");
    public static string PickNumber => T("Pick your number:", "เลือกเลขของคุณ:");
    public static string After(string text) => T($"After: {text}", $"หลังจากนั้น: {text}");
    public static string Then(string text) => T($"Then: {text}", $"ต่อไป: {text}");
    public static string SoakNow(int n) => T($">> SOAK PUDDLE #{n} NOW <<", $">> เข้ารับก้อนที่ {n} เดี๋ยวนี้ <<");
    public static string PuddleNotMine(int n) => T($"Puddle #{n} now (not yours)", $"ก้อนที่ {n} กำลังมา (ไม่ใช่ของคุณ)");
}
