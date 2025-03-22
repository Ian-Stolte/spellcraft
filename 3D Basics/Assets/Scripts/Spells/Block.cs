using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Block : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    private Vector2 lastPos; // Stores the offset between the mouse click and the window position
    [HideInInspector] public RectTransform rectTransform;
    private Canvas canvas;
    private bool dragging;

    private List<Block> blocks = new List<Block>();
    public GameObject leftSpace;
    public GameObject rightSpace;

    private GameObject targetSpace;
    [HideInInspector] public Symbol symbol;
    public Block left;
    public Block right;

    [Header("Spell Effects")]
    public string tag;
    [SerializeField] private List<string> blockedTags;
    public GameObject hitbox;
    public float cd;


    private void Awake()
    {
        Collider2D[] allBlocks = Physics2D.OverlapBoxAll(Vector2.zero, new Vector2(9999, 9999), 0, LayerMask.GetMask("Block"));
        foreach (Collider2D c in allBlocks)
        {
            blocks.Add(c.GetComponent<Block>());
        }
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        symbol = transform.GetChild(0).GetComponent<Symbol>();
        string[] modTags = new string[]{"passive"};
        if (!Array.Exists(modTags, t => t == tag))
        {
            string cdTxt = ((cd+"").Length == 1) ? cd + ".0s" : cd + "s";
            transform.GetChild(4).GetComponent<TMPro.TextMeshProUGUI>().text = cdTxt;
        }
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
            foreach(Collider2D c in hits)
            {
                Block script = c.GetComponent<Block>();
                if (c.gameObject != gameObject && script != null)
                {
                    //TODO: prevent adding multiple shapes
                    if (rectTransform.anchoredPosition.x > script.rectTransform.anchoredPosition.x && script.right == null && script.ValidTag(tag, name, false))
                    {
                        targetSpace = script.rightSpace;
                        targetSpace.SetActive(true);
                        break;
                    }
                    else if (rectTransform.anchoredPosition.x < script.rectTransform.anchoredPosition.x && script.left == null && script.ValidTag(tag, name, true))
                    {
                        targetSpace = script.leftSpace;
                        targetSpace.SetActive(true);
                        break;
                    }
                }
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!SpellManager.Instance.spellsLocked)
        {
            dragging = true;
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
        if (!SpellManager.Instance.spellsLocked)
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
            rectTransform.anchoredPosition = localMousePos - lastPos;
            
            // Bound the window to the border of the UI
            float newX = Mathf.Clamp(rectTransform.anchoredPosition.x, -(860 - rectTransform.sizeDelta.x), 860 - rectTransform.sizeDelta.x);
            float newY = Mathf.Clamp(rectTransform.anchoredPosition.y, -415, 375);
            rectTransform.anchoredPosition = new Vector2(newX, newY);

            targetSpace = null;
            ResetSymbol(false);
            ResetSymbol(true);
            if (left != null)
            {
                left.right = null;
                left = null;
            }
            if (right != null)
            {
                right.left = null;
                right = null;
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!SpellManager.Instance.spellsLocked)
        {
            dragging = false;
            if (targetSpace != null)
            {
                Vector3 offset = new Vector3(2 + (rectTransform.sizeDelta.x - 100)/2, 0, 0);
                if (targetSpace.name == "Left Space")
                {
                    rectTransform.position = targetSpace.GetComponent<RectTransform>().position - offset;
                    right = targetSpace.transform.parent.GetComponent<Block>();
                    right.left = this;
                }
                else
                {
                    rectTransform.position = targetSpace.GetComponent<RectTransform>().position + offset;
                    left = targetSpace.transform.parent.GetComponent<Block>();
                    left.right = this;
                }
                targetSpace.SetActive(false);
            }
        }
    }


    public void ResetSymbol(bool toRight)
    {
        transform.GetChild(0).GetComponent<Symbol>().ResetPos();
        if (toRight && right != null)
        {
            right.ResetSymbol(true);
        }
        else if (!toRight && left != null)
        {
            left.ResetSymbol(false);
        }
    }


    public bool ValidTag(string tag, string name, bool toRight)
    {
        if (blockedTags.Contains(tag) || blockedTags.Contains(name))
            return false;
        else if (toRight)
        {
            if (right == null)
                return true;
            else
                return right.ValidTag(tag, right.name, true);
        }
        else
        {
            if (left == null)
                return true;
            else
                return left.ValidTag(tag, left.name, false);
        }
    }
}