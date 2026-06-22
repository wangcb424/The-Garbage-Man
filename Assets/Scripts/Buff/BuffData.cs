using UnityEngine;

[CreateAssetMenu(fileName = "NewBuffData", menuName = "Game/Buff Data")]
public class BuffData : ScriptableObject
{
    public enum BuffType
    {
        Heal,
        MaxHealth,
        MoveSpeed,
        AttackSpeed,
        CritChance,
        BlockReduction
    }

    [Header("Basic Info")]
    public string buffName = "New Buff";

    [TextArea]
    public string description = "Buff description";

    public Sprite icon;

    [Header("Shop Settings")]
    public int cost = 10;

    [Header("Buff Effect")]
    public BuffType buffType = BuffType.MaxHealth;
    public float value = 1f;

    public void ApplyBuff()
    {
        if (GlobalRunManager.Instance == null)
        {
            Debug.Log("GlobalRunManager was not found.");
            return;
        }

        if (buffType == BuffType.Heal)
        {
            GlobalRunManager.Instance.HealPlayer(Mathf.RoundToInt(value));
        }
        else if (buffType == BuffType.MaxHealth)
        {
            GlobalRunManager.Instance.IncreaseMaxHealth(Mathf.RoundToInt(value));
        }
        else if (buffType == BuffType.MoveSpeed)
        {
            GlobalRunManager.Instance.IncreaseMoveSpeed(value);
        }
        else if (buffType == BuffType.AttackSpeed)
        {
            GlobalRunManager.Instance.IncreaseAttackSpeedMultiplier(value);
        }
        else if (buffType == BuffType.CritChance)
        {
            GlobalRunManager.Instance.IncreaseCritChance(value);
        }
        else if (buffType == BuffType.BlockReduction)
        {
            GlobalRunManager.Instance.IncreaseBlockDamageReduction(value);
        }

        Debug.Log("Applied buff: " + buffName);
    }

    public string GetEffectText()
    {
        if (buffType == BuffType.Heal)
        {
            return "Heal +" + Mathf.RoundToInt(value);
        }

        if (buffType == BuffType.MaxHealth)
        {
            return "Max Health +" + Mathf.RoundToInt(value);
        }

        if (buffType == BuffType.MoveSpeed)
        {
            return "Move Speed +" + value.ToString("F1");
        }

        if (buffType == BuffType.AttackSpeed)
        {
            return "Attack Speed +" + value.ToString("F2");
        }

        if (buffType == BuffType.CritChance)
        {
            return "Crit Chance +" + (value * 100f).ToString("F0") + "%";
        }

        if (buffType == BuffType.BlockReduction)
        {
            return "Block +" + (value * 100f).ToString("F0") + "%";
        }

        return "";
    }
}