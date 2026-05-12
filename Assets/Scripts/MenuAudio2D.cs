using UnityEngine;
using UnityEngine.EventSystems;

public class MenuAudio2D : MonoBehaviour
{
    private GameObject lastSelectedObject;
    private bool hasInitializedSelection;

    private void Start()
    {
        lastSelectedObject = EventSystem.current != null
            ? EventSystem.current.currentSelectedGameObject
            : null;

        hasInitializedSelection = true;
    }

    private void Update()
    {
        if (EventSystem.current == null)
        {
            return;
        }

        GameObject currentSelectedObject = EventSystem.current.currentSelectedGameObject;

        if (!hasInitializedSelection)
        {
            lastSelectedObject = currentSelectedObject;
            hasInitializedSelection = true;
            return;
        }

        if (currentSelectedObject == null)
        {
            return;
        }

        if (lastSelectedObject == null)
        {
            lastSelectedObject = currentSelectedObject;
            return;
        }

        if (currentSelectedObject != lastSelectedObject)
        {
            AudioManager2D.Instance?.PlayMenuMove();
            lastSelectedObject = currentSelectedObject;
        }
    }

    public void PlayMenuSelect()
    {
        AudioManager2D.Instance?.PlayMenuSelect();
    }

    public void PlayMenuBack()
    {
        AudioManager2D.Instance?.PlayMenuBack();
    }

    public void PlayPause()
    {
        AudioManager2D.Instance?.PlayPause();
    }

    public void PlayGameStart()
    {
        AudioManager2D.Instance?.PlayGameStart();
    }

    public void PlayPressStart()
    {
        AudioManager2D.Instance?.PlayPressStart();
    }
}