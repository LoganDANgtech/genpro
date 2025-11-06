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
        
        
        [Tooltip("Les niveaux de du terrain.")]
        public TerrainLODLevel[] Terrainlevels = new TerrainLODLevel[]
        {
            new TerrainLODLevel(){ name = "Water", transitionHeight = -0.5f , color = new Color(157, 194, 203)},
            new TerrainLODLevel(){ name = "Sand", transitionHeight = 0f ,color = new Color(199, 192, 168)},
            new TerrainLODLevel(){ name = "Grass", transitionHeight = 0.5f ,color = new Color(42, 182, 115)},
            new TerrainLODLevel(){ name = "Rock", transitionHeight = 1f , color = new Color(138, 160, 163)},
        };


        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            FastNoiseLite fnl = new FastNoiseLite(RandomService.Seed);

            var Watertemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Water");
            var Sandtemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Sand");
            var Grasstemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Grass");
            var Rocktemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Rock");

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
                    if (noiseatcoord > Terrainlevels[2].transitionHeight)//rock
                    {
                        GridGenerator.AddGridObjectToCell(chosenCell, Rocktemplate, false);
                    }
                    else if (noiseatcoord > Terrainlevels[1].transitionHeight)//grass
                    {
                        GridGenerator.AddGridObjectToCell(chosenCell, Grasstemplate, false);
                    }
                    else if (noiseatcoord > Terrainlevels[0].transitionHeight)//sand
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