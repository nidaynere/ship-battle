using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TurnBasedFW;

public class Player : MonoBehaviour
{
    public Playable Playable;

    public Image Filler;

    private Transform meshHolder;

    public void Set(Playable _playable, GameObject _meshHolder, GameObject _healthUI)
    {
        Playable = _playable;
        Filler = _healthUI.transform.Find("filler").GetComponent<Image>();
        meshHolder = _meshHolder.transform;
    }

    public void ApplyPath(float worldScale, Vector3[] path, float moveSpeed, System.Action OnPathCompleted)
    {
        StartCoroutine(StartPath(worldScale, path, moveSpeed, OnPathCompleted));
    }

    IEnumerator StartPath(float worldScale, Vector3[] path, float moveSpeed, System.Action OnPathCompleted)
    {
        // multiply by worldScale.
        int length = path.Length;
        for (int i = 0; i < length; i++)
        {
            path[i] *= worldScale;

            //GameObject.CreatePrimitive(PrimitiveType.Cube).transform.position = path[i];
        }

        int targetPoint = -1;
        float progress;
        Vector3 startPos;
        float speed = 1f;

        bool NextPoint ()
        {
            targetPoint++;
            bool result = targetPoint >= path.Length || path[targetPoint] == transform.position;

            startPos = transform.position;

            if (!result)
            {
                 speed = Vector3.Distance(transform.position, path[targetPoint]) + 1;
                 meshHolder.rotation = Quaternion.LookRotation(path[targetPoint] - transform.position);
            }

            progress = 0;

            return result;
        }

        NextPoint();
        
        while (true)
        {
            yield return new WaitForEndOfFrame();

            progress = Mathf.Min(1, progress + (Time.deltaTime / speed) * moveSpeed);

            transform.position = Vector3.Lerp(startPos, path[targetPoint], progress);

            if (progress == 1)
            {
                bool isDone = NextPoint();
                if (isDone)
                    break;
            }
        }

        OnPathCompleted?.Invoke();
    }
}
