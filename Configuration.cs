using System;
using Dalamud.Configuration;

namespace NisiAssist;

[Serializable]
public sealed class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 3;

    public int Language { get; set; } = 0;                // 0 = English, 1 = Thai

    // ---- display ----
    public bool OverlayLocked { get; set; } = false;
    public bool ShowIcons { get; set; } = true;
    public bool ShowOnlyMe { get; set; } = true;          // false = also show a party table (debug / callouts)
    public bool ShowCountdown { get; set; } = true;       // countdown of the pass window
    public bool ShowMyNisiTimer { get; set; } = true;     // remaining duration of the Nisi you hold
    public bool ShowUpcoming { get; set; } = true;        // show the next pass shortly before its window opens
    public float UpcomingLead { get; set; } = 5.0f;       // seconds before the window to start showing
    public float FontScale { get; set; } = 1.6f;
    public float UiScale { get; set; } = 1.0f;            // overall multiplier for all overlay text (use ~2 on a 4K screen)
    public float BackgroundAlpha { get; set; } = 0.55f;

    // ---- water / lightning debuff countdown ----
    public bool ShowDebuffCountdown { get; set; } = true;
    public float DebuffCountdownSeconds { get; set; } = 5.0f;   // start showing when this many seconds remain
    public float CountdownFontScale { get; set; } = 7.0f;       // (old, unused since 0.2.1)
    public float CountdownSize { get; set; } = 220f;            // height of the big number in pixels
    public bool DebuffSoundEnabled { get; set; } = true;
    public int DebuffTickSound { get; set; } = 2;               // in-game chat sound effect <se.N> (1-16), played every second
    public int DebuffFinalSound { get; set; } = 6;              // played on the last second

    // ---- wormhole (phase 3 part 2) ----
    // Defaults follow the Nukemaru-style wormhole (corner / puddle layout). 1-8 = your Limit Cut number.
    // Edit the text in /nisi to match your own strat.
    public bool ShowWormhole { get; set; } = true;
    public int WormholeNumber { get; set; } = 0;           // 0 = not set yet
    public string[] WormholeEnter { get; set; } =
    {
        "Start NW corner (with 5).",
        "Start NE corner (with 6).",
        "Start SW corner (with 7). Don't dodge in a hurry.",
        "Start SE corner (with 8). Don't dodge in a hurry.",
        "WEST puddle #1 first.",
        "EAST puddle #1 first.",
        "WEST puddle #2 first.",
        "EAST puddle #2 first.",
    };
    public string[] WormholeAfter { get; set; } =
    {
        "Then WEST puddle #3. Your turn: face OUTSIDE. BJ in front of you? Stand still until Apocalyptic Ray ends.",
        "Then EAST puddle #3. Your turn: face MIDDLE. BJ in front of you? Stand still until Apocalyptic Ray ends.",
        "After the dice, go to the side (9 o'clock). Your turn: face OUTSIDE. BJ may jump on you.",
        "After the dice, go to the side (3 o'clock). Your turn: face MIDDLE. BJ may jump on you.",
        "Then NW red circle (dice). Your turn: face OUTSIDE.",
        "Then NE red circle (dice). Your turn: face MIDDLE.",
        "Then SW red circle (dice). Your turn: face OUTSIDE.",
        "Then SE red circle (dice). Your turn: face MIDDLE.",
    };
    public string WormholeFooter { get; set; } =
        "Stack in the middle -> Enumeration: tanks north, healers south, count DPS go by priority D1>D2>D3>D4 (north first), DPS without a count stay out -> Mega Holy x2";
    public string[] WormholeEnterTh { get; set; } =
    {
        "เริ่มที่มุมตะวันตกเฉียงเหนือ (NW) (คู่กับเลข 5)",
        "เริ่มที่มุมตะวันออกเฉียงเหนือ (NE) (คู่กับเลข 6)",
        "เริ่มที่มุมตะวันตกเฉียงใต้ (SW) (คู่กับเลข 7) ไม่ต้องรีบหลบ",
        "เริ่มที่มุมตะวันออกเฉียงใต้ (SE) (คู่กับเลข 8) ไม่ต้องรีบหลบ",
        "ไปรูหนอนทางตะวันตก รอบที่ 1 ก่อน",
        "ไปรูหนอนทางตะวันออก รอบที่ 1 ก่อน",
        "ไปรูหนอนทางตะวันตก รอบที่ 2 ก่อน",
        "ไปรูหนอนทางตะวันออก รอบที่ 2 ก่อน",
    };
    public string[] WormholeAfterTh { get; set; } =
    {
        "จากนั้นไปรูหนอนตะวันตก รอบที่ 3 ถึงคิวของคุณ: หันออกนอก ถ้า BJ อยู่ตรงหน้า ยืนนิ่งจน Apocalyptic Ray จบ",
        "จากนั้นไปรูหนอนตะวันออก รอบที่ 3 ถึงคิวของคุณ: หันเข้ากลาง ถ้า BJ อยู่ตรงหน้า ยืนนิ่งจน Apocalyptic Ray จบ",
        "หลังลูกเต๋าจบ ไปข้างสนาม (9 นาฬิกา) ถึงคิวของคุณ: หันออกนอก BJ อาจกระโดดใส่",
        "หลังลูกเต๋าจบ ไปข้างสนาม (3 นาฬิกา) ถึงคิวของคุณ: หันเข้ากลาง BJ อาจกระโดดใส่",
        "จากนั้นไปวงแดง NW (ลูกเต๋า) ถึงคิวของคุณ: หันออกนอก",
        "จากนั้นไปวงแดง NE (ลูกเต๋า) ถึงคิวของคุณ: หันเข้ากลาง",
        "จากนั้นไปวงแดง SW (ลูกเต๋า) ถึงคิวของคุณ: หันออกนอก",
        "จากนั้นไปวงแดง SE (ลูกเต๋า) ถึงคิวของคุณ: หันเข้ากลาง",
    };
    public string WormholeFooterTh { get; set; } =
        "รวมกลางรับ stack -> Enumeration: แท็งก์อยู่เหนือ ฮีลเลอร์อยู่ใต้ DPS ที่ได้วงนับเรียงลำดับ D1>D2>D3>D4 (ไปเหนือก่อน) DPS ที่ไม่ได้วงหลบออกข้าง -> Mega Holy 2 ครั้ง";

    // ---- timing (seconds) ----
    // All defaults come from Cactbot's TEA triggers + timeline (nominal values; tweak if your group's pull is faster/slower).
    // Pass 3: Cactbot alerts 8s after Brute Justice starts Flarethrower, for 9s.
    public float Pass3DelayAfterCastStart { get; set; } = 8.0f;
    public float Pass3Duration { get; set; } = 9.0f;
    // Pass 4: Cactbot alerts 15s before Propeller Wind, for 14s (so the window closes 1s before it).
    public float Pass4LeadBeforePropeller { get; set; } = 15.0f;
    public float Pass4EndBeforePropeller { get; set; } = 1.0f;
    // Timeline: Flarethrower resolves at 315.5s, Propeller Wind at 354.5s -> 39.0s apart.
    // Used until the real Propeller Wind cast is seen (which then gives the exact time).
    public float PropellerAfterFlarethrower { get; set; } = 39.0f;

    /// <summary>Copies the default wormhole texts (used when upgrading from an older saved config).</summary>
    public void ResetWormholeText()
    {
        var d = new Configuration();
        WormholeEnter = d.WormholeEnter; WormholeAfter = d.WormholeAfter; WormholeFooter = d.WormholeFooter;
        WormholeEnterTh = d.WormholeEnterTh; WormholeAfterTh = d.WormholeAfterTh; WormholeFooterTh = d.WormholeFooterTh;
    }

    public void Save() => Plugin.PluginInterface.SavePluginConfig(this);
}
