using HM.CodeBase;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class SortApp_View : AView
{
    [Header("Visualization Area")]
    [SerializeField] private RectTransform _container; // 슬롯이 배치될 캔버스 영역
    [SerializeField] private SortSlot_view _slotPrefab; // 슬롯 프리팹

    [Header("Controls")]
    [SerializeField] private Button _sliceButton; // [New] 슬라이스 버튼
    [SerializeField] private Button _sortButton;
    [SerializeField] private Button _resetButton;
    [SerializeField] private Button _randomButton;

    // Presenter가 구독할 이벤트
    public event Action OnSliceActioned;
    public event Action OnSortActioned;
    public event Action OnResetActioned;
    public event Action OnRandomActioned;

    private List<SortSlot_view> _spawnedSlots = new List<SortSlot_view>();
    private float _slotWidth;

    public override void Open()
    {
        base.Open();
    }

    private void Awake()
    {
        // 버튼 이벤트 연결
        if ( _sliceButton != null ) _sliceButton.onClick.AddListener(() => OnSliceActioned?.Invoke());
        if ( _sortButton != null ) _sortButton.onClick.AddListener(() => OnSortActioned?.Invoke());
        if ( _resetButton != null ) _resetButton.onClick.AddListener(() => OnResetActioned?.Invoke());

        _randomButton.onClick.AddListener(()=>OnRandomActioned?.Invoke());
    }

    // 데이터(잘린 이미지들)를 받아 화면에 배치
    public void GenerateSlots(SortItem[] items)
    {
        // 1. 초기화
        ClearSlots();

        if ( items == null || items.Length == 0 ) return;

        // 2. 슬롯 너비 계산 (컨테이너 너비 / 개수)
        float tContainerWidth = _container.rect.width;
        float tContainerHeight = _container.rect.height;
        _slotWidth = tContainerWidth / items.Length;

        // 3. 생성 및 배치
        for ( int i = 0; i < items.Length; i++ )
        {
            SortSlot_view tSlot = Instantiate(_slotPrefab, _container);

            // 이미지 설정
            tSlot.SetSprite(items[ i ].SlicedSprite);
            tSlot.SetColor(Color.white);

            // RectTransform 설정
            RectTransform tRt = tSlot.RectTrans;
            tRt.anchorMin = new Vector2(0 , 1); // Top-Left
            tRt.anchorMax = new Vector2(0 , 1);
            tRt.pivot = new Vector2(0 , 1);
            tRt.sizeDelta = new Vector2(_slotWidth , tContainerHeight); // 높이는 꽉 차게

            // 위치 잡기
            UpdateSlotPosition(tSlot , i);

            _spawnedSlots.Add(tSlot);
        }
    }

    // 슬롯 위치 갱신 (인덱스 기반)
    public void UpdateSlotPosition(SortSlot_view slot , int index)
    {
        if ( slot == null ) return;
        float tPosX = index * _slotWidth;
        slot.RectTrans.anchoredPosition = new Vector2(tPosX , 0);
    }

    public void SwapSlots(int indexA , int indexB)
    {
        if ( !IsIndexValid(indexA) || !IsIndexValid(indexB) ) return;

        // 1. 리스트 내 순서 스왑
        SortSlot_view tTemp = _spawnedSlots[indexA];
        _spawnedSlots[ indexA ] = _spawnedSlots[ indexB ];
        _spawnedSlots[ indexB ] = tTemp;

        // 2. 화면상 위치(Position) 스왑
        UpdateSlotPosition(_spawnedSlots[ indexA ] , indexA);
        UpdateSlotPosition(_spawnedSlots[ indexB ] , indexB);
    }

    public void HighlightSlot(int index , Color color)
    {
        if ( IsIndexValid(index) )
        {
            _spawnedSlots[ index ].SetColor(color);
        }
    }

    /// <summary>
    /// [New] 모든 슬롯의 색상을 원래대로(흰색) 초기화합니다.
    /// Presenter의 코드를 줄여주는 헬퍼 메서드입니다.
    /// </summary>
    public void ResetAllSlotsColor()
    {
        foreach ( var slot in _spawnedSlots )
        {
            if ( slot != null ) slot.SetColor(Color.white);
        }
    }

    public void OnSortFinished()
    {
        // 정렬 완료 시 전체 초록색 점등
        foreach ( var slot in _spawnedSlots )
        {
            if ( slot != null ) slot.SetColor(Color.green);
        }
    }

    public void SetInputInteractive(bool isInteractive)
    {
        // 정렬 중 버튼 클릭 방지
        if ( _sliceButton != null ) _sliceButton.interactable = isInteractive;
        if ( _randomButton != null ) _randomButton.interactable = isInteractive;
        if ( _sortButton != null ) _sortButton.interactable = isInteractive;
        if ( _resetButton != null ) _resetButton.interactable = isInteractive;
    }
    private bool IsIndexValid(int index)
    {
        return index >= 0 && index < _spawnedSlots.Count;
    }

    private void ClearSlots()
    {
        foreach ( var slot in _spawnedSlots )
        {
            if ( slot != null ) Destroy(slot.gameObject);
        }
        _spawnedSlots.Clear();
    }

    private void OnDestroy()
    {
        _sliceButton?.onClick.RemoveAllListeners();
        _sortButton?.onClick.RemoveAllListeners();
        _resetButton?.onClick.RemoveAllListeners();
    }
}
