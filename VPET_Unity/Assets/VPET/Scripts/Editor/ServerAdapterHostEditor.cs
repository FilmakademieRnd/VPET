using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace vpet
{
    [CustomEditor(typeof(ServerAdapterHost))]
    public class ServerAdapterHostEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            ServerAdapterHost adapter = (ServerAdapterHost)target;

            EditorGUILayout.Space();
            adapter.enableVIVE = EditorGUILayout.BeginToggleGroup("VIVE Integration", adapter.enableVIVE);
            adapter.selectionMode = (ServerAdapterHost.VIVESelection)EditorGUILayout.EnumPopup("Selection Mode", adapter.selectionMode);
            adapter.updateRate = EditorGUILayout.Slider(new GUIContent("Update Rate", "Updates per second"), adapter.updateRate, 1, 90);
            adapter.fileOutput = EditorGUILayout.Toggle(new GUIContent("File Output", "Writes tracker data to file (recording)"), adapter.fileOutput);
            adapter.seperator = EditorGUILayout.TextField(new GUIContent("Seperator", "Value seperator in file"), adapter.seperator);
            adapter.seperator = (adapter.seperator != CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator) ? adapter.seperator : "|";
            adapter.seperator = (adapter.seperator.Length > 1) ? adapter.seperator.Substring(0,1) : adapter.seperator;
            adapter.precision = EditorGUILayout.IntSlider(new GUIContent("Precision", "Places after the decimal"), adapter.precision, 1, 8);
            adapter.point = EditorGUILayout.TextField(new GUIContent("Point", "Decimal point seperator"), adapter.point);
            adapter.point = (adapter.point.Length > 1) ? adapter.point.Substring(0, 1) : adapter.point;
            EditorGUILayout.EndToggleGroup();

            if (!adapter.enableVIVE)
                EditorGUILayout.HelpBox("To use VIVE integration, it must be enabled!", MessageType.Info);
        }
    }
}