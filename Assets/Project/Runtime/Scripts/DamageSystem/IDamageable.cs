using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class OnEntityDamagedEventArgs : EventArgs {
    public IDamageDealer DamageDealerSource { get; set; }
    public int DamageAmount { get; set; }
    public Vector2 ContactPoint { get; set; }

    public OnEntityDamagedEventArgs(IDamageDealer _damageDealerSource = null, int _damageAmount = 0, Vector2 _contactPoint = default) {
        DamageDealerSource = _damageDealerSource;
        DamageAmount = _damageAmount;
        ContactPoint = _contactPoint;
    }
}

public interface IDamageable {
    public HealthSystem HealthSystem { get; set; }

    void Damage(object sender, OnEntityDamagedEventArgs entityDamagedArgs);
}
