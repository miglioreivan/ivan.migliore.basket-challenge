using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class ManagerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text pointsText;
    private int score;

    public void AddPoints(int p)
    {
        score += p;
        String text = ("Points: " + score);
        pointsText.SetText(text);
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        score = 0;
    }
}
