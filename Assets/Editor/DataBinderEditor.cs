using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

// DataBinderコンポーネントのカスタムエディタを定義します。
// これにより、Unityエディタ上での表示と操作をカスタマイズできます。
[CustomEditor(typeof(DataBinder))]
public class DataBinderEditor : Editor
{
    // SerializedPropertyは、Unityのシリアライズシステムを通じてプロパティにアクセスするためのものです。
    // これにより、Undo/RedoやPrefabの変更が正しく処理されます。
    private SerializedProperty targetModelProp;
    private SerializedProperty propertyPathProp;
    private SerializedProperty formatStringProp;
    private SerializedProperty textComponentProp;
    private SerializedProperty imageComponentProp;
    private SerializedProperty sliderComponentProp;

    // ドロップダウンリストに表示される利用可能なプロパティのリストです。
    private string[] availableProperties;
    // ドロップダウンリストで現在選択されているプロパティのインデックスです。
    private int selectedPropertyIndex;

    // エディタが有効になったときに呼び出されます。
    private void OnEnable()
    {
        // DataBinderスクリプト内の各publicフィールドに対応するSerializedPropertyを見つけます。
        targetModelProp = serializedObject.FindProperty("targetModel");
        propertyPathProp = serializedObject.FindProperty("propertyPath");
        formatStringProp = serializedObject.FindProperty("formatString");
        textComponentProp = serializedObject.FindProperty("textComponent");
        imageComponentProp = serializedObject.FindProperty("imageComponent");
        sliderComponentProp = serializedObject.FindProperty("sliderComponent");

        // 利用可能なプロパティのリストを初期化します。
        UpdateAvailableProperties();
    }

    // Inspector GUIを描画する際に呼び出されます。
    public override void OnInspectorGUI()
    {
        // シリアライズされたオブジェクトの最新の値をInspectorに反映させます。
        serializedObject.Update();

        // targetModelのプロパティフィールドを描画します。
        // targetModelが変更されたかどうかを検知するためにBeginChangeCheck/EndChangeCheckを使用します。
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(targetModelProp);
        if (EditorGUI.EndChangeCheck())
        {
            // targetModelが変更された場合、利用可能なプロパティのリストを更新します。
            UpdateAvailableProperties();
        }

        // targetModelが設定されている場合、プロパティパスをドロップダウンで選択できるようにします。
        if (targetModelProp.objectReferenceValue != null)
        {
            // ポップアップ（ドロップダウン）メニューを描画し、選択されたインデックスを更新します。
            selectedPropertyIndex = EditorGUILayout.Popup("Property Path", selectedPropertyIndex, availableProperties);
            // 選択されたプロパティパスをpropertyPathPropに設定します。
            propertyPathProp.stringValue = availableProperties[selectedPropertyIndex];
        }
        else
        {
            // targetModelが設定されていない場合、propertyPathを通常のテキストフィールドとして表示します。
            EditorGUILayout.PropertyField(propertyPathProp);
        }

        // その他のプロパティフィールドを描画します。
        EditorGUILayout.PropertyField(formatStringProp);
        EditorGUILayout.PropertyField(textComponentProp);
        EditorGUILayout.PropertyField(imageComponentProp);
        EditorGUILayout.PropertyField(sliderComponentProp);

        // Inspectorでの変更をシリアライズされたオブジェクトに適用します。
        serializedObject.ApplyModifiedProperties();
    }

    // targetModelに基づいて利用可能なプロパティのリストを更新するヘルパーメソッドです。
    private void UpdateAvailableProperties()
    {
        // targetModelがnullの場合、プロパティリストを空にします。
        if (targetModelProp.objectReferenceValue == null)
        {
            availableProperties = new string[0];
            selectedPropertyIndex = 0;
            return;
        }

        List<string> properties = new List<string>();
        // targetModelの型を取得します。
        Type modelType = targetModelProp.objectReferenceValue.GetType();

        // publicなフィールドを検索し、リストに追加します。
        foreach (FieldInfo field in modelType.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            properties.Add(field.Name);
        }

        // publicなプロパティを検索し、リストに追加します。
        foreach (PropertyInfo prop in modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            properties.Add(prop.Name);
        }

        // ネストされたプロパティ（例: FinalStat.attack）を検索し、リストに追加します。
        // 現在は1階層のネストのみをサポートしています。
        foreach (PropertyInfo prop in modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // プロパティがクラス型であり、かつstring型ではない場合（stringはプリミティブとして扱われるため）
            if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
            {
                // ネストされたプロパティのpublicなプロパティを検索し、"親.子"の形式で追加します。
                foreach (PropertyInfo nestedProp in prop.PropertyType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    properties.Add($"{prop.Name}.{nestedProp.Name}");
                }
                // ネストされたプロパティのpublicなフィールドを検索し、"親.子"の形式で追加します。
                foreach (FieldInfo nestedField in prop.PropertyType.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    properties.Add($"{prop.Name}.{nestedField.Name}");
                }
            }
        }

        // プロパティリストをアルファベット順にソートします。
        availableProperties = properties.OrderBy(p => p).ToArray();

        // 現在のpropertyPathが利用可能なプロパティリストに含まれているかを確認し、選択インデックスを設定します。
        string currentPath = propertyPathProp.stringValue;
        selectedPropertyIndex = System.Array.IndexOf(availableProperties, currentPath);
        // もし現在のパスが見つからない、かつ利用可能なプロパティがある場合、最初のプロパティをデフォルトとして選択します。
        if (selectedPropertyIndex == -1 && availableProperties.Length > 0)
        {
            selectedPropertyIndex = 0;
            propertyPathProp.stringValue = availableProperties[0];
        }
        // 利用可能なプロパティがない場合、propertyPathを空にします。
        else if (availableProperties.Length == 0)
        {
            propertyPathProp.stringValue = string.Empty;
        }
    }
}
