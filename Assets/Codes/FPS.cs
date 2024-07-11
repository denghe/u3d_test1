using UnityEngine;
public class FPSDisplay : MonoBehaviour
{
    private float deltaTime = 0.0f;

    [Tooltip("值越大，越小")]
    public int size = 20;

    private void Awake()
    {
        //Application.targetFrameRate = -1;
    }

    void Update()
    {
        // 计算每帧之间的时间差  
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }
    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;
        GUIStyle style = new GUIStyle();
        Rect rect = new Rect(0, 0, w, h * 2 / size); // 设置帧率显示区域的位置和大小  
        //Rect rect = new Rect(0, 0, 200, 100); // 设置帧率显示区域的位置和大小  
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / size;
        style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
        float fps = 1.0f / deltaTime;
        float ms = deltaTime * 1000.0f;
        string text = string.Format("{0:0.} FPS | {1:0.} ms", fps, ms);
        GUI.Label(rect, text, style);
    }
}
