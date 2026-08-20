using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct SimpleUpgrade
{
    public string title;
    public string statText;
}

public class SimpleUpgradeShop : MonoBehaviour
{
    [Header("Put your upgrade choices here")]
    public SimpleUpgrade[] allUpgrades;

    [Header("Drag your 3 cards' UI components here")]
    public TMP_Text[] titleTexts;
    public TMP_Text[] statTexts;
    public Button[] chooseButtons;

    [Header("Scene to return to")]
    public string gameplaySceneName = "SampleScene";

    void Start()
    {
        SetupCards();
    }

    void SetupCards()
    {
        // Randomly assign a different upgrade to each of the 3 cards on screen
        for (int i = 0; i < chooseButtons.Length; i++)
        {
            if (allUpgrades.Length == 0) return;

            int randomIndex = Random.Range(0, allUpgrades.Length);
            SimpleUpgrade chosen = allUpgrades[randomIndex];

            // Set the text on the UI
            titleTexts[i].text = chosen.title;
            statTexts[i].text = chosen.statText;

            // Make the button send the player back to the game when clicked
            int index = i; // Needed for button listener
            chooseButtons[i].onClick.AddListener(() => {
                ApplyAndReturn(chosen);
            });
        }
    }

    void ApplyAndReturn(SimpleUpgrade upgrade)
    {
        Debug.Log("Chosen upgrade: " + upgrade.title);
        // TODO: Add your stat increase logic here (e.g., GameManager.Instance.engineering += 3;)

        // Go back to the game scene
        SceneManager.LoadScene(gameplaySceneName);
    }
}