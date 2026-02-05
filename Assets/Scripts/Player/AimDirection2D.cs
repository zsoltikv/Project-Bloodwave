using UnityEngine;

public class AimDirection2D : MonoBehaviour
{
    public Vector2 Direction { get; private set; } = Vector2.right;

    public void SetFromMove(Vector2 move)
    {
        if (move.sqrMagnitude > 0.0001f)
            Direction = move.normalized;
    }
}