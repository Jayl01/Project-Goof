using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is just a data holding script that the map generator will be able to expect for placement assisting.
/// </summary>
public class MapDetail : MonoBehaviour
{
    public int width;
    public int height;
    [Tooltip("The maximum amount of these that can spawn in a map. The amount that actually spawns will never be greater than this number, but can always be lower.")]
    public int expectedAmount;
    [Tooltip("The y-coordinate that this object can spawn at.")]
    public int altitude;
    [Tooltip("The variance of the altitude that this object can spawn at. It goes from -x to x!")]
    public int altitudeVariance;
    [Tooltip("Whether or not this object is able to spawn regardless of whether or not a tile already exists in those x-z coordinates.")]
    public bool canSpawnOverTiles;
    public bool canClip;
    public int horizontalRotation;
    public int verticalRotation;
    public int horizontalRotationVariance;
    public int verticalRotationVariance;
}
