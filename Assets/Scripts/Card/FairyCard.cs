using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static FairyGrowthUI;
using static UnityEngine.Rendering.DebugUI;

public class FairyCard : Card
{
    /// <summary>
    /// プレイヤーデータが変更されたときに発生するイベント。
    /// </summary>
    public GameEvent OnPlayerDataModified;

    public int Rank { get; set; } = 1;
    public Dictionary<int, Equipment> equipSocket = new Dictionary<int, Equipment>();

    [JsonIgnore]
    private bool isStatDirty = true;
    [JsonIgnore]
    private Stat _finalStat;

    [JsonIgnore]
    public Stat FinalStat
    {
        get
        {
            if (isStatDirty)
            {
                RecalculateAndCacheStats();
                isStatDirty = false;
            }
            return _finalStat;
        }
    }

    public FairyCard(int id)
    {
        PrivateID = ID = id;
        var table = DataTableMgr.GetTable<CharacterTable>();
        var stringTable = DataTableMgr.GetTable<StringTable>();

        PrivateID = ID = id;
        Name = stringTable.dic[table.dic[ID].CharName].Value;
        Grade = table.dic[ID].CharStartingGrade;
        Level = 1;
        Experience = 0;
        Grade = table.dic[ID].CharStartingGrade;
        IsUse = false;
        isStatDirty = true;
    }

    public void Init()
    {
        isStatDirty = true;
        Player.Instance.OnStatUpdate = () => isStatDirty = true;
    }

    public void LevelUp(int level, int exp)
    {
        Level = level;
        Experience = exp;
        isStatDirty = true;
        OnPlayerDataModified?.Raise();
    }

    public void GradeUp()
    {
        if (Grade >= 5)
            return;
        Grade++;
        OnPlayerDataModified?.Raise();
    }

    public void RankUp()
    {
        if (Rank >= 4)
            return;

        equipSocket.Clear();
        Rank++;
        isStatDirty = true;
        OnPlayerDataModified?.Raise();
    }

    public void SetEquip(int slotNum, Equipment equip)
    {
        equipSocket.TryAdd(slotNum, equip);
        equip.OnStatUpdate = () => isStatDirty = true;
        isStatDirty = true;
        OnPlayerDataModified?.Raise();
    }

    public Stat FairyStatCalculator(CharData data, int lv)
    {
        Stat result = new Stat();

        result.attack = data.CharAttack + data.CharAttackIncrease * lv - 1;
        result.pDefence = data.CharPDefence + data.CharPDefenceIncrease * lv - 1;
        result.mDefence = data.CharMDefence + data.CharMDefenceIncrease * lv - 1;
        result.hp = data.CharMaxHP + data.CharHPIncrease * lv - 1;
        result.criticalRate = data.CharCritRate;
        result.attackSpeed = data.CharSpeed;
        result.accuracy = data.CharAccuracy;
        result.avoid = data.CharAvoid;
        result.resistance = data.CharResistance;

        return result;
    }

    public Stat AbilityCalculator(Stat charStat, PlayerAbilityData abilityData)
    {
        Stat result = charStat;
        result.attack = charStat.attack * abilityData.AbilityAttack;
        result.hp = charStat.hp * abilityData.AbilityHP;
        result.pDefence = charStat.pDefence * abilityData.AbilityDefence;
        result.mDefence = charStat.mDefence * abilityData.AbilityDefence;
        result.criticalRate = charStat.criticalRate * abilityData.AbilityCriticalRate;
        return result;
    }

    private void RecalculateAndCacheStats()
    {
        var table = DataTableMgr.GetTable<CharacterTable>();
        var charStat = FairyStatCalculator(table.dic[ID], Level);
        charStat += RankStat(table.dic[ID]);

        var playerTable = DataTableMgr.GetTable<PlayerTable>();
        var playerAbilityTable = DataTableMgr.GetTable<PlayerAbilityTable>();
        if (playerAbilityTable.dic.TryGetValue(playerTable.dic[Player.Instance.Level].PlayerAbility, out var playerAbilityData))
        {
            charStat = AbilityCalculator(charStat, playerAbilityData);
        }

        charStat += TotalEquipmentStats();
        charStat.battlePower = BattlePowerCalculator(Rank, charStat, table.dic[ID].CharAttackFactor);

        _finalStat = charStat;
    }

    public float BattlePowerCalculator(int rank, Stat stat, float attackFactor)
    {
        return rank * 350 + stat.attack * attackFactor + stat.hp * 1.3f + (stat.pDefence + stat.mDefence + 100) * 1.4f;
    }

    public Stat TotalEquipmentStats()
    {
        var totalStat = new Stat();
        if (equipSocket.Count < 1)
            return totalStat;

        foreach (var equip in equipSocket)
        {
            totalStat += equip.Value.EquipStatCalculator();
        }
        return totalStat;
    }

    public Stat RankStat(CharData charData)
    {
        Stat result = new Stat();
        var equipTable = DataTableMgr.GetTable<EquipTable>();

        for (int i = 1; i < Rank; i++)
        {
            for (int j = 1; j < 7; j++)
            {
                var key = Convert.ToInt32($"30{charData.CharPosition}{j}0{i}");
                var equipData = equipTable.dic[key];
                var equipStat = EquipDataToStat(equipData);
                result += equipStat;
            }
        }
        return result;
    }

    public Stat EquipDataToStat(EquipData equipData)
    {
        Stat result = new Stat();

        result.attack = equipData.EquipAttack;
        result.pDefence = equipData.EquipPDefence;
        result.mDefence = equipData.EquipMDefence;
        result.hp = equipData.EquipMaxHP;
        result.criticalRate = equipData.EquipCriticalRate;
        result.attackSpeed = equipData.EquipAttackSpeed;
        result.accuracy = equipData.EquipAccuracy;
        result.avoid = equipData.EquipAvoid;
        result.resistance = equipData.EquipRegistance;

        return result;
    }
}

