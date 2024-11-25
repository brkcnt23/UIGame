using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NEXUS.Utilities;

public class Test_Dice : MonoBehaviour
{
    void OnEnable()
    {
        RollTest();
        RollSuccessTest();
        RollCriticalSuccessTest();
        RollCriticalFailureTest();
    }

    public void RollTest()
    {
        Print("Rolling a D100: " + Dice.RollD100());
        Print("Rolling a D20: " + Dice.RollD20());
        Print("Rolling a D12: " + Dice.RollD12());
        Print("Rolling a D10: " + Dice.RollD10());
        Print("Rolling a D8: " + Dice.RollD8());
        Print("Rolling a D6: " + Dice.RollD6());
        Print("Rolling a D4: " + Dice.RollD4());
    }

    public void RollSuccessTest()
    {
        int targetNumber = 10;
        int roll = Dice.RollD20();
        int modifier = 2;
        int difficulty = 5;

        Print("Rolling a D20: " + roll);
        Print("Target Number: " + targetNumber);
        Print("Modifier: " + modifier);
        Print("Difficulty: " + difficulty);

        Print("Roll Success: " + Dice.RollSuccess(targetNumber, roll));
        Print("Roll Success with Modifier: " + Dice.RollSuccess(targetNumber, roll, modifier));
        Print("Roll Success with Modifier and Difficulty: " + Dice.RollSuccess(targetNumber, roll, modifier, difficulty));
    }

    public void RollCriticalSuccessTest()
    {
        int roll = Dice.RollD20();

        Print("Rolling a D20: " + roll);
        Print("Critical Success: " + Dice.RollCriticalSuccess(roll));
    }

    public void RollCriticalFailureTest()
    {
        int roll = Dice.RollD20();

        Print("Rolling a D20: " + roll);
        Print("Critical Failure: " + Dice.RollCriticalFailure(roll));
    }

    void Print(string message)
    {
        Debug.Log($"{message}\nSender:\"{this.GetType().Name}\" class in \"{this.gameObject.name}\" object");
    }
}
