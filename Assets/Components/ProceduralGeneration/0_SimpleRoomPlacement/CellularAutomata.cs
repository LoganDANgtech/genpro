using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;
using VTools.Utility;

namespace Components.ProceduralGeneration.SimpleRoomPlacement
{
    [CreateAssetMenu(menuName = "Procedural Generation Method/CellularAutomata")]
    public class CellularAutomata : ProceduralGenerationMethod
    {
        [Header("Room Parameters")]
        [SerializeField, UnityEngine.Range(0, 100)] private int NoiseDensity = 50;
        [SerializeField, UnityEngine.Range(0, 8)] private int detection = 5;


        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {

            Debug.Log($"Starting Grid instantiation ...");
            var time = DateTime.Now;
            BuildGround();
            BuildNoiseWater();
            Debug.Log($"Instantiation completed in {(DateTime.Now - time).TotalSeconds: 0.00} seconds.");

            for (int i = 0; i < _maxSteps; i++)
            {
                SmoothNoiseWater();
                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);

            }
        }

        private void BuildNoiseWater()
        {
            var groundTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Water");

            // Instantiate ground blocks
            for (int x = 0; x < Grid.Width; x++)
            {
                for (int z = 0; z < Grid.Lenght; z++)
                {
                    if (!Grid.TryGetCellByCoordinates(x, z, out var chosenCell))
                    {
                        Debug.LogError($"Unable to get cell on coordinates : ({x}, {z})");
                        continue;
                    }

                    if (RandomService.Range(0,100) < NoiseDensity)
                    {
                        GridGenerator.AddGridObjectToCell(chosenCell, groundTemplate, true);
                    }

                }
            }
        }

        private void SmoothNoiseWater()
        {
            var waterTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Water"); //0
            var groundTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Grass"); //1

            List<bool> newgrid = new List<bool>();

            Cell chosenCell;
            // Instantiate ground blocks
            for (int x = 0; x < Grid.Width; x++)
            {
                for (int z = 0; z < Grid.Lenght; z++)
                {
                    int surround = 0;

                    for (int i = 0; i < 2; i++)
                    {
                        if (Grid.TryGetCellByCoordinates(x-1+i, z-1, out chosenCell) && chosenCell.ContainObject && chosenCell.GridObject.Template.Name == "Grass")
                        {
                            surround++;
                        }
                    }
                    if (Grid.TryGetCellByCoordinates(x - 1, z, out  chosenCell) && chosenCell.ContainObject && chosenCell.GridObject.Template.Name == "Grass")
                    {
                        surround++;
                    }
                    if (Grid.TryGetCellByCoordinates(x + 1, z, out  chosenCell) && chosenCell.ContainObject && chosenCell.GridObject.Template.Name == "Grass")
                    {
                        surround++;
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        if (Grid.TryGetCellByCoordinates(x - 1+i, z+1, out chosenCell) && chosenCell.ContainObject && chosenCell.GridObject.Template.Name == "Grass")
                        {
                            surround++;
                        }
                    }
                    

                    if (surround >= detection)
                    {
                        newgrid.Add(true); //deviens du sol
                    }
                    else
                    {
                        newgrid.Add(false);
                    }
                }
            }

            for(int x = 0;x < Grid.Width; x++)
            {

                for (int z = 0; z < Grid.Lenght; z++)
                {
                    if (!Grid.TryGetCellByCoordinates(x, z, out chosenCell))
                    {
                        Debug.LogError($"Unable to get cell on coordinates : ({x}, {z * Grid.Width})");
                        continue;
                    }

                    if (newgrid[x + z*Grid.Width])
                    {
                        GridGenerator.AddGridObjectToCell(chosenCell, groundTemplate, true);
                    }
                    else
                    {
                        GridGenerator.AddGridObjectToCell(chosenCell, waterTemplate, true);

                    }
                }
            }
        }
    }
}