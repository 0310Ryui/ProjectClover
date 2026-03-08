using UnityEngine;
using UnityEngine.UI;

public class MagicUiManager : MonoBehaviour
{
    private const string GeneratedRootName = "MagicUiRoot";

    private UIManager _uiManager;
    private MagicManager _magicManager;
    private Font _font;

    private RectTransform _generatedRoot;
    private RectTransform _listContainer;
    private Text _summaryText;

    public static MagicUiManager EnsureSetup(GameObject overlay, MagicManager magicManager = null, UIManager uiManager = null)
    {
        if (overlay == null) return null;

        var manager = overlay.GetComponent<MagicUiManager>();
        if (manager == null)
        {
            manager = overlay.AddComponent<MagicUiManager>();
        }

        manager._magicManager = magicManager != null ? magicManager : FindFirstObjectByType<MagicManager>();
        manager._uiManager = uiManager;
        manager.InitializeIfNeeded();
        return manager;
    }

    private void Awake()
    {
        InitializeIfNeeded();
    }

    private void OnEnable()
    {
        InitializeIfNeeded();
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (_listContainer == null) return;
        ResolveDependencies();

        foreach (Transform child in _listContainer)
        {
            Destroy(child.gameObject);
        }

        if (_magicManager == null || _magicManager.allRecipes == null || _magicManager.allRecipes.Count == 0)
        {
            _summaryText.text = "研究できる魔法がありません";
            CreateEmptyState("レシピが未設定です");
            return;
        }

        var inventory = SaveManager.Current.player.inventory;
        _summaryText.text = "所持金 " + inventory.money + " G";

        foreach (var recipe in _magicManager.allRecipes)
        {
            if (recipe == null) continue;
            CreateRecipeCard(recipe);
        }
    }

    private void InitializeIfNeeded()
    {
        if (_generatedRoot != null) return;

        _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        HideLegacyChildren();
        BuildLayout();
        RefreshUI();
    }

    private void ResolveDependencies()
    {
        if (_magicManager == null)
        {
            _magicManager = FindFirstObjectByType<MagicManager>();
        }
    }

    private void HideLegacyChildren()
    {
        foreach (Transform child in transform)
        {
            if (child.name == GeneratedRootName) continue;
            child.gameObject.SetActive(false);
        }
    }

