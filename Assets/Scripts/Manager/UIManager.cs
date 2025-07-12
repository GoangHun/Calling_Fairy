using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    public UI stageUI;
    public UI stage2UI;
    public List<UI> UIs;

    public Slider masterVolumeSlider;
    public Slider bgmVolumeSlider;
    public Slider seVolumeSlider;

    public Action OnMainSceneUpdateUI;
    public MessageModal modalWindow;
    public LvUpModal lvUpModal;
    public BreakLimitModal breakLimitModal;
    public ItemDropStageInfoModal stageInfoModal;
    public DetailStatModal detailStatModal;
    public ObjectPoolManager objPoolMgr;
    public GameObject blockPanel;
    
    private Stack<UI> uiStack = new Stack<UI>();
    
    /// <summary>
    /// 現在アクティブなUIを取得します。
    /// </summary>
    /// <returns>現在アクティブなUI or null</returns>
    public UI CurrentUI
    {
        get
        {
            if (uiStack.Count > 0)
                return uiStack.Peek();
            return null;
        }
    }

    public AudioClip[] seClips = new AudioClip[9];

    /// <summary>
    /// 指定UIをスタックにプッシュし、現在のUIを非アクティブにします。
    /// </summary>
    /// <param name="ui">スタックにプッシュしてActiveするUI</param>
    public void PushUI(UI ui)
    {
        if (CurrentUI != null)
        {
            CurrentUI.gameObject.SetActive(false);
        }
        uiStack.Push(ui);
        ui.gameObject.SetActive(true);
    }

    /// <summary>
    /// 現在のUIをスタックからポップし、前のUIをアクティブにします。
    /// </summary>
    public void PopUI()
    {
        if (CurrentUI != null)
        {
            UI poppedUI = uiStack.Pop();
            poppedUI.gameObject.SetActive(false);

            if (CurrentUI != null)
            {
                CurrentUI.gameObject.SetActive(true);
            }
        }
    }
    
    private void Start()
    {
        OnMainSceneUpdateUI?.Invoke();
        OpenStageWindow();
        SetAudioManager();
    }

    public void SetAudioManager()
    {
        AudioManager.Instance.masterSlider = masterVolumeSlider;
        AudioManager.Instance.bgmSlider = bgmVolumeSlider;
        AudioManager.Instance.seSlider = seVolumeSlider;
    }

    public void DirectOpenUI(int index)
    {
        UIs[index].ActiveUI();
    }

    public void ReturnHome()
    {
        while (uiStack.Count > 0)
        {
            PopUI();
        }
    }

    public void OpenStageWindow()
    {
        if (StageGo.IsWindowOpen)
        {
            StageGo.IsWindowOpen = false;
            DirectOpenUI((int)StageGo.StageIndex - 1);
        }
    }
    public void SESelect(int num)
    {
        if (num < seClips.Length)
        {
            AudioManager.Instance.PlaySE(seClips[num]);
        }
    }
}
