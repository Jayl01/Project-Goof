using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is just a data holding script that the map generator will be able to expect for placement assisting.
/// </summary>
public class InnerMapDetail : MonoBehaviour
{
    public int width;
    public int height;
    [Tooltip("The maximum amount of these that can spawn in a map. The amount that actually spawns will never be greater than this number, but can always be lower.")]
    public int expectedAmount;
    [Tooltip("The added height above the y-coordinate of the ground.")]
    public int heightAboveBase;
    [Tooltip("The variance of the height that this object can spawn at. It goes from 0 to x!")]
    public int heightVariance;
    [Tooltip("The minimum distance from all walls this object will spawn in.")]
    public int wallPadding;
    public int horizontalRotation;
    public int verticalRotation;
    public int horizontalRotationVariance;
    public int verticalRotationVariance;
    public bool canClip;
}
