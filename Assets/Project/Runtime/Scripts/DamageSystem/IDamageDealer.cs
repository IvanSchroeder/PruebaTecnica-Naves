using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;

public interface IDamageDealer {
    public Collider2D HitboxTrigger { get; set; }
    public LayerMask DamageablesLayers { get; set; }
    public int DamageDealerLayer { get; set; }
    public int DamageAmount { get; set; }

    void OnTriggerEnter2D(Collider2D collision) {}
}
