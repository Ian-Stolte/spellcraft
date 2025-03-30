using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private LayerMask interactable;
    [SerializeField] private float interactDist;
    private List<GameObject> objs = new List<GameObject>();
    [SerializeField] private GameObject[] textObjs;

    public List<Item> inventory = new List<Item>();
    [SerializeField] private Transform itemPopups;

    [Header("Planting")]
    public bool planting;
    [SerializeField] private GameObject plantingCircle;
    [SerializeField] private GameObject plantUI;
    [SerializeField] private Material validMat;
    [SerializeField] private Material invalidMat;
    [SerializeField] private Transform treeParent;
    [SerializeField] private GameObject treePrefab;


    void Update()
    {
        objs.Clear();
        foreach (GameObject g in textObjs)
            g.SetActive(false);

        if (GetComponent<PlayerMovement>().grounded)
        {
            Collider[] cols = Physics.OverlapSphere(transform.position, interactDist, interactable);
            foreach (Collider c in cols)
            {
                Vector3 dir = c.transform.position - transform.position;
                Interactable script = c.GetComponent<Interactable>();
                if (script != null)
                {
                    if (Vector3.Angle(dir, transform.forward) < 80 && script.active)
                    {
                        objs.Add(c.gameObject);
                        ShowText(script.text);
                    }
                }
            }

            //Planting
            if (Input.GetKeyDown(KeyCode.P))
            {
                planting = !planting;
                plantingCircle.SetActive(planting);
                plantUI.SetActive(planting);
            }

            if (planting)
            {
                Item p = inventory.Find(i => i.name == "pinecones");
                plantUI.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = "" + p.quantity;
                plantingCircle.transform.position = transform.position + 3*transform.forward - new Vector3(0, 0.3f, 0);
                bool validPlant = p.quantity > 0;
                Collider[] overlaps = Physics.OverlapSphere(plantingCircle.transform.position, 3);
                foreach (Collider c in overlaps)
                {
                    if (c.transform.parent == treeParent)
                        validPlant = false;
                }

                plantingCircle.GetComponent<MeshRenderer>().material = validPlant ? validMat : invalidMat;
                plantUI.transform.GetChild(0).GetComponent<Image>().color = validPlant ? new Color32(185, 213, 182, 99) : new Color32(220, 133, 131, 99);

                if (Input.GetKeyDown(KeyCode.Space) && validPlant)
                {
                    GameObject tree = Instantiate(treePrefab, plantingCircle.transform.position + new Vector3(0, 0.4f, 0), Quaternion.identity, treeParent);
                    tree.GetComponent<Tree>().text = textObjs[1];
                    tree.GetComponent<Tree>().takeText = textObjs[0];
                    p.quantity--;
                }
            }
        }
        
        foreach (GameObject o in objs)
        {
            Interactable script = o.GetComponent<Interactable>();
            if (Input.GetKeyDown(script.keybind))
                script.Interact(this);
        }
    }


    private void ShowText(GameObject text)
    {
        text.SetActive(true);
        //logic to place them vertically to avoid overlap
    }


    public void AddItem(string name, int q)
    {
        Item item = inventory.Find(i => i.name == name);
        if (item == null)
        {
            Debug.Log("Item with name '" + name + "' not found!");
        }
        else
        {
            item.quantity += q;
            
            float lastY = 480;
            if (itemPopups.childCount > 0)
                lastY = itemPopups.GetChild(itemPopups.childCount-1).GetComponent<RectTransform>().anchoredPosition.y - 130;
            GameObject popup = Instantiate(item.popup, Vector3.zero, Quaternion.identity, itemPopups);
            popup.GetComponent<RectTransform>().anchoredPosition = new Vector3(935, lastY, 0);
            popup.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "+" + q;
            StartCoroutine(FadeItemPopup(popup));
        }
    }

    private IEnumerator FadeItemPopup(GameObject popup)
    {
        yield return new WaitForSeconds(1f);
        for (float i = 1; i > 0; i -= 0.01f)
        {
            popup.GetComponent<CanvasGroup>().alpha = i;
            yield return new WaitForSeconds(0.01f);
        }
        Transform parent = popup.transform.parent;
        Destroy(popup);
        for (float i = 0; i < 0.5f; i += 0.01f)
        {
            foreach (Transform child in parent)
            {
                child.GetComponent<RectTransform>().anchoredPosition += new Vector2(0, 130/50f);
            }
            yield return new WaitForSeconds(0.01f);
        }
    }
}


[System.Serializable]
public class Item
{
    public string name;
    public int quantity;

    public GameObject popup;
}