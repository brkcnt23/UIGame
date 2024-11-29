using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NavUISystem : MonoBehaviour
{
    [Header("Profile Panel")]
    public GameObject profilePanel;

    [Header("Profile UI Elements")]
    public TMP_Text levelText;
    public TMP_Text experienceText;
    public TMP_Text strengthText;
    public TMP_Text dexterityText;
    public TMP_Text constitutionText;
    public TMP_Text charismaText;
    public TMP_Text smitherSkillLevelText;
    public TMP_Text tannerSkillLevelText;
    public TMP_Text carpenterSkillLevelText;
    public TMP_Text masonSkillLevelText;
    public TMP_Text alchemistSkillLevelText;
    public TMP_Text totalBattlesFoughtText;
    public TMP_Text totalBattlesWonText;
    public TMP_Text totalBattlesLostText;
    public TMP_Text companionsText;

    public void OnProfileButtonClick()
    {
        UpdateProfileData();
        profilePanel.SetActive(true);
    }

    private void UpdateProfileData()
    {
        PlayerData pd = PlayerStatHandler.Instance.pd;

        levelText.text = $"Level: {pd.Level}";
        experienceText.text = $"Experience: {pd.Experience} / {pd.MaxExperience}";
        strengthText.text = $"Strength: {pd.Strength}";
        dexterityText.text = $"Dexterity: {pd.Dexterity}";
        constitutionText.text = $"Constitution: {pd.Constitution}";
        charismaText.text = $"Charisma: {pd.Charisma}";

        smitherSkillLevelText.text = $"Smither Skill Level: {pd.SmitherSkillLevel}";
        tannerSkillLevelText.text = $"Tanner Skill Level: {pd.TannerSkillLevel}";
        carpenterSkillLevelText.text = $"Carpenter Skill Level: {pd.CarpenterSkillLevel}";
        masonSkillLevelText.text = $"Mason Skill Level: {pd.MasonSkillLevel}";
        alchemistSkillLevelText.text = $"Alchemist Skill Level: {pd.AlchemistSkillLevel}";

        totalBattlesFoughtText.text = $"Total Battles Fought: {pd.TotalBattlesFought}";
        totalBattlesWonText.text = $"Total Battles Won: {pd.TotalBattlesWon}";
        totalBattlesLostText.text = $"Total Battles Lost: {pd.TotalBattlesLost}";

        companionsText.text = "Companions:\n";
        foreach (var companion in pd.Companions)
        {
            companionsText.text += $"{companion.Name} (Level {companion.Level})\n";
        }
    }
}
