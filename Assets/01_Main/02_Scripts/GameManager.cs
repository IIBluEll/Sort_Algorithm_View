using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Texture2D SourceImage;
    public SortApp_View _view;

    private void Start()
    {
        SortApp_presenter sortApp = new(_view,SourceImage);
    }
}
