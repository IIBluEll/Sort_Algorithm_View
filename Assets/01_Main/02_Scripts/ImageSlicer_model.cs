using Cysharp.Threading.Tasks;
using System;
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

public class SortAlgorithm_model
{
    /// <summary>
    /// 배열을 무작위로 섞습니다. (Fisher-Yates Shuffle)
    /// </summary>
    public void Shuffle(SortItem[] array)
    {
        if ( array == null || array.Length <= 1 ) return;

        System.Random tRnd = new System.Random();
        int tN = array.Length;

        while ( tN > 1 )
        {
            tN--;
            int tK = tRnd.Next(tN + 1);

            // 데이터 Swap
            SortItem tTemp = array[tK];
            array[ tK ] = array[ tN ];
            array[ tN ] = tTemp;
        }
    }

    public async UniTask ExecuteSortAsync(
            ISortStrategy strategy ,
            SortItem[] items ,
            Action<int , int> onCompare ,
            Action<int , int> onSwap ,
            Action onComplete)
    {
        if ( strategy == null )
        {
            Debug.LogError("SortAlgorithm_model: 정렬 전략(Strategy)이 없습니다.");
            return;
        }

        // 전략 실행
        await strategy.SortAsync(items , onCompare , onSwap);

        // 완료 콜백
        onComplete?.Invoke();
    }
}

public class BubbleSort : ISortStrategy
{
    public async UniTask SortAsync(SortItem[] items , Action<int , int> onCompare , Action<int , int> onSwap)
    {
        if ( items == null || items.Length <= 1 ) return;

        int tCount = items.Length;

        for ( int i = 0; i < tCount - 1; i++ )
        {
            for ( int j = 0; j < tCount - i - 1; j++ )
            {
                // 1. 비교 시각화 알림
                onCompare?.Invoke(j , j + 1);
                await UniTask.Delay(20); // [시각화 대기]

                // 값 비교
                if ( items[ j ].Value > items[ j + 1 ].Value )
                {
                    // 2. 데이터 교환 (논리적 스왑)
                    SortItem tTemp = items[j];
                    items[ j ] = items[ j + 1 ];
                    items[ j + 1 ] = tTemp;

                    // 3. 교환 시각화 알림
                    onSwap?.Invoke(j , j + 1);
                    await UniTask.Delay(20); // [시각화 대기]
                }
            }
        }
    }
}

public class QuickSort : ISortStrategy
{
    public async UniTask SortAsync(SortItem[] items , Action<int , int> onCompare , Action<int , int> onSwap)
    {
        if ( items == null || items.Length <= 1 ) return;

        // 재귀 함수 시작 (0 ~ 끝 인덱스)
        await QuickSortRecursive(items , 0 , items.Length - 1 , onCompare , onSwap);
    }

    private async UniTask QuickSortRecursive(SortItem[] items , int low , int high , Action<int , int> onCompare , Action<int , int> onSwap)
    {
        if ( low < high )
        {
            // 피벗을 기준으로 배열을 분할하고 피벗의 최종 위치를 받음
            int tPivotIndex = await Partition(items, low, high, onCompare, onSwap);

            // 피벗 기준 좌/우 부분 배열을 각각 다시 정렬
            await QuickSortRecursive(items , low , tPivotIndex - 1 , onCompare , onSwap);
            await QuickSortRecursive(items , tPivotIndex + 1 , high , onCompare , onSwap);
        }
    }

    private async UniTask<int> Partition(SortItem[] items , int low , int high , Action<int , int> onCompare , Action<int , int> onSwap)
    {
        // 가장 오른쪽 요소를 피벗으로 선택
        SortItem tPivot = items[high];

        // 작은 요소들이 들어갈 위치 (i)
        int i = (low - 1);

        for ( int j = low; j < high; j++ )
        {
            // 1. 피벗과 현재 요소 비교 시각화
            onCompare?.Invoke(j , high);
            await UniTask.Delay(20); // [시각화 대기]

            // 현재 요소가 피벗보다 작다면
            if ( items[ j ].Value < tPivot.Value )
            {
                i++;

                // 2. 교환 (i번째와 j번째)
                if ( i != j )
                {
                    SortItem tTemp = items[i];
                    items[ i ] = items[ j ];
                    items[ j ] = tTemp;

                    // 교환 시각화
                    onSwap?.Invoke(i , j);
                    await UniTask.Delay(20); // [시각화 대기]
                }
            }
        }

        // 3. 피벗을 제자리(작은 요소들 바로 다음)로 이동
        int tPivotFinalIndex = i + 1;
        if ( tPivotFinalIndex != high )
        {
            SortItem tTempPivot = items[tPivotFinalIndex];
            items[ tPivotFinalIndex ] = items[ high ];
            items[ high ] = tTempPivot;

            onSwap?.Invoke(tPivotFinalIndex , high);
            await UniTask.Delay(20);
        }

        return tPivotFinalIndex;
    }
}
