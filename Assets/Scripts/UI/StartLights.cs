using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StartLights : MonoBehaviour {

    [SerializeField] private Image[] lamps;
    [SerializeField] private PlayerInputProvider player;
    [SerializeField] private float lampInterval = 1f;
    [SerializeField] private Vector2 holdRange = new Vector2(0.1f, 3f);
    [SerializeField] private Color litColor = Color.red;
    [SerializeField] private Color darkColor = new Color(0.15f, 0.15f, 0.15f);

    public event Action Started;
    public event Action FalseStarted;

    private bool watchingForFalseStart;

    private IEnumerator Start() {
        SetAllLamps(darkColor);
        watchingForFalseStart = true;

        foreach (Image lamp in lamps) {
            yield return new WaitForSeconds(lampInterval);
            lamp.color = litColor;
        }

        yield return new WaitForSeconds(UnityEngine.Random.Range(holdRange.x, holdRange.y));

        watchingForFalseStart = false;
        SetAllLamps(darkColor);
        Started?.Invoke();
    }

    private void Update() {
        if (!watchingForFalseStart || player.Throttle <= 0f) return;

        watchingForFalseStart = false;
        Debug.Log("Jump start");
        FalseStarted?.Invoke();
    }

    private void SetAllLamps(Color color) {
        foreach (Image lamp in lamps) {
            lamp.color = color;
        }
    }
}
