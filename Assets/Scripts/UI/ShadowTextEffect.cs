using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ShadowTextEffect : MonoBehaviour {

    Text shadowText;
    Text highlightText;

    GameObject highlightObject;
    RectTransform highlightRect;

    Outline shadowOutline;
    Outline highlightOutline;

    public Color HighlightColor = Color.white;
    public Color HighlightOutline = Color.black;
    public Color ShadowColor = Color.black;
    public Color ShadowOutline = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    public Vector2 HighlightOffset = new Vector2(-4, 4);
    public Vector2 HighlightOutlineOffset = new Vector2(2, 2);
    public Vector2 ShadowOutlineOffset = new Vector2(1, -1);

    const string TEXT_NAME = "Text Highlight";

    string lastText = null;
    bool? lastVisibility = null;

    private void Update()
    {
        if (shadowText != null && lastText != shadowText.text)
            highlightText.text = lastText = shadowText.text;

        if (shadowText != null && (!lastVisibility.HasValue || lastVisibility.Value != shadowText.enabled))
            lastVisibility = highlightText.enabled = shadowText.enabled;
    }

    ShadowTextEffect parentComponent;

    // Use this for initialization
    void Start () {

        // Get and configure shadow text
        shadowText = GetComponent<Text>();
        shadowText.color = ShadowColor;

        // Get and configure shadow outline
        shadowOutline = shadowText.gameObject.AddComponent<Outline>();
        shadowOutline.effectColor = ShadowOutline;
        shadowOutline.effectDistance = ShadowOutlineOffset;

        // Get and configure highlight game object
        highlightObject = new GameObject(TEXT_NAME, new Type[] { typeof(RectTransform), typeof(Text), typeof(Outline) });
        highlightObject.transform.SetParent(transform);
        highlightRect = highlightObject.GetComponent<RectTransform>();
        highlightRect.anchorMin = new Vector2(0, 0);
        highlightRect.anchorMax = new Vector2(1, 1);
        highlightRect.offsetMin = HighlightOffset;
        highlightRect.offsetMax = HighlightOffset;
        highlightRect.localScale = Vector3.one;

        // Get and configure highlight text
        highlightText = highlightObject.GetComponent<Text>();
        highlightText.color = HighlightColor;
        highlightText.alignByGeometry = shadowText.alignByGeometry;
        highlightText.alignment = shadowText.alignment;
        highlightText.font = shadowText.font;
        highlightText.fontSize = shadowText.fontSize;
        highlightText.fontStyle = shadowText.fontStyle;
        highlightText.horizontalOverflow = shadowText.horizontalOverflow;
        highlightText.lineSpacing = shadowText.lineSpacing;
        highlightText.resizeTextForBestFit = shadowText.resizeTextForBestFit;
        highlightText.resizeTextMaxSize = shadowText.resizeTextMaxSize;
        highlightText.resizeTextMinSize = shadowText.resizeTextMinSize;
        highlightText.supportRichText = shadowText.supportRichText;
        highlightText.text = shadowText.text;
        highlightText.verticalOverflow = shadowText.verticalOverflow;


        // Get and configure highlight outline
        highlightOutline = highlightObject.GetComponent<Outline>();
        highlightOutline.effectColor = HighlightOutline;
        highlightOutline.effectDistance = HighlightOutlineOffset;
    }

}
