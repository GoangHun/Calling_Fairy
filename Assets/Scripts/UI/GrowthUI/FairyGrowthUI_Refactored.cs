using Coffee.UIExtensions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FairyGrowthUI : UI
{
    [Header("Data & Models")]
    public FairyCard Card { get; private set; }
    public EquipSlot SelectedSlot { get; set; } = null;
    private CharData charData;

    [Header("Views & Panels")]
    public UIView leftCardView;
    public UIView leftEquipView;
    public UIView statInfoView;
    public FairyLevelUpView fairyLevelUpView;
    public FairyEquipmentView fairyEquipmentView;
    public FairyBreakLimitView fairyBreakLimitView;
    public TabGroup tabGroup;
    public List<Tab> tabButtons;
    public ScrollRect statInfoScrollView;

    public override void ActiveUI()
    {
        base.ActiveUI();
        if (tabGroup != null && tabGroup.tabButtons.Count > 0)
        {
            tabGroup.OnTabSelected(tabGroup.tabButtons[0]);
        }
    }

    public void Init(FairyCard card)
    {
        Card = card;
        charData = DataTableMgr.GetTable<CharacterTable>().dic[Card.ID];
        SelectedSlot = null;

        statInfoView.gameObject.SetActive(false);
        fairyLevelUpView.gameObject.SetActive(false);
        fairyEquipmentView.gameObject.SetActive(false);
        fairyBreakLimitView.gameObject.SetActive(false);

        tabGroup?.OnTabSelected(tabButtons?[0]);
        SetLeftPanel();
        SetRightPanel();
    }

    public void SetLeftPanel()
    {
        bool isEquipTab = tabGroup.selectedTab.Equals(tabButtons?[3]);
        leftCardView.gameObject.SetActive(!isEquipTab);
        leftEquipView.gameObject.SetActive(isEquipTab);

        if (isEquipTab)
        {
            leftEquipView.Bind(Card);
        }
        else
        {
            leftCardView.Bind(Card);
        }
    }

    public void SetRightPanel()
    {
        statInfoView.gameObject.SetActive(false);
        fairyLevelUpView.gameObject.SetActive(false);
        fairyEquipmentView.gameObject.SetActive(false);
        fairyBreakLimitView.gameObject.SetActive(false);

        if (tabGroup.selectedTab == tabButtons[0])
        {
            statInfoView.gameObject.SetActive(true);
            SetStatView();
        }
        else if (tabGroup.selectedTab == tabButtons[1])
        {
            fairyLevelUpView.gameObject.SetActive(true);
            fairyLevelUpView.Init(Card, charData);
        }
        else if (tabGroup.selectedTab == tabButtons[2])
        {
            fairyBreakLimitView.gameObject.SetActive(true);
            fairyBreakLimitView.Init(this, Card);
        }
        else if (tabGroup.selectedTab == tabButtons[3])
        {
            fairyEquipmentView.gameObject.SetActive(true);
            fairyEquipmentView.Init(this, Card, charData);
        }
    }

    public void SetStatView()
    {
        statInfoView.Bind(Card);
        statInfoScrollView.verticalNormalizedPosition = 1f;
    }
}


