using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class SkillCheckMInigameLogic : NetworkBehaviour
{
    public static float skillCheckPoints = -500;
    public static Action OnSucceedSkillCheckMiniGame;

    public int pointsGainMultiplayer = 1;
    public float min = 0f;
    public float max = 5f;
    public float speed = 1.5f;
    public GameObject skillCheckIndicator, greenTarget, goldTarget, progressBarrIndicator;

    private float _t = 0f;
    private bool _goingUp = true;

    private void OnDisable()
    {
        OnSucceedSkillCheckMiniGame = null;
        StopAllCoroutines();
    }

    public void StartSkillCheckMiniGame()
    {
        skillCheckPoints = -500;
        gameObject.SetActive(true);
        StartCoroutine(SkillCheckCoroutine());
    }

    private IEnumerator SkillCheckCoroutine()
    {
        RectTransform skillRect = skillCheckIndicator.GetComponent<RectTransform>();
        RectTransform progressRect = progressBarrIndicator.GetComponent<RectTransform>();

        while (true)
        {
            // Lerp logic
            if (_goingUp)
            {
                _t += Time.deltaTime * speed;
                if (_t >= 1f) _goingUp = false;
            }
            else
            {
                _t -= Time.deltaTime * speed;
                if (_t <= 0f) _goingUp = true;
            }

            float value = Mathf.Lerp(min, max, _t);
            skillRect.anchoredPosition = new Vector2(value, 0);

            // Input check
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (RectOverlaps(skillRect, greenTarget.GetComponent<RectTransform>()))
                {
                    if (RectOverlaps(skillRect, goldTarget.GetComponent<RectTransform>()))
                    {
                        // Gold skill check
                        skillCheckPoints += 15 * pointsGainMultiplayer;
                        PlaceTargetOnRandomPosition();
                        print(skillCheckPoints);
                    }
                    else
                    {
                        // Green skill check
                        skillCheckPoints += 5 * pointsGainMultiplayer;
                        PlaceTargetOnRandomPosition();
                        print(skillCheckPoints);
                    }
                }
                else
                {
                    // Miss
                    skillCheckPoints -= 5 * pointsGainMultiplayer;
                    print(skillCheckPoints);
                }
            }

            Vector2 offsetMax = progressRect.offsetMax;
            offsetMax.x = skillCheckPoints;
            progressRect.offsetMax = offsetMax;

            // Skill check success
            if (skillCheckPoints >= 0)
            {
                OnSucceedSkillCheckMiniGame?.Invoke();
                gameObject.SetActive(false);
                yield break; 
            }

            yield return null; 
        }
    }

    private void PlaceTargetOnRandomPosition()
    {
        float positionOnX = Mathf.Lerp(-200, 200, Random.Range(0f, 1f));
        greenTarget.GetComponent<RectTransform>().anchoredPosition = new Vector2(positionOnX, 0);
    }
    
    bool RectOverlaps(RectTransform a, RectTransform b)
    {
        Rect rectA = GetWorldRect(a);
        Rect rectB = GetWorldRect(b);

        return rectA.Overlaps(rectB);
    }

    Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        // bottom-left corner
        Vector3 bl = corners[0];
        // top-right corner
        Vector3 tr = corners[2];

        return new Rect(bl.x, bl.y, tr.x - bl.x, tr.y - bl.y);
    }
}
