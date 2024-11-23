using System.Collections.Generic;
using UnityEngine;

public class SettlementHandler : MonoBehaviour
{
    public static SettlementHandler Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public List<Settlement> settlements = new List<Settlement>();

    public Settlement settlement = new Settlement();


    JSONDataHandler handler = new JSONDataHandler();


    void OnEnable()
    {
        settlement.OnPopulationChanged += HandlePopulationChanged;
        settlement.OnWealthChanged += HandleWealthChanged;
        settlement.OnQualityChanged += HandleQualityChanged;
        settlement.OnSettlementUpgraded += HandleSettlementUpgraded;
        settlement.OnTavernEntered += HandleTavernEntered;
        settlement.OnTownHallEntered += HandleTownHallEntered;
        settlement.OnWallEntered += HandleWallEntered;
        settlement.OnShopEntered += HandleShopEntered;
    }

    void OnDisable()
    {
        settlement.OnPopulationChanged -= HandlePopulationChanged;
        settlement.OnWealthChanged -= HandleWealthChanged;
        settlement.OnQualityChanged -= HandleQualityChanged;
        settlement.OnSettlementUpgraded -= HandleSettlementUpgraded;
        settlement.OnTavernEntered -= HandleTavernEntered;
        settlement.OnTownHallEntered -= HandleTownHallEntered;
        settlement.OnWallEntered -= HandleWallEntered;
        settlement.OnShopEntered -= HandleShopEntered;
    }

        void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            // Create a new town with residentials
            Town town = new Town
            {
                Name = "MyTown",
                Population = 5000,
                Wealth = 20000,
                Quality = 80,
                Shops = new List<Shops>
                {
                    new Shops { Name = "Blacksmith Shop", ShopType = ShopTypes.Blacksmith },
                    new Shops { Name = "Alchemist Shop", ShopType = ShopTypes.Alchemist }
                },
                Tavern = new Taverns
                {
                    Name = "Golden Inn",
                    quests = new List<Quest_SO_Constructor>()
                },
                TownHall = new TownHalls
                {
                    Name = "Central Town Hall",
                    jobs = new List<Job_SO_Constructor>()
                    // Initialize jobs
                },
                Walls = new Walls
                {
                    Name = "Stone Wall",
                    level = 2,
                    maxLevel = 5
                }
            };

            handler.SaveSettlements(new List<Settlement> { town });
        }
    
        if (Input.GetKeyDown(KeyCode.L))
        {
            settlements = handler.LoadSettlements();
            foreach (Settlement settlement in settlements)
            {
                Print($"Loaded settlement: {settlement.Name}");
            }
        }
    }


        //listener for the events
        void HandlePopulationChanged(int population)
        {
            Print($"Population changed to {population}");
        }

        void HandleWealthChanged(int wealth)
        {
            Print($"Wealth changed to {wealth}");
        }

        void HandleQualityChanged(int quality)
        {
            Print($"Quality changed to {quality}");
        }

        void HandleSettlementUpgraded()
        {
            Print("Settlement upgraded");
        }

        void HandleTavernEntered(Taverns tavern)
        {
            Print($"Entered {tavern.Name}");
        }

        void HandleTownHallEntered(TownHalls townHall)
        {
            Print($"Entered {townHall.Name}");
        }

        void HandleWallEntered(Walls wall)
        {
            Print($"Entered {wall.Name}");
        }

        void HandleShopEntered(Shops shop)
        {
            Print($"Entered {shop.Name}");
        }


        void Print(string message)
        {
            Debug.Log($"{message}\nSender:\"{this.GetType().Name}\" class in \"{this.gameObject.name}\"");
        }
    }