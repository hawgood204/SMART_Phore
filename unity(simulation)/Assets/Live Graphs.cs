using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LiveGraph : MonoBehaviour
{
    public ForceAndImpulseMonitor monitor; // 측정 대상
    public RawImage graphImage;            // 패널 안의 RawImage
    public Color forceColor = Color.green;
    public Color impulseColor = Color.red;

    Texture2D tex;
    int width = 300;
    int height = 100;

    void Start()
    {
        tex = new Texture2D(width, height);
        tex.filterMode = FilterMode.Point;
        graphImage.texture = tex;
    }

    void Update()
    {
        DrawGraph();
    }

    void DrawGraph()
    {
        // 배경 전체 지우기 (black)
        Color clear = new Color(0, 0, 0, 0.7f);
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                tex.SetPixel(x, y, clear);

        if (monitor == null) return;

        DrawLineGraph(monitor.forceHistory, forceColor);
        DrawLineGraph(monitor.impulseHistory, impulseColor);

        tex.Apply();
    }

    void DrawLineGraph(List<float> values, Color col)
    {
        if (values.Count < 2) return;

        float maxVal = Mathf.Max(0.01f, Mathf.Max(values.ToArray()));
        int sampleCount = Mathf.Min(values.Count, width);

        for (int i = 1; i < sampleCount; i++)
        {
            float v0 = values[i - 1] / maxVal; 
            float v1 = values[i] / maxVal;

            int y0 = Mathf.FloorToInt(v0 * (height - 1));
            int y1 = Mathf.FloorToInt(v1 * (height - 1));

            DrawLine(i - 1, y0, i, y1, col);
        }
    }

    void DrawLine(int x0, int y0, int x1, int y1, Color col)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            tex.SetPixel(x0, y0, col);

            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;

            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx)  { err += dx; y0 += sy; }
        }
    }
}
