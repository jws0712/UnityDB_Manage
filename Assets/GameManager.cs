using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.UI;

[System.Serializable]
public class Item
{
    public Item(string _Type, string _Name, string _Explain, string _Number, bool _isUsing)
    { Type = _Type; Name = _Name; Explain = _Explain; Number = _Number; isUsing = _isUsing; }

    public string Type, Name, Explain, Number;
    public bool isUsing;

}


public class GameManager : MonoBehaviour
{
    public TextAsset ItemDatabase;
    public List<Item> AllItemList, MyItemList, CurItemList;
    public string curType = "Character";
    public GameObject[] Slot, UsingImage;
    public Image[] TabImage, ItemImage;
    public Sprite TabIdleSprite, TabSelectSprite;
    public Sprite[] ItemSprite;
    public GameObject ExpainPanel;
    public RectTransform CanvasRect;
    public InputField ItemNameInput, ItemNumberInput;
    IEnumerator PointerCoroutine;
    RectTransform ExplainRect;
    void Start()
    {
        string[] line = ItemDatabase.text.Substring(0, ItemDatabase.text.Length - 1).Split('\n');
        for(int i = 0; i < line.Length; i++)
        {
            string[] row = line[i].Split('\t');

            AllItemList.Add(new Item(row[0], row[1], row[2], row[3], row[4] == "TRUE"));
        }

        Load();

        ExplainRect = ExpainPanel.GetComponent<RectTransform>();
    }

    void Update()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(CanvasRect, Input.mousePosition, Camera.main, out Vector2 anchoredPos);
        ExpainPanel.GetComponent<RectTransform>().anchoredPosition = anchoredPos + new Vector2(-180f, -165f);
    }

    public void GetItemClick()
    {
        Item curItem = MyItemList.Find(x => x.Name == ItemNameInput.text);
        if(curItem != null)
        {
            curItem.Number = (int.Parse(curItem.Number) + int.Parse(ItemNumberInput.text)).ToString();
        }
        else
        {
            Item curAllItem = AllItemList.Find(x => x.Name == ItemNameInput.text);
            if(curAllItem != null)
            {
                curAllItem.Number = ItemNumberInput.text;
                MyItemList.Add(curAllItem);
            }
        }
        Save();
    }

    public void RemoveItemClick()
    {
        Item curItem = MyItemList.Find(x => x.Name == ItemNameInput.text);
        if(curItem != null )
        {
            int curNumber = int.Parse(curItem.Number) - int.Parse(ItemNumberInput.text);

            if(curNumber <= 0)
            {
                MyItemList.Remove(curItem);
            }
            else
            {
                curItem.Number = curNumber.ToString();
            }
            Save();
        }
    }

    public void SlotClick(int slotNum)
    {
        Item CurItem = CurItemList[slotNum];
        Item UsingItem = CurItemList.Find(x => x.isUsing == true);

        if(curType == "Character")
        {
            if(UsingItem != null) UsingItem.isUsing = false;
            CurItem.isUsing = true;
        }
        else
        {
            CurItem.isUsing = !CurItem.isUsing;
            if (UsingItem != null) UsingItem.isUsing = false; 
        }

        Save();
    }

    public void TabClick(string tabName)
    {
        curType = tabName;
        CurItemList = MyItemList.FindAll(x => x.Type == tabName);

        for(int i = 0;i < Slot.Length;i++)
        {
            bool isExist = i < CurItemList.Count;

            Slot[i].SetActive(isExist);
            Slot[i].GetComponentInChildren<Text>().text = isExist ? CurItemList[i].Name : " " ;

            if (isExist)
            {
                ItemImage[i].sprite = ItemSprite[AllItemList.FindIndex(x => x.Name == CurItemList[i].Name)];
                UsingImage[i].SetActive(CurItemList[i].isUsing);
            }
        }

        int tabNum = 0;
        switch (tabName)
        {
            case "Character": tabNum = 0; break;
            case "Balloon": tabNum = 1; break;
        }
        for(int i = 0; i < TabImage.Length ; i++)
        {
            TabImage[i].sprite = i == tabNum ? TabSelectSprite : TabIdleSprite;
        }
    }

    public void PointerEenter(int slotNum)
    {
        PointerCoroutine = PointerEnterDelay(slotNum);
        StartCoroutine(PointerCoroutine);

        ExpainPanel.GetComponentInChildren<Text>().text = CurItemList[slotNum].Name;
        ExpainPanel.transform.GetChild(2).GetComponent<Image>().sprite = Slot[slotNum].transform.GetChild(1).GetComponent<Image>().sprite;
        ExpainPanel.transform.GetChild(3).GetComponent<Text>().text = CurItemList[slotNum].Number + "°³";
        ExpainPanel.transform.GetChild(4).GetComponent<Text>().text = CurItemList[slotNum].Explain;
    }

    IEnumerator PointerEnterDelay(int slotNum)
    {
        yield return new WaitForSeconds(0.5f);
        ExpainPanel.gameObject.SetActive(true);

    }

    public void PointerExit(int slotNum)
    {
        StopCoroutine(PointerCoroutine);
        ExpainPanel.gameObject.SetActive(false);
    }

    void Save()
    {
        string jdata = JsonConvert.SerializeObject(MyItemList);
        File.WriteAllText(Application.dataPath + "/Resources/MyItemText.txt", jdata);

        TabClick(curType);
    }

    void Load()
    {
        string jdata = File.ReadAllText(Application.dataPath + "/Resources/MyItemText.txt");
        MyItemList = JsonConvert.DeserializeObject<List<Item>>(jdata);

        TabClick(curType);
    }
}
