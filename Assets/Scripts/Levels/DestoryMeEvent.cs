using UnityEngine;

public class DestoryMeEvent : MonoBehaviour {

	#region Private Methods

	/// <summary>
	/// Destroys this game object - called by animation events
	/// </summary>
	private void DestoryMe() => Destroy(gameObject);

	#endregion
}
