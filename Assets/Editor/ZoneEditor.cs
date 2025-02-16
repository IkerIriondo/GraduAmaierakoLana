using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Zone))]
public class ZoneEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Zone zone = (Zone)target;

        DrawDefaultInspector();

        EditorGUILayout.LabelField("Waypoints Dictionary", EditorStyles.boldLabel);

        foreach(var pair in zone.waypointsDictionary)
        {
            EditorGUILayout.LabelField(pair.Key.ToString(), $"{pair.Value.Count} Waypoints");

            foreach(var waypoint in pair.Value)
            {
                EditorGUILayout.ObjectField(waypoint.name, waypoint, typeof(Waypoint), true);
            }
        }
    }
}
