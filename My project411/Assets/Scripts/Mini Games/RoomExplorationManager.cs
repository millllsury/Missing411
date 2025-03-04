using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
        interactableObjectsContainer.SetActive(false); // Скрываем предметы в тёмной комнате
        arrowHint.SetActive(false); // Скрываем стрелку изначально
        Invoke(nameof(ShowArrowHint), 1f); // Показываем через 2 секунды
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
        Debug.Log($"GameStateManager.Instance.IsKeyCollected( \"GoldenKey6\"): {GameStateManager.Instance.IsKeyCollected("GoldenKey6")}");
        if (!GameStateManager.Instance.IsKeyCollected( "GoldenKey6"))
        {
            if (isMoved)
            {
                keyObject1.gameObject.SetActive(true);
            }
            else
            {
                keyObject1.gameObject.SetActive(false);
            }
        }
    }
    
    
}
