using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PropSortingRegister : MonoBehaviour
{
    public Transform propBottom;

    void Awake()
    {
        if (propBottom == null)
        {
            Debug.LogError($"{name}: PropBottom missing!");
            return;
        }

        int order =
            SortingBandManager.Instance.GetSortingOrder(propBottom.position.y);

        GetComponent<SpriteRenderer>().sortingOrder = order;
    }
}
