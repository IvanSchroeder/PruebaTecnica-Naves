using UnityEngine;
using ExtensionMethods;
using Utilities;

public class ShootingController : MonoBehaviour {
    [UnityEngine.Range(0.01f, 2.0f)] public float shootRate = 0.2f;

    public GameObject ProjectilePrefab;

    CountdownTimer shootRateTimer;
    public bool canShoot = true;
    public bool isShooting = false;

    protected void OnEnable() {
        PlayerInputHandler.OnShootStart += SetShootingState;
        PlayerInputHandler.OnShootStop += SetShootingState;

        shootRateTimer = new CountdownTimer(shootRate);
        shootRateTimer.OnTimerStart += () => canShoot = false;
        shootRateTimer.OnTimerStop += () => canShoot = true;
    }

    protected void OnDisable() {
        PlayerInputHandler.OnShootStart -= SetShootingState;
        PlayerInputHandler.OnShootStop -= SetShootingState;

        shootRateTimer.OnTimerStart -= () => canShoot = false;
        shootRateTimer.OnTimerStop -= () => canShoot = true;
    }

    protected void Start() {
        shootRateTimer.Start();
    }

    protected void Update() {
        CheckShootingConditions();
    }

    void SetShootingState(bool inputState) => isShooting = inputState;

    void CheckShootingConditions() {
        shootRateTimer.Tick(Time.deltaTime);

        if (isShooting && canShoot) {
            Shoot();
            shootRateTimer.Restart();
        }
    }

    void Shoot() {
        GameObject projectile = Instantiate(ProjectilePrefab, transform.position, transform.parent.rotation);
        AudioManager.instance.PlaySFX("ProjectileShot");
    }
}
