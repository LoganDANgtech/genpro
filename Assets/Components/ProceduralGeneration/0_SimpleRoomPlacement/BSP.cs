using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;
using VTools.Utility;

namespace Components.ProceduralGeneration.SimpleRoomPlacement
{
    [CreateAssetMenu(menuName = "Procedural Generation Method/BSP")]
    public class BSP : ProceduralGenerationMethod
    {
        [Header("Room Parameters")]
        [SerializeField] private int _maxRooms = 10;
        [SerializeField] private Vector2Int widthRange;
        [SerializeField] private Vector2Int heightRange;

        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            List<RectInt> rooms = new List<RectInt> { };
            List<RectInt> cuttedparts = new List<RectInt> { new RectInt(0, 0, Grid.Width, Grid.Lenght) };
            List<RectInt> root = new List<RectInt> { };
            List<RectInt> child = new List<RectInt> { };

            RectInt room = RectInt.zero;
            int it = 0;

            while ((cuttedparts.Count - root.Count) < _maxRooms || it > _maxSteps)
            {
                (RectInt, RectInt) cuttedpart = Cut(cuttedparts[it], 2); //0 hor - 1 ver - 2 random

                if (cuttedpart.Item2 == new RectInt(-1, -1, -1, -1))
                {
                    cuttedpart = Cut(cuttedparts[it], 1);
                }
                else if (cuttedpart.Item2 == new RectInt(-2, -2, -2, -2))
                {
                    cuttedpart = Cut(cuttedparts[it], 0);
                }

                if (cuttedpart.Item1 != RectInt.zero)
                {
                    root.Add(cuttedparts[it]);
                    cuttedparts.Add(cuttedpart.Item1);
                    cuttedparts.Add(cuttedpart.Item2);
                }

                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);

                it++;
            }

            child = new List<RectInt>(cuttedparts);
            foreach (RectInt alrdycut in root)
            {
                child.Remove(alrdycut);
            }

            for (int i = 0; i < child.Count; i++)
            {
                room.width = RandomService.Range(widthRange);
                room.height = RandomService.Range(heightRange);

                room.x = child[i].x + RandomService.Range(1, child[i].width - room.width);
                room.y = child[i].y + RandomService.Range(1, child[i].height - room.height);


                rooms.Add(room);
                BuildRoom(room);

                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
            }

            for (int i = cuttedparts.Count - 1; i > 1; i -= 2)
            {
                if (i - root.Count >= 0)
                {
                    BuildCorridor(rooms[i - root.Count], rooms[i - root.Count - 1]);
                }
                else
                {
                    BuildCorridor(rooms[i], rooms[i-1]);
                }


                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
            }

            BuildGround();
            // Final ground building.
        }

        private (RectInt, RectInt) Cut(RectInt cut, int forcecut)
        {
            RectInt cuttedpart1 = RectInt.zero;
            RectInt cuttedpart2 = RectInt.zero;

            int cutdirection = forcecut;
            if (forcecut == 2)
            {
                cutdirection = RandomService.Range(0, 2);
            }


            if (cutdirection == 0) // 0-hor 1-ver
            {

                cuttedpart1 = CutHorizontally(cut, -1);

                if (cuttedpart1 == RectInt.zero)
                {
                    return (cuttedpart1, new(-1, -1, -1, -1));
                }

                cuttedpart2 = CutHorizontally(cut, cuttedpart1.height);
            }
            else
            {

                cuttedpart1 = CutVertically(cut, -1);

                if (cuttedpart1 == RectInt.zero)
                {
                    return (cuttedpart1, new(-2, -2, -2, -2));
                }

                cuttedpart2 = CutVertically(cut, cuttedpart1.width);
            }

            return (cuttedpart1, cuttedpart2);
        }

        private RectInt CutHorizontally(RectInt cut, int lastcut)
        {
            RectInt cuttedpart = RectInt.zero;

            if (lastcut == -1)
            {
                int heightrndmrange = cut.height - (heightRange.y + 1) * 2;
                if (heightrndmrange <= 1)
                {
                    return cuttedpart;
                }
                int newheight = RandomService.Range(0, heightrndmrange);
                cuttedpart.height = newheight + heightRange.y;
                cuttedpart.width = cut.width;
                cuttedpart.position = cut.position;
            }
            else
            {
                cuttedpart.width = cut.width;
                cuttedpart.height = cut.height - lastcut;
                cuttedpart.y = cut.y + lastcut;
                cuttedpart.x = cut.x;
            }

            return cuttedpart;
        }

        private RectInt CutVertically(RectInt cut, int lastcut)
        {
            RectInt cuttedpart = RectInt.zero;

            if (lastcut == -1)
            {
                int widthrndmrange = cut.width - (widthRange.y + 1) * 2;

                if (widthrndmrange <= 1)
                {
                    return cuttedpart;
                }

                int newwidth = RandomService.Range(0, widthrndmrange);
                cuttedpart.width = newwidth + widthRange.y;
                cuttedpart.height = cut.height;
                cuttedpart.position = cut.position;
            }
            else
            {
                cuttedpart.width = cut.width - lastcut;
                cuttedpart.height = cut.height;
                cuttedpart.y = cut.y;
                cuttedpart.x = cut.x + lastcut;
            }

            return cuttedpart;
        }

    }
}