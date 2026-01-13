using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PropFrontSorting : MonoBehaviour
{
    public Transform propBottom;

    SpriteRenderer propRenderer;
    SpriteRenderer playerRenderer;
    Transform playerFeet;

    void Awake()
    {
        propRenderer = GetComponent<SpriteRenderer>();
    }

    public void Init(Transform feet)
    {
        playerFeet = feet;

        if (playerFeet != null)
            playerRenderer = playerFeet.GetComponentInParent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (playerFeet == null || playerRenderer == null || propBottom == null)
            return;

        if (playerFeet.position.y > propBottom.position.y)
        {
            propRenderer.sortingOrder =
                playerRenderer.sortingOrder + 1;
        }
        else
        {
            propRenderer.sortingOrder =
                playerRenderer.sortingOrder - 1;
        }
    }
}
