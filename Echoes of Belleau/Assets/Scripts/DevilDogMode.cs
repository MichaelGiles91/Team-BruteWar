
using UnityEngine;

public class DevilDogMode : MonoBehaviour
{

    [SerializeField] float DevilDogDuration = 1.0f;
   

    bool DogActive = false;
    float currentTimer = 0.0f;

    public bool isActive => DogActive;
    public float CurrentTimer => currentTimer;

    // Update is called once per frame
    void Update()
    {
        HandleInput();
        updateDogTimer();
    }
    void HandleInput()
    {
        if (Input.GetButtonDown("DevilDog") && !DogActive)
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

        if (currentTimer <= 0.0f)
        {
            DeactivateDevilDogMode();
        }
    }

    void DeactivateDevilDogMode()
    {
        DogActive = false;
        currentTimer = 0f;
        Debug.Log("Devil Dog Mode Ended");
    }
}
