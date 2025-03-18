using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Symbol : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    private Vector2 lastPos;
    private Vector2 startingPos;
    [HideInInspector] public RectTransform rectTransform;
    private Canvas canvas;
    //private bool dragging;

    public bool canMove;
    public Vector2 min;
    public Vector2 max;


    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startingPos = rectTransform.anchoredPosition;
        canvas = GetComponentInParent<Canvas>();
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (SpellManager.Instance.spellsLocked && canMove)
        {
            //dragging = true;
            // Convert the mouse position to local space relative to the RectTransform
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                eventData.position,
                canvas.worldCamera,
                out lastPos
            );
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (SpellManager.Instance.spellsLocked && canMove)
        {
            // Bring the dragged window to the front
            transform.parent.SetSiblingIndex(transform.parent.childCount - 1);

            Vector2 localMousePos;
            // Convert the mouse position to local space relative to the canvas
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.worldCamera,
                out localMousePos
            );
            rectTransform.anchoredPosition = localMousePos - lastPos - transform.parent.GetComponent<RectTransform>().anchoredPosition;
            
            // Bound the window to the border of the UI
            float newX = Mathf.Clamp(rectTransform.anchoredPosition.x, min.x, max.x);
            float newY = Mathf.Clamp(rectTransform.anchoredPosition.y, min.y, max.y);
            rectTransform.anchoredPosition = new Vector2(newX, newY);
        }
    }

    public void ResetPos()
    {
        rectTransform.anchoredPosition = startingPos;
    }
}