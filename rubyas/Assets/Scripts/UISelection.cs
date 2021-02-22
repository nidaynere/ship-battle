using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UISelection : MonoBehaviour
{
    [System.Serializable]
    public class IntEvent : UnityEvent<int> { }

    public IntEvent OnValueChanged;

    [SerializeField] private bool initializeAtStart = true;

    int currentSelection = -1;
    // Start is called before the first frame update
    void Start()
    {
        if (initializeAtStart)
            Initialize(0);
    }

    public void Initialize (int Index)
    {
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            int index = i;
            transform.GetChild(i).GetComponentInChildren<Button>().onClick.AddListener(() => {
                Select(index);
            });
        }

        Select(Index); // Select default
    }

    private void Select(int i)
    {
        if (currentSelection != -1)
        {
            // Revert.
            var lastSelection = transform.GetChild(currentSelection);
            if (lastSelection != null)
            {
                lastSelection.Find("Active").gameObject.SetActive(false);
            }
        }

        currentSelection = i;

        var targetSelection = transform.GetChild(currentSelection);
        if (targetSelection != null)
        {
            targetSelection.Find("Active").gameObject.SetActive(true);
        }

        OnValueChanged.Invoke(currentSelection);
    }
}
