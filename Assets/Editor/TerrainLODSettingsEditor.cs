using Components.ProceduralGeneration.SimpleRoomPlacement;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainFNL))]
public class TerrainLODSettingsEditor : Editor
{
    private const float BarHeight = 30f;
    private const float leftspacing = 5f;
    private const float HandleWidth = 5f;
    private const float HandledrawWidth = 2f;

    private int draggingLOD = -1;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty Terrainlevels = serializedObject.FindProperty("Terrainlevels");

        EditorGUILayout.LabelField("Terrain Distribution", EditorStyles.boldLabel);
        Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(BarHeight));
        rect.width -= leftspacing;
        DrawLODBar(rect, Terrainlevels);

        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(Terrainlevels, true);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawLODBar(Rect rect, SerializedProperty Terrainlevels)
    {
        if (Terrainlevels.arraySize == 0) return;

        Event e = Event.current;
        float x = rect.x;
        float totalWidth = rect.width;
        float prevPercent = 0f;

        for (int i = 0; i < Terrainlevels.arraySize; i++)
        {
            SerializedProperty level = Terrainlevels.GetArrayElementAtIndex(i);
            float percent = Mathf.Clamp((level.FindPropertyRelative("transitionHeight").floatValue+1)/2,-1f,1f);

            float width = totalWidth * (percent - prevPercent);
            Rect lodRect = new Rect(x, rect.y, totalWidth * percent - (x - rect.x), BarHeight);

            // Couleur
            Color color = level.FindPropertyRelative("color").colorValue;
            EditorGUI.DrawRect(lodRect, color);

            // Texte
            GUI.Label(lodRect, level.FindPropertyRelative("name").stringValue, EditorStyles.whiteLabel);

            // Handle (curseur)
            if (i < Terrainlevels.arraySize - 1)
            {
                float handleX = rect.x + totalWidth * percent - HandleWidth / 2f;
                float handleDrawX = rect.x + totalWidth * percent - HandledrawWidth / 2f;
                Rect handleRect = new Rect(handleX, rect.y, HandleWidth, BarHeight);
                Rect handleDrawRect = new Rect(handleDrawX, rect.y, HandledrawWidth, BarHeight);

                EditorGUIUtility.AddCursorRect(handleRect, MouseCursor.ResizeHorizontal);

                // Interaction souris
                if (e.type == EventType.MouseDown && handleRect.Contains(e.mousePosition))
                {
                    draggingLOD = i;
                    e.Use();
                }

                if (draggingLOD == i && e.type == EventType.MouseDrag)
                {
                    float mousePercent = Mathf.InverseLerp(rect.x, rect.xMax, e.mousePosition.x);
                    mousePercent = Mathf.Clamp(mousePercent*2-1, -1f, 1f);

                    float minvalue = -1f;

                    if (i > 0)
                    {
                        minvalue = Terrainlevels.GetArrayElementAtIndex(i - 1).FindPropertyRelative("transitionHeight").floatValue;
                    }

                    float maxvalue = Terrainlevels.GetArrayElementAtIndex(i + 1).FindPropertyRelative("transitionHeight").floatValue;

                    mousePercent = Mathf.Clamp(mousePercent, minvalue, maxvalue);
                    level.FindPropertyRelative("transitionHeight").floatValue = mousePercent;

                    serializedObject.ApplyModifiedProperties();
                    e.Use();
                }

                if (e.type == EventType.MouseUp && draggingLOD == i)
                {
                    draggingLOD = -1;
                    e.Use();
                }

                // Dessin du curseur
                EditorGUI.DrawRect(handleDrawRect, Color.black);
            }
            else
            {
                level.FindPropertyRelative("transitionHeight").floatValue = 1;
            }

            x = rect.x + totalWidth * percent;
            prevPercent = percent;
        }

        if (GUI.changed)
            serializedObject.ApplyModifiedProperties();
    }
}