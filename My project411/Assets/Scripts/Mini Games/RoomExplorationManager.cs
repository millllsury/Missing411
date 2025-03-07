using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class RoomExplorationManager : MonoBehaviour
{
    [SerializeField] private SpriteRenderer bgSprite;
    [SerializeField] private GameObject interactableObjectsContainer; 
    [SerializeField] private Button candle;
    private FeedbackManager feedbackManager;
    [SerializeField] private Canvas keysGroup;

    [SerializeField] private Image boxImage; 
    [SerializeField] private Button boxButton;

    private bool isOpen = false;
    private bool isRoomLit = false;
    private bool isMoved = false;
    [SerializeField] private GameObject keyObject1;
    [SerializeField] private GameObject keyObject2;

    [SerializeField] private GameObject arrowHint; 
    


    void Start()
    {
        FeedbackManager.Instance.ShowMessage("Look around the room!");
        keyObject1.gameObject.SetActive(false);
        keyObject2.gameObject.SetActive(false);
        bgSprite.sprite = Resources.Load<Sprite>("Backgrounds/MainRoom/houseMainRoomNight");
        interactableObjectsContainer.SetActive(false); // �������� �������� � ����� �������
        arrowHint.SetActive(false); // �������� ������� ����������
        Invoke(nameof(ShowArrowHint), 1f); // ���������� ����� 2 �������
    }

  

    void ShowArrowHint()
    {
        if (isRoomLit) return;
        arrowHint.SetActive(true);
    }
    
    public void LightCandle()
    {
        if (arrowHint != null)
        {
            arrowHint.SetActive(false);

        }

        if (!isRoomLit)
        {
            SoundManager.Instance.PlaySoundByName("Candle");
            isRoomLit = true;
            bgSprite.sprite = Resources.Load<Sprite>("Backgrounds/MainRoom/houseMainRoom");
            interactableObjectsContainer.SetActive(true); 
            candle.gameObject.SetActive(false);
        }
    }

    private string boxOpenPath = "UI/boxOpen"; 
    private string boxClosedPath = "UI/boxClosed";
    int slotIndex = GameStateManager.Instance.GetSelectedSlotIndex();
    public void OnBoxClick()
    {
        isOpen = !isOpen;

        if (boxImage != null)
        {

            if (isOpen)
            {
                SoundManager.Instance.PlaySoundByName("ChestOpen");
            }
            else {
                SoundManager.Instance.PlaySoundByName("ChestClose");

            }

            string spritePath = isOpen ? boxOpenPath : boxClosedPath;
            Sprite newSprite = Resources.Load<Sprite>(spritePath);
            if (newSprite != null)
            {
                boxImage.sprite = newSprite;
            }
            else
            {
                Debug.LogError($"Sprite not found in Resources/{spritePath}");
            }
        }
        
        if (!GameStateManager.Instance.IsKeyCollected("GoldenKey5"))
        {
            if (isOpen)
            {
                keyObject2.gameObject.SetActive(true);
            }
            else
            {
                keyObject2.gameObject.SetActive(false);
            }
        }
       
    }

    public void OnGlassClick()
    {
        isMoved = !isMoved;
        bool isKeyCollected = GameStateManager.Instance.IsKeyCollected("GoldenKey6");

        if (isMoved)
        {
            SoundManager.Instance.PlaySoundByName("MovingGlass");
            if (!isKeyCollected)
            {
                keyObject1.gameObject.SetActive(true);
            }
        }
        else
        {
            SoundManager.Instance.PlaySoundByName("MovingBack");
            if (!isKeyCollected)
            {
                StartCoroutine(KeyDisable());
            }
        }
    }




    private IEnumerator KeyDisable()
    {
        yield return new WaitForSeconds(0.5f); 
        keyObject1.gameObject.SetActive(false);
    }

}
