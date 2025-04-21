using AdsAppView.Utility;
using UnityEditor;
using UnityEngine;

namespace AdsAppView.Editors
{
    [CustomEditor(typeof(BuildVersionHolder))]
    class BuildVersionHolderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Apply to project settings"))
                ((BuildVersionHolder)target).ApplyToProjectSettings();
        }
    }
}
