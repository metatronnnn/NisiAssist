// =====================================================================
// TEA (The Epic of Alexander Ultimate) - heavy hit callouts
// For Triggevent (xpdota/event-trigger): paste into the "Groovy" tab.
//
// How to use:
//   1. Open Triggevent -> "Groovy" tab -> paste this whole script.
//   2. Edit MY_ROLE below ('tank', 'healer' or 'dps').
//   3. Press Run.
//   4. Save As (e.g. "TEA heavy hits") and tick "Run on startup"
//      so it loads every time. (Scratch/Example scripts are not saved.)
//
// Re-running the script replaces the old triggers (same "named" values),
// so editing and re-running is safe.
// =====================================================================

// ---------------------------- settings ----------------------------
def MY_ROLE = 'dps'          // 'tank' | 'healer' | 'dps'

def ENABLED = [
    raidwide  : true,        // big party-wide damage
    tankbuster: true,        // tank buster (shown to tanks + healers only)
    shared    : true,        // shared tank buster (tanks + healers only)
    enrage    : true,        // enrage casts
]

def USE_TTS = true           // false = on-screen text only

// ---------------------------- skill table -------------------------
// id = action id (hex, from the Cactbot TEA timeline)
def hits = [
    // Phase 1: Living Liquid
    [id: 0x4826, name: 'Cascade',                    kind: 'raidwide'],
    [id: 0x49B0, name: 'Fluid Swing',                kind: 'tankbuster'],
    [id: 0x49B3, name: 'Cascade (Enrage)',          kind: 'enrage'],

    // Phase 2: Brute Justice / Cruise Chaser
    [id: 0x4854, name: 'J Kick',                     kind: 'raidwide'],
    [id: 0x49C2, name: 'Whirlwind',                  kind: 'raidwide'],
    [id: 0x4847, name: 'Double Rocket Punch',        kind: 'shared'],
    [id: 0x4856, name: 'Final Sentence',             kind: 'raidwide'],
    [id: 0x483A, name: 'Eternal Darkness (Enrage)',  kind: 'enrage'],

    // Phase 3: Alexander Prime
    [id: 0x4A80, name: 'Chastening Heat',            kind: 'tankbuster'],
    [id: 0x4A83, name: 'Mega Holy',                  kind: 'raidwide'],
    [id: 0x4875, name: 'Eternal Darkness (Enrage)',  kind: 'enrage'],
    [id: 0x4879, name: 'Divine Judgment (Enrage)',   kind: 'enrage'],

    // Phase 4: Perfect Alexander
    [id: 0x4891, name: 'Ordained Punishment',        kind: 'tankbuster'],
    [id: 0x4892, name: 'Ordained Capital Punishment', kind: 'shared'],
]

// ---------------------------- look & feel -------------------------
def tagOf   = [raidwide: 'RAIDWIDE', tankbuster: 'TANK BUSTER', shared: 'SHARED BUSTER', enrage: 'ENRAGE']
def speakOf = [raidwide: 'Raidwide', tankbuster: 'Tank buster', shared: 'Shared buster', enrage: 'Enrage']
def colorOf = [raidwide: [255, 170, 0], tankbuster: [90, 170, 255], shared: [90, 230, 255], enrage: [255, 60, 60]]

def isTankOrHealer = (MY_ROLE == 'tank' || MY_ROLE == 'healer')

// seconds left on the cast, one decimal
def secsLeft = { e -> String.format('%.1f', e.estimatedRemainingDuration.toMillis() / 1000.0) }
// target name if the event has one
def targetOf = { e -> try { e.target?.name ?: '' } catch (Throwable t) { '' } }

// ---------------------------- build triggers ----------------------
hits.each { h ->
    def kind = h.kind
    if (!ENABLED[kind]) return
    if ((kind == 'tankbuster' || kind == 'shared') && !isTankOrHealer) return

    def hid    = h.id
    def tag    = tagOf[kind]
    def rgb    = colorOf[kind]
    def spoken = USE_TTS ? ((kind == 'raidwide' && MY_ROLE == 'healer') ? 'Raidwide, heal' : speakOf[kind]) : ''

    groovyTriggers.add {
        named "TEA heavy ${h.name} ${Integer.toHexString(hid)}"
        type AbilityCastStart
        when { it.abilityIdMatches(hid) }
        sequence { e1, s ->
            callout { tts spoken text "${tag}: ${e1.ability.name} ${targetOf(e1)}  ${-> secsLeft(e1)}s" color rgb[0], rgb[1], rgb[2] }
        }
    }
}
