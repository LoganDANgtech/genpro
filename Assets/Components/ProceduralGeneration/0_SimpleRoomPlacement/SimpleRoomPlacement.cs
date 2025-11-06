using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;
using VTools.Utility;

namespace Components.ProceduralGeneration.SimpleRoomPlacement
{
    [CreateAssetMenu(menuName = "Procedural Generation Method/Simple Room Placement")]
    public class SimpleRoomPlacement : ProceduralGenerationMethod
    {
        [Header("Room Parameters")]
        [SerializeField] private int _maxRooms = 10;
        [SerializeField] private Vector2Int widthRange;
        [SerializeField] private Vector2Int heightRange;
        
        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            List<RectInt> rooms = new List<RectInt> { };
            RectInt room = new(0, 0, 0, 0);

            for (int i = 0; i < _maxSteps; i++)
            {
                if (rooms.Count != _maxRooms)
                {
                    // Check for cancellation
                    cancellationToken.ThrowIfCancellationRequested();


                    room.width = RandomService.Range(widthRange);
                    room.height = RandomService.Range(heightRange);

                    room.x = RandomService.Range(0, Grid.Width - room.width);
                    room.y = RandomService.Range(0, Grid.Lenght - room.width);

                    if (CanPlaceRoom(room, 2))
                    {
                        BuildRoom(room);
                        rooms.Add(room);
                    }
                }
                else
                {
                    for (int j = 0; j < rooms.Count - 1; j++)
                    {
                        BuildCorridor(rooms[j], rooms[j + 1]);
                        await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
                    }
                }
                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
            }

            


            BuildGround();
            // Final ground building.
        }
        

        
    }
}