
using UnityEngine;

public class DevilDogMode : MonoBehaviour
{

    [SerializeField] float DevilDogDuration = 10.0f;
    [SerializeField] float currentPoints = 0;
    [SerializeField] float maxPoints = 100;
    [SerializeField] int killPoints = 5;
    [SerializeField] int dogTagPoints = 20;

    bool DogActive = false;
    float currentTimer = 0.0f;

    public bool isActive => DogActive;
    public float CurrentTimer => currentTimer;

    // Update is called once per frame
    void Start()
    {
        updateDevilDogUI();
    }
    void Update()
    {
        HandleInput();
        updateDogTimer();
        if (Input.GetKeyDown(KeyCode.P))
        {
            AddPoints(5);
        }
    }
    void HandleInput()
    {
        if (Input.GetButtonDown("DevilDog") && !DogActive && currentPoints >= maxPoints)
        {
            DevilDogModeOn();
        }
    }
    void DevilDogModeOn()
    {
        DogActive = true;
        currentTimer = DevilDogDuration;
        Debug.Log("Devil Dog Mode activated!");
    }
    void updateDogTimer()
    {
        if (!DogActive)
        {
            return;
        }
        currentTimer -= Time.deltaTime;

        float drainPerSecond = maxPoints / DevilDogDuration;
        currentPoints -= drainPerSecond * Time.deltaTime;
        currentPoints = Mathf.Clamp(currentPoints, 0f, maxPoints);

        updateDevilDogUI();

        if (currentTimer <= 0.0f || currentPoints <= 0f)
        {
            DeactivateDevilDogMode();
        }
    }
    void updateDevilDogUI()
    {
        if(gameManager.instance != null)
        {
            gameManager.instance.updateDevilDogBar(currentPoints, maxPoints);
        }
    }
    public void AddPoints(float amount)
    {
        if (DogActive)
        { return; }


        currentPoints += amount;
        currentPoints = Mathf.Clamp(currentPoints, 0f, maxPoints);
        updateDevilDogUI();
       
    }
    void DeactivateDevilDogMode()
    {
        DogActive = false;
        currentTimer = 0f;
        currentPoints = 0f;
        updateDevilDogUI();
        Debug.Log("Devil Dog Mode Ended");
    }
    public void AddKillPoints()
    {
        AddPoints(killPoints);
    }
    public void AddDogTagPoints()
    {
        AddPoints(dogTagPoints);
    }
}
