﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CursorBot : MonoBehaviour {

    public Transform cursorObject;
    public LineRenderer cursorLine;


    Vector3 screenPos = Vector3.zero;
    public Vector3 screenPosSmoothed = Vector3.zero;
    Vector3 screenPosVel = Vector3.zero;
    float screenPosSmoothTime = 0.1f;

    List<Vector2> lastPoints = new List<Vector2>();
    bool selecting = false;
    bool initialPosSent = false;

    // Helpers for drawing fake circles
    Queue<Vector2> cursorPositions = new Queue<Vector2>();

	void Start () {
        cursorObject = (GameObject.Instantiate(Resources.Load("PlayerCursor")) as GameObject).transform;
        cursorLine = cursorObject.gameObject.GetComponent<LineRenderer>();
	}


    public void DrawCircle(Vector2 center, float radius, float duration = 3.0f)
    {
        int ptsPerSec = 30;
        int totalPts = (int)(ptsPerSec * duration);
        float angleInc = 2.0f * Mathf.PI / (float)totalPts;
        float angle = 0.0f;
        for (int i = 0; i < totalPts+1; ++i)
        {
            cursorPositions.Enqueue(center + radius * Mathf.Cos(angle) * Vector2.right + radius * Mathf.Sin(angle) * Vector2.up);
            angle += angleInc;
        }
        screenPos = new Vector3(center.x + radius, center.y, 0.0f);
        screenPosSmoothed = screenPos;
    }

    void LateUpdate()
    {
        if( cursorLine == null )
        {
            PlayerSync playerSync = (GameObject.Instantiate(PersistentGameManager.Inst.playerSyncObj, Vector3.zero, Quaternion.identity) as GameObject).GetComponent<PlayerSync>();
            cursorLine = playerSync.cursorLine;
            cursorObject = playerSync.cursorObject;
        }
            

        screenPosSmoothed = Vector3.SmoothDamp(screenPosSmoothed, screenPos, ref screenPosVel, screenPosSmoothTime);

        bool isDrawing = cursorPositions.Count > 0;
        if( isDrawing )
        {
            Vector2 thisPos = cursorPositions.Dequeue();
            Debug.LogError(thisPos);
            screenPos = new Vector3(thisPos.x, thisPos.y);

            screenPos.x = Mathf.Clamp(screenPos.x, 0f, Screen.width);
            screenPos.y = Mathf.Clamp(screenPos.y, 0f, Screen.height);

            Ray cursorRay = Camera.main.ScreenPointToRay(new Vector3(screenPosSmoothed.x, Screen.height - screenPosSmoothed.y, 0f));
            cursorObject.position = cursorRay.origin + cursorRay.direction * 1f;
        }

        if (isDrawing && !selecting)
            selecting = true;

        if (!isDrawing && selecting)
        {
            selecting = false;
            Destroy(cursorObject.gameObject);
            Destroy(this);
            return;
        }


        // Collect points for gestural control.
        if (selecting)
            if ((lastPoints.Count < 2) || (Vector3.Distance(screenPosSmoothed, lastPoints[lastPoints.Count - 1]) > 10f))
                lastPoints.Add(screenPosSmoothed);

        if (lastPoints.Count > 2)
        {
            cursorLine.SetVertexCount(lastPoints.Count + 1);
            for (int i = 0; i < lastPoints.Count; i++)
            {
                Ray pointRay = Camera.main.ScreenPointToRay(new Vector3(lastPoints[i].x, Screen.height - lastPoints[i].y, 0f));
                cursorLine.SetPosition(i, pointRay.origin + pointRay.direction * 1f);
            }
            cursorLine.SetPosition(lastPoints.Count, cursorObject.position);

            cursorLine.enabled = true;
        }
        else
            cursorLine.enabled = false;

    } // End of LateUpdate().
}
