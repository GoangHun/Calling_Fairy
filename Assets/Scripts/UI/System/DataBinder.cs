using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DataBinder : MonoBehaviour
{
    [Tooltip("バインディングするデータモデル (FairyCard または Equipment コンポーネント)")]
    public Component targetModel; // FairyCard または Equipment

    [Tooltip("モデル内でのプロパティパス (例: 'Name', 'FinalStat.attack')")]
    public string propertyPath;

    [Tooltip("テキストのフォーマット文字列 (例: 'Lv.{0}', '攻撃力: {0:N0}')")]
    public string formatString = "{0}";

    [Header("UI Components")]
    public TextMeshProUGUI textComponent;
    public Image imageComponent;
    public Slider sliderComponent;

    /// <summary>
    /// UIを更新するメソッド。
    /// targetModelとpropertyPathが設定されている場合、指定されたプロパティの値を取得し、
    /// 対応するUIコンポーネントに反映します。
    /// </summary>
    public void UpdateUI()
    {
        if (targetModel == null || string.IsNullOrEmpty(propertyPath)) return;

        object value = GetValueFromPath(targetModel, propertyPath);

        if (textComponent != null)
        {
            textComponent.text = string.Format(formatString, value);
        }
        else if (imageComponent != null)
        {
            if (value is Sprite sprite)
            {
                imageComponent.sprite = sprite;
            }
            else if (value is string path)
            {
                imageComponent.sprite = Resources.Load<Sprite>(path);
            }
        }
        else if (sliderComponent != null)
        {
            if (value is float floatValue)
            {
                sliderComponent.value = floatValue;
            }
            else if (value is int intValue)
            {
                sliderComponent.value = intValue;
            }
        }
    }

    /// <summary>
    /// 指定されたオブジェクトとパスから値を取得するヘルパーメソッド。
    /// パスはドットで区切られたプロパティやフィールドの名前を指定します。
    /// </summary>
    /// <param name="source">値を取得するオブジェクト</param>
    /// <param name="path">プロパティやフィールドのパス (例: "Name", "FinalStat.attack")</param>
    /// <returns>取得した値。存在しない場合はnull。</returns>
    private object GetValueFromPath(object source, string path)
    {
        object current = source;
        string[] parts = path.Split('.');

        foreach (string part in parts)
        {
            if (current == null) return null;

            // フィールド検索
            FieldInfo field = current.GetType().GetField(part, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                current = field.GetValue(current);
                continue;
            }

            // プロパティ検索
            PropertyInfo property = current.GetType().GetProperty(part, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property != null)
            {
                current = property.GetValue(current);
                continue;
            }

            // メソッド検索 (引数なしのメソッドのみ)
            MethodInfo method = current.GetType().GetMethod(part, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, CallingConventions.Any, System.Type.EmptyTypes, null);
            if (method != null)
            {
                current = method.Invoke(current, null);
                continue;
            }

            // 何も見つからなかった場合
            return null;
        }
        return current;
    }
}

