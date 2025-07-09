using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FairyLevelUpView : MonoBehaviour
{
    private FairyCard card;
    private CharData charData;
    private ExpTable expTable;

    // --- Simulation State ---
    private int sampleLv;
    private int sampleExp;
    private int tempExp = 0;
    private int isBonusCount;
    
    // --- Item Management ---
    private List<ItemButton> itemButtons = new List<ItemButton>();
    private List<Item> selectedItems = new List<Item>();

    [Header("Prefabs")]
    public GameObject itemButtonPrefab;

    [Header("UI Components")]
    public TextMeshProUGUI lvText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI pDefenceText;
    public TextMeshProUGUI mDefenceText;
    public TextMeshProUGUI expText;
    public Image expSlider;
    public Transform spiritStoneSpace;
    public Button lvUpButton;
    public ParticleSystem lvUpParticle;

    public void Init(FairyCard fairyCard, CharData characterData)
    {
        card = fairyCard;
        charData = characterData;
        expTable = DataTableMgr.GetTable<ExpTable>();
        
        gameObject.SetActive(true);
        SetLvUpView();
    }

    public void Deactive()
    {
        gameObject.SetActive(false);
    }

    private void SetLvUpView()
    {
        ClearSpiritStoneScrollView();
        SetSample();
        SetSpiritStoneScroolView();
        RecalculateSimulation(); // Initial calculation
    }

    private void SetSample()
    {
        selectedItems.Clear();
        // Reset simulation variables to the card's actual state
        sampleLv = card.Level;
        sampleExp = card.Experience;
        tempExp = 0;
        isBonusCount = 0;
        lvUpButton.interactable = false;
    }

    private void UpdateStatText(int level, int exp)
    {
        var stat = StatCalculator(charData, level);
        lvText.text = level.ToString();
        attackText.text = stat.attack.ToString();
        hpText.text = stat.hp.ToString();
        pDefenceText.text = stat.pDefence.ToString();
        mDefenceText.text = stat.mDefence.ToString();
        expText.text = $"{exp} / {expTable.dic[level].Exp}";
        expSlider.fillAmount = (float)exp / expTable.dic[level].Exp;
    }

    private void SetSpiritStoneScroolView()
    {
        foreach (var dir in InvManager.spiritStoneInv.Inven)
        {
            if (dir.Value.Count == 0) continue;

            var go = Instantiate(itemButtonPrefab, spiritStoneSpace);
            var itemButton = go.GetComponent<ItemButton>();
            itemButtons.Add(itemButton);
            itemButton.Init(dir.Value);
            itemButton.OnClick += HandleItemClick;
        }
    }

    private void ClearSpiritStoneScrollView()
    {
        itemButtons.Clear();
        for (int i = spiritStoneSpace.childCount - 1; i >= 0; i--)
        {
            Destroy(spiritStoneSpace.GetChild(i).gameObject);
        }
    }

    private bool HandleItemClick(Item item, bool isPositive)
    {
        if (isPositive)
        {
            selectedItems.Add(item);
        }
        else
        {
            selectedItems.Remove(item);
        }

        bool success = RecalculateSimulation();

        if (!success)
        {
            // If adding failed, revert the list change.
            if (isPositive)
            {
                selectedItems.Remove(item);
            }
        }

        return success;
    }

    private bool RecalculateSimulation()
    {
        // 1. Reset simulation state to card's base state
        int currentSimLv = card.Level;
        int currentSimExp = card.Experience;
        int totalAddedExp = 0;
        int bonusCount = 0;

        var itemTable = DataTableMgr.GetTable<ItemTable>();

        // 2. Aggregate total EXP from the list of selected items
        foreach (var selectedItem in selectedItems)
        {
            if (itemTable.dic.TryGetValue(selectedItem.ID, out var itemData))
            {
                int expValue = itemData.value2;
                if (charData.CharProperty == itemData.value1)
                {
                    expValue = (int)(expValue * 1.5f);
                    bonusCount++;
                }
                totalAddedExp += expValue;
            }
        }

        // 3. Apply the aggregated EXP to a temporary simulation
        currentSimExp += totalAddedExp;

        while (currentSimExp >= expTable.dic[currentSimLv].Exp)
        {
            currentSimExp -= expTable.dic[currentSimLv].Exp;
            currentSimLv++;
        }

        // 4. Check for level cap violation
        if (!CheckGrade(card.Grade, currentSimLv))
        {
            UIManager.Instance.modalWindow.OpenPopup(GameManager.stringTable[407].Value, GameManager.stringTable[328].Value);
            return false; // Indicate failure
        }

        // 5. The simulation is valid. Commit the results to the class fields.
        sampleLv = currentSimLv;
        sampleExp = currentSimExp;
        tempExp = totalAddedExp;
        isBonusCount = bonusCount;

        // 6. Update UI
        UpdateStatText(sampleLv, sampleExp);
        lvUpButton.interactable = selectedItems.Any();

        return true;
    }

    public void TryShowLevelUpEffect()
    {
        if (lvUpButton.interactable == false) return;

        UIManager.Instance.blockPanel.SetActive(true);
        lvUpParticle.Play();
    }

    private void LevelUp()
    {
        card.LevelUp(sampleLv, sampleExp);

        foreach (var button in itemButtons)
        {
            button.UseItem();
        }
        
        SetLvUpView();
    }

    public void OpenLevleUpPopup()
    {
        if (lvUpParticle.particleCount <= 1)
        {
            UIManager.Instance.blockPanel.SetActive(false);

            var stringTable = DataTableMgr.GetTable<StringTable>();
            var statsName = $"{stringTable.dic[305].Value}\n{stringTable.dic[306].Value}\n{stringTable.dic[307].Value}\n{stringTable.dic[308].Value}\n{stringTable.dic[313].Value}";

            UIManager.Instance.lvUpModal.OpenPopup(stringTable.dic[332].Value, stringTable.dic[330].Value + tempExp, sampleExp,
                expTable.dic[sampleLv].Exp, statsName, GetLvUpResult(card.Level, sampleLv), stringTable.dic[1].Value, null, isBonusCount > 0);

            LevelUp();
        }
    }

    private string GetLvUpResult(int beforeLv, int afterLv)
    {
        var beforeStat = StatCalculator(charData, beforeLv);
        var afterStat = StatCalculator(charData, afterLv);

        return $"{beforeLv} -> {afterLv}\n" +
            $"{beforeStat.attack} -> {afterStat.attack}\n" +
            $"{beforeStat.hp} -> {afterStat.hp}\n" +
            $"{beforeStat.pDefence} -> {afterStat.pDefence}\n" +
            $"{beforeStat.mDefence} -> {afterStat.pDefence}";
    }

    private bool CheckGrade(int grade, int level)
    {
        return grade * 10 + 10 >= level;
    }

    private Stat StatCalculator(CharData data, int lv)
    {
        Stat result = new Stat();
        result.attack = data.CharAttack + data.CharAttackIncrease * lv;
        result.pDefence = data.CharPDefence + data.CharPDefenceIncrease * lv;
        result.mDefence = data.CharMDefence + data.CharMDefenceIncrease * lv;
        result.hp = data.CharMaxHP + data.CharHPIncrease * lv;
        return result;
    }
}