using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FairyEquipmentView : MonoBehaviour
{
    private FairyGrowthUI rootUI;
    private FairyCard card;
    private CharData charData;
    private EquipSlot selectedSlot;

    private int tempExp = 0;
    private int equipParticleCount = 0;
    private int equipSampleLv;
    private int equipSampleExp;
    private List<ItemButton> enforceStoneButtons = new List<ItemButton>();

    [Header("Views & Panels")]
    public UIView equipCreateView; // 장비 제작 정보 패널
    public GameObject equipGrowthView; // 장비 성장 정보 패널

    [Header("Prefabs")]
    public GameObject equipItemButtonPrefab;

    [Header("EquipCreate View")]
    public TextMeshProUGUI equipName;
    public Image equipPieceImage;
    public Image pieceCountSlider;
    public Text pieceCountText;
    public TextMeshProUGUI equipAttackText;
    public TextMeshProUGUI equipHpText;
    public TextMeshProUGUI equipPDefenceText;
    public TextMeshProUGUI equipMDefenceText;

    [Header("EquipGrowth View")]
    public TextMeshProUGUI equipName2;
    public Image equipImage;
    public Image equipExpSlider;
    public Text equipExpText;
    public TextMeshProUGUI equipLvText;
    public TextMeshProUGUI equipAttackText2;
    public TextMeshProUGUI equipHpText2;
    public TextMeshProUGUI equipPDefenceText2;
    public TextMeshProUGUI equipMDefenceText2;
    public Transform enforceStoneSpace;
    public Button equipLvUpButton;
    public ParticleSystem equipExpParticle;

    [Header("RankUp Effect")]
    public GameObject rankUpAttractors;
    public List<ParticleSystem> rankUpParticles;
    public ParticleSystem fairyAttractorParticle2;

    public void Init(FairyGrowthUI root, FairyCard fairyCard, CharData characterData)
    {
        rootUI = root;
        card = fairyCard;
        charData = characterData;
        selectedSlot = rootUI.SelectedSlot;

        gameObject.SetActive(true);
        SetEquipView();
    }

    public void Deactive()
    {
        gameObject.SetActive(false);
    }

    public void SetEquipView()
    {
        selectedSlot = rootUI.SelectedSlot; // Ensure selected slot is up-to-date
        bool isEquipSelected = selectedSlot != null;
        bool hasEquip = isEquipSelected && selectedSlot.Equipment != null;

        equipCreateView.gameObject.SetActive(isEquipSelected && !hasEquip);
        equipGrowthView.SetActive(isEquipSelected && hasEquip);
        rankUpAttractors.SetActive(!isEquipSelected); // 랭크업 파티클은 장비 미선택 시에만

        if (isEquipSelected)
        {
            if (hasEquip)
            {
                ClearEnforceStoneScrollView();
                SetEnforceStoneScroolView();
                SetEquipSample(selectedSlot.Equipment);
                SetEquipGrowthInfoBox(DataTableMgr.GetTable<EquipTable>().dic[selectedSlot.Equipment.ID], equipSampleLv, equipSampleExp);
            }
            else
            {
                var key = Convert.ToInt32($"30{charData.CharPosition}{selectedSlot.slotNumber}0{card.Rank}");
                var equipData = DataTableMgr.GetTable<EquipTable>().dic[key];
                SetEquipInfoBox(equipData);
            }
        }
        else
        {
            InitEquipInfoBox();
        }
    }

    public void EquipItem()
    {
        if (selectedSlot == null)
            return;

        var position = charData.CharPosition;
        var key = Convert.ToInt32($"30{position}{selectedSlot.slotNumber}0{card.Rank}");
        var newEquipment = new Equipment(key);

        selectedSlot.CreateAndSetEquipment(newEquipment);
        card.SetEquip(selectedSlot.slotNumber, newEquipment);
        SetEquipView();
    }

    public void TryShowRankUpEffect()
    {
        if (card.equipSocket.Count != 6)
            return;

        foreach (var value in card.equipSocket.Values)
        {
            if (value == null)
                return;
        }

        rankUpAttractors.SetActive(true);

        foreach (var particle in rankUpParticles)
        {
            UIManager.Instance.blockPanel.SetActive(true);
            particle.Play();
        }
    }

    public void TryRankUp()
    {
        equipParticleCount++;

        if (equipParticleCount < 6)
            return;
        
        StartCoroutine(WaitForParticleCompletionThenRankUp(fairyAttractorParticle2));
    }

    IEnumerator WaitForParticleCompletionThenRankUp(ParticleSystem particle)
    {
        yield return new WaitForSeconds(particle.main.duration);

        equipParticleCount = 0;
        card.RankUp();
        rootUI.SelectedSlot = null;
        rootUI.SetLeftPanel();
        SetEquipView();
        rankUpAttractors.SetActive(false);
        UIManager.Instance.blockPanel.SetActive(false);
    }

    public void OpenItemDropStageInfoPopup()
    {
        if (selectedSlot == null)
            return;

        var position = charData.CharPosition;
        var key = Convert.ToInt32($"30{position}{selectedSlot.slotNumber}0{card.Rank}");
        var equipTable = DataTableMgr.GetTable<EquipTable>();

        UIManager.Instance.stageInfoModal.OpenPopup("드랍 스테이지 정보", equipTable.dic[key].EquipPiece);
    }

    public void OpenEquipDetailInfoPopup()
    {
        if (selectedSlot == null) 
            return;

        var position = charData.CharPosition;
        var key = Convert.ToInt32($"30{position}{selectedSlot.slotNumber}0{card.Rank}");

        if (selectedSlot.Equipment == null)
        {
            UIManager.Instance.detailStatModal.OpenPopup("장비 상세 정보", new Equipment(key));
        }
        else
        {
            UIManager.Instance.detailStatModal.OpenPopup("장비 상세 정보", selectedSlot.Equipment);
        }
    }
    
    private void InitEquipInfoBox()
    {
        equipName.text = "장비 이름";
        equipPieceImage.sprite = Resources.Load<Sprite>("StatStatus/Empty");
        pieceCountSlider.fillAmount = 0;
        pieceCountText.text = $"0 / 0";
        equipAttackText.text = "0";
        equipHpText.text = "0";
        equipPDefenceText.text = "0";
        equipMDefenceText.text = "0";
    }

    private void SetEquipInfoBox(EquipData equipData)
    {
        var itemTable = DataTableMgr.GetTable<ItemTable>();
        var stringTable = DataTableMgr.GetTable<StringTable>();

        if (itemTable.dic.TryGetValue(equipData.EquipPiece, out ItemData itemData))
        {
            equipPieceImage.sprite = Resources.Load<Sprite>(itemData.icon);
        }
        equipName.text = stringTable.dic[equipData.EquipName].Value;
        if (InvManager.equipPieceInv.Inven.TryGetValue(equipData.EquipPiece, out EquipmentPiece piece))
        {
            pieceCountSlider.fillAmount = (float)piece.Count / equipData.EquipPieceNum;
            pieceCountText.text = $"{piece.Count} / {equipData.EquipPieceNum}";
        }
        else
        {
            pieceCountSlider.fillAmount = 0f / equipData.EquipPieceNum;
            pieceCountText.text = $"0 / {equipData.EquipPieceNum}";
        }
        equipAttackText.text = equipData.EquipAttack.ToString();
        equipHpText.text = equipData.EquipMaxHP.ToString();
        equipPDefenceText.text = equipData.EquipPDefence.ToString();
        equipMDefenceText.text = equipData.EquipMDefence.ToString();
    }

    private void SetEquipGrowthInfoBox(EquipData equipData, int sampleLv, int sampleExp)
    {
        var stringTable = DataTableMgr.GetTable<StringTable>();
        var expTable = DataTableMgr.GetTable<EquipExpTable>();

        equipLvText.text = $"{sampleLv}";
        equipImage.sprite = Resources.Load<Sprite>(equipData.EquipIcon);
        equipName2.text = stringTable.dic[equipData.EquipName].Value;
        equipExpSlider.fillAmount = (float)sampleExp / expTable.dic[sampleLv].Exp;
        equipExpText.text = $"{sampleExp} / {expTable.dic[sampleLv].Exp}";

        var stat = StatCalculator(equipData, sampleLv);

        equipAttackText2.text = stat.attack.ToString();
        equipHpText2.text = stat.hp.ToString();
        equipPDefenceText2.text = stat.pDefence.ToString();
        equipMDefenceText2.text = stat.mDefence.ToString();
    }

    private Stat StatCalculator(EquipData data, int lv)
    {
        Stat result = new Stat();
        result.attack = data.EquipAttack + data.EquipAttackIncrease * lv;
        result.pDefence = data.EquipPDefence + data.EquipPDefenceIncrease * lv;
        result.mDefence = data.EquipMDefence + data.EquipMDefenceIncrease * lv;
        result.hp = data.EquipMaxHP + data.EquipHPIncrease * lv;
        return result;
    }

    private void SetEnforceStoneScroolView()
    {
        Set(10004);
        Set(10005);
        Set(10006);
        
        void Set(int id)
        {
            if (InvManager.itemInv.Inven.TryGetValue(id, out Item enforceStone))
            {
                if (enforceStone.Count > 0)
                {
                    var go = Instantiate(equipItemButtonPrefab, enforceStoneSpace);
                    var itemButton = go.GetComponent<ItemButton>();
                    enforceStoneButtons.Add(itemButton);
                    itemButton.Init(enforceStone);
                    itemButton.OnClick += EquipSimulation;
                }
            }
        }
    }

    private void ClearEnforceStoneScrollView()
    {
        enforceStoneButtons.Clear();
        for (int i = enforceStoneSpace.childCount - 1; i >= 0; i--)
        {
            var child = enforceStoneSpace.GetChild(i);
            Destroy(child.gameObject);
        }
    }

    private bool EquipSimulation(Item item, bool isPositive)
    {
        if (selectedSlot == null || selectedSlot.Equipment == null)
            return false;

        if (equipSampleLv >= 30)
            return false;

        var expTable = DataTableMgr.GetTable<EquipExpTable>();
        var itemTable = DataTableMgr.GetTable<ItemTable>();

        if (itemTable.dic.TryGetValue(item.ID, out ItemData itemData))
        {
            if (isPositive)
            {
                equipSampleExp += itemData.value2;
                tempExp += itemData.value2;

                while (equipSampleExp >= expTable.dic[equipSampleLv].Exp)
                {
                    equipSampleExp -= expTable.dic[equipSampleLv].Exp;
                    equipSampleLv++;
                }

                if (equipSampleLv > 30)
                {
                    equipSampleExp -= itemData.value2;
                    tempExp -= itemData.value2;

                    while (equipSampleExp < 0)
                    {
                        equipSampleLv--;
                        equipSampleExp += expTable.dic[equipSampleLv].Exp;
                    }
                    return false;
                }
            }
            else // 감소
            {
                equipSampleExp -= itemData.value2;
                tempExp -= itemData.value2;

                while(equipSampleExp < 0)
                {
                    equipSampleLv--;
                    equipSampleExp += expTable.dic[equipSampleLv].Exp;
                }
            }
        }
        equipLvUpButton.interactable = equipSampleLv != selectedSlot.Equipment.Level || equipSampleExp != selectedSlot.Equipment.Exp;
        SetEquipGrowthInfoBox(DataTableMgr.GetTable<EquipTable>().dic[selectedSlot.Equipment.ID], equipSampleLv, equipSampleExp);

        return true;
    }

    private void SetEquipSample(Equipment equipment)
    {
        if (equipment == null) 
            return;

        tempExp = 0;
        equipSampleLv = equipment.Level;
        equipSampleExp = equipment.Exp;
    }

    public void TryShowEquipLvUpEffect()
    {
        if (selectedSlot == null || selectedSlot.Equipment == null)
            return;

        if (equipSampleLv > 30)
            return;

        if (equipLvUpButton.interactable == false)
            return;

        UIManager.Instance.blockPanel.SetActive(true);
        equipExpParticle.Play();
    }

    public void EquipLvUp()
    {
        if (equipExpParticle.particleCount <= 1)
        {
            var stringTable = DataTableMgr.GetTable<StringTable>();

            var statsName = $"{stringTable.dic[305].Value}\n{stringTable.dic[306].Value}\n{stringTable.dic[307].Value}\n{stringTable.dic[308].Value}\n{stringTable.dic[313].Value}";
            UIManager.Instance.lvUpModal.OpenPopup($"{stringTable.dic[302].Value}", $"{stringTable.dic[330].Value} " + tempExp, equipSampleExp,
                DataTableMgr.GetTable<EquipExpTable>().dic[equipSampleLv].Exp, statsName, GetLvUpResult(selectedSlot.Equipment.Level, equipSampleLv), stringTable.dic[1].Value, null, false);

            selectedSlot.Equipment.LevelUp(equipSampleLv, equipSampleExp);

            foreach (var button in enforceStoneButtons)
            {
                button.UseItem();
            }

            equipLvUpButton.interactable = false;
            // TODO: This should be handled by an event
            // rootUI.leftEquipView.Bind(card);

            UIManager.Instance.blockPanel.SetActive(false);
        }
    }

    private string GetLvUpResult(int beforeLv, int afterLv)
    {
        var beforeStat = StatCalculator(DataTableMgr.GetTable<EquipTable>().dic[selectedSlot.Equipment.ID], beforeLv);
        var afterStat = StatCalculator(DataTableMgr.GetTable<EquipTable>().dic[selectedSlot.Equipment.ID], afterLv);

        return $"{beforeLv} -> {afterLv}\n" +
            $"{beforeStat.attack} -> {afterStat.attack}\n" +
            $"{beforeStat.hp} -> {afterStat.hp}\n" +
            $"{beforeStat.pDefence} -> {afterStat.pDefence}\n" +
            $"{beforeStat.mDefence} -> {afterStat.pDefence}";
    }
}
