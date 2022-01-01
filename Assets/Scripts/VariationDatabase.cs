using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VariationDatabase : MonoBehaviour
{
    public static String[] variationsList = new string[1000];
    public static bool foundIt = false;
    public static bool needHighlight = false;

    public static int counter = 0;


    public static void readfile()
    {

        string line;

        //string fileName = @"C:\Users\GIGABYTE\Desktop\variations.txt";
        //string fileName = @"C:\Users\GIGABYTE\Desktop\chess\3D_Chess\Assets\variations.txt";
        string fileName = @"C:\Users\GIGABYTE\Desktop\Personal\chess\3D_Chess\Assets\variations.txt";

        // Read the file and display it line by line.  
        System.IO.StreamReader file = new System.IO.StreamReader(fileName);
        while ((line = file.ReadLine()) != null)
        {
            //print(line);
            variationsList[counter] = line;
            //System.Console.WriteLine(line);
            //print(variationsList[counter]);
            counter++;
        }

        file.Close();

        print("There were " + counter + " lines.");

    }

    //for each line
    //split on spaces
    //non binary trees and traversal

    public static String VariationFinder(String input)
    {
        //compare string
        String myString = "";
        String returnMove = "";


        //print("Input = " + input);
        //print("Chapter1_1 = " + Chapter1_1);

        //foreach (string i in variationsList)
        //{
        //    print(i);

        //}
        for (int i = 0; i < counter; i++)
        {
            myString = variationsList[i];
            if (myString.Contains(input))
            {
                //print("yes!");
                //then return the value
                string[] partialVariation = input.Split(' ');
                string[] fullVariation = myString.Split(' ');

                //print(partialVariation);
                //print(fullVariation);

                //foreach (var word in partialVariation)
                //{
                //    print(word);
                //}

                //foreach (var word in fullVariation)
                //{
                //    print(word);
                //}

                returnMove = fullVariation[partialVariation.Length - 1];
                //print("Return move = " + returnMove);
                foundIt = true;
                break;
            }
            else
            {
                //print("no...");
                foundIt = false;
            }
        }
        //print("Novelty! Can't help ya here!");
        return returnMove;
    }

    public static String VariationFinderHighlights(String input)
    {
        //compare string
        String myString = "";
        String returnMove = "";
        LinkedList<string> sentence;

        for (int i = 0; i < counter; i++)
        {
            myString = variationsList[i];
            print(myString);
            if (myString.Contains(input))
            {
                //print("yes!");
                //then return the value
                string[] partialVariation = input.Split(' ');
                string[] fullVariation = myString.Split(' ');

                //print(partialVariation);
                //print(fullVariation);

                foreach (var word in partialVariation)
                {
                    //print("balls");
                    //print(word);
                }

                if(partialVariation[0] == "")
                {
                    print("bologna");
                    string[] test = myString.Split(' ');
                    //print(myString[0]);

                    //foreach (var word in test)
                    //{
                    //    //print("balls");
                    //    print(word);
                    //}
                    print(test[0]);//the output here is 1.e4. lets just start by highlighting the square.
                    //HighLightDatabaseMoves();

                    //need to highlight the e2 pawn.

                }

                //foreach (var word in fullVariation)
                //{
                //    print(word);
                //}

                returnMove = fullVariation[partialVariation.Length - 1];
                //print("Return move = " + returnMove);
                needHighlight = true;
            }
            else
            {
                //print("no...");
                needHighlight = false;
            }
        }

        return returnMove;
    }

}