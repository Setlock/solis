using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Planet))]
public class PlanetEditor : Editor
{
    Planet planet;
    Editor planetEditor;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        //If Generate Planet button is pressed then generate planet
        if (GUILayout.Button("Generate Planet"))
        {
            planet.Regenerate();
        }
        //Set GUI label to display as generated planet preview texture
        GUILayout.Label(planet.planetTexture);
        DrawSettingsEditor(planet.planetSettings, ref planet.settingsFoldout, ref planetEditor);
    }

    void DrawSettingsEditor(Object settings, ref bool foldout, ref Editor editor)
    {
        if (settings != null)
        {
            foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);
            if (foldout)
            {
                CreateCachedEditor(settings, null, ref editor);
                editor.OnInspectorGUI();
            }
        }
    }

    private void OnEnable()
    {
        planet = (Planet)target;
    }
}
