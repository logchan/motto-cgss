using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public partial class PlayManager
{
    private void LoadButtonSprites(int numBtn, string[] skinPaths)
    {
        _buttonSprites = new List<Sprite>();

        // find touchpadX.png then touchpad.png for each button
        for (int i = 0; i < numBtn; ++i)
        {
            foreach (var path in skinPaths)
            {
                var file = Path.Combine(path, String.Format("touchpad{0}.png", i));
                if (!File.Exists(file))
                {
                    file = Path.Combine(path, "touchpad.png");
                    if (!File.Exists(file))
                        continue;
                }

                _buttonSprites.Add(UnityHelper.SpriteFromTexture(UnityHelper.TextureFromFile(file, 200)));
                break;
            }
        }
    }

    private void LoadNoteSprites(string[] skinPaths)
    {
        _noteSprites = new List<Sprite>();
        var textureNames = new[] { "circle.png", "left.png", "right.png", "hold.png" };
        foreach (var textureName in textureNames)
        {
            foreach (var path in skinPaths)
            {
                var file = Path.Combine(path, textureName);
                if (File.Exists(file))
                {
                    var texture = UnityHelper.TextureFromFile(file, SceneSettings.SpriteSize);
                    _noteSprites.Add(UnityHelper.SpriteFromTexture(texture));

                    break;
                }
            }
        }
    }
}
