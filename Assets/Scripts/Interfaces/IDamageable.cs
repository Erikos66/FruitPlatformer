using UnityEngine;

// Interface is intended to be implemented by any class that can take damage

public interface IDamageable {

	void Die() {

		// Fallback method in case the class does not implement the Die() method
		Debug.LogWarning("Die() method not implemented in " + this.GetType().Name);
	}

}
