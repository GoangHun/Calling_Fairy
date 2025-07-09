using System.Collections.Generic;
using UnityEngine;

public class UIView : MonoBehaviour
{
    private List<DataBinder> binders = new List<DataBinder>();

    private void Awake()
    {
        // DataBinderコンポーネントをすべて取得
        // trueを指定して非アクティブな子オブジェクトも含めて取得
        GetComponentsInChildren<DataBinder>(true, binders);
    }

    public void Bind(FairyCard fairy)
    {
        foreach (var binder in binders)
        {
            binder.targetModel = fairy; // DataBinderのtargetModel設定
            binder.UpdateUI();
        }
    }

    public void Bind(Equipment equipment)
    {
        foreach (var binder in binders)
        {
            binder.targetModel = equipment; // DataBinderのtargetModel設定
            binder.UpdateUI();
        }
    }

    /// <summary>
    /// すべてのDataBinderを更新します。
    /// </summary>
    public void UpdateAll()
    {
        foreach (var binder in binders)
        {
            binder.UpdateUI();
        }
    }
}
