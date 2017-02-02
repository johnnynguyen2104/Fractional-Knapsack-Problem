using System;
using System.Collections.Generic;
using System.Linq;

namespace TitanSoft
{
    class Program
    {
        static void Main(string[] args)
        {
            //testcase 1  "1 350 550\n2 200 300\n3 150 150\n4 100 100\n5 200 300\n6 100 200"
            //testcase 2 "1 400 200\n2 500 300\n3 500 300\n4 200 200\n5 400 200\n6 950 800"
            string data = "1 400 200\n2 400 200\n3 500 300\n4 200 200\n5 500 300"; 
            WareHouse<Item> warehouse = new WareHouse<Item>() {
                Modules = new List<Module>()
                {
                    new Module() { ModuleName = "A" },
                    new Module() { ModuleName = "B" },
                    new Module() { ModuleName = "C" } 
                }
            };

            warehouse.AddItem(data);
            warehouse.DisplayOptimalCombination();

            Console.ReadLine();
        }


    }

    #region Interfaces

    interface IPallet<TItemKey> where TItemKey : struct
    {
        TItemKey[] GetItemIdList();
    }

    interface IItem
    {
        int RunningId { get; set; }

        int Width { get; set; } //as millimeters

        int Length { get; } //as millimeters

        int Weight { get; set; } //as Kilogram

        double OptimalDensityWidth { get; }

        double OptimalDensityWeight { get; }

        void MappingData(string[] dataArray);
    }
    #endregion

    #region Classes



    class WareHouse<TItem> where TItem : IItem, new()
    {
        private List<Module> modules = new List<Module>();

        public List<Module> Modules
        {
            get { return modules.OrderBy(a => a.ModuleName).ToList(); }
            set { modules = value; }
        }

        private List<TItem> Items { get; set; } = new List<TItem>();

        private readonly Dictionary<int, string> ErrorMessages = new Dictionary<int, string>();

        public WareHouse()
        {
            ErrorMessages.Add(1, "Warehouse have no Module for insert data, please add module.");
            ErrorMessages.Add(2, "Duplicated RunningId, please input valid data.");
            ErrorMessages.Add(3, "Not enought module for pallet. Please add more module to the warehouse.");
        }
        

        private void FormatInputData(string userInput)
        {
            if (!string.IsNullOrEmpty(userInput))
            {
                if (Modules == null || Modules.Count == 0)
                {
                    throw new FormatException(ErrorMessages[1]);
                }

                ProcessFormat(userInput);
            }
        }

        private void ProcessFormat(string userInput)
        {
            var lines = userInput.Trim().Split(new[] { '\r', '\n' });

            if (lines != null && lines.Length > 0)
            {

                foreach (var item in lines)
                {
                    var dataArray = item.Trim().Split(null);

                    if (dataArray != null && dataArray.Length > 0)
                    {
                        TItem data = new TItem();
                        data.MappingData(dataArray);

                        if (!Items.Any(a => a.RunningId == data.RunningId))
                        {
                            Items.Add(data);
                        }
                        else
                        {
                            throw new Exception(ErrorMessages[2]);
                        }
                    }
                }
            }
        }

        public void AddItem(string userInput)
        {
            FormatInputData(userInput);
        }

        private List<Pallet> OptimalCombination()
        {
            List<Pallet> pallets = new List<Pallet>();
            List<TItem> data;

            while ((data = Items.Where( b => !pallets.SelectMany(c => c.Items)
                                                    .Any(d => d.RunningId == b.RunningId))
                                                    .OrderByDescending(b => b.OptimalDensityWeight)
                                                    .ToList()).Count > 0)
            {
                pallets.Add(ProcessPallet(data));
            }

            return pallets;
        }

        private Pallet ProcessPallet(List<TItem> data)
        {
            Pallet pal = new Pallet();

            foreach (var item in data)
            {
                int calculationWidth = item.Width + pal.TotalWidth,
                    calculationWeight = item.Weight + pal.TotalWeight;

                if (calculationWidth <= pal.Width 
                    && calculationWeight <= pal.Weight)
                {
                    pal.Items.Add(item);
                }
            }

            return pal;
        }

        public void DisplayOptimalCombination()
        {
            var result = OptimalCombination();

            //validate module's space for pallet
            if (Modules.Count < result.Count)
            {
                throw new Exception(ErrorMessages[3]);
            }

            foreach (var module in Modules)
            {
                var priorityPallet = result.OrderByDescending(a => a.TotalWidth)
                                            .ThenByDescending(a => a.TotalWeight)
                                            .ThenByDescending(a => a.NumberOfItems)
                                            .ThenBy(a => a.Items.Min(b => b.RunningId))
                                            .FirstOrDefault();
                //remove it from result 
                result.Remove(priorityPallet);

                Console.WriteLine(string.Format("{0}: {1}", module.ModuleName, string.Join(",", priorityPallet == null ? new int[] { } : priorityPallet.GetItemIdList())));

            }
        }
    }

    class Module
    {
        public string ModuleName { get; set; }
    }

    class Pallet : IPallet<int>
    {
        public int Width { get; } = 1100; //as millimeters

        public int Length { get; } = 1100; //as millimeters

        public int Weight { get; } = 1000; //as Kilogram

        public int NumberOfItems { get { return Items.Count; } }

        public List<IItem> Items { get; set; } = new List<IItem>();

        public int TotalWidth { get { return Items.Sum(a => a.Width); } }

        public int TotalWeight { get { return Items.Sum(a => a.Weight); } }

        public int[] GetItemIdList()
        {
            if (Items != null && Items.Count > 0)
            {
                return Items.OrderByDescending(a => a.Width)
                            .ThenByDescending(a => a.Weight)
                            .ThenBy(a => a.RunningId)        
                            .Select(a => a.RunningId).ToArray();
            }

            return null;
        }
    }

    class Item : IItem
    {

        private readonly Dictionary<int, string> ErrorMessages = new Dictionary<int, string>();

        public Item()
        {
            ErrorMessages.Add(1, "RunningId can't be smaller than 1 or we have something wrong in the input pattern.");
            ErrorMessages.Add(2, "Width can't be smaller than 1 or we have something wrong in the input pattern.");
            ErrorMessages.Add(3, "Weight can't be smaller than 1 or we have something wrong in the input pattern.");
        }

        private int runningId { get; set; }

        public int width { get; set; }

        public int weight { get; set; }

        public int RunningId
        {
            get { return runningId; }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentException(ErrorMessages[1]);
                }

                runningId = value;
            }
        }

        public int Width
        {
            get { return width; }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentException(ErrorMessages[2]);
                }

                width = value;
            }
        } //as millimeters

        public int Length { get; } = 1100; //as millimeters

        public int Weight
        {
            get { return weight; }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentException(ErrorMessages[3]);
                }

                weight = value;
            }
        } //as Kilogram

        public double OptimalDensityWidth
        {
            get
            {
                return ((double)Width / Weight);
            }
        }

        public double OptimalDensityWeight
        {
            get
            {
                return ((double)Weight / Width) * 0.1;
            }
        }

        public void MappingData(string[] dataArray)
        {
            if (dataArray != null && dataArray.Length > 0)
            {
                //for Id
                int id = 0;
                int.TryParse(dataArray[0], out id);
                RunningId = id;

                if (dataArray.Length > 1)
                {
                    int width = 0;
                    int.TryParse(dataArray[1], out width);
                    Width = width;
                }

                if (dataArray.Length > 2)
                {
                    int weight = 0;
                    int.TryParse(dataArray[2], out weight);
                    Weight = weight;
                }

            }
        }
    }
    #endregion
}
