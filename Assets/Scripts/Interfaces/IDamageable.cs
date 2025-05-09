using System;
using UnityEngine;

public interface IDamageable {

	void Die() {
		Debug.LogWarning("Die() method not implemented in " + this.GetType().Name);
	}

}
