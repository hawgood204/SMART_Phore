using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ForceGraphWindow : EditorWindow
{
    private static List<ForceSender> cachedSenders = new List<ForceSender>();
    private HashSet<string> selectedPaths = new HashSet<string>();
    private Vector2 scrollPos;

    [MenuItem("Window/Force Graph Pro")]
    static void Open()
    {
        var window = GetWindow<ForceGraphWindow>("Force Graph Pro");
        window.Show();
    }

    // -------------------------------------------------------
    // 초기 설정
    // -------------------------------------------------------
    private void OnEnable()
    {
        LoadSelection();
        EditorApplication.hierarchyChanged += RefreshSenderList;
        RefreshSenderList();
    }

    private void OnDisable()
    {
        SaveSelection();
        EditorApplication.hierarchyChanged -= RefreshSenderList;
    }

    // -------------------------------------------------------
    // ForceSender 목록 갱신
    // -------------------------------------------------------
    private static void RefreshSenderList()
    {
        cachedSenders = GameObject.FindObjectsByType<ForceSender>(FindObjectsSortMode.None)
            .Where(s => s != null)
            .ToList();
    }

    // -------------------------------------------------------
    // 선택 상태 저장/로드
    // -------------------------------------------------------
    private void LoadSelection()
    {
        string s = EditorPrefs.GetString("FG_Selected", "");
        selectedPaths = new HashSet<string>(
            s.Split('|').Where(str => !string.IsNullOrEmpty(str))
        );
    }

    private void SaveSelection()
    {
        EditorPrefs.SetString("FG_Selected", string.Join("|", selectedPaths));
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }

    // -------------------------------------------------------
    // GUI 메인
    // -------------------------------------------------------
    private void OnGUI()
    {
        GUILayout.Label("Force Graph (Overlay Mode)", EditorStyles.boldLabel);

        DrawSenderList();
        GUILayout.Space(15);
        DrawGraph();
    }

    // -------------------------------------------------------
    // ForceSender 선택 리스트
    // -------------------------------------------------------
    private void DrawSenderList()
    {
        GUILayout.Label("Select ForceSender(s):");

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.MaxHeight(250));

        foreach (var sender in cachedSenders)
        {
            if (sender == null) continue;

            string path = sender.GetHierarchyPath();
            bool isSelected = selectedPaths.Contains(path);

            bool newToggle = EditorGUILayout.ToggleLeft(path, isSelected);

            if (newToggle && !isSelected)
                selectedPaths.Add(path);
            else if (!newToggle && isSelected)
                selectedPaths.Remove(path);
        }

        EditorGUILayout.EndScrollView();
    }

    // -------------------------------------------------------
    // Force 그래프
    // -------------------------------------------------------
    private void DrawGraph()
    {
        if (!Application.isPlaying)
        {
            GUILayout.Label("Waiting for Play Mode...");
            return;
        }

        var activeSenders = cachedSenders
            .Where(s => s != null && selectedPaths.Contains(s.GetHierarchyPath()))
            .ToList();

        if (activeSenders.Count == 0)
        {
            GUILayout.Label("No ForceSender selected.");
            return;
        }

        // ---------------------------------------------------
        // 좌측 여백 확보
        // ---------------------------------------------------
        Rect graphRect = GUILayoutUtility.GetRect(650, 250);
        float leftMargin = 60f;
        graphRect.x += leftMargin;
        graphRect.width -= leftMargin;

        EditorGUI.DrawRect(graphRect, new Color(0.15f, 0.15f, 0.15f));

        Handles.BeginGUI();

        Color[] colors = {
            Color.white, Color.cyan, Color.green, Color.yellow,
            Color.red, Color.magenta, Color.blue
        };

        // 최대 Force 찾기
        float maxVal = activeSenders.Max(s =>
            (s.History != null && s.History.Count > 0) ? s.History.Max() : 0f
        );
        maxVal = Mathf.Max(maxVal, 0.001f);

        // ---------------------------------------------------
        // 그래프 선 그리기
        // ---------------------------------------------------
        int colorIndex = 0;

        foreach (var sender in activeSenders)
        {
            var data = sender.History;
            if (data == null || data.Count < 2) continue;

            Handles.color = colors[colorIndex % colors.Length];
            colorIndex++;

            for (int i = 1; i < data.Count; i++)
            {
                float t1 = (float)(i - 1) / (data.Count - 1);
                float t2 = (float)i / (data.Count - 1);

                float x1 = graphRect.x + t1 * graphRect.width;
                float x2 = graphRect.x + t2 * graphRect.width;

                float y1 = graphRect.y + graphRect.height -
                           (data[i - 1] / maxVal) * graphRect.height;

                float y2 = graphRect.y + graphRect.height -
                           (data[i] / maxVal) * graphRect.height;

                Handles.DrawLine(new Vector3(x1, y1), new Vector3(x2, y2));
            }
        }

        Handles.EndGUI();

        GUI.color = Color.white;

        // ================
        // 자동 눈금 생성
        // ================
        DrawAutoYTicks(graphRect, maxVal);

        int count = activeSenders[0].History.Count;
        float totalTime = count * Time.fixedDeltaTime;
        DrawAutoXTicks(graphRect, totalTime);

        // ---------------------------------------------------
        // 축 라벨
        // ---------------------------------------------------
        GUI.Label(
            new Rect(graphRect.x - 50, graphRect.y + graphRect.height / 2 - 10, 100, 20),
            "Force (N)"
        );

        GUI.Label(
            new Rect(graphRect.x + graphRect.width - 50, graphRect.y + graphRect.height + 5, 100, 20),
            "Time (s)"
        );

        // ---------------------------------------------------
        // Legend
        // ---------------------------------------------------
        float legendX = graphRect.x + graphRect.width - 150;
        float legendY = graphRect.y + 10;

        int legendIndex = 0;

        foreach (var sender in activeSenders)
        {
            EditorGUI.DrawRect(
                new Rect(legendX, legendY + legendIndex * 20, 15, 15),
                colors[legendIndex % colors.Length]
            );

            GUI.Label(
                new Rect(legendX + 20, legendY + legendIndex * 20, 150, 20),
                sender.name
            );

            legendIndex++;
        }
    }

    // -------------------------------------------------------
    // 자동 Y축 눈금
    // -------------------------------------------------------
    private void DrawAutoYTicks(Rect r, float maxVal)
    {
        float nice = GetNiceNumber(maxVal);

        for (float v = 0; v <= maxVal; v += nice)
        {
            float ratio = v / maxVal;
            float y = r.y + r.height - ratio * r.height;

            GUI.Label(new Rect(r.x - 45, y - 8, 40, 20), v.ToString("F1"));

            Handles.color = new Color(1, 1, 1, 0.12f);
            Handles.DrawLine(
                new Vector3(r.x, y),
                new Vector3(r.x + r.width, y)
            );
        }
    }

    // -------------------------------------------------------
    // 자동 X축 눈금
    // -------------------------------------------------------
    private void DrawAutoXTicks(Rect r, float totalTime)
    {
        float nice = GetNiceNumber(totalTime);

        for (float t = 0; t <= totalTime; t += nice)
        {
            float ratio = t / totalTime;
            float x = r.x + ratio * r.width;

            GUI.Label(new Rect(x - 10, r.y + r.height + 2, 60, 20), t.ToString("F2"));

            Handles.color = new Color(1, 1, 1, 0.12f);
            Handles.DrawLine(
                new Vector3(x, r.y),
                new Vector3(x, r.y + r.height)
            );
        }
    }

    // -------------------------------------------------------
    // "보기 좋은 간격" 생성 함수
    // -------------------------------------------------------
    private float GetNiceNumber(float range)
    {
        float exp = Mathf.Floor(Mathf.Log10(range));
        float baseVal = Mathf.Pow(10, exp);
        float fraction = range / baseVal;

        if (fraction <= 1.0f) return baseVal * 0.2f;
        if (fraction <= 2.0f) return baseVal * 0.5f;
        if (fraction <= 5.0f) return baseVal * 1.0f;

        return baseVal * 2.0f;
    }
}

// -------------------------------------------------------
// Transform 경로 헬퍼
// -------------------------------------------------------
public static class TransformPathExtension
{
    public static string GetHierarchyPath(this ForceSender s)
    {
        return s.transform.GetHierarchyPath();
    }

    public static string GetHierarchyPath(this Transform t)
    {
        if (t.parent == null)
            return t.name;

        return t.parent.GetHierarchyPath() + "/" + t.name;
    }
}
