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
    private Block upgrade;
    [HideInInspector] public Symbol symbol;
    public Block left;
    public Block right;

    [Header("Children")]
    public GameObject cdText;
    public GameObject typeTxt;
    public GameObject nameTxt;
    public GameObject highlight;

    [Header("Spell Effects")]
    public string type;
    public string tag;
    [SerializeField] private List<string> blockedTags;
    public float cd;
    public int rarity;

    [Header("Saves")]
    private Vector3 posSAVE;
    private Vector3 symbolPosSAVE;
    private Block leftSAVE;
    private Block rightSAVE;


    private void Awake()
    {
        /*Collider2D[] allBlocks = Physics2D.OverlapBoxAll(Vector2.zero, new Vector2(9999, 9999), 0, LayerMask.GetMask("Block"));
        foreach (Collider2D c in allBlocks)
        {
            blocks.Add(c.GetComponent<Block>());
        }*/
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        symbol = transform.GetChild(0).GetComponent<Symbol>();
        symbol.GetComponent<Image>().enabled = false;
        
        string[] modTags = new string[]{"passive"};
        if (!Array.Exists(modTags, t => t == tag))
        {
            //if (GameManager.Instance.doubleSpeed)
            //    cd = cd*0.5f;
            string cdTxt = ((cd+"").Length == 1) ? cd + ".0s" : cd + "s";
            cdText.GetComponent<TMPro.TextMeshProUGUI>().text = cdTxt;
        }
        //TODO: add type text here (instead of 1 by 1)
    }

    private void Update()
    {
        if (dragging)
        {
            foreach (Transform child in transform.parent)
            {
                Block bl = child.GetComponent<Block>();
                bl.leftSpace.SetActive(false);
                bl.rightSpace.SetActive(false);
                if (bl.left == null && bl.right == null)
                    bl.highlight.SetActive(false);
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

            upgrade = null;
            Collider2D[] tightHits = Physics2D.OverlapBoxAll(b.center, b.extents*1.5f, 0, LayerMask.GetMask("Block"));
            foreach (Collider2D c in tightHits)
            {
                if (c.name == name && c.gameObject != gameObject)
                {
                    Block bl = c.GetComponent<Block>();
                    bl.highlight.SetActive(true);
                    bl.leftSpace.SetActive(false);
                    bl.rightSpace.SetActive(false);
                    upgrade = bl;
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
            if (upgrade != null)
            {
                upgrade.highlight.SetActive(false);
                upgrade.cd -= 1f;
                string cdTxt = ((upgrade.cd+"").Length == 1) ? upgrade.cd + ".0s" : upgrade.cd + "s";
                upgrade.cdText.GetComponent<TMPro.TextMeshProUGUI>().text = cdTxt;
                Destroy(gameObject);
            }
            else if (targetSpace != null)
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
                SpellManager.Instance.compileButton.GetComponent<Button>().interactable = true;
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
        typeTxt.GetComponent<CanvasGroup>().alpha = 1;
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