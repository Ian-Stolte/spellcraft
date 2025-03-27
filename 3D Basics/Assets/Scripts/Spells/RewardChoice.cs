using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardChoice : MonoBehaviour
{
    public int num;
    public int width;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ShowRewards(num);
        }
    }

    public void ShowRewards(int n)  //likely change to Block[] and get n from .Length
    {
        if (n < width) //just one row
        {
            List<int> currRow = new List<int>();
            for (int i = 0; i < n; i++)
            {
                currRow.Add(i);
                MakeRow(currRow, 0);
            }
        }
        else if (n%width == 1)
        {
            //first row of width-1
            List<int> currRow = new List<int>();
            for (int i = 0; i < width-1; i++)
            {
                currRow.Add(i);
            }
            MakeRow(currRow, 0);
            currRow.Clear();

            //middle rows
            int rowCount = 0;
            int rowNum = 1;
            for (int i = width-1; i < n-2; i++)
            {
                rowCount++;
                currRow.Add(i);
                if (rowCount == width)
                {
                    MakeRow(currRow, rowNum);
                    rowCount = 0;
                    currRow.Clear();
                    rowNum++;
                }
            }

            //last row of 2
            for (int i = n-2; i < n; i++)
            {
                currRow.Add(i); 
            }
            MakeRow(currRow, rowNum);
        }
        else
        {
            int rowNum = 0;
            int rowCount = 0;
            List<int> currRow = new List<int>(); //change to block
            for (int i = 0; i < n; i++)
            {
                rowCount++;
                currRow.Add(i);
                if (rowCount == width)
                {
                    MakeRow(currRow, rowNum);
                    rowCount = 0;
                    currRow.Clear();
                    rowNum++;
                }
            }
            MakeRow(currRow, rowNum);
        }
    }

    private void MakeRow(List<int> row, int rowNum)
    {   
        for (int i = 0; i < row.Count; i++)
        {
            Debug.Log("(" + rowNum + ", " + i + ") -> (" + rowNum + ", " + (i + 0.5f - row.Count/2f) + ")");
        }
    }
}
