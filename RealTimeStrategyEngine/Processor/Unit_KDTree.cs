using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RealTimeStrategyEngine.Processor
{
    class Unit_KDTree
    {
        protected int SplitDimensionIndex;
        bool isLeafNode = false;
        protected float SplitPosition;
        protected Unit_KDTree LessEqualNode;
        protected Unit_KDTree GreaterNode;
        protected Unit[] Constituents;

        /// <summary>
        /// Organizes Units by X Y Z coordinates
        /// More efficient to recreate a tree rather than update it
        /// </summary>
        /// <param name="ParentSplitDimensionIndex">Outside of this constructor, inputs should be -1</param>
        /// <param name="UnitList">Should be converted from a List</param>
        public Unit_KDTree(int ParentSplitDimensionIndex, Unit[] UnitList)
        {
            SplitDimensionIndex = (ParentSplitDimensionIndex + 1) % 3;
            if (UnitList.Length > 2)
            {
                int MedianIndex = UnitList.Length / 2;
                PartialSortListAt(MedianIndex, SplitDimensionIndex, ref UnitList);
                SplitPosition = UnitList[MedianIndex].IndexablePosition[SplitDimensionIndex];
                Unit[] LowerList = new Unit[MedianIndex + 1];
                Unit[] UpperList = new Unit[UnitList.Length - MedianIndex - 1];
                Array.Copy(UnitList, 0, LowerList, 0, LowerList.Length);
                Array.Copy(UnitList, MedianIndex + 1, UpperList, 0, UpperList.Length);
                LessEqualNode = new Unit_KDTree(SplitDimensionIndex, LowerList);
                GreaterNode = new Unit_KDTree(SplitDimensionIndex, UpperList);
            }
            else
            {
                Constituents = UnitList;
                isLeafNode = true;
            }
        }

        public void InsertUnit(Unit Newbie)
        {
            if (isLeafNode)
            {
                Array.Resize<Unit>(ref Constituents, Constituents.Length + 1);
                Constituents[Constituents.Length - 1] = Newbie;
            }
            else
            {
                if (Newbie.IndexablePosition[SplitDimensionIndex] > SplitPosition)
                {
                    GreaterNode.InsertUnit(Newbie);
                }
                else
                {
                    LessEqualNode.InsertUnit(Newbie);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Searcher"></param>
        /// <param name="Closest">Can be a null input</param>
        /// <param name="Distance">Can be up to float.PositiveInfinity</param>
        public void FindNearestUnit(Unit Searcher, ref Unit Closest, ref float Distance)
        {
            if (isLeafNode)
            {
                for (int i = 0; i < Constituents.Length; i++)
                {
                    if (Constituents[i].HitPoints > 0.0)
                    {
                        float tempDistance = (Constituents[i].Position - Searcher.Position).Length();
                        if (tempDistance < Distance)
                        {
                            Closest = Constituents[i];
                            Distance = tempDistance;
                        }
                    }
                }
            }
            else
            {
                if (Searcher.IndexablePosition[SplitDimensionIndex] > SplitPosition)
                {
                    GreaterNode.FindNearestUnit(Searcher, ref Closest, ref Distance);
                    if (Searcher.IndexablePosition[SplitDimensionIndex] - Distance < SplitPosition)
                    {
                        LessEqualNode.FindNearestUnit(Searcher, ref Closest, ref Distance);
                    }
                }
                else
                {
                    LessEqualNode.FindNearestUnit(Searcher, ref Closest, ref Distance);
                    if (Searcher.IndexablePosition[SplitDimensionIndex] + Distance > SplitPosition)
                    {
                        GreaterNode.FindNearestUnit(Searcher, ref Closest, ref Distance);
                    }
                }
            }
        }

        /// <summary>
        /// (Obsolete) Performs a Merge Sort on the Unsorted list based on the DimensionIndex 
        /// </summary>
        private static void SortByDimension(ref Unit[] Unsorted, int DimensionIndex)
        {
            if (Unsorted.Length > 0)
            {
                List<List<Unit>> Sorter = new List<List<Unit>>();
                for (int i = 0; i < Unsorted.Length; i++)
                {
                    Sorter.Add(new List<Unit>());
                    Sorter[i].Add(Unsorted[i]);
                }
                while (Sorter.Count > 1)
                {
                    for (int i = 0; i < Sorter.Count - 1; i++)
                    {
                        for (int a = 0; a < Sorter[i].Count; a++)
                        {
                            for (int b = 0; b < Sorter[i + 1].Count; b++)
                            {
                                if (Sorter[i + 1][b].IndexablePosition[DimensionIndex] < Sorter[i][a].IndexablePosition[DimensionIndex])
                                {
                                    Sorter[i].Insert(a++, Sorter[i + 1][b]);
                                    Sorter[i + 1].RemoveAt(b--);
                                }
                            }
                        }
                        Sorter[i].AddRange(Sorter[i + 1]);
                        Sorter.RemoveAt(i + 1);
                    }
                }
                Unsorted = Sorter[0].ToArray();
            }
        }
        /// <summary>
        /// Performs a modified reflexive Quick Sort on the Unsorted list while looking for the specified Index
        /// A majority of the data should not be sorted in the process
        /// </summary>
        private static void PartialSortListAt(int Index, int DimensionIndex, ref Unit[] Unsorted)
        {
            if (Unsorted.Length > 3)
            {
                Unit[] PartialSorted = new Unit[Unsorted.Length];
                //Find an approximate median of the Unsorted data using the median of three method
                //Keep track of the Pivot's index since it must be ignored while sorting
                int[] MedianOfThreeIndices = new int[] { 
                    MyGame.random.Next(Unsorted.Length)
                    , MyGame.random.Next(Unsorted.Length)
                    , MyGame.random.Next(Unsorted.Length) };
                PartialSorted[0] = Unsorted[MedianOfThreeIndices[0]];
                PartialSorted[1] = Unsorted[MedianOfThreeIndices[1]];
                PartialSorted[2] = Unsorted[MedianOfThreeIndices[2]];
                Unit Pivot = Unsorted[MyGame.random.Next(Unsorted.Length)];
                int PivotIndex;
                if (PartialSorted[0].IndexablePosition[DimensionIndex] <= PartialSorted[1].IndexablePosition[DimensionIndex]
                    && PartialSorted[0].IndexablePosition[DimensionIndex] > PartialSorted[2].IndexablePosition[DimensionIndex])
                {
                    Pivot = PartialSorted[0];
                    PivotIndex = MedianOfThreeIndices[0];
                }
                else if (PartialSorted[1].IndexablePosition[DimensionIndex] <= PartialSorted[0].IndexablePosition[DimensionIndex]
                    && PartialSorted[1].IndexablePosition[DimensionIndex] > PartialSorted[2].IndexablePosition[DimensionIndex])
                {
                    Pivot = PartialSorted[1];
                    PivotIndex = MedianOfThreeIndices[1];
                }
                else
                {
                    Pivot = PartialSorted[2];
                    PivotIndex = MedianOfThreeIndices[2];
                }
                //Sort the Pivot into its correct location, ignoring the Pivot itself
                int StartCounter = 0;
                int EndCounter = Unsorted.Length - 1;
                for (int i = 0; i < PivotIndex; i++)
                {
                    if (Unsorted[i].IndexablePosition[DimensionIndex] > Pivot.IndexablePosition[DimensionIndex])
                    {
                        PartialSorted[EndCounter--] = Unsorted[i];
                    }
                    else
                    {
                        PartialSorted[StartCounter++] = Unsorted[i];
                    }
                }
                for (int i = PivotIndex + 1; i < Unsorted.Length; i++)
                {
                    if (Unsorted[i].IndexablePosition[DimensionIndex] > Pivot.IndexablePosition[DimensionIndex])
                    {
                        PartialSorted[EndCounter--] = Unsorted[i];
                    }
                    else
                    {
                        PartialSorted[StartCounter++] = Unsorted[i];
                    }
                }
                //Check to see if the code is working properly
                if (StartCounter != EndCounter)
                {
                    throw new Exception("Index preceding the pivot is not equal to the index after the pivot");
                }
                else
                {
                    PartialSorted[StartCounter] = Pivot;
                }
                Unsorted = PartialSorted;
                //Grab the a section of the array for further sorting to find the specified Index
                if (StartCounter == Index)
                {
                    return;
                }
                else if (Index > StartCounter)
                {
                    PartialSorted = new Unit[Unsorted.Length - StartCounter - 1];
                    Array.Copy(Unsorted, StartCounter + 1, PartialSorted, 0, PartialSorted.Length);
                    PartialSortListAt(Index - StartCounter - 1, DimensionIndex, ref PartialSorted);
                    Array.Copy(PartialSorted, 0, Unsorted, StartCounter + 1, PartialSorted.Length);
                }
                else
                {
                    PartialSorted = new Unit[StartCounter];
                    Array.Copy(Unsorted, 0, PartialSorted, 0, PartialSorted.Length);
                    PartialSortListAt(Index, DimensionIndex, ref PartialSorted);
                    Array.Copy(PartialSorted, 0, Unsorted, 0, PartialSorted.Length);
                }
            }
            else
            {
                //Merge sort the ones that are too small
                SortByDimension(ref Unsorted, DimensionIndex);
            }
        }
    }
}
