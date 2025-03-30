using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Symbol : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    private Vector2 lastPos;
    private Vector2 startingPos;
    [HideInInspector] public RectTransform rectTransform;
    private RectTransform parentRect;
    private Canvas canvas;

    public bool canMove;
    public Vector2 min;
    public Vector2 max;

    public int adjSymbols;


    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        parentRect = transform.parent as RectTransform;
        startingPos = rectTransform.anchoredPosition;
        canvas = GetComponentInParent<Canvas>();
    }


    private void Update()
    {
        if (SpellManager.Instance.spellsLocked && canMove)
        {
            Bounds b = GetComponent<BoxCollider2D>().bounds;
            adjSymbols = Physics2D.OverlapBoxAll(b.center, b.extents, 0, LayerMask.GetMask("Symbol")).Length;
        }
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (SpellManager.Instance.spellsLocked && canMove)
        {
            // Convert the mouse position to local space relative to the RectTransform
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                canvas.worldCamera,
                out lastPos
            );
            lastPos -= rectTransform.anchoredPosition;
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
                parentRect,
                eventData.position,
                canvas.worldCamera,
                out localMousePos
            );
            rectTransform.anchoredPosition = localMousePos - lastPos;
            
            // Bound the window to the border of the UI
            float newX = Mathf.Clamp(rectTransform.anchoredPosition.x, min.x, max.x);
            float newY = Mathf.Clamp(rectTransform.anchoredPosition.y, min.y, max.y);
            rectTransform.anchoredPosition = new Vector2(newX, newY);
        }
    }

    public void ResetPos()
    {
        rectTransform.anchoredPosition = startingPos;
        GetComponent<Image>().enabled = false;
    }
}