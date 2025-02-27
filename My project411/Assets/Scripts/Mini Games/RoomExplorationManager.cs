using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class RoomExplorationManager : MonoBehaviour
{
    [SerializeField] private SpriteRenderer bgSprite;
    [SerializeField] private GameObject interactableObjectsContainer; // ������ ���������
    //[SerializeField] private GameObject exitButton; // ������ ������
    [SerializeField] private Button candle;
    private FeedbackManager feedbackManager;
    //private int selectedSlotIndex = GameStateManager.Instance.GetSelectedSlotIndex();
    [SerializeField] private Canvas keysGroup;

    [SerializeField] private Image boxImage; // ������ �� UI Image (�� SpriteRenderer!)
    [SerializeField] private Button boxButton;

    private bool isOpen = false;
    private bool isRoomLit = false;

    [SerializeField] private GameObject keyObject1;
    [SerializeField] private GameObject keyObject2;

    [SerializeField] private GameObject arrowHint; // ������ �� �������
    


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
            isRoomLit = true;
            bgSprite.sprite = Resources.Load<Sprite>("Backgrounds/MainRoom/houseMainRoom");
            interactableObjectsContainer.SetActive(true); 
            candle.gameObject.SetActive(false);
            keyObject1.gameObject.SetActive(true);
        }
    }

    private string boxOpenPath = "UI/boxOpen"; 
    private string boxClosedPath = "UI/boxClosed";

    public void OnBoxClick()
    {
        isOpen = !isOpen;

        if (boxImage != null)
        {
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

        if (isOpen)
        {
            keyObject2.gameObject.SetActive(true);
        }
        else
        {
            keyObject2.gameObject.SetActive(false);
        }
    }


    // ���������� ��������� � ���������
    public void GotKeyReward(Button button)
    {

        string keyID = button.gameObject.name;

        // Checking if the key has already been got
        /*if (GameStateManager.Instance.IsKeyCollected(selectedSlotIndex, keyID))
        {
            Debug.Log($"[GotKeyReward] ���� {keyID} ��� ������, ����������.");
            if (button != null)
            {
                button.gameObject.SetActive(false);
            }
            return;
        }*/

        // Add the key to the saved ones
        //GameStateManager.Instance.SaveKeyCollected(selectedSlotIndex, keyID);

        // We are giving out a reward
        CurrencyManager.Instance.AddKeys(1);
        feedbackManager.ShowMessage("You've found a key!");

        // Torn off the btn
        if (button != null)
        {
            button.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Error in GotKeyReward!");
        }
    }

    // ����� �� ����-����
    public void ExitRoom()
    {
        Debug.Log("����� �� �������");

    }
}
