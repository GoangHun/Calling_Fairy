using System;
using UnityEngine;

public class UI : MonoBehaviour, IUI
{
    public event Action OnActive = null;
    public event Action OnNonActive = null;

    /// <summary>
    /// UIをアクティブし、UIManagerにプッシュします。
    /// </summary>
    public virtual void ActiveUI()
    {
        if (OnActive != null)
        {
            OnActive();
        }
        UIManager.Instance.PushUI(this);
    }
     
    /// <summary>
    /// UIを非アクティブにし、UIManagerからポップします。
    /// </summary>
    public virtual void NonActiveUI()
    {
        if (OnNonActive != null)
        {
            OnNonActive();
        }
        UIManager.Instance.PopUI();
    }
}
