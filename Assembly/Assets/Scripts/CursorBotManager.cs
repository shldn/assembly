using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CursorBotManager : MonoBehaviour {

    public int maxNumCursors = 25;
    public float delayBetweenStarts = 0.25f; // seconds
    public bool useRandomColors = true;
    public static CursorBotManager Inst = null;

    private System.DateTime lastSpawnTime = System.DateTime.Now;

    private static HashSet<CursorBot> allBots = new HashSet<CursorBot>();
    public static void RemoveBot(CursorBot bot)
    {
        allBots.Remove(bot);
    }

    void Awake()
    {
        Inst = this;
    }

	void Update () {
        if (allBots.Count > maxNumCursors || (System.DateTime.Now - lastSpawnTime).TotalSeconds < delayBetweenStarts)
            return;

        int edgeBuffer = 20;
        CursorBot bot = gameObject.AddComponent<CursorBot>();
        bot.DrawCircle(new Vector2(Random.Range(edgeBuffer, Screen.width - edgeBuffer), Random.Range(edgeBuffer, Screen.height - edgeBuffer)), Random.Range(50.0f, 0.5f * Screen.height), Random.Range(1.0f, 5.0f));
        allBots.Add(bot);

        lastSpawnTime = System.DateTime.Now;
	}
}
