# Unity Project Code Style Guide

This document outlines the coding standards for our Unity project. Please follow these guidelines when writing or modifying code. When using Copilot or other AI assistants, refer to this document for the expected formatting.

## General Formatting

- Use tab indentation (4 spaces)
- Place opening braces on the same line as statements (`if (condition) {`)
- Use a single blank line between regions and methods
- Always add access modifiers (public, private, etc.) even when default

## Naming Conventions

- **Private Variables**: Use camelCase with underscore prefix (`_privateVariable`)
- **Protected Variables**: Use camelCase with underscore prefix (`_protectedVariable`)
- **Public Variables**: Use camelCase without prefix (`publicVariable`)
- **Constants**: Use ALL_CAPS_WITH_UNDERSCORES (`CONST_VARIABLE`)
- **Methods**: Use PascalCase (`PublicMethod`)

## Comments and Documentation

- Add a comment to the right of each variable explaining its purpose
- Use XML documentation for methods (except Unity built-in methods)
- For longer methods, add comments explaining what each part does

## Code Organization

- Use `#region` directives to organize code into logical sections:
  - Variables
  - Unity Methods
  - Public Methods
  - Private Methods
  - Coroutines

## Unity-Specific

- Use `[Header]` attributes to group related variables in the inspector
- Use `[SerializeField]` to expose private variables in the inspector
- Keep variables nicely spaced and aligned for readability

## Example

```csharp
public class ExampleClass : MonoBehaviour {

    #region Variables
    
    [Header("Component References")]
    private Animator _anim;         // Reference to the animator component
    private Rigidbody2D _rb;        // Reference to the rigidbody component
    
    [Header("Movement Properties")]
    [SerializeField] private float _moveSpeed = 5f;  // Speed of the object
    [SerializeField] public float jumpForce = 10f;   // Jump force of the object
    
    private const float MAX_SPEED = 10f;  // Maximum allowed speed
    
    #endregion
    
    #region Unity Methods
    
    private void Start() {
        _rb = GetComponent<Rigidbody2D>();    // Get reference to rigidbody
        _anim = GetComponent<Animator>();     // Get reference to animator
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Makes the character jump with the specified force
    /// </summary>
    /// <param name="multiplier">Force multiplier for the jump</param>
    public void Jump(float multiplier) {
        // Apply vertical force
        _rb.AddForce(Vector2.up * jumpForce * multiplier, ForceMode2D.Impulse);
    }
    
    #endregion
}
```

See `ScriptFormatting.cs` for a complete reference on code style.
