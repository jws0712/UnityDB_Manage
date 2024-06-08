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
    public TextAsset ItemDatabase; //엑셀에서 작성한 데이터를 txt로 변환한 것을 받음

    public List<Item> AllItemList, MyItemList, CurItemList; //AllItemList : 게임 내에 있는 모든 아이템 리스트
                                                            //MyItemLIst : 내가 가지고 있는 아이템
                                                            //CurItemLIst : 현재 타입의 아이템
    public string curType = "Character"; //인벤토리창에 해당되는 타입

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
        //Character	Pig	돼지	1	FALSE
        //Character Creeper 크리퍼 1   FALSE
        //Character   Bee 벌   1   FALSE
        //Balloon RedBalloon 빨간풍선    1   FALSE
        //Balloon GreenBalloon 초록풍선    1   FALSE
        //Balloon BlueBalloon 파란풍선    1   FALSE
        //전체를 가져오고 줄바꿈을 기준으로 나누고 나눈것을 line에 넣어준다
        for (int i = 0; i < line.Length; i++)
        {
            string[] row = line[i].Split('\t');
            //Character	Pig	돼지	1	FALSE 를 가져오고 텝 기준으로 나누고 나눈것을 row에 넣어준다
            //AllItemList.Add(new Item(row[0], row[1], row[2], row[3], row[4] == "TRUE"));
            AllItemList.Add(new Item(row[0], row[1], row[2], row[3], row[4] == "TRUE"));
            // # new Item으로 넣지 않으면 매서드로 인식하게 때문에 new를 넣어주어야 한다
            //Character	Pig	돼지	1	FALSE
            //row[0 ~ 3] 에는 차례로 Character Pig 돼지 1 에 해당됨 [4] 에는 row[4] 번째가 TRUE와 같다면 bool true가 할당되고 FALSE라면 bool false가 할당됨
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
    /// 아이템 얻기 버튼을 눌렀을때 실행되는 함수
    /// </summary>
    public void GetItemClick()
    {
        Item curItem = MyItemList.Find(x => x.Name == ItemNameInput.text); //인풋필드에 적힌 이름과 같은 요소를 MyItemList에서 찾음
        ItemNumberInput.text = ItemNumberInput.text == "" ? "1" : ItemNumberInput.text; 
        //넘버잇풋필드에 아무것도 적지 않았다면 1을 보여주고 공백이 아니라면 적은것을 그대로 보여줌
        if(curItem != null) //인풋필드에 적은 이름의 오브젝트를 가지고 있다면 실행
        {
            curItem.Number = (int.Parse(curItem.Number) + int.Parse(ItemNumberInput.text)).ToString();
            // # int.Parse 문자열을 정수로 바꾸는 매서드
            //MyItemList에 있는 오브젝트의 숫자를 현재 숫자와 넘버필드의 적은 숫자를 더함
        }
        else //이름을 적은 오브젝트가 없다면 실행
        {
            Item curAllItem = AllItemList.Find(x => x.Name == ItemNameInput.text); //인풋필드에 적힌 이름과 같은 요소를 AllItemList에서 찾음
            if (curAllItem != null) //찾은 오브젝트가 널이 아니라면
            {
                curAllItem.Number = ItemNumberInput.text; //넘버필드에 적은 숫자할당
                MyItemList.Add(curAllItem); //MyItemList 에 찾은 오브젝트를 넣어줌
            }
        }
        Save();
    }

    /// <summary>
    /// 아이템 삭제 버튼을 눌렀을때 실행되는 함수
    /// </summary>
    public void RemoveItemClick()
    {
        Item curItem = MyItemList.Find(x => x.Name == ItemNameInput.text);
        //인풋필드에 적힌 이름과 같은 요소를 MyItemList에서 찾음
        if (curItem != null )
        {
            int curNumber = int.Parse(curItem.Number) - int.Parse(ItemNumberInput.text == "" ? "1" : ItemNumberInput.text); 
            //MyItemList에 있는 오브젝트의 숫자를 현재 숫자와 넘버필드의 적은 숫자를 빼줌

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
    /// 아이템 초기화 버튼을 눌렀을때 실행되는 함수
    /// </summary>
    public void ResetItemClick()
    {
        Item BasicItem = AllItemList.Find(x => x.Name == "Pig"); //Pig아이템을 할당함
        MyItemList = new List<Item> { BasicItem }; //Pig만 들어있는 리스트를 새로 만들어 할당
        Save();
    }

    /// <summary>
    /// 슬롯을 클릭했을때 실행되는 함수
    /// </summary>
    /// <param name="slotNum"></param>
    public void SlotClick(int slotNum)
    {
        Item CurItem = CurItemList[slotNum];
        Item UsingItem = CurItemList.Find(x => x.isUsing == true);

        if(curType == "Character")
        {
            if(UsingItem != null) UsingItem.isUsing = false; //사용되고 있는 아이템이 있다면 미사용 상태로 전환
            CurItem.isUsing = true; //현제 아이템을 사용상태로 전환
        }
        else //Balloon일때
        {
            //ture             //false
            CurItem.isUsing = !CurItem.isUsing; //사용상태를 껐다 킬수 있음
            if (UsingItem != null) UsingItem.isUsing = false; //사용되고 있는 아이템이 있다면 미사용 상태로 전환
        }

        Save(); //변경사항을 저장함
    }

    /// <summary>
    /// 텝 클릭을 했을떄 실행되는 함수
    /// </summary>
    /// <param name="tabName"></param>
    public void TabClick(string tabName)
    {
        curType = tabName; //현재 타입을 tabName으로 할당
        CurItemList = MyItemList.FindAll(x => x.Type == tabName);
        //CurItemList에 MyItemList에서 x(리스트의 요소) 에 x.Type(리스트 요소의 타입)이 현재타입의 이름이랑 같은 요소를 모두 찾는다

        for(int i = 0;i < Slot.Length;i++) //받아온 슬롯의 개수만큼 돌아감
        {
            bool isExist = i < CurItemList.Count; //i 가 CurItemList 요소의 개수 보다 작을때 True를 반환함

            Slot[i].SetActive(isExist); //true를 받은 슬롯 배열의 요소만 활성화됨
            Slot[i].GetComponentInChildren<Text>().text = isExist ? CurItemList[i].Name : " " ; 
            //슬롯 오브젝트 안에 있는 텍스트를 isExist가 True면 CurItemList의 i번째에 있는 이름을 보여주고 False라면 공백을 보여줌

            if (isExist)
            {
                ItemImage[i].sprite = ItemSprite[AllItemList.FindIndex(x => x.Name == CurItemList[i].Name)];
                //아이템 이미지의 스프라이트를 AllItemList중 x(리스트의 요소를 지칭함) x.Name(리스트의 요소의 이름) 이 CurItemList의 i번쨰의 이름과 같은 인덱스 숫자를 반환함
                UsingImage[i].SetActive(CurItemList[i].isUsing); //CurItemList i번쨰의 요소가 isUsing이 True면 사용중 이미지를 띄움
            }
        }
        

        //텝버튼이 눌러 졌을떄 스프라이트를 바쿠는 코드
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
    /// 마우스를 슬룻 위에 뒀을때 실행되는 로직
    /// </summary>
    /// <param name="slotNum"></param>
    public void PointerEenter(int slotNum)
    {
        PointerCoroutine = PointerEnterDelay(slotNum);
        StartCoroutine(PointerCoroutine);

        ExpainPanel.GetComponentInChildren<Text>().text = CurItemList[slotNum].Name;
        ExpainPanel.transform.GetChild(2).GetComponent<Image>().sprite = Slot[slotNum].transform.GetChild(1).GetComponent<Image>().sprite;
        ExpainPanel.transform.GetChild(3).GetComponent<Text>().text = CurItemList[slotNum].Number + "개";
        ExpainPanel.transform.GetChild(4).GetComponent<Text>().text = CurItemList[slotNum].Explain;
        // # GameObject.transform.GetChild(i) GameObject의 자식오브젝트중에 i 번째 오브젝트를 가져옴
    }

    /// <summary>
    /// 마우스가 슬롯위에 0.5초동안 있을때
    /// </summary>
    /// <param name="slotNum"></param>
    /// <returns></returns>
    IEnumerator PointerEnterDelay(int slotNum)
    {
        yield return new WaitForSeconds(0.5f);
        ExpainPanel.gameObject.SetActive(true);

    }

    /// <summary>
    /// 마우스를 슬롯 위에서 땠을때
    /// </summary>
    /// <param name="slotNum"></param>
    public void PointerExit(int slotNum)
    {
        StopCoroutine(PointerCoroutine);
        ExpainPanel.gameObject.SetActive(false);
    }

    /// <summary>
    /// JSON을 이용해서 데이터를 저장하는 함수
    /// </summary>
    void Save()
    {
        string jdata = JsonConvert.SerializeObject(MyItemList); //MyItemList 직렬화해서 jdata에 저장한다
        // # JSON은 리스트를 정렬화 하는 것을 의미한다
        File.WriteAllText(Application.dataPath + "/Resources/MyItemText.txt", jdata); 
        //해당 유니티 프로젝트의 문서 경로에 더해서 Resources에 MyItemText.txt라는 텍스트 파일을 생성해 저장한다

        TabClick(curType);
    }
    /// <summary>
    /// JSON을 이용해서 저장된 데이터를 불러오는 함수
    /// </summary>
    void Load()
    {
        string jdata = File.ReadAllText(Application.dataPath + "/Resources/MyItemText.txt"); 
        //jdata에 해당 유니티 프로젝트의 문서 경로에 더해서 Resources에 MyItemText.txt라는 텍스트 파일에 적해있는 텍스트를 할당한다
        MyItemList = JsonConvert.DeserializeObject<List<Item>>(jdata);
        //jdata를 Item타입의 List로 역직렬화 해서 MyItemList에 할당한다

        TabClick(curType);
    }
}
