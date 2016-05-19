using UnityEngine;
using System.Collections;

public static class Utility
{
    public static IEnumerator FadeCoroutine(SpriteRenderer rd, float fadeStart, float fadeTarget, float timeSec, bool active)
    {
        float currentTime = Time.time;
        rd.material.color = new Color(rd.material.color.r, rd.material.color.g, rd.material.color.b, fadeStart);

        while (Time.time - currentTime <= timeSec)
        {
            float lerp = (Time.time - currentTime) / timeSec;
            float alpha = Mathf.Lerp(fadeStart, fadeTarget, lerp);
            rd.material.color = new Color(rd.material.color.r, rd.material.color.g, rd.material.color.b, alpha);

            yield return null;
        }
        rd.material.color = new Color(rd.material.color.r, rd.material.color.g, rd.material.color.b, fadeTarget);
        rd.gameObject.SetActive(active);

        yield return null;
    }

    public static IEnumerator FadeCoroutineUI(CanvasGroup grp, float fadeStart, float fadeTarget, float timeSec, bool active)
    {
        float currentTime = Time.time;

        grp.alpha = fadeStart;

        while (Time.time - currentTime <= timeSec)
        {
            float lerp = (Time.time - currentTime) / timeSec;

            grp.alpha = Mathf.Lerp(fadeStart, fadeTarget, lerp);
            yield return null;
        }
        grp.alpha = fadeTarget;
        grp.gameObject.SetActive(active);

        yield return null;
    }

    public static IEnumerator TransformCoroutine(Transform t, Vector3 startPos, Quaternion startRot, Vector3 startScale,
                                                    Vector3 targetPos, Quaternion targetRot, Vector3 targetScale, float timeSec, bool active)
    {
        float currentTime = Time.time;

        t.localPosition = startPos;
        t.localRotation = startRot;
        t.localScale = startScale;

        while (Time.time - currentTime <= timeSec)
        {
            float lerp = (Time.time - currentTime) / timeSec;

            t.localPosition = Vector3.Lerp(startPos, targetPos, lerp);
            t.localRotation = Quaternion.Slerp(startRot, targetRot, lerp);
            t.localScale = Vector3.Lerp(startScale, targetScale, lerp);
            yield return null;
        }

        t.localPosition = targetPos;
        t.localRotation = targetRot;
        t.localScale = targetScale;

        t.gameObject.SetActive(active);

        yield return null;
    }

    public static IEnumerator TransformCoroutineUI(RectTransform rt, Vector3 startPos, Quaternion startRot, Vector3 startScale,
                                                    Vector3 targetPos, Quaternion targetRot, Vector3 targetScale, float timeSec, bool active)
    {
        float currentTime = Time.time;

        rt.anchoredPosition = startPos;
        rt.localRotation = startRot;
        rt.localScale = startScale;

        while (Time.time - currentTime <= timeSec)
        {
            float lerp = (Time.time - currentTime) / timeSec;

            rt.anchoredPosition = Vector3.Lerp(startPos, targetPos, lerp);
            rt.localRotation = Quaternion.Slerp(startRot, targetRot, lerp);
            rt.localScale = Vector3.Lerp(startScale, targetScale, lerp);
            yield return null;
        }

        rt.anchoredPosition = targetPos;
        rt.localRotation = targetRot;
        rt.localScale = targetScale;

        rt.gameObject.SetActive(active);

        yield return null;
    }

    public static bool IsCursorInUIBounds(RectTransform rt, Vector2 cursorPos)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(rt, cursorPos);

        //Vector2 rMin = new Vector2(rt.rect.xMin, rt.rect.yMin);
        //Vector2 rMax = new Vector2(rt.rect.xMax, rt.rect.yMax);

        //rMin = RectTransformUtility.WorldToScreenPoint(Camera.main, rMin);
        //rMax = RectTransformUtility.WorldToScreenPoint(Camera.main, rMax);

        //if(
        //    cursorPos.x > rt.rect.xMin &&
        //    cursorPos.x < rt.rect.xMax &&
        //    cursorPos.y > rt.rect.yMin &&
        //    cursorPos.y < rt.rect.yMax
        //    )
        //{
        //    return true;
        //}
        //else
        //{
        //    return false;
        //}
    }
}
