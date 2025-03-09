using ExtensionMethods;
using UnityEngine;

public class ScreenWrap : MonoBehaviour {
    private Rigidbody2D Rb;

    void Start() {
        if (Rb.IsNull()) Rb = this.GetComponentInHierarchy<Rigidbody2D>();
    }

    void Update() {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);

        float rightSideOfScreenInWorld = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height)).x;
        float leftSideOfScreenInWorld = Camera.main.ScreenToWorldPoint(new Vector2(0f, 0f)).x;
        float topOfScreenInWorld = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height)).y;
        float bottomOfScreenInWorld = Camera.main.ScreenToWorldPoint(new Vector2(0f, 0f)).y;

        if (screenPos.x <= 0 && Rb.linearVelocity.x < 0) {
            transform.position = new Vector2(rightSideOfScreenInWorld, transform.position.y);
        }
        else if (screenPos.x >= Screen.width && Rb.linearVelocity.x > 0) {
            transform.position = new Vector2(leftSideOfScreenInWorld, transform.position.y);
        }
        else if (screenPos.y >= Screen.height && Rb.linearVelocity.y > 0) {
            transform.position = new Vector2(transform.position.x, bottomOfScreenInWorld);
        }
        else if (screenPos.y <= 0 && Rb.linearVelocity.y < 0) {
            transform.position = new Vector2(transform.position.x, topOfScreenInWorld);
        }
    }
}
