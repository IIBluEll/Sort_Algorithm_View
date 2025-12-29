using UnityEngine;

public class ImageSlicer_model
{
    // 텍스처를 N등분하여 SortItem 배열로 반환
    public SortItem[] Slice(Texture2D sourceTexture , int count)
    {
        if ( sourceTexture == null || count <= 0 ) return null;

        SortItem[] tItems = new SortItem[count];

        // 텍스처 전체 너비/높이
        float tTexWidth = sourceTexture.width;
        float tTexHeight = sourceTexture.height;

        // 조각 하나의 너비
        float tSliceWidth = tTexWidth / count;

        for ( int i = 0; i < count; i++ )
        {
            // 자를 영역(Rect) 계산
            // 텍스처 좌표계는 좌측 하단이 (0,0)이지만, Sprite 슬라이스는 보통 좌측부터 순서대로 함
            float tX = i * tSliceWidth;

            // Rect(x, y, width, height)
            Rect tRect = new Rect(tX, 0, tSliceWidth, tTexHeight);

            // 스프라이트 생성
            Sprite tSprite = Sprite.Create(
                    sourceTexture,
                    tRect,
                    new Vector2(0.5f, 0.5f) // 피벗 중앙
                );

            // 데이터 생성
            tItems[ i ] = new SortItem
            {
                Value = i , // 정렬 기준값 (원래 순서)
                SlicedSprite = tSprite
            };
        }

        return tItems;
    }
}
