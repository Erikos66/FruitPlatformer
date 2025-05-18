using UnityEditor;

public static class InspectorLockToggle
{
	[MenuItem("Tools/Toggle Inspector Lock %e")] // Ctrl + E
	private static void ToggleLock()
	{
		var inspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
		var window = EditorWindow.GetWindow(inspectorType);

		var isLockedProperty = inspectorType.GetProperty("isLocked", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
		bool currentLock = (bool)isLockedProperty.GetValue(window, null);
		isLockedProperty.SetValue(window, !currentLock, null);

		window.Repaint();
	}
}
