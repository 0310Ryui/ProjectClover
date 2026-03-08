using UnityEngine;
using UnityEngine.UI;

var go = GameObject.Find("Text_MainTimer");
if (go == null) return "Text_MainTimer not found in scene!";

var text = go.GetComponent<Text>();
var rect = go.GetComponent<RectTransform>();

string info = $"ActiveSelf: {go.activeSelf}, ActiveInHierarchy: {go.activeInHierarchy}\n";
info += $"LocalPos: {rect.localPosition}, AnchoredPos: {rect.anchoredPosition}, SizeDelta: {rect.sizeDelta}\n";
info += $"AnchorMin: {rect.anchorMin}, AnchorMax: {rect.anchorMax}\n";
info += $"Scale: {rect.localScale}\n";
if (text != null) {
    info += $"TextProp: '{text.text}', FontSize: {text.fontSize}, Color: {text.color}\n";
    info += $"Font: {(text.font != null ? text.font.name : "NULL")}\n";
    info += $"HorizontalOverflow: {text.horizontalOverflow}, VerticalOverflow: {text.verticalOverflow}\n";
} else {
    info += "No Text component!\n";
}
return info;
