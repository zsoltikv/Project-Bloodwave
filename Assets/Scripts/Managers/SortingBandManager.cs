using System.Collections.Generic;
using UnityEngine;

public class SortingBandManager : MonoBehaviour
{
    public static SortingBandManager Instance;

    [Header("Band Settings")]
    public float bandHeight = 0.1f;
    public int minOrder = -1500;
    public int maxOrder = 1500;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public int GetSortingOrder(float worldY)
    {
        int order = -Mathf.RoundToInt(worldY / bandHeight);
        return Mathf.Clamp(order, minOrder, maxOrder);
    }
}