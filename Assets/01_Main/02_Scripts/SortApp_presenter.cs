using HM.CodeBase;
using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

public interface ISortStrategy
{
    /// <summary>
    /// 정렬 알고리즘을 비동기로 수행합니다.
    /// </summary>
    /// <param name="items">정렬 대상 데이터</param>
    /// <param name="onCompare">두 인덱스 비교 시 호출</param>
    /// <param name="onSwap">두 인덱스 교환 시 호출</param>
    UniTask SortAsync(SortItem[] items , Action<int , int> onCompare , Action<int , int> onSwap);
}
public class SortApp_presenter : APresenter
{
    private readonly SortApp_View _view;
    private readonly ImageSlicer_model _slicerModel;
    private readonly SortAlgorithm_model _sortModel;

    // 테스트용 원본 텍스처 (실제로는 로드하거나 매니저에서 주입받을 수 있음)
    private Texture2D _sourceTexture;
    private const int DEFAULT_SLICE_COUNT = 50; // 일단 고정 개수 10개

    // 현재 정렬 데이터
    private SortItem[] _currentItems;
    private bool _isSorting = false;

    public SortApp_presenter(SortApp_View view , Texture2D sourceTexture)
    {
        _view = view;
        _sourceTexture = sourceTexture;

        // 모델 생성
        _slicerModel = new ImageSlicer_model();
        _sortModel = new SortAlgorithm_model();

        ConnectEvents();
    }

    private void ConnectEvents()
    {
        _view.OnSliceActioned += OnSliceActioned;
        _view.OnSortActioned += OnSortActioned; // 추후 구현
        _view.OnRandomActioned += OnRandomActioned; // [New] 이벤트 연결
        _view.OnResetActioned += OnResetActioned;   // [New] (구현 예정)
    }

    public override void Open()
    {
        _view.Open();
    }

    public override void Close()
    {
        _view.Close();
    }

    public override void Dispose()
    {
        if ( _view != null )
        {
            _view.OnSliceActioned -= OnSliceActioned;
            _view.OnSortActioned -= OnSortActioned;
            _view.OnRandomActioned -= OnRandomActioned; // [New] 이벤트 연결
            _view.OnResetActioned -= OnResetActioned;   // [New] (구현 예정)
        }
    }

    // --- Event Handlers ---

    private void OnSliceActioned()
    {
        if ( _sourceTexture == null )
        {
            Debug.LogError("Source Texture is missing!");
            return;
        }

        // 1. 모델에게 슬라이스 요청
        _currentItems = _slicerModel.Slice(_sourceTexture , DEFAULT_SLICE_COUNT);

        // 2. 뷰에게 생성 요청
        _view.GenerateSlots(_currentItems);

        Debug.Log($"Sliced image into {DEFAULT_SLICE_COUNT} pieces.");
    }

    private void OnSortActioned()
    {
        if ( _isSorting ) return;
        if ( _currentItems == null ) return;

        // 비동기 프로세스 시작
        RunSortProcess_async().Forget();
    }

    private void OnRandomActioned()
    {
        if ( _currentItems == null || _currentItems.Length == 0 )
        {
            Debug.LogWarning("섞을 데이터가 없습니다. 먼저 Slice를 하세요.");
            return;
        }

        // 1. 모델에게 섞기 요청 (데이터 순서 변경)
        _sortModel.Shuffle(_currentItems);

        // 2. View에게 변경된 순서대로 다시 그려달라고 요청
        // (프로토타입이므로 즉시 갱신 방식인 GenerateSlots 재사용)
        _view.GenerateSlots(_currentItems);
    }

    // [New] 리셋 (선택 사항: 다시 정렬된 상태로 되돌리기 등)
    private void OnResetActioned()
    {
        // 필요하다면 다시 Slice를 호출하거나 정렬 로직 수행
        OnSliceActioned();
    }

    // 정렬 프로세스 실행
    private async UniTaskVoid RunSortProcess_async()
    {
        _isSorting = true;
        _view.SetInputInteractive(false); // UI 잠금

        // 1. 사용할 전략 선택 (여기서 BubbleSort 선택)
        ISortStrategy tStrategy = new BubbleSort();

        // 2. 모델에게 실행 위임
        await _sortModel.ExecuteSortAsync(
            tStrategy ,
            _currentItems ,
            onCompare: (idxA , idxB) =>
            {
                // 비교 시각화: 빨간색 강조
                _view.ResetAllSlotsColor();
                _view.HighlightSlot(idxA , Color.red);
                _view.HighlightSlot(idxB , Color.blue);
            } ,
            onSwap: (idxA , idxB) =>
            {
                // 교환 시각화: 위치 변경
                _view.SwapSlots(idxA , idxB);
            } ,
            onComplete: () =>
            {
                // 완료 시각화: 초록색 점등
                //ResetSlotColors();
                _view.OnSortFinished();
            }
        );

        _isSorting = false;
        _view.SetInputInteractive(true); // UI 잠금 해제
    }
}
