using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.UI;
using Unity.VisualScripting;

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
    public TextAsset ItemDatabase; //�������� �ۼ��� �����͸� txt�� ��ȯ�� ���� ����

    public List<Item> AllItemList, MyItemList, CurItemList; //AllItemList : ���� ���� �ִ� ��� ������ ����Ʈ
                                                            //MyItemLIst : ���� ������ �ִ� ������
                                                            //CurItemLIst : ���� Ÿ���� ������
    public string curType = "Character"; //�κ��丮â�� �ش�Ǵ� Ÿ��

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
        //Character	Pig	����	1	FALSE
        //Character Creeper ũ���� 1   FALSE
        //Character   Bee ��   1   FALSE
        //Balloon RedBalloon ����ǳ��    1   FALSE
        //Balloon GreenBalloon �ʷ�ǳ��    1   FALSE
        //Balloon BlueBalloon �Ķ�ǳ��    1   FALSE
        //��ü�� �������� �ٹٲ��� �������� ������ �������� line�� �־��ش�
        for (int i = 0; i < line.Length; i++)
        {
            string[] row = line[i].Split('\t');
            //Character	Pig	����	1	FALSE �� �������� �� �������� ������ �������� row�� �־��ش�
            //AllItemList.Add(new Item(row[0], row[1], row[2], row[3], row[4] == "TRUE"));
            AllItemList.Add(new Item(row[0], row[1], row[2], row[3], row[4] == "TRUE"));
            // # new Item���� ���� ������ �ż���� �ν��ϰ� ������ new�� �־��־�� �Ѵ�
            //Character	Pig	����	1	FALSE
            //row[0 ~ 3] ���� ���ʷ� Character Pig ���� 1 �� �ش�� [4] ���� row[4] ��°�� TRUE�� ���ٸ� bool true�� �Ҵ�ǰ� FALSE��� bool false�� �Ҵ��
        }

        Load();

        ExplainRect = ExpainPanel.GetComponent<RectTransform>();
    }

    void Update()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(CanvasRect, Input.mousePosition, Camera.main, out Vector2 anchoredPos);
        ExplainRect.anchoredPosition = anchoredPos + new Vector2(-180f, -165f);
    }

    /// <summary>
    /// ������ ��� ��ư�� �������� ����Ǵ� �Լ�
    /// </summary>
    public void GetItemClick()
    {
        Item curItem = MyItemList.Find(x => x.Name == ItemNameInput.text); //��ǲ�ʵ忡 ���� �̸��� ���� ��Ҹ� MyItemList���� ã��
        ItemNumberInput.text = ItemNumberInput.text == "" ? "1" : ItemNumberInput.text; 
        //�ѹ���ǲ�ʵ忡 �ƹ��͵� ���� �ʾҴٸ� 1�� �����ְ� ������ �ƴ϶�� �������� �״�� ������
        if(curItem != null) //��ǲ�ʵ忡 ���� �̸��� ������Ʈ�� ������ �ִٸ� ����
        {
            curItem.Number = (int.Parse(curItem.Number) + int.Parse(ItemNumberInput.text)).ToString();
            // # int.Parse ���ڿ��� ������ �ٲٴ� �ż���
            //MyItemList�� �ִ� ������Ʈ�� ���ڸ� ���� ���ڿ� �ѹ��ʵ��� ���� ���ڸ� ����
        }
        else //�̸��� ���� ������Ʈ�� ���ٸ� ����
        {
            Item curAllItem = AllItemList.Find(x => x.Name == ItemNameInput.text); //��ǲ�ʵ忡 ���� �̸��� ���� ��Ҹ� AllItemList���� ã��
            if (curAllItem != null) //ã�� ������Ʈ�� ���� �ƴ϶��
            {
                curAllItem.Number = ItemNumberInput.text; //�ѹ��ʵ忡 ���� �����Ҵ�
                MyItemList.Add(curAllItem); //MyItemList �� ã�� ������Ʈ�� �־���
            }
        }
        Save();
    }

    /// <summary>
    /// ������ ���� ��ư�� �������� ����Ǵ� �Լ�
    /// </summary>
    public void RemoveItemClick()
    {
        Item curItem = MyItemList.Find(x => x.Name == ItemNameInput.text);
        //��ǲ�ʵ忡 ���� �̸��� ���� ��Ҹ� MyItemList���� ã��
        if (curItem != null )
        {
            int curNumber = int.Parse(curItem.Number) - int.Parse(ItemNumberInput.text == "" ? "1" : ItemNumberInput.text); 
            //MyItemList�� �ִ� ������Ʈ�� ���ڸ� ���� ���ڿ� �ѹ��ʵ��� ���� ���ڸ� ����

            if (curNumber <= 0)
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

    /// <summary>
    /// ������ �ʱ�ȭ ��ư�� �������� ����Ǵ� �Լ�
    /// </summary>
    public void ResetItemClick()
    {
        Item BasicItem = AllItemList.Find(x => x.Name == "Pig"); //Pig�������� �Ҵ���
        MyItemList = new List<Item> { BasicItem }; //Pig�� ����ִ� ����Ʈ�� ���� ����� �Ҵ�
        Save();
    }

    /// <summary>
    /// ������ Ŭ�������� ����Ǵ� �Լ�
    /// </summary>
    /// <param name="slotNum"></param>
    public void SlotClick(int slotNum)
    {
        Item CurItem = CurItemList[slotNum];
        Item UsingItem = CurItemList.Find(x => x.isUsing == true);

        if(curType == "Character")
        {
            if(UsingItem != null) UsingItem.isUsing = false; //���ǰ� �ִ� �������� �ִٸ� �̻�� ���·� ��ȯ
            CurItem.isUsing = true; //���� �������� �����·� ��ȯ
        }
        else //Balloon�϶�
        {
            //ture             //false
            CurItem.isUsing = !CurItem.isUsing; //�����¸� ���� ų�� ����
            if (UsingItem != null) UsingItem.isUsing = false; //���ǰ� �ִ� �������� �ִٸ� �̻�� ���·� ��ȯ
        }

        Save(); //��������� ������
    }

    /// <summary>
    /// �� Ŭ���� ������ ����Ǵ� �Լ�
    /// </summary>
    /// <param name="tabName"></param>
    public void TabClick(string tabName)
    {
        curType = tabName; //���� Ÿ���� tabName���� �Ҵ�
        CurItemList = MyItemList.FindAll(x => x.Type == tabName);
        //CurItemList�� MyItemList���� x(����Ʈ�� ���) �� x.Type(����Ʈ ����� Ÿ��)�� ����Ÿ���� �̸��̶� ���� ��Ҹ� ��� ã�´�

        for(int i = 0;i < Slot.Length;i++) //�޾ƿ� ������ ������ŭ ���ư�
        {
            bool isExist = i < CurItemList.Count; //i �� CurItemList ����� ���� ���� ������ True�� ��ȯ��

            Slot[i].SetActive(isExist); //true�� ���� ���� �迭�� ��Ҹ� Ȱ��ȭ��
            Slot[i].GetComponentInChildren<Text>().text = isExist ? CurItemList[i].Name : " " ; 
            //���� ������Ʈ �ȿ� �ִ� �ؽ�Ʈ�� isExist�� True�� CurItemList�� i��°�� �ִ� �̸��� �����ְ� False��� ������ ������

            if (isExist)
            {
                ItemImage[i].sprite = ItemSprite[AllItemList.FindIndex(x => x.Name == CurItemList[i].Name)];
                //������ �̹����� ��������Ʈ�� AllItemList�� x(����Ʈ�� ��Ҹ� ��Ī��) x.Name(����Ʈ�� ����� �̸�) �� CurItemList�� i������ �̸��� ���� �ε��� ���ڸ� ��ȯ��
                UsingImage[i].SetActive(CurItemList[i].isUsing); //CurItemList i������ ��Ұ� isUsing�� True�� ����� �̹����� ���
            }
        }
        

        //�ܹ�ư�� ���� ������ ��������Ʈ�� ����� �ڵ�
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

    /// <summary>
    /// ���콺�� ���� ���� ������ ����Ǵ� ����
    /// </summary>
    /// <param name="slotNum"></param>
    public void PointerEenter(int slotNum)
    {
        PointerCoroutine = PointerEnterDelay(slotNum);
        StartCoroutine(PointerCoroutine);

        ExpainPanel.GetComponentInChildren<Text>().text = CurItemList[slotNum].Name;
        ExpainPanel.transform.GetChild(2).GetComponent<Image>().sprite = Slot[slotNum].transform.GetChild(1).GetComponent<Image>().sprite;
        ExpainPanel.transform.GetChild(3).GetComponent<Text>().text = CurItemList[slotNum].Number + "��";
        ExpainPanel.transform.GetChild(4).GetComponent<Text>().text = CurItemList[slotNum].Explain;
        // # GameObject.transform.GetChild(i) GameObject�� �ڽĿ�����Ʈ�߿� i ��° ������Ʈ�� ������
    }

    /// <summary>
    /// ���콺�� �������� 0.5�ʵ��� ������
    /// </summary>
    /// <param name="slotNum"></param>
    /// <returns></returns>
    IEnumerator PointerEnterDelay(int slotNum)
    {
        yield return new WaitForSeconds(0.5f);
        ExpainPanel.gameObject.SetActive(true);

    }

    /// <summary>
    /// ���콺�� ���� ������ ������
    /// </summary>
    /// <param name="slotNum"></param>
    public void PointerExit(int slotNum)
    {
        StopCoroutine(PointerCoroutine);
        ExpainPanel.gameObject.SetActive(false);
    }

    /// <summary>
    /// JSON�� �̿��ؼ� �����͸� �����ϴ� �Լ�
    /// </summary>
    void Save()
    {
        string jdata = JsonConvert.SerializeObject(MyItemList); //MyItemList ����ȭ�ؼ� jdata�� �����Ѵ�
        // # JSON�� ����Ʈ�� ����ȭ �ϴ� ���� �ǹ��Ѵ�
        File.WriteAllText(Application.dataPath + "/Resources/MyItemText.txt", jdata); 
        //�ش� ����Ƽ ������Ʈ�� ���� ��ο� ���ؼ� Resources�� MyItemText.txt��� �ؽ�Ʈ ������ ������ �����Ѵ�

        TabClick(curType);
    }
    /// <summary>
    /// JSON�� �̿��ؼ� ����� �����͸� �ҷ����� �Լ�
    /// </summary>
    void Load()
    {
        string jdata = File.ReadAllText(Application.dataPath + "/Resources/MyItemText.txt"); 
        //jdata�� �ش� ����Ƽ ������Ʈ�� ���� ��ο� ���ؼ� Resources�� MyItemText.txt��� �ؽ�Ʈ ���Ͽ� �����ִ� �ؽ�Ʈ�� �Ҵ��Ѵ�
        MyItemList = JsonConvert.DeserializeObject<List<Item>>(jdata);
        //jdata�� ItemŸ���� List�� ������ȭ �ؼ� MyItemList�� �Ҵ��Ѵ�

        TabClick(curType);
    }
}
