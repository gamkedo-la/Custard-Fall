using System.Collections;
using UnityEngine;

public class ConditionalEnablee : MonoBehaviour
{
    private Player _player;

    private void Awake()
    {
        _player = FindObjectOfType<Player>();
        Debug.Log($"Didn't find player {!_player}");
    }

    private void Start()
    {
        ShowChildren(_player.ownsGrapplingHook);
        if (!_player.ownsGrapplingHook)
        {
            //XXX stupid but works, really need to decouple and refactor into event based
            StartCoroutine(WaitTillPlayerHasGrapplingHook());
        }
    }

    private IEnumerator WaitTillPlayerHasGrapplingHook()
    {
        while (!_player.ownsGrapplingHook)
        {
            yield return new WaitForSeconds(1);
        }

        ShowChildren(_player.ownsGrapplingHook);
    }

    private void ShowChildren(bool show)
    {
        var transform = gameObject.transform;
        var childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(show);
        }
    }
}