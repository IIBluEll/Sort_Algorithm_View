using HM.CodeBase;
using UnityEngine;

public class SortApp_presenter : APresenter
{
    private readonly SortApp_View _view;
    private readonly ImageSlicer_model _slicerModel;

    // 테스트용 원본 텍스처 (실제로는 로드하거나 매니저에서 주입받을 수 있음)
    private Texture2D _sourceTexture;
    private const int DEFAULT_SLICE_COUNT = 30; // 일단 고정 개수 10개

    // 현재 정렬 데이터
    private SortItem[] _currentItems;

    public SortApp_presenter(SortApp_View view , Texture2D sourceTexture)
    {
        _view = view;
        _sourceTexture = sourceTexture;

        // 모델 생성
        _slicerModel = new ImageSlicer_model();

        ConnectEvents();
    }

    private void ConnectEvents()
    {
        _view.OnSliceActioned += OnSliceActioned;
        _view.OnSortActioned += OnSortActioned; // 추후 구현
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
        // 추후 정렬 로직 구현
        Debug.Log("Sort Requested");
    }
}
