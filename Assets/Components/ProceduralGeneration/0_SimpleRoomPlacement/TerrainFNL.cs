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
    [CreateAssetMenu(menuName = "Procedural Generation Method/TerrainFNL")]
    public class TerrainFNL : ProceduralGenerationMethod
    {
        /*[Header("Room Parameters")]
        [SerializeField, UnityEngine.Range(0, 100)] private int NoiseDensity = 50;
        [SerializeField, UnityEngine.Range(0, 8)] private int detection = 5;*/



        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            FastNoiseLite fnl = new FastNoiseLite(RandomService.Seed);

            var Rocktemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Rock");
            var Grasstemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Grass");
            var Sandtemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Sand");
            var Watertemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Water");

            for (int x = 0; x < Grid.Width; x++)
            {
                for (int z = 0; z < Grid.Lenght; z++)
                {
                    if (!Grid.TryGetCellByCoordinates(x, z, out var chosenCell))
                    {
                        Debug.LogError($"Unable to get cell on coordinates : ({x}, {z})");
                        continue;
                    }

                    float noiseatcoord = fnl.GetNoise(x, z);
                    if (noiseatcoord > 0.8f)//rock
                    {
                        GridGenerator.AddGridObjectToCell(chosenCell, Rocktemplate, false);
                    }
                    else if (noiseatcoord > 0.2f)//grass
                    {
                        GridGenerator.AddGridObjectToCell(chosenCell, Grasstemplate, false);
                    }
                    else if (noiseatcoord > -0.3f)//sand
                    {
                        GridGenerator.AddGridObjectToCell(chosenCell, Sandtemplate, false);
                    }
                    else //water
                    {
                        GridGenerator.AddGridObjectToCell(chosenCell, Watertemplate, false);
                    }

                }
            }



        }

    }
}