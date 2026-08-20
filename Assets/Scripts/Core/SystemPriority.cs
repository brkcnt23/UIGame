/// <summary>
/// Single place where execution order lives.
///
/// Reading this file tells you the whole boot order of the game.
/// If two systems fight over ordering, the fix belongs here — not in the scene.
///
/// Bands:
///   000-099  Core infrastructure
///   100-199  Data repositories (must load before anything reads them)
///   200-299  World simulation (runs before the player acts on the world)
///   300-399  Player-facing systems
///   400-499  Derived / reactive systems
///   900+     UI (always last: it reads state everyone else has settled)
/// </summary>
public static class SystemPriority
{
    // --- 000-099 Core -------------------------------------------------
    public const int Resources        = 10;
    public const int Save             = 20;
    public const int Time             = 30;

    // --- 100-199 Repositories ----------------------------------------
    public const int ItemDatabase     = 100;
    public const int RecipeDatabase   = 110;
    public const int TitleDatabase    = 120;
    public const int NpcRepository    = 130;
    public const int SettlementData   = 140;

    // --- 200-299 World simulation ------------------------------------
    public const int WorldSim         = 200;
    public const int Economy          = 210;
    public const int Shop             = 220;

    // --- 300-399 Player systems --------------------------------------
    public const int PlayerStats      = 300;
    public const int Inventory        = 310;
    public const int Food             = 320;
    public const int Experience       = 330;
    public const int Trait            = 335;
    public const int Title            = 340;
    public const int Companion        = 350;
    public const int Crafting         = 360;
    public const int JobLimits        = 365;
    public const int Job              = 370;
    public const int Quest            = 380;

    // --- 400-499 Reactive --------------------------------------------
    public const int Travel           = 400;
    public const int EventSystem      = 410;
    public const int Battle           = 420;
    public const int Building         = 430;

    // --- 900+ UI ------------------------------------------------------
    public const int UIRouter         = 900;
    public const int UIPanel          = 950;
}
