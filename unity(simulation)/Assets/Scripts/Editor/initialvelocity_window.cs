using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class InitialVelocityWindow : EditorWindow
{
    private static List<Rigidbody> rigidBodies = new List<Rigidbody>();
    private Vector2 scroll;

    private HashSet<GameObject> selectedObjects = new HashSet<GameObject>();

    private Vector3 globalVelocity = Vector3.zero;
    private Vector3 globalAngular = Vector3.zero;

    [MenuItem("Window/Initial Velocity Setter")]
    static void Init()
    {
        GetWindow<InitialVelocityWindow>("Initial Velocity");
    }

    private void OnEnable()
    {
        RefreshList();
    }

    private void OnHierarchyChange()
    {
        RefreshList();
        Repaint();
    }

    private void RefreshList()
    {
        rigidBodies = GameObject.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None)
            .ToList();
    }

    private void OnGUI()
    {
        GUILayout.Label("Initial Velocity Control", EditorStyles.boldLabel);

        globalVelocity = EditorGUILayout.Vector3Field("Velocity", globalVelocity);
        globalAngular = EditorGUILayout.Vector3Field("Angular Vel.", globalAngular);

        if (GUILayout.Button("Apply to ALL"))
        {
            foreach (var rb in rigidBodies)
                ApplySetter(rb.gameObject);
        }

        GUILayout.Space(15);
        GUILayout.Label("Select individual rigidbodies", EditorStyles.boldLabel);

        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(250));

        foreach (var rb in rigidBodies)
        {
            if (rb == null) continue;

            bool selected = selectedObjects.Contains(rb.gameObject);
            bool toggled = EditorGUILayout.ToggleLeft(rb.name, selected);

            if (toggled) selectedObjects.Add(rb.gameObject);
            else selectedObjects.Remove(rb.gameObject);
        }

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Apply to SELECTED"))
        {
            foreach (var obj in selectedObjects)
                ApplySetter(obj);
        }
    }

    private void ApplySetter(GameObject obj)
    {
        var setter = obj.GetComponent<InitialVelocitySetter>();
        if (setter == null)
            setter = Undo.AddComponent<InitialVelocitySetter>(obj);

        setter.initialVelocity = globalVelocity;
        setter.initialAngularVelocity = globalAngular;

        EditorUtility.SetDirty(setter);
    }
}
