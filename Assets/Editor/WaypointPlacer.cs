using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SecurityBotController))]
public class WaypointPlacerEditor : Editor
{
    private bool placingMode = false;

    void OnSceneGUI()
    {
        SecurityBotController bot = (SecurityBotController)target;

        if (!placingMode) return;

        Event e = Event.current;

        // LEFT CLICK to place waypoint
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Create waypoint object
                GameObject wp = new GameObject("Waypoint_" + bot.patrolPoints.Length);
                wp.transform.position = hit.point;
                wp.transform.SetParent(bot.transform.parent);

                // Add to patrol array
                var list = new System.Collections.Generic.List<Transform>(bot.patrolPoints);
                list.Add(wp.transform);
                bot.patrolPoints = list.ToArray();

                EditorUtility.SetDirty(bot);

                e.Use();
            }
        }

        // ESC to exit placing mode
        if (e.keyCode == KeyCode.Escape)
        {
            placingMode = false;
            SceneView.RepaintAll();
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        if (!placingMode)
        {
            if (GUILayout.Button("➕ Start Placing Waypoints (Click In Scene)"))
            {
                placingMode = true;
            }
        }
        else
        {
            if (GUILayout.Button("⛔ Stop Placing Waypoints"))
            {
                placingMode = false;
            }
        }
    }
}
