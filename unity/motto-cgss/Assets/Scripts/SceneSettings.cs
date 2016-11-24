public static class SceneSettings
{
    static SceneSettings()
    {
        ApproachRate = 90;
        SwipeThreshold = 0.1f;
    }

    public const float HitStatusTime = 1000;
    public const float ButtonAnimationTime = 300;
    public const float ComboAnimationTime = 200;
    public const float ComboAnimationScale = 0.1f;

    public static float SwipeThreshold { get; set; }

    public const int SpriteSize = 200;
    public static int NoteSize { get; set; }
    public static int NoteRadius { get; set; }
    public static float NoteRadiusNormalized { get; set; }
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

