using UnityEngine;
using UnityEngine.UI; // Include the UI namespace
using TMPro;

public class GoldCountUI : MonoBehaviour
{
    [SerializeField] protected TMP_Text GoldCount; // Reference to the UI Text element
    protected int goldBagCount = 0; // Number of gold bags collected

    // Method to update the gold bag count and the UI text
    public void UpdateGoldCount()
    {
        goldBagCount++; // Increment the count
        GoldCount.text = "Gold Bags Collected: " + goldBagCount; // Update the UI text
    }
}
