using System.Collections;
using UnityEngine;

public class InitialTutorialPopup : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return null;

        if (gameManager.instance != null)
        {
            gameManager.instance.ShowTutorial("Keybinds", "Player Keybinds", "WASD to Move.\n Mouse 1 to Fire.\n ScrollWheel to change weapons.\n V to Melee.\n G to throw a grenade.\n M for map.\n Press Esc to continue.");
        }
    }
}