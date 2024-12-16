[System.Serializable]
public class Walls : Residentials
{
    public override void LevelUpResidential(ref PlayerData _Player)
    {
        base.LevelUpResidential(ref _Player);
        upgradeHour = CalculateUpgradeHour(_Player);
    }
}