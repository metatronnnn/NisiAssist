using System;
using Dalamud.Configuration;

namespace NisiAssist;

[Serializable]
public sealed class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

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
