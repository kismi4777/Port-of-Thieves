using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace UnityBridge
{
    /// <summary>
    /// Расширенные операции Unity для MCP
    /// Включает управление Canvas, UI элементами, префабами и сценами
    /// </summary>
    public static class UnityOperationsExtended
    {
        // ===== UI ELEMENT CREATION =====
        
        public static OperationResult CreateUIElement(UnityRequest request)
        {
            try
            {
                var elementType = request.GetValue<string>("element_type");
                var elementName = request.GetValue("element_name", $"New {elementType}");
                var parentName = request.GetValue<string>("parent_name");
                var position = request.GetValue("position", Vector2.zero);
                var size = request.GetValue("size", Vector2.one * 100);
                
                if (string.IsNullOrEmpty(elementType))
                    return OperationResult.Fail("element_type parameter is required");
                
                // Найти родительский объект
                GameObject parent = null;
                if (!string.IsNullOrEmpty(parentName))
                {
                    parent = GameObject.Find(parentName);
                    if (parent == null)
                        return OperationResult.Fail($"Parent object '{parentName}' not found");
                }
                
                // Создать UI элемент
                GameObject element = null;
                switch (elementType.ToLower())
                {
                    case "button":
                        element = CreateButton(elementName, parent);
                        break;
                    case "text":
                    case "textmeshpro":
                        element = CreateText(elementName, parent);
                        break;
                    case "image":
                        element = CreateImage(elementName, parent);
                        break;
                    case "inputfield":
                    case "input":
                        element = CreateInputField(elementName, parent);
                        break;
                    case "panel":
                        element = CreatePanel(elementName, parent);
                        break;
                    case "slider":
                        element = CreateSlider(elementName, parent);
                        break;
                    case "toggle":
                        element = CreateToggle(elementName, parent);
                        break;
                    case "dropdown":
                        element = CreateDropdown(elementName, parent);
                        break;
                    default:
                        return OperationResult.Fail($"Unknown UI element type: {elementType}");
                }
                
                if (element == null)
                    return OperationResult.Fail($"Failed to create {elementType}");
                
                // Настроить позицию и размер
                var rectTransform = element.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = position;
                    rectTransform.sizeDelta = size;
                }
                
                // Отметить сцену как измененную
                UnityEditor.EditorUtility.SetDirty(element);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                    UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                
                var message = $"Successfully created {elementType} '{elementName}'";
                return OperationResult.Ok(message, $"UI Element created: {element.name}");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"Create UI element failed: {ex.Message}");
            }
        }
        
        public static OperationResult SetUIProperties(UnityRequest request)
        {
            try
            {
                var objectName = request.GetValue<string>("object_name");
                var properties = request.GetValue<Dictionary<string, object>>("properties", new Dictionary<string, object>());
                
                if (string.IsNullOrEmpty(objectName))
                    return OperationResult.Fail("object_name parameter is required");
                
                var gameObject = GameObject.Find(objectName);
                if (gameObject == null)
                    return OperationResult.Fail($"GameObject '{objectName}' not found");
                
                var rectTransform = gameObject.GetComponent<RectTransform>();
                if (rectTransform == null)
                    return OperationResult.Fail($"Object '{objectName}' is not a UI element");
                
                // Применить свойства
                foreach (var prop in properties)
                {
                    switch (prop.Key.ToLower())
                    {
                        case "position":
                            if (prop.Value is List<object> posList && posList.Count >= 2)
                            {
                                var pos = new Vector2(Convert.ToSingle(posList[0]), Convert.ToSingle(posList[1]));
                                rectTransform.anchoredPosition = pos;
                            }
                            break;
                        case "size":
                            if (prop.Value is List<object> sizeList && sizeList.Count >= 2)
                            {
                                var size = new Vector2(Convert.ToSingle(sizeList[0]), Convert.ToSingle(sizeList[1]));
                                rectTransform.sizeDelta = size;
                            }
                            break;
                        case "anchor_min":
                            if (prop.Value is List<object> anchorMinList && anchorMinList.Count >= 2)
                            {
                                var anchorMin = new Vector2(Convert.ToSingle(anchorMinList[0]), Convert.ToSingle(anchorMinList[1]));
                                rectTransform.anchorMin = anchorMin;
                            }
                            break;
                        case "anchor_max":
                            if (prop.Value is List<object> anchorMaxList && anchorMaxList.Count >= 2)
                            {
                                var anchorMax = new Vector2(Convert.ToSingle(anchorMaxList[0]), Convert.ToSingle(anchorMaxList[1]));
                                rectTransform.anchorMax = anchorMax;
                            }
                            break;
                        case "pivot":
                            if (prop.Value is List<object> pivotList && pivotList.Count >= 2)
                            {
                                var pivot = new Vector2(Convert.ToSingle(pivotList[0]), Convert.ToSingle(pivotList[1]));
                                rectTransform.pivot = pivot;
                            }
                            break;
                    }
                }
                
                // Отметить сцену как измененную
                UnityEditor.EditorUtility.SetDirty(gameObject);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                    UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                
                var message = $"Successfully updated UI properties for '{objectName}'";
                return OperationResult.Ok(message, $"Properties applied: {string.Join(", ", properties.Keys)}");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"Set UI properties failed: {ex.Message}");
            }
        }
        
        public static OperationResult ListUIElements(UnityRequest request)
        {
            try
            {
                var canvasName = request.GetValue<string>("canvas_name");
                
                var uiElements = new List<Dictionary<string, object>>();
                var allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                
                foreach (var obj in allObjects)
                {
                    var rectTransform = obj.GetComponent<RectTransform>();
                    if (rectTransform == null) continue;
                    
                    // Если указан конкретный Canvas, проверяем принадлежность
                    if (!string.IsNullOrEmpty(canvasName))
                    {
                        var canvas = obj.GetComponentInParent<Canvas>();
                        if (canvas == null || canvas.name != canvasName) continue;
                    }
                    
                    var elementInfo = new Dictionary<string, object>
                    {
                        ["name"] = obj.name,
                        ["type"] = GetUIElementType(obj),
                        ["position"] = new[] { rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y },
                        ["size"] = new[] { rectTransform.sizeDelta.x, rectTransform.sizeDelta.y },
                        ["active"] = obj.activeInHierarchy
                    };
                    
                    uiElements.Add(elementInfo);
                }
                
                var message = $"Found {uiElements.Count} UI elements";
                return OperationResult.Ok(message, uiElements);
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"List UI elements failed: {ex.Message}");
            }
        }
        
        // ===== ADVANCED PREFAB MANAGEMENT =====
        
        public static OperationResult CreatePrefabFromSelection(UnityRequest request)
        {
            try
            {
                var prefabPath = request.GetValue<string>("prefab_path");
                var prefabName = request.GetValue("prefab_name", "New Prefab");
                
                if (string.IsNullOrEmpty(prefabPath))
                    return OperationResult.Fail("prefab_path parameter is required");
                
                var selectedObjects = UnityEditor.Selection.gameObjects;
                if (selectedObjects.Length == 0)
                    return OperationResult.Fail("No objects selected");
                
                if (selectedObjects.Length == 1)
                {
                    // Создать префаб из одного объекта
                    var prefab = UnityEditor.PrefabUtility.SaveAsPrefabAsset(selectedObjects[0], prefabPath);
                    if (prefab == null)
                        return OperationResult.Fail("Failed to create prefab");
                    
                    var message = $"Successfully created prefab from '{selectedObjects[0].name}'";
                    return OperationResult.Ok(message, $"Prefab created at: {prefabPath}");
                }
                else
                {
                    // Создать префаб из нескольких объектов (как Variant)
                    var parentObj = new GameObject(prefabName);
                    foreach (var obj in selectedObjects)
                    {
                        obj.transform.SetParent(parentObj.transform);
                    }
                    
                    var prefab = UnityEditor.PrefabUtility.SaveAsPrefabAsset(parentObj, prefabPath);
                    if (prefab == null)
                        return OperationResult.Fail("Failed to create prefab from multiple objects");
                    
                    var message = $"Successfully created prefab from {selectedObjects.Length} objects";
                    return OperationResult.Ok(message, $"Prefab created at: {prefabPath}");
                }
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"Create prefab from selection failed: {ex.Message}");
            }
        }
        
        public static OperationResult UpdatePrefab(UnityRequest request)
        {
            try
            {
                var prefabPath = request.GetValue<string>("prefab_path");
                var objectName = request.GetValue<string>("object_name");
                
                if (string.IsNullOrEmpty(prefabPath))
                    return OperationResult.Fail("prefab_path parameter is required");
                    
                if (string.IsNullOrEmpty(objectName))
                    return OperationResult.Fail("object_name parameter is required");
                
                var gameObject = GameObject.Find(objectName);
                if (gameObject == null)
                    return OperationResult.Fail($"GameObject '{objectName}' not found");
                
                var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null)
                    return OperationResult.Fail($"Prefab not found at path: {prefabPath}");
                
                // Обновить префаб
                UnityEditor.PrefabUtility.SaveAsPrefabAsset(gameObject, prefabPath);
                
                // Обновить AssetDatabase
                UnityEditor.AssetDatabase.Refresh();
                UnityEditor.AssetDatabase.SaveAssets();
                
                var message = $"Successfully updated prefab '{prefab.name}'";
                return OperationResult.Ok(message, $"Prefab updated at: {prefabPath}");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"Update prefab failed: {ex.Message}");
            }
        }
        
        // ===== ADVANCED SCRIPT MANAGEMENT =====
        
        public static OperationResult CreateScriptTemplate(UnityRequest request)
        {
            try
            {
                var scriptName = request.GetValue<string>("script_name");
                var templateType = request.GetValue("template_type", "monobehaviour");
                var namespaceName = request.GetValue<string>("namespace_name");
                
                if (string.IsNullOrEmpty(scriptName))
                    return OperationResult.Fail("script_name parameter is required");
                
                var scriptContent = GenerateScriptTemplate(scriptName, templateType, namespaceName);
                var scriptPath = CreateScriptAsset(scriptName, scriptContent);
                
                if (string.IsNullOrEmpty(scriptPath))
                    return OperationResult.Fail("Failed to create script asset");
                
                var message = $"Successfully created {templateType} script '{scriptName}'";
                return OperationResult.Ok(message, $"Script created at: {scriptPath}");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"Create script template failed: {ex.Message}");
            }
        }
        
        public static OperationResult AddComponentToAll(UnityRequest request)
        {
            try
            {
                var scriptName = request.GetValue<string>("script_name");
                var tagFilter = request.GetValue<string>("tag_filter");
                var nameFilter = request.GetValue<string>("name_filter");
                
                if (string.IsNullOrEmpty(scriptName))
                    return OperationResult.Fail("script_name parameter is required");
                
                var scriptType = FindScriptTypeByName(scriptName);
                if (scriptType == null)
                    return OperationResult.Fail($"Script '{scriptName}' not found");
                
                var allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                var addedCount = 0;
                
                foreach (var obj in allObjects)
                {
                    // Применить фильтры
                    if (!string.IsNullOrEmpty(tagFilter) && obj.tag != tagFilter) continue;
                    if (!string.IsNullOrEmpty(nameFilter) && !obj.name.Contains(nameFilter)) continue;
                    
                    // Проверить, есть ли уже компонент
                    if (obj.GetComponent(scriptType) != null) continue;
                    
                    // Добавить компонент
                    obj.AddComponent(scriptType);
                    addedCount++;
                }
                
                // Отметить сцену как измененную
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                    UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                
                var message = $"Successfully added '{scriptName}' to {addedCount} objects";
                return OperationResult.Ok(message, $"Components added: {addedCount}");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"Add component to all failed: {ex.Message}");
            }
        }
        
        // ===== SCENE MANAGEMENT =====
        
        public static OperationResult CreateEmptyScene(UnityRequest request)
        {
            try
            {
                var sceneName = request.GetValue("scene_name", "New Scene");
                
                var newScene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                    UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects);
                
                var scenePath = $"Assets/Scenes/{sceneName}.unity";
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(newScene, scenePath);
                
                var message = $"Successfully created new scene '{sceneName}'";
                return OperationResult.Ok(message, $"Scene created at: {scenePath}");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"Create empty scene failed: {ex.Message}");
            }
        }
        
        public static OperationResult LoadScene(UnityRequest request)
        {
            try
            {
                var scenePath = request.GetValue<string>("scene_path");
                
                if (string.IsNullOrEmpty(scenePath))
                    return OperationResult.Fail("scene_path parameter is required");
                
                var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
                
                var message = $"Successfully loaded scene '{scene.name}'";
                return OperationResult.Ok(message, $"Scene loaded: {scenePath}");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"Load scene failed: {ex.Message}");
            }
        }
        
        public static OperationResult SaveScene(UnityRequest request)
        {
            try
            {
                var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
                var success = UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
                
                if (!success)
                    return OperationResult.Fail("Failed to save scene");
                
                var message = $"Successfully saved scene '{scene.name}'";
                return OperationResult.Ok(message, $"Scene saved: {scene.path}");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"Save scene failed: {ex.Message}");
            }
        }
        
        // ===== HELPER METHODS =====
        
        private static RenderMode ParseRenderMode(string renderMode)
        {
            switch (renderMode.ToLower())
            {
                case "screenspaceoverlay":
                    return RenderMode.ScreenSpaceOverlay;
                case "screenspacecamera":
                    return RenderMode.ScreenSpaceCamera;
                case "worldspace":
                    return RenderMode.WorldSpace;
                default:
                    return RenderMode.ScreenSpaceOverlay;
            }
        }
        
        private static GameObject CreateButton(string name, GameObject parent)
        {
            var buttonObj = new GameObject(name);
            var rectTransform = buttonObj.AddComponent<RectTransform>();
            var image = buttonObj.AddComponent<UnityEngine.UI.Image>();
            var button = buttonObj.AddComponent<UnityEngine.UI.Button>();
            
            // Создать дочерний объект для текста
            var textObj = new GameObject("Text");
            var textRect = textObj.AddComponent<RectTransform>();
            var text = textObj.AddComponent<UnityEngine.UI.Text>();
            
            textObj.transform.SetParent(buttonObj.transform);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            text.text = "Button";
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.color = Color.black;
            text.alignment = TextAnchor.MiddleCenter;
            
            if (parent != null)
                buttonObj.transform.SetParent(parent.transform);
            
            return buttonObj;
        }
        
        private static GameObject CreateText(string name, GameObject parent)
        {
            var textObj = new GameObject(name);
            var rectTransform = textObj.AddComponent<RectTransform>();
            var text = textObj.AddComponent<UnityEngine.UI.Text>();
            
            text.text = "New Text";
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.color = Color.black;
            text.alignment = TextAnchor.MiddleCenter;
            
            if (parent != null)
                textObj.transform.SetParent(parent.transform);
            
            return textObj;
        }
        
        private static GameObject CreateImage(string name, GameObject parent)
        {
            var imageObj = new GameObject(name);
            var rectTransform = imageObj.AddComponent<RectTransform>();
            var image = imageObj.AddComponent<UnityEngine.UI.Image>();
            
            image.color = Color.white;
            
            if (parent != null)
                imageObj.transform.SetParent(parent.transform);
            
            return imageObj;
        }
        
        private static GameObject CreateInputField(string name, GameObject parent)
        {
            var inputObj = new GameObject(name);
            var rectTransform = inputObj.AddComponent<RectTransform>();
            var image = inputObj.AddComponent<UnityEngine.UI.Image>();
            var inputField = inputObj.AddComponent<UnityEngine.UI.InputField>();
            
            // Создать дочерний объект для текста
            var textObj = new GameObject("Text");
            var textRect = textObj.AddComponent<RectTransform>();
            var text = textObj.AddComponent<UnityEngine.UI.Text>();
            
            textObj.transform.SetParent(inputObj.transform);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 6);
            textRect.offsetMax = new Vector2(-10, -6);
            
            text.text = "";
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.color = Color.black;
            text.supportRichText = false;
            
            inputField.textComponent = text;
            
            if (parent != null)
                inputObj.transform.SetParent(parent.transform);
            
            return inputObj;
        }
        
        private static GameObject CreatePanel(string name, GameObject parent)
        {
            var panelObj = new GameObject(name);
            var rectTransform = panelObj.AddComponent<RectTransform>();
            var image = panelObj.AddComponent<UnityEngine.UI.Image>();
            
            image.color = new Color(1f, 1f, 1f, 0.1f);
            
            if (parent != null)
                panelObj.transform.SetParent(parent.transform);
            
            return panelObj;
        }
        
        private static GameObject CreateSlider(string name, GameObject parent)
        {
            var sliderObj = new GameObject(name);
            var rectTransform = sliderObj.AddComponent<RectTransform>();
            var image = sliderObj.AddComponent<UnityEngine.UI.Image>();
            var slider = sliderObj.AddComponent<UnityEngine.UI.Slider>();
            
            // Создать Background
            var backgroundObj = new GameObject("Background");
            var bgRect = backgroundObj.AddComponent<RectTransform>();
            var bgImage = backgroundObj.AddComponent<UnityEngine.UI.Image>();
            
            backgroundObj.transform.SetParent(sliderObj.transform);
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            bgRect.anchoredPosition = Vector2.zero;
            
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            slider.fillRect = bgRect;
            
            if (parent != null)
                sliderObj.transform.SetParent(parent.transform);
            
            return sliderObj;
        }
        
        private static GameObject CreateToggle(string name, GameObject parent)
        {
            var toggleObj = new GameObject(name);
            var rectTransform = toggleObj.AddComponent<RectTransform>();
            var toggle = toggleObj.AddComponent<UnityEngine.UI.Toggle>();
            
            // Создать Background
            var backgroundObj = new GameObject("Background");
            var bgRect = backgroundObj.AddComponent<RectTransform>();
            var bgImage = backgroundObj.AddComponent<UnityEngine.UI.Image>();
            
            backgroundObj.transform.SetParent(toggleObj.transform);
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            bgRect.anchoredPosition = Vector2.zero;
            
            bgImage.color = Color.white;
            toggle.targetGraphic = bgImage;
            
            if (parent != null)
                toggleObj.transform.SetParent(parent.transform);
            
            return toggleObj;
        }
        
        private static GameObject CreateDropdown(string name, GameObject parent)
        {
            var dropdownObj = new GameObject(name);
            var rectTransform = dropdownObj.AddComponent<RectTransform>();
            var image = dropdownObj.AddComponent<UnityEngine.UI.Image>();
            var dropdown = dropdownObj.AddComponent<UnityEngine.UI.Dropdown>();
            
            if (parent != null)
                dropdownObj.transform.SetParent(parent.transform);
            
            return dropdownObj;
        }
        
        private static string GetUIElementType(GameObject obj)
        {
            if (obj.GetComponent<UnityEngine.UI.Button>() != null) return "Button";
            if (obj.GetComponent<UnityEngine.UI.Text>() != null) return "Text";
            if (obj.GetComponent<UnityEngine.UI.Image>() != null) return "Image";
            if (obj.GetComponent<UnityEngine.UI.InputField>() != null) return "InputField";
            if (obj.GetComponent<UnityEngine.UI.Slider>() != null) return "Slider";
            if (obj.GetComponent<UnityEngine.UI.Toggle>() != null) return "Toggle";
            if (obj.GetComponent<UnityEngine.UI.Dropdown>() != null) return "Dropdown";
            if (obj.GetComponent<Canvas>() != null) return "Canvas";
            return "UIElement";
        }
        
        private static string GenerateScriptTemplate(string scriptName, string templateType, string namespaceName)
        {
            var usingStatements = "using UnityEngine;\nusing UnityEngine.UI;\n";
            var namespaceStart = string.IsNullOrEmpty(namespaceName) ? "" : $"namespace {namespaceName}\n{{\n    ";
            var namespaceEnd = string.IsNullOrEmpty(namespaceName) ? "" : "\n}";
            
            switch (templateType.ToLower())
            {
                case "monobehaviour":
                    return $@"{usingStatements}
{namespaceStart}public class {scriptName} : MonoBehaviour
{{
    void Start()
    {{
        // Инициализация
    }}
    
    void Update()
    {{
        // Обновление каждый кадр
    }}
}}{namespaceEnd}";
                
                case "singleton":
                    return $@"{usingStatements}
{namespaceStart}public class {scriptName} : MonoBehaviour
{{
    public static {scriptName} Instance {{ get; private set; }}
    
    void Awake()
    {{
        if (Instance == null)
        {{
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }}
        else
        {{
            Destroy(gameObject);
        }}
    }}
}}{namespaceEnd}";
                
                case "ui_controller":
                    return $@"{usingStatements}
{namespaceStart}public class {scriptName} : MonoBehaviour
{{
    [Header(""UI References"")]
    public Button startButton;
    public Text statusText;
    
    void Start()
    {{
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);
    }}
    
    void OnStartButtonClicked()
    {{
        if (statusText != null)
            statusText.text = ""Button clicked!"";
    }}
    
    void OnDestroy()
    {{
        if (startButton != null)
            startButton.onClick.RemoveListener(OnStartButtonClicked);
    }}
}}{namespaceEnd}";
                
                default:
                    return $@"{usingStatements}
{namespaceStart}public class {scriptName} : MonoBehaviour
{{
    void Start()
    {{
        // Инициализация
    }}
}}{namespaceEnd}";
            }
        }
        
        private static string CreateScriptAsset(string scriptName, string scriptContent)
        {
            try
            {
                // Определить папку для скриптов
                var scriptsFolder = "Assets/Scripts";
                if (!UnityEditor.AssetDatabase.IsValidFolder(scriptsFolder))
                {
                    UnityEditor.AssetDatabase.CreateFolder("Assets", "Scripts");
                }
                
                // Создать полный путь к файлу
                var scriptPath = $"{scriptsFolder}/{scriptName}.cs";
                
                // Проверить, не существует ли уже файл
                if (UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(scriptPath) != null)
                {
                    // Если файл существует, добавить номер
                    var counter = 1;
                    while (UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(scriptPath) != null)
                    {
                        scriptPath = $"{scriptsFolder}/{scriptName}_{counter}.cs";
                        counter++;
                    }
                }
                
                // Записать файл
                System.IO.File.WriteAllText(scriptPath, scriptContent);
                
                // Обновить AssetDatabase
                UnityEditor.AssetDatabase.Refresh();
                
                return scriptPath;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error creating script asset: {ex.Message}");
                return null;
            }
        }
        
        private static System.Type FindScriptTypeByName(string scriptName)
        {
            try
            {
                // Искать во всех загруженных ассемблиях
                var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                
                foreach (var assembly in assemblies)
                {
                    try
                    {
                        var types = assembly.GetTypes();
                        foreach (var type in types)
                        {
                            if (type.Name.Equals(scriptName, StringComparison.OrdinalIgnoreCase) ||
                                type.FullName.Equals(scriptName, StringComparison.OrdinalIgnoreCase))
                            {
                                // Проверить, что это MonoBehaviour
                                if (typeof(MonoBehaviour).IsAssignableFrom(type))
                                {
                                    return type;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Игнорировать ошибки доступа к типам
                        continue;
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error finding script type: {ex.Message}");
                return null;
            }
        }
    }
}
