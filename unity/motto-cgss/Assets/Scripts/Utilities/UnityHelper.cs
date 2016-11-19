using System.IO;
using UnityEngine;

// ReSharper disable once CheckNamespace
public static class UnityHelper
{
    public static Texture2D TextureFromFile(string path, int size)
    {
        var texture = new Texture2D(size, size);
        texture.LoadImage(File.ReadAllBytes(path));

        return texture;
    }

    public static Sprite SpriteFromTexture(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f));
    }
}
