using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TeleportOnSelect : MonoBehaviour
{
    public Transform target;
    public Transform xrOrigin;

    UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        interactable.selectEntered.AddListener(OnSelect);
    }

    void OnDestroy()
    {
        if (interactable != null)
            interactable.selectEntered.RemoveListener(OnSelect);
    }

    void OnSelect(SelectEnterEventArgs args)
    {
        if (!target || !xrOrigin) return;

        xrOrigin.position = target.position;
        xrOrigin.rotation = Quaternion.Euler(0f, target.eulerAngles.y, 0f);
    }
}