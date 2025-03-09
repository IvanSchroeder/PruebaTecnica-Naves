using UnityEngine;

[CreateAssetMenu(menuName = "Data/Color Data")]
public class ColorSO : ValueSO<Color> {
    [field: SerializeField] public string ColorName { get; private set; } = "White";
    
    public override void OnDisable() {
        if (resetsOnEnable) Value = Default;
    }

    public override void OnEnable() {
        if (resetsOnDisable) Value = Default;
    }

    public string GetColorName() {
        return ColorName;
    }
}
