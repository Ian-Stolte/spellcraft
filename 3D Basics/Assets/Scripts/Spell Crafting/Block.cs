using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Block : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    private Vector2 lastPos;
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

    [Header("Children")]
    public GameObject cdText;
    public GameObject latin;
    public GameObject nameTxt;
    public GameObject highlight;

    [Header("Spell Effects")]
    public string tag;
    [SerializeField] private List<string> blockedTags;
    public GameObject hitbox;
    public float cd;
    public int rarity;

    [Header("Saves")]
    private Vector3 posSAVE;
    private Vector3 symbolPosSAVE;
    private Block leftSAVE;
    private Block rightSAVE;


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
        symbol.GetComponent<Image>().enabled = false;
        string[] modTags = new string[]{"passive"};
        if (!Array.Exists(modTags, t => t == tag))
        {
            string cdTxt = ((cd+"").Length == 1) ? cd + ".0s" : cd + "s";
            cdText.GetComponent<TMPro.TextMeshProUGUI>().text = cdTxt;
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
                    if (rectTransform.anchoredPosition.x > script.rectTransform.anchoredPosition.x && script.right == null && script.ValidTag(this, false))
                    {
                        targetSpace = script.rightSpace;
                        targetSpace.SetActive(true);
                        break;
                    }
                    else if (rectTransform.anchoredPosition.x < script.rectTransform.anchoredPosition.x && script.left == null && script.ValidTag(this, true))
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
            
            // Bound to the border of the UI
            float newX = Mathf.Clamp(rectTransform.anchoredPosition.x, -(860 - rectTransform.sizeDelta.x), 850 - rectTransform.sizeDelta.x);
            float newY = Mathf.Clamp(rectTransform.anchoredPosition.y, -415, 415);
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
        Color c = GetComponent<Image>().color;
        GetComponent<Image>().color = new Color(c.r, c.g, c.b, 1);
        symbol.ResetPos();
        highlight.SetActive(false); // highlight
        nameTxt.GetComponent<CanvasGroup>().alpha = 1;
        latin.GetComponent<CanvasGroup>().alpha = 1;
        cdText.SetActive(true);

        if (toRight && right != null)
        {
            right.ResetSymbol(true);
        }
        else if (!toRight && left != null)
        {
            left.ResetSymbol(false);
        }
    }


    public bool ValidTag(Block script, bool toRight)
    {
        if (blockedTags.Contains(script.tag) || script.blockedTags.Contains(tag))
        {
            return false;
        }
        else if (toRight)
        {
            if (right == null)
                return true;
            else
                return right.ValidTag(script, true);
        }
        else
        {
            if (left == null)
                return true;
            else
                return left.ValidTag(script, false);
        }
    }


    public void SaveState()
    {
        posSAVE = GetComponent<RectTransform>().anchoredPosition;
        symbolPosSAVE = symbol.GetComponent<RectTransform>().anchoredPosition;
        leftSAVE = left;
        rightSAVE = right;
    }

    public void ReadState()
    {
        GetComponent<RectTransform>().anchoredPosition = posSAVE;
        symbol.transform.GetComponent<RectTransform>().anchoredPosition = symbolPosSAVE;
        left = leftSAVE;
        right = rightSAVE;
    }
}