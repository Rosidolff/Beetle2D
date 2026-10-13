using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreText; 

    public void AddScore(int score)
    {
        scoreText.text = score.ToString("00000"); 
    }
}
