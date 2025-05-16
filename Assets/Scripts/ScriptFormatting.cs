using UnityEngine;

// This file is intended to be a format example for the rest of the scripts in the project.

public class ScriptFormatting : MonoBehaviour {

	// We use Regions to group related variables and methods together
	// We use tab spacing to indent code
	// We have a single blank line between regions and methods

	#region Variables

	// Variables and References go here
	// We comment to the right of each variable to explain its purpose
	// We use [Header] to group variables of a related nature together in the inspector
	// We use [SerializeField] to expose private variables in the inspector
	// we keep them nicely spaced and aligned for readability

	/*
	Example Variables:

	[Header("Component References")]
	private Animator _anim; // Reference to the animator component
	private Rigidbody2D _rb; // Reference to the rigidbody component

	[Header("Movement Properties")]
	[SerializeField] private float _moveSpeed = 5f; // Speed of the object
	[SerializeField] public float jumpForce = 10f; // Jump force of the object
	*/

	// We always define the access modifier of the variable, even if its private
	// We use camelCase for private and public variables, with an underscore at the start for private and protected. For example: _privateVariable
	// for const we use ALL_CAPS_WITH_UNDERSCORES. For example: CONST_VARIABLE

	#endregion

	#region Unity Methods

	// Unity native methods go here
	// We dont need a summary for these methods, as Unity already provides one
	// We do comment on any custom methods we call inside unity native methods, with short explanations

	#endregion

	#region Public Methods

	// Public methods go here
	// We make a summary for each method to explain its purpose
	// If the method begins to get longer, we add comments inside the method to explain what each part does and why

	// Example of a Summary:

	/// <summary>
	/// Brief description of what the method does.
	/// </summary>
	/// <param name="paramName">Description of parameters</param>
	/// <returns>Description of return value</returns>


	#endregion

	#region Private Methods

	// Custom private methods go here
	// We make a summary for each method to explain its purpose
	// If the method begins to get longer, we add comments inside the method to explain what each part does and why

	// Example of a Summary:

	/// <summary>
	/// Brief description of what the method does.
	/// </summary>
	/// <param name="paramName">Description of parameters</param>
	/// <returns>Description of return value</returns>

	#endregion

	#region Coroutines

	// Coroutines go here
	// We make a summary for each coroutine to explain its purpose
	// If the coroutine begins to get longer, we add comments inside the coroutine to explain what each part does and why

	// Example of a Summary:

	/// <summary>
	/// Brief description of what the coroutine does.
	/// </summary>
	/// <param name="paramName">Description of parameters</param>
	/// <returns>Description of return value</returns>

	#endregion



}
