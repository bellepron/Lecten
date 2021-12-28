using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Ball Settings", order = 1)]
public class BallSettings : ScriptableObject
{
    public float swipeSensivity_X = 30;
    public float swipeSensivity_Y = 300;
    public float bounceVelocityMag = 3;
}