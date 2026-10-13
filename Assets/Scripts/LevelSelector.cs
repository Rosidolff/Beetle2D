using UnityEngine;
using UnityEngine.UI;


public class LevelSelector : MonoBehaviour
{
    [SerializeField] private int levelsPerButton = 1; // Levels to complete to unlock each button (for the Clown)
    // in levelSelector we use the script to enable each buttion based on its level, but the clown need 3 levels to unlock it. 
    //so we use this script to manage that too.

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get the highest unlocked level from PlayerPrefs
        int unLockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1); 

        //Get the Layout Group gameobject with all the levels buttons
        Transform levels = transform.GetChild(0);

        for (int i = 0; i < levels.childCount; i++)
        {
            Button levelButton = levels.GetChild(i).GetComponent<Button>();
             // Enable the button if the level is unlocked
            levelButton.interactable = (i * levelsPerButton < unLockedLevel); //1 for levels, 3 for the clown,
            // so if the unlocked level is 3, the first button will be enabled, but the second will be disabled.
        }

    }

 
}
