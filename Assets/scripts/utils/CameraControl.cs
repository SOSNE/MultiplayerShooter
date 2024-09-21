using UnityEngine;

public class CameraControl : MonoBehaviour
{
    void Start()
    {
        float screenAspect = (float)Screen.width / Screen.height;

        // Calculate the scaling needed to adjust the viewport
        float scale = screenAspect /(16f / 9f);
        
        print(scale + "scale");

        if (scale < 1.0f)
        {
            //when screen is higher then wider
            Camera.main.rect = new Rect(0, (1f - scale) / 2f, 1, scale);
        }
        else
        {
            //when screen is wider then higher
            float widthScale = (16f / 9f) / screenAspect;
            Camera.main.rect = new Rect((1f - widthScale) / 2f, 0f, widthScale, 1f);
        }
    }
}
