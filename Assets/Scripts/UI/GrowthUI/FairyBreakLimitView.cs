using UnityEngine;
using UnityEngine.UI;

public class FairyBreakLimitView : MonoBehaviour
{
    private FairyGrowthUI rootUI;
    private FairyCard card;

    [Header("UI Components")]
    public UIView breakLimitView;
    public Button limitBreakButton;
    public ParticleSystem breakLimitParticle;

    public void Init(FairyGrowthUI root, FairyCard fairyCard)
    {
        rootUI = root;
        card = fairyCard;

        gameObject.SetActive(true);
        SetBreakLimitView();
    }

    public void Deactive()
    {
        gameObject.SetActive(false);
    }

    private void SetBreakLimitView()
    {
        breakLimitView.Bind(card);
        limitBreakButton.interactable = card.Grade < 5;
    }

    public void TryShowBreakLimitEffect()
    {
        var table = DataTableMgr.GetTable<BreakLimitTable>();
        if (InvManager.itemInv.Inven[10003].Count >= table.dic[card.Grade].CharPieceNeeded)
        {
            UIManager.Instance.blockPanel.SetActive(true);
            breakLimitParticle.Play();
        }
    }

    public void TryBreakLimit()
    {
        if (breakLimitParticle.particleCount <= 1)
        {
            var table = DataTableMgr.GetTable<BreakLimitTable>();
            var stringTable = DataTableMgr.GetTable<StringTable>();

            InvManager.RemoveItem(InvManager.itemInv.Inven[10003], table.dic[card.Grade].CharPieceNeeded);
            card.GradeUp();
            UIManager.Instance.breakLimitModal.OpenPopup(stringTable.dic[303].Value, (card.Grade - 1).ToString(), (card.Grade).ToString());

            rootUI.SetLeftPanel();
            SetBreakLimitView();

            UIManager.Instance.blockPanel.SetActive(false);
        }
    }
}
