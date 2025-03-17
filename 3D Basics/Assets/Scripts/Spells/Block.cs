using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Block : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    private Vector2 startPoint; // Stores the offset between the mouse click and the window position
    [HideInInspector] public RectTransform rectTransform;
    private Canvas canvas;
    private bool dragging;

    private List<Block> blocks = new List<Block>();
    public GameObject leftSpace;
    public GameObject rightSpace;


    private void Start()
    {
        Collider2D[] allBlocks = Physics2D.OverlapBoxAll(Vector2.zero, new Vector2(9999, 9999), 0, LayerMask.GetMask("Block"));
        foreach (Collider2D c in allBlocks)
        {
            blocks.Add(c.GetComponent<Block>());
        }
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        if (dragging)
        {
            foreach (Block bl in blocks)
            {
                bl.leftSpace.SetActive(false);
                bl.rightSpace.SetActive(false);
            }

            Bounds b = GetComponent<BoxCollider2D>().bounds;
            Collider2D[] hits = Physics2D.OverlapBoxAll(b.center, b.extents*3, 0, LayerMask.GetMask("Block"));
            //swap to just one? (make sure we ignore self)
            //could then store in a var & just snap to that one when we call PointerUp
            foreach(Collider2D c in hits)
            {
                Block script = c.GetComponent<Block>();
                if (c.gameObject != gameObject && script != null)
                {
                    if (rectTransform.anchoredPosition.x > script.rectTransform.anchoredPosition.x)
                    {
                        Debug.Log(c.name + ": RIGHT");
                        script.rightSpace.SetActive(true);
                    }
                    else
                    {
                        Debug.Log(c.name + ": LEFT");
                        script.leftSpace.SetActive(true);
                    }
                }
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        dragging = true;
        // Convert the mouse position to local space relative to the RectTransform
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            canvas.worldCamera,
            out startPoint
        );
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Bring the dragged window to the front
        transform.SetSiblingIndex(transform.parent.childCount - 1);

        Vector2 localMousePos;
        // Convert the mouse position to local space relative to the canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out localMousePos
        );

        // Move the window while maintaining the initial offset from the cursor
        rectTransform.anchoredPosition = localMousePos - startPoint;
        
        // Bound the window to the border of the UI
        float newX = Mathf.Clamp(rectTransform.anchoredPosition.x, -(860 - rectTransform.sizeDelta.x), 860 - rectTransform.sizeDelta.x);
        float newY = Mathf.Clamp(rectTransform.anchoredPosition.y, -415, 375);
        rectTransform.anchoredPosition = new Vector2(newX, newY);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        dragging = false;
        Bounds b = GetComponent<BoxCollider2D>().bounds;
        Collider2D[] hits = Physics2D.OverlapBoxAll(b.center, b.extents*2, 0, LayerMask.GetMask("Block"));
        foreach(Collider2D c in hits)
        {
            if (c.gameObject != gameObject)
                Debug.Log("LOCK TO: " + c.name);
        }
    }
}