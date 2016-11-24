using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class MottoSettings
{
    public MottoSettings()
    {
        UserDataPath = "";
        NoteSizeFactor = 0.12f;
        ButtonYFactor = 0.15625f;
        BetweenButtonsFactor = 0.14f;
        ShooterHeightFactor = 0.6181f;
    }

    public string UserDataPath { get; set; }
    public float NoteSizeFactor { get; set; }
    public float ButtonYFactor { get; set; }
    public float BetweenButtonsFactor { get; set; }
    public float ShooterHeightFactor { get; set; }
}

