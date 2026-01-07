using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHp = 20;
    public float Hp { get; private set; }
    public bool IsDead => Hp <= 0;

    private void Awake() => Hp = maxHp;

    public float TakeDamage(float amount)
    {
        if (IsDead) return 0;
        float before = Hp;
        Hp = Mathf.Max(0, Hp - amount);
        return before - Hp; // ténylegesen levont sebzés
    }
}
