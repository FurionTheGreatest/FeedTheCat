using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomObjectSelector : MonoBehaviour
{
    //good [0] and bad [1] object chance to spawn
    public int[] objectSelectPercentTable = {70, 30};
    //objects chance to spawn table
    public int[] goodObjectPercentTable = {50, 30, 15, 5};
    public int[] badObjectPercentTable = {40, 20, 15, 15,10};
    //sum of chosen table
    public int totalChance;
    public int randomChance;

    /// <summary>
    /// Which object to spawn, good or bad?
    /// </summary>
    /// <returns>True - good, false - bad</returns>
    public bool IsGoodObject()
    {
        randomChance = CalculateRandomNumberFromTable(objectSelectPercentTable);

        //compare is my random weight <= to *good* weight
        return randomChance <= objectSelectPercentTable?[0];
    }
    /// <summary>
    /// Calculate a chance from table to choose object
    /// </summary>
    /// <param name="table">Array with chances</param>
    /// <returns>Return an int chance to choose object</returns>
    private int CalculateRandomNumberFromTable(IEnumerable<int> table)
    {
        CalculateTotalChance(table);
        return  Random.Range(0, totalChance);
    }
    /// <summary>
    /// Calculate total chance of the table
    /// </summary>
    /// <param name="table">Array with chances</param>
    private void CalculateTotalChance(IEnumerable<int> table)
    {
        totalChance = table.Sum();
    }
    /// <summary>
    /// Select object with random chance
    /// </summary>
    /// <param name="table">Array with chances</param>
    /// <returns>Return index of prefab to spawn</returns>
    public int SelectObjectFromTable(IEnumerable<int> table)
    {
        //to list to avoid multiple ienumarations
        var listOfChances = table.ToList();
        //calculate total weight of chosen table
        CalculateTotalChance(listOfChances);
        //calculate random for this table
        randomChance = CalculateRandomNumberFromTable(listOfChances);
        //select object from table
        int index;
        for (index = 0; index < listOfChances.Count; index++)
        {
            var weight = listOfChances[index];
            //compare if random > to the current weight, if not we get object, else subtract and check next
            if (randomChance > weight)
                randomChance -= weight;
            else
                //stop on this index
                break;
        }
        return index;
    }
}
