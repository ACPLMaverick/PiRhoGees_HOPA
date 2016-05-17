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
}
