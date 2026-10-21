using System;
using Dalamud.Configuration;

namespace NisiAssist;

[Serializable]
public sealed class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

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
    public float CountdownSize { get; set; } = 120f;            // height of the big number in pixels
    public bool DebuffSoundEnabled { get; set; } = true;
    public int DebuffTickSound { get; set; } = 2;               // in-game chat sound effect <se.N> (1-16), played every second
    public int DebuffFinalSound { get; set; } = 6;              // played on the last second

    // ---- wormhole (phase 3 part 2) ----
    // Defaults follow Cactbot's "Wormhole" strat (TPS-based): 1-8 = Limit Cut number. Edit the text in /nisi to match your own strat.
    public bool ShowWormhole { get; set; } = true;
    public int WormholeNumber { get; set; } = 0;           // 0 = not set yet
    public string[] WormholeEnter { get; set; } =
    {
        "Left of robot, look OUTSIDE. Puddle #3",
        "Back right (opposite robot), look MIDDLE. Puddle #3",
        "Back left (opposite robot). No puddle",
        "Right of robot. No puddle",
        "Left robot side. Puddle #1",
        "Right robot side. Puddle #1",
        "Left robot side, then cardinal. Puddle #2",
        "Right robot side, then cardinal. Puddle #2",
    };
    public string[] WormholeAfter { get; set; } =
    {
        "Knockback cleave: face OUTSIDE. E/W cardinals safe after chakrams",
        "Knockback charge: face MIDDLE. Maybe move behind Brute Justice at super jump",
        "Knockback cleave: face OUTSIDE. Maybe move behind Brute Justice at super jump",
        "Knockback charge: face MIDDLE. E/W cardinals safe after chakrams",
        "Knockback cleave: face OUTSIDE. E/W cardinals safe after chakrams",
        "Knockback charge: face MIDDLE. E/W cardinals safe after chakrams",
        "Knockback cleave: face OUTSIDE. E/W cardinals safe after chakrams",
        "Knockback charge: face MIDDLE. E/W cardinals safe after chakrams",
    };

    public string[] WormholeEnterTh { get; set; } =
    {
        "ซ้ายของหุ่น หันออกนอก รับก้อน #3",
        "หลังขวา (ตรงข้ามหุ่น) หันเข้ากลาง รับก้อน #3",
        "หลังซ้าย (ตรงข้ามหุ่น) ไม่รับก้อน",
        "ขวาของหุ่น ไม่รับก้อน",
        "ข้างหุ่นฝั่งซ้าย รับก้อน #1",
        "ข้างหุ่นฝั่งขวา รับก้อน #1",
        "ข้างหุ่นฝั่งซ้าย แล้วไปทิศหลัก รับก้อน #2",
        "ข้างหุ่นฝั่งขวา แล้วไปทิศหลัก รับก้อน #2",
    };
    public string[] WormholeAfterTh { get; set; } =
    {
        "น็อกแบ็ก cleave: หันออกนอก ทิศตะวันออก/ตะวันตกปลอดภัยหลังจักรผ่าน",
        "น็อกแบ็ก charge: หันเข้ากลาง อาจต้องไปหลัง Brute Justice ตอน Super Jump",
        "น็อกแบ็ก cleave: หันออกนอก อาจต้องไปหลัง Brute Justice ตอน Super Jump",
        "น็อกแบ็ก charge: หันเข้ากลาง ทิศตะวันออก/ตะวันตกปลอดภัยหลังจักรผ่าน",
        "น็อกแบ็ก cleave: หันออกนอก ทิศตะวันออก/ตะวันตกปลอดภัยหลังจักรผ่าน",
        "น็อกแบ็ก charge: หันเข้ากลาง ทิศตะวันออก/ตะวันตกปลอดภัยหลังจักรผ่าน",
        "น็อกแบ็ก cleave: หันออกนอก ทิศตะวันออก/ตะวันตกปลอดภัยหลังจักรผ่าน",
        "น็อกแบ็ก charge: หันเข้ากลาง ทิศตะวันออก/ตะวันตกปลอดภัยหลังจักรผ่าน",
    };

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

    public void Save() => Plugin.PluginInterface.SavePluginConfig(this);
}