    private void BuildLayout()
    {
        _generatedRoot = CreateUIObject(GeneratedRootName, transform);
        Stretch(_generatedRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        _generatedRoot.gameObject.AddComponent<Image>().color = new Color(0.14f, 0.11f, 0.16f, 0.96f);

        var header = CreateUIObject("Header", _generatedRoot);
        Stretch(header, new Vector2(0f, 0.88f), new Vector2(1f, 1f), new Vector2(28f, -20f), new Vector2(-28f, -20f));

        var title = CreateText("Title", header, "Magic Research", 34, TextAnchor.MiddleLeft, new Color(0.98f, 0.91f, 0.79f));
        Stretch(title.rectTransform, new Vector2(0f, 0.3f), new Vector2(0.52f, 1f), Vector2.zero, Vector2.zero);

        _summaryText = CreateText("Summary", header, string.Empty, 24, TextAnchor.MiddleLeft, new Color(0.86f, 0.82f, 0.73f));
        Stretch(_summaryText.rectTransform, new Vector2(0f, 0f), new Vector2(0.52f, 0.34f), Vector2.zero, Vector2.zero);

        var closeButton = CreateButton("Btn_CloseMagicGenerated", header, "閉じる", new Color(0.7f, 0.27f, 0.25f), OnCloseClicked);
        Stretch((RectTransform)closeButton.transform, new Vector2(0.8f, 0.18f), new Vector2(1f, 0.82f), Vector2.zero, Vector2.zero);

        var viewport = CreateUIObject("ListViewport", _generatedRoot);
        Stretch(viewport, new Vector2(0f, 0f), new Vector2(1f, 0.86f), new Vector2(28f, 28f), new Vector2(-28f, -18f));
        viewport.gameObject.AddComponent<Image>().color = new Color(0.23f, 0.19f, 0.26f, 0.92f);
        viewport.gameObject.AddComponent<Mask>().showMaskGraphic = true;

        var scrollRect = viewport.gameObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 25f;

        var content = CreateUIObject("Content", viewport);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;

        var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.spacing = 16f;
        layout.padding = new RectOffset(0, 0, 8, 8);

        var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewport;
        scrollRect.content = content;
        _listContainer = content;
    }

    private void CreateRecipeCard(MagicRecipeData recipe)
    {
        var card = CreateUIObject("Recipe_" + recipe.recipeId, _listContainer);
        card.gameObject.AddComponent<Image>().color = new Color(0.95f, 0.9f, 0.83f, 1f);
        var layoutElement = card.gameObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 178f;

        var title = CreateText("RecipeName", card, recipe.recipeName, 28, TextAnchor.UpperLeft, new Color(0.18f, 0.12f, 0.08f));
        Stretch(title.rectTransform, new Vector2(0.03f, 0.68f), new Vector2(0.68f, 0.94f), Vector2.zero, Vector2.zero);

        var requirements = CreateText("Requirements", card, BuildRequirementText(recipe), 20, TextAnchor.UpperLeft, new Color(0.28f, 0.2f, 0.16f));
        Stretch(requirements.rectTransform, new Vector2(0.03f, 0.1f), new Vector2(0.68f, 0.62f), Vector2.zero, Vector2.zero);

        var status = CreateText("Status", card, GetStatusText(recipe), 20, TextAnchor.UpperLeft, GetStatusColor(recipe));
        Stretch(status.rectTransform, new Vector2(0.72f, 0.58f), new Vector2(0.97f, 0.88f), Vector2.zero, Vector2.zero);

        var actionButton = CreateButton("Btn_Research", card, GetButtonLabel(recipe), new Color(0.42f, 0.32f, 0.65f), () => OnResearchClicked(recipe));
        Stretch((RectTransform)actionButton.transform, new Vector2(0.72f, 0.16f), new Vector2(0.97f, 0.46f), Vector2.zero, Vector2.zero);
        actionButton.interactable = IsButtonInteractable(recipe);

        var buttonImage = actionButton.GetComponent<Image>();
        buttonImage.color = actionButton.interactable ? new Color(0.42f, 0.32f, 0.65f) : new Color(0.45f, 0.45f, 0.45f);
    }

    private void CreateEmptyState(string message)
    {
        var empty = CreateText("EmptyState", _listContainer, message, 26, TextAnchor.MiddleCenter, new Color(0.94f, 0.9f, 0.82f));
        var layoutElement = empty.gameObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 120f;
    }

    private void OnResearchClicked(MagicRecipeData recipe)
    {
        if (_magicManager == null || recipe == null) return;

        _magicManager.ResearchMagic(recipe);
        RefreshUI();
    }

    private void OnCloseClicked()
    {
        if (_uiManager != null) _uiManager.HideMagicOverlay();
        else gameObject.SetActive(false);
    }

    private bool IsButtonInteractable(MagicRecipeData recipe)
    {
        if (_magicManager == null || recipe == null) return false;
        if (!recipe.isConsumable && SaveManager.Current.player.unlockedMagics.Contains(recipe.recipeId)) return false;
        return _magicManager.CanResearch(recipe);
    }

    private string GetButtonLabel(MagicRecipeData recipe)
    {
        if (recipe == null) return "研究不可";
        if (!recipe.isConsumable && SaveManager.Current.player.unlockedMagics.Contains(recipe.recipeId)) return "研究済み";
        return _magicManager != null && _magicManager.CanResearch(recipe) ? "研究する" : "素材不足";
    }

    private string GetStatusText(MagicRecipeData recipe)
    {
        if (recipe == null) return "状態不明";

        if (!recipe.isConsumable && SaveManager.Current.player.unlockedMagics.Contains(recipe.recipeId))
        {
            return "解放済み";
        }

        return _magicManager != null && _magicManager.CanResearch(recipe) ? "今すぐ研究できます" : "素材かお金が不足しています";
    }

    private Color GetStatusColor(MagicRecipeData recipe)
    {
        if (recipe != null && !recipe.isConsumable && SaveManager.Current.player.unlockedMagics.Contains(recipe.recipeId))
        {
            return new Color(0.16f, 0.5f, 0.24f);
        }

        return _magicManager != null && _magicManager.CanResearch(recipe)
            ? new Color(0.18f, 0.47f, 0.28f)
            : new Color(0.65f, 0.24f, 0.22f);
    }

    private static string BuildRequirementText(MagicRecipeData recipe)
    {
        var inventory = SaveManager.Current.player.inventory;
        var text = "必要コスト\n";
        text += "お金: " + recipe.requiredMoney + " G / 所持 " + inventory.money + " G";

        if (recipe.requiredMaterials == null || recipe.requiredMaterials.Count == 0)
        {
            return text + "\n素材: なし";
        }

        for (var i = 0; i < recipe.requiredMaterials.Count; i++)
        {
            var requirement = recipe.requiredMaterials[i];
            if (requirement.material == null) continue;
            var ownedAmount = inventory.GetMaterialCount(requirement.material.materialId);
            text += "\n" + requirement.material.materialName + " x" + requirement.amount + " / 所持 " + ownedAmount;
        }

        return text;
    }

    private Button CreateButton(string objectName, Transform parent, string label, Color backgroundColor, UnityEngine.Events.UnityAction onClick)
    {
        var buttonObject = CreateUIObject(objectName, parent).gameObject;
        var image = buttonObject.AddComponent<Image>();
        image.color = backgroundColor;

        var button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        var text = CreateText("Text", (RectTransform)button.transform, label, 22, TextAnchor.MiddleCenter, Color.white);
        Stretch(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(10f, 8f), new Vector2(-10f, -8f));
        return button;
    }

    private Text CreateText(string objectName, Transform parent, string value, int fontSize, TextAnchor alignment, Color color)
    {
        var textObject = CreateUIObject(objectName, parent).gameObject;
        var text = textObject.AddComponent<Text>();
        text.font = _font;
        text.text = value;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static RectTransform CreateUIObject(string objectName, Transform parent)
    {
        var go = new GameObject(objectName, typeof(RectTransform));
        var rectTransform = go.GetComponent<RectTransform>();
        rectTransform.SetParent(parent, false);
        rectTransform.localScale = Vector3.one;
        return rectTransform;
    }

    private static void Stretch(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = offsetMin;
        rectTransform.offsetMax = offsetMax;
    }
}
