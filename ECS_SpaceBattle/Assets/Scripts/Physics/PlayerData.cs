using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Transforms;

public struct PlayerData : IComponentData
{
    public Translation Position;
}
