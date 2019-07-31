using UnityEditor;
using UnityEngine;

public abstract class VGDLPhysics
{
    public static string GRID = "GRID";
    public static string CONT = "CONT";

    public Vector2 gridSize;
    public abstract VGDLMovementTypes passiveMovement(VGDLSprite sprite);
    public abstract VGDLMovementTypes activeMovement(VGDLSprite sprite, Vector2 action, float speed = -1);
    public abstract float distance(Rect r1, Rect r2);
}


public enum VGDLMovementTypes
{
    STILL,
    ROTATE,
    MOVE
}