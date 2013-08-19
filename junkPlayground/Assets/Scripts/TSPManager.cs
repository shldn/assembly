using UnityEngine;
using System;
using System.Collections;
using System.ComponentModel;
using System.Threading;

using AForge;
using AForge.Genetic;

public class TSPManager : MonoBehaviour {

    private int citiesCount = 20;
    private int populationSize = 40;
    private int iterations = 100;
    private int selectionMethod = 0;
    private float scaleFactor = 0.02f;
    private bool greedyCrossover = true;
    private double[,] map = null;
    private Thread workerThread = null;
    private volatile bool needToStop = false;

    void UpdateMap(double[,] map)
    {
        CityFactory.Inst.DeleteAll();
        for (int i = 0; i < citiesCount; i++)
            CityFactory.Inst.GetCity(scaleFactor * (float)map[i, 0], scaleFactor * (float)map[i, 1]);        
    }

    void UpdatePath(double[,] path)
    {
        for (int i = 0; i < citiesCount; ++i)
        {
            Debug.DrawLine(new Vector3(scaleFactor * (float)path[i, 0], CityFactory.Inst.cityHeight, scaleFactor * (float)path[i, 1]),
                            new Vector3(scaleFactor * (float)path[i + 1, 0], CityFactory.Inst.cityHeight, scaleFactor * (float)path[i + 1, 1]), // path array goes to citiesCount+1
                            Color.cyan, 1000); 
        }
    }

    void ErasePath()
    {
    }

    // Generate new map for the Traivaling Salesman problem
    private void GenerateMap()
    {
        System.Random rand = new System.Random((int)DateTime.Now.Ticks);

        // create coordinates array
        map = new double[citiesCount, 2];

        for (int i = 0; i < citiesCount; i++)
        {
            map[i, 0] = rand.Next(1001);
            map[i, 1] = rand.Next(1001);
        }

        // set the map
        UpdateMap(map);
        // erase path if it is
        ErasePath();
    }

    void SearchSolution()
    {
        // create fitness function
        TSP.TSPFitnessFunction fitnessFunction = new TSP.TSPFitnessFunction(map);
        // create population
        Population population = new Population(populationSize,
            (greedyCrossover) ? new TSP.TSPChromosome(map) : new PermutationChromosome(citiesCount),
            fitnessFunction,
            (selectionMethod == 0) ? (ISelectionMethod)new EliteSelection() :
            (selectionMethod == 1) ? (ISelectionMethod)new RankSelection() :
            (ISelectionMethod)new RouletteWheelSelection()
            );
        // iterations
        int i = 1;

        // path
        double[,] path = new double[citiesCount + 1, 2];

        // loop
        while (!needToStop)
        {
            // run one epoch of genetic algorithm
            population.RunEpoch();

            // display current path
            ushort[] bestValue = ((PermutationChromosome)population.BestChromosome).Value;

            for (int j = 0; j < citiesCount; j++)
            {
                path[j, 0] = map[bestValue[j], 0];
                path[j, 1] = map[bestValue[j], 1];
            }
            path[citiesCount, 0] = map[bestValue[0], 0];
            path[citiesCount, 1] = map[bestValue[0], 1];

            UpdatePath(path);

            // set current iteration's info
            fitnessFunction.PathLength(population.BestChromosome);

            // increase current iteration
            i++;

            //
            if ((iterations != 0) && (i > iterations))
                break;
        }
    }

    void OnGUI()
    {
        GUILayout.BeginVertical();
        if (GUILayout.Button("Create Cities"))
            GenerateMap();

        if (GUILayout.Button("Start"))
            SearchSolution();
        GUILayout.EndVertical();
    }
}
