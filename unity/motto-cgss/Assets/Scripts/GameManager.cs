using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Xml.Serialization;
using motto_cgss_core.Model;
using motto_cgss_core.Utility;

public static class GameManager
{
    private static readonly int[] ArTable = { 1800, 1788, 1776, 1764, 1752, 1740, 1728, 1716, 1704, 1692, 1680, 1668, 1656, 1644, 1632, 1620, 1608, 1596, 1584, 1572, 1560, 1548, 1536, 1524, 1512, 1500, 1488, 1476, 1464, 1452, 1440, 1428, 1416, 1404, 1392, 1380, 1368, 1356, 1344, 1332, 1320, 1308, 1296, 1284, 1272, 1260, 1248, 1236, 1224, 1212, 1200, 1185, 1170, 1155, 1140, 1125, 1110, 1095, 1080, 1065, 1050, 1035, 1020, 1005, 990, 975, 960, 945, 930, 915, 900, 885, 870, 855, 840, 825, 810, 795, 780, 765, 750, 735, 720, 705, 690, 675, 660, 645, 630, 615, 600, 585, 570, 555, 540, 525, 510, 495, 480, 465, 450 };

    private static readonly string SettingsFile;
    private static readonly XmlSerializer SettingsSerializer = new XmlSerializer(typeof(MottoSettings));

    static GameManager()
    {
        DataPath = Application.persistentDataPath;
        SettingsFile = Path.Combine(DataPath, "motto-settings.xml");

        LoadSettings();
        WriteSettings();

        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            // find user-specified data path
            DataPath = Settings.UserDataPath.Length > 0 ? Settings.UserDataPath : DataPath;
        }
    }

    // ReSharper disable once InconsistentNaming
    private static readonly Dictionary<BeatmapInfo, List<Beatmap>> _beatmaps = new Dictionary<BeatmapInfo, List<Beatmap>>();
    public static Dictionary<BeatmapInfo, List<Beatmap>> Beatmaps {
        get { return _beatmaps; }
    }

    public static Beatmap BeatmapToPlay { get; set; }
    public static string DataPath { get; private set; }
    public static MottoSettings Settings { get; private set; }

    public static int ArToTime(int ar)
    {
        ar = MathHelper.Clamp(ar, 0, 100);
        return ArTable[ar];
    }

    public static void LoadSettings()
    {
        MottoSettings settings = null;
        try
        {
            using (var fs = new FileStream(SettingsFile, FileMode.Open))
                settings = SettingsSerializer.Deserialize(fs) as MottoSettings;
        }
        catch (Exception)
        {
            // ignore
        }

        if (settings == null)
            settings = new MottoSettings();

        Settings = settings;
    }

    public static bool WriteSettings()
    {
        try
        {
            using (var fs = new FileStream(SettingsFile, FileMode.Create))
                SettingsSerializer.Serialize(fs, Settings);
            return true;
        }
        catch (Exception)
        {
            // TODO: log
            return false;
        }
    }

    public static void LoadBeatmaps()
    {
        
        var di = new DirectoryInfo(Path.Combine(DataPath, "Beatmaps"));
        if (!di.Exists)
            di.Create();

        foreach (var subdir in di.GetDirectories())
        {
            var infoFile = Path.Combine(subdir.FullName, "info.txt");
            if (!File.Exists(infoFile))
                continue;

            var diffFiles = Directory.GetFiles(subdir.FullName, "*.diff.txt");
            if (diffFiles.Length < 1)
                continue;

            try
            {
                var info = new BeatmapInfo
                {
                    FileName = subdir.Name,
                    Path = subdir.FullName
                };
                info.Parse(File.ReadAllLines(infoFile));

                _beatmaps[info] = new List<Beatmap>();

                foreach (var bmFile in diffFiles)
                {
                    var bm = new Beatmap(info);
                    bm.Parse(File.ReadAllLines(bmFile));
                    _beatmaps[info].Add(bm);
                }
            }
            catch (Exception ex)
            {
                // TODO: write message
                Debug.Log(ex.Message);
            }
        }
    }
}
