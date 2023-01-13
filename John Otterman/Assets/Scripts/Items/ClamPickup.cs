using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClamPickup : ItemPickup
{
    protected override void OnPlayerReached()
    {
        GameManager.OnClamsGained(10);
    }
}
