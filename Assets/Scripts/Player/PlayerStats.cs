using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int XP { get; private set; } = 0;
    public int Coins { get; private set; } = 0;

    public void AddXP(int amount)
    {
        XP += amount;
        Debug.Log($"XP gained: {amount}. Total XP: {XP}");
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
        Debug.Log($"Coins gained: {amount}. Total Coins: {Coins}");
    }
}