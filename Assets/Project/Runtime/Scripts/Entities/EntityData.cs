using UnityEngine;
using ExtensionMethods;

public abstract class EntityData : ScriptableObject {
    public abstract void Init();
    public abstract void OnEnable();

    public bool copyHealthSO = false;
    public IntSO EntityHealth;
    public IntSO EntityMaxHealth;
}
