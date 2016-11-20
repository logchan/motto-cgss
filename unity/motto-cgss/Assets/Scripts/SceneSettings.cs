public static class SceneSettings
{
    static SceneSettings()
    {
        ApproachRate = 90;
    }

    public const float HitStatusTime = 200;

    public const float SwipeThreshold = 100;

    public const float NoteSizeFactor = 0.1497f;
    public const float ButtonYFactor = 0.15625f;
    public const float BetweenButtonsFactor = 0.11067f;
    public const float ShooterHeightFactor = 0.6181f;
    public const int ButtonHitFrames = 10;

    public const int SpriteSize = 200;
    public static int NoteSize { get; set; }
    public static int NoteRadius { get; set; }
    public static int ButtonY { get; set; }
    public static int BetweenButtons { get; set; }
    public static int ShooterHeight { get; set; }

    public static int ApproachRate { get; set; }

    public static bool Hidden { get; set; }
    public const double HiddenStart = 0.5;
    public const double HiddenLength = 0.3;

    public static bool Auto { get; set; }
    public static bool AutoBot { get; set; }
    public static bool DoubleTime { get; set; }
    public static bool HalfTime { get; set; }
    public static float SkipTime { get; set; }
}

