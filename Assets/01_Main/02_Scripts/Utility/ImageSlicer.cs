using UnityEngine;
using System.Collections.Generic;

public static class ImageSlicer
{
    public static List<Sprite> SliceTexture(Texture2D texture, int count)
    {
        List<Sprite> tSPrite = new List<Sprite>();

        int tSliceWidth = texture.width / count;
        int tHeight = texture.height;

        for(int i = 0; i < count; i++)
        {
            float tX = i * tSliceWidth;

            Rect tRect = new Rect(tX, 0, tSliceWidth, tHeight);

            Sprite tNewSprite = Sprite.Create(texture, tRect, new Vector2(0.5f,0.5f));

            tSPrite.Add(tNewSprite);
        }

        return tSPrite;
    }
}
