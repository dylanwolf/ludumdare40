using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slideout : MonoBehaviour {

    RectTransform _rt;

    enum SlideState
    {
        Extended,
        Collapsing,
        Collapsed,
        Extending
    }

    SlideState state;
    public float SlideSpeed = 1.0f;
    float extensionSize;

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
        extensionSize = _rt.offsetMax.x;
        state = SlideState.Extended;
    }

    Vector2 tmpV2;
    public void ToggleSlideout()
    {
        switch(state)
        {
            case SlideState.Extended:
                StartCoroutine(CollapseSlide());
                break;
            case SlideState.Collapsed:
                StartCoroutine(ExtendSlide());
                break;
        }

    }

    IEnumerator ExtendSlide()
    {
        state = SlideState.Extending;
        while (_rt.offsetMin.x < 0)
        {
            tmpV2 = _rt.offsetMin;
            tmpV2.x += (Time.deltaTime * SlideSpeed);
            tmpV2.x = Mathf.Clamp(tmpV2.x, -extensionSize, 0);
            _rt.offsetMin = tmpV2;

            tmpV2 = _rt.offsetMax;
            tmpV2.x = extensionSize + _rt.offsetMin.x;
            _rt.offsetMax = tmpV2;

            yield return null;
        }

        state = SlideState.Extended;
    }

    IEnumerator CollapseSlide()
    {
        state = SlideState.Collapsing;
        while (_rt.offsetMin.x > -extensionSize)
        {
            tmpV2 = _rt.offsetMin;
            tmpV2.x -= (Time.deltaTime * SlideSpeed);
            tmpV2.x = Mathf.Clamp(tmpV2.x, -extensionSize, 0);
            _rt.offsetMin = tmpV2;

            tmpV2 = _rt.offsetMax;
            tmpV2.x = extensionSize + _rt.offsetMin.x;
            _rt.offsetMax = tmpV2;

            yield return null;
        }

        state = SlideState.Collapsed;
    }
}
