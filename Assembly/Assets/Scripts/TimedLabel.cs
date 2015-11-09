using UnityEngine;
using System.Globalization;

public class TimedLabel : MonoBehaviour {

    public string label;
    public float fadeTime;

    // events
    public delegate void FadeCompleteEventHandler(object sender);
    public FadeCompleteEventHandler FadeComplete;

    void Update() {
        if (fadeTime < 0 && FadeComplete != null) {
            FadeComplete(this);
            enabled = false;
        }
        fadeTime -= Time.deltaTime;
    }

	void OnGUI () {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        //screenPos.y = Screen.height - screenPos.y;
        if (screenPos.z < 0f)
            return;

        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        GUI.skin.label.fontSize = Mathf.CeilToInt(Mathf.Clamp(20f / (screenPos.z * 0.01f), 0, 50) * Screen.height / 1000f);

        GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(fadeTime * 0.3f));

        GUI.Label(MathUtilities.CenteredSquare(screenPos.x, screenPos.y - (Screen.height * 3f / screenPos.z), 1000f), CultureInfo.CurrentCulture.TextInfo.ToTitleCase(label));
    }
}
