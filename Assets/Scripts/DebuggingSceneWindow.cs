/// </summary>
/// 以下のページを参考にさせていただきました
/// KLabGames Tech Blog - Unity なんでもインスペクタに表示していくやつ
/// http://klabgames.tech.blog.jp.klab.com/archives/1047665593.html
/// <summary>

using System;

#if UNITY_EDITOR
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class DebuggingSceneWindow
{
  /// <summary>
  /// ウィンドウを表示させたくない場合はここにtrueを入れる
  /// </summary>
  const bool Hidden = false;

  /// <summary>
  /// ウィンドウの大きさ
  /// </summary>
  readonly static Rect windowSize = new Rect(10, 24, 300, 46);

  /// <summary>
  /// The empty options.
  /// </summary>
  readonly static GUILayoutOption[] emptyOptions = new GUILayoutOption[0];

  /// <summary>
  /// The type of the inspectable.
  /// </summary>
  readonly static Type InspectableType = typeof(InspectableAttribute);

  /// <summary>
  /// Typeに応じたInspector表示ロジックを格納
  /// 拡張したい場合はこいつに追加する
  /// </summary>
  /// <value>The inspect action dict.</value>
  public static Dictionary<Type, Action<string, object>> InspectActionDict { get; private set; }

  static bool isActive = true;

  static bool IsActive { get { return isActive; } set { isActive = value; } }


  /// <summary>
  /// Initializes the <see cref="DebuggingInspectorDrawer"/> class.
  /// </summary>
  static DebuggingSceneWindow()
  {
    InspectActionDict = new Dictionary<Type, Action<string, object>>();

    // Register default actions
    RegisterInspectAction(typeof(Int32), (name, obj) => EditorGUILayout.IntField(name, (Int32)obj, emptyOptions));
    RegisterInspectAction(typeof(String), (name, obj) => EditorGUILayout.TextField(name, (String)obj, emptyOptions));
    RegisterInspectAction(typeof(Boolean), (name, obj) => EditorGUILayout.Toggle(name, (Boolean)obj, emptyOptions));
    RegisterInspectAction(typeof(Single), (name, obj) => EditorGUILayout.FloatField(name, (Single)obj, emptyOptions));
    RegisterInspectAction(typeof(Vector2), (name, obj) => EditorGUILayout.Vector2Field(name, (Vector2)obj, emptyOptions));
    RegisterInspectAction(typeof(Vector3), (name, obj) => EditorGUILayout.Vector3Field(name, (Vector3)obj, emptyOptions));
    RegisterInspectAction(typeof(Vector4), (name, obj) => EditorGUILayout.Vector4Field(name, (Vector4)obj, emptyOptions));
    RegisterInspectAction(typeof(Color), (name, obj) => EditorGUILayout.ColorField(name, (Color)obj, emptyOptions));

    // Register array actions
    RegisterInspectAction(typeof(Int32[]), ArrayInspectAction);
    RegisterInspectAction(typeof(String[]), ArrayInspectAction);
    RegisterInspectAction(typeof(Boolean[]), ArrayInspectAction);
    RegisterInspectAction(typeof(Single[]), ArrayInspectAction);
    RegisterInspectAction(typeof(Vector2[]), ArrayInspectAction);
    RegisterInspectAction(typeof(Vector3[]), ArrayInspectAction);
    RegisterInspectAction(typeof(Vector4[]), ArrayInspectAction);
    RegisterInspectAction(typeof(Color[]), ArrayInspectAction);

    RegisterInspectAction(typeof(Int32[][]), ArrayInspectAction);
    RegisterInspectAction(typeof(String[][]), ArrayInspectAction);
    RegisterInspectAction(typeof(Boolean[][]), ArrayInspectAction);
    RegisterInspectAction(typeof(Single[][]), ArrayInspectAction);
    RegisterInspectAction(typeof(Vector2[][]), ArrayInspectAction);
    RegisterInspectAction(typeof(Vector3[][]), ArrayInspectAction);
    RegisterInspectAction(typeof(Vector4[][]), ArrayInspectAction);
    RegisterInspectAction(typeof(Color[][]), ArrayInspectAction);

    RegisterInspectAction(typeof(Int32[,]), ArrayInspectAction);
    RegisterInspectAction(typeof(String[,]), ArrayInspectAction);
    RegisterInspectAction(typeof(Boolean[,]), ArrayInspectAction);
    RegisterInspectAction(typeof(Single[,]), ArrayInspectAction);
    RegisterInspectAction(typeof(Vector2[,]), ArrayInspectAction);
    RegisterInspectAction(typeof(Vector3[,]), ArrayInspectAction);
    RegisterInspectAction(typeof(Vector4[,]), ArrayInspectAction);
    RegisterInspectAction(typeof(Color[,]), ArrayInspectAction);

    SceneView.onSceneGUIDelegate += OnSceneGUI;
  }

  // ウィンドウを画面に出す
  static void OnSceneGUI(SceneView sceneView)
  {
    if (Hidden) { return; }

    GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);

    Handles.BeginGUI();
    GUILayout.Window(1, windowSize, DrawWindow, "Debugging Inspector");
    Handles.EndGUI();
  }

  // ウィンドウの描画処理
  public static void DrawWindow(int id)
  {
    EditorGUILayout.BeginHorizontal();
    IsActive = EditorGUILayout.ToggleLeft("IsActive", IsActive);
    EditorGUILayout.EndHorizontal();

    if (!IsActive) { return; }
    GUILayout.Space(4f);

    // Start inspection
    foreach (var component in GetAllInspectableComponents())
    {
      GUILayout.Box("", GUILayout.Width(windowSize.width), GUILayout.Height(1));
      GUILayout.Space(1f);

      EditorGUILayout.BeginVertical(emptyOptions);
      EditorGUILayout.BeginHorizontal(emptyOptions);

      EditorGUI.indentLevel = 0;
      EditorGUILayout.ObjectField(component.GetType().ToString(), component, typeof(MonoScript), false);
      EditorGUILayout.EndHorizontal();

      EditorGUI.indentLevel = 1;
      Inspect(component);
      EditorGUILayout.EndVertical();
      GUILayout.Space(7f);
    }

  }

  /// <summary>
  /// Registers the inspect action.
  /// </summary>
  /// <param name="type">Type.</param>
  /// <param name="action">Action.</param>
  public static void RegisterInspectAction(Type type, Action<string, object> action)
  {
    InspectActionDict.Add(type, action);
  }

  /// <summary>
  /// Registers array inspect action.
  /// </summary>
  static void ArrayInspectAction(string name, object obj)
  {
    EditorGUILayout.LabelField(name);
    if (obj == null)
    {
      return;
    }

    var array = (Array)obj;

    switch (array.Rank)
    {
      case 1:
        EditorGUI.indentLevel++;
        for (int i = 0; i < array.Length; i++)
        {
          var value = array.GetValue(i);

          InspectObject("Element " + i, value.GetType(), value);
        }
        EditorGUI.indentLevel--;
        break;
      case 2:
        EditorGUI.indentLevel++;
        for (int i = 0; i < array.GetLength(0); i++)
        {
          EditorGUILayout.LabelField("Element " + i);
          EditorGUI.indentLevel++;
          for (int j = 0; j < array.GetLength(1); j++)
          {
            var value = array.GetValue(i, j);
            InspectObject("Element " + j, value.GetType(), value);
          }
          EditorGUI.indentLevel--;
        }
        EditorGUI.indentLevel--;
        break;
    }
  }


  /// <summary>
  /// ロード済みSceneのルートTransfromを返す
  /// </summary>
  /// <returns>The root transforms.</returns>
  static IEnumerable<Transform> LoadedScenesRootTransforms()
  {
    int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount;
    for (int i = 0; i < sceneCount; i++)
    {
      var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
      if (!scene.isLoaded) { continue; }

      foreach (var go in scene.GetRootGameObjects())
      {
        yield return go.transform;
      }

    }
  }

  /// <summary>
  /// Gets the inspectable components.
  /// </summary>
  /// <returns>The inspectable components.</returns>
  /// <param name="transform">Transform.</param>
  static IEnumerable<Component> GetInspectableComponents(Transform transform)
  {
    foreach (var co in transform.GetComponentsInChildren(typeof(Component)))
    {
      if (IsInspectableComponent(co))
      {
        yield return co;
      }
    }
  }

  /// <summary>
  /// Determines if is inspectable component the specified component.
  /// </summary>
  /// <returns><c>true</c> if is inspectable component the specified component; otherwise, <c>false</c>.</returns>
  /// <param name="component">Component.</param>
  static bool IsInspectableComponent(Component component)
  {
    if (component == null)
    {
      return false;
    }
    var type = component.GetType();
    var attrs = type.GetCustomAttributes(false);
    return attrs.Any(attr => attr.GetType() == InspectableType);
  }

  /// <summary>
  /// Gets all inspectable components.
  /// </summary>
  /// <returns>The all inspectable components.</returns>
  static IEnumerable<Component> GetAllInspectableComponents()
  {
    foreach (var transform in LoadedScenesRootTransforms())
    {
      foreach (var co in GetInspectableComponents(transform))
      {
        yield return co;
      }
    }
  }

  /// <summary>
  /// 受け取ったobjectの値をウィンドウへ表示
  /// </summary>
  /// <param name="name">Name.</param>
  /// <param name="type">Type.</param>
  /// <param name="_object">Object.</param>
  static void InspectObject(String name, Type type, object _object)
  {
    // Registered types in InspectActionDict.
    if (InspectActionDict.ContainsKey(type))
    {
      InspectActionDict[type].Invoke(name, _object);
      return;
    }

    // Unity object
    var unityObject = _object as UnityEngine.Object;
    if (unityObject != null)
    {
      EditorGUILayout.ObjectField(name, unityObject, type, false, emptyOptions);
      return;
    }

    // Enum
    if (type.IsEnum)
    {
      EditorGUILayout.EnumPopup(name, (Enum)_object, emptyOptions);
      return;
    }

    // null check
    if (_object == null)
    {
      EditorGUILayout.TextField(name, null, emptyOptions);
      return;
    }

    // object has Inspectable attribute
    if (_object.GetType().GetCustomAttributes(true).Any(attr => attr.GetType() == InspectableType))
    {
      EditorGUILayout.LabelField(name);
      EditorGUI.indentLevel += 1;
      Inspect(_object);
      EditorGUI.indentLevel -= 1;
      return;
    }

    // Unregistered Types
    EditorGUILayout.TextField(name, _object.ToString(), emptyOptions);
  }

  /// <summary>
  /// Extracts the inspectables.
  /// </summary>
  /// <returns>The inspectables.</returns>
  /// <param name="type">Type.</param>
  static IEnumerable<MemberInfo> ExtractInspectableMembers(IReflect type)
  {
    var members = type.GetMembers(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

    foreach (var member in members)
    {
      var attrs = member.GetCustomAttributes(true);

      if (attrs.Any(a => a.GetType() == InspectableType))
      {
        yield return member;
      }
    }
  }

  /// <summary>
  /// 指定されたObjectのField, Propertyを取得しInspectableAtrributeがついてたらWindowへ表示
  /// </summary>
  /// <param name="obj">object.</param>
  public static void Inspect(object obj)
  {
    var type = obj.GetType();

    foreach (var member in ExtractInspectableMembers(type))
    {
      if (member.MemberType == MemberTypes.Field)
      {
        var field = (FieldInfo)member;
        InspectObject(field.Name, field.FieldType, field.GetValue(obj));
        continue;
      }

      if (member.MemberType == MemberTypes.Property)
      {
        var property = (PropertyInfo)member;
        InspectObject(property.Name, property.PropertyType, property.GetValue(obj, null));
        continue;
      }

    }
  }

}
#endif

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
public class InspectableAttribute : Attribute
{
}
