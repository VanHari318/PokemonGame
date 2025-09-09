using Unity.Burst;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using static UnityEngine.UI.Button;
using UnityEngine.Serialization;

public class ButtonControl : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public UnityEvent onHoverEnter;
    public UnityEvent onHoverExit;
    public UnityEvent onHold;
    public UnityEvent onRelease;
    public void OnPointerEnter(PointerEventData eventData)
    {

        onHoverEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {

        onHoverExit?.Invoke();
    }

    public void OnPointerDown(PointerEventData eventData)
    {

        onHold?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {

        onRelease?.Invoke();
    }
}
