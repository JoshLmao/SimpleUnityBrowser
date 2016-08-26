using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BrowserUI : MonoBehaviour
{

    public Canvas MainCanvas;
    public InputField UrlField;
    public Image Background;
    public bool KeepUIVisible = false;


   

    public void Show()
    {
        UrlField.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
        UrlField.placeholder.gameObject.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
        UrlField.textComponent.gameObject.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
        Background.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
    }

    public void Hide()
    {
        if (!KeepUIVisible)
        { 
            if (!UrlField.isFocused)
            {
                UrlField.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
                UrlField.placeholder.gameObject.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
                UrlField.textComponent.gameObject.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
                Background.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
            }
            else
            {
                Show();
            }
        }
    }



    void Update()
    {
        if (UrlField.isFocused&&!KeepUIVisible)
        {
            Show();
        }
    }


}
