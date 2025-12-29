using UnityEngine;
using UnityEngine.UI;

public class SortSlot_view : MonoBehaviour
{
    [SerializeField] private Image _image;
    private RectTransform _rectTransform;

    // 자주 쓰는 컴포넌트 캐싱 (프로퍼티 활용)
    public RectTransform RectTrans
    {
        get
        {
            if ( _rectTransform == null )
                _rectTransform = GetComponent<RectTransform>();
            return _rectTransform;
        }
    }

    // 이미지 설정
    public void SetSprite(Sprite sprite)
    {
        if ( _image != null )
            _image.sprite = sprite;
    }

    // 색상 변경
    public void SetColor(Color color)
    {
        if ( _image != null )
            _image.color = color;
    }
}
