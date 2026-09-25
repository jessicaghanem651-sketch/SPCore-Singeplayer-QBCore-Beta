using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace SPCore
{
    public sealed class Main : Script
    {
        private readonly InventoryStore store = new InventoryStore();
        private InventoryConfig cfg;
        private LootManager loot;
        private ShopManager shops;
        private InventoryUI ui;
        private WeedManager weed;
        private TrapManager trap;
        private SurvivalNeeds needs;
        private DateTime nextSync = DateTime.MinValue;

        public Main()
        {
            Tick += OnTick;
            KeyDown += OnKeyDown;
            Aborted += OnAborted;

            string root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts");
            Directory.CreateDirectory(root);
            string configPath = Path.Combine(root, "inventory_config.json");
            cfg = Config.Load(configPath);
            store.Initialize(root, cfg);
            loot = new LootManager(cfg, store);
            shops = new ShopManager(cfg, store, loot);
            weed = new WeedManager(store, shops, loot);
            trap = new TrapManager(store, loot, shops);
            needs = new SurvivalNeeds(store);
            ui = new InventoryUI(cfg, store, loot, shops);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (ui.Open) {
                if (ui.HandleAmountKey(e.KeyCode)) return;
                if (e.KeyCode == Keys.R) ui.RotateHeld();
                else if (e.KeyCode == Keys.E) { if (Game.Player.Character==null || !shops.IsNearBlockedShop(Game.Player.Character.Position)) ui.QuickTakeNearest(); }
                else if (e.KeyCode == Keys.U) ui.ToggleGasUnderCounter();
                else if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.I) ui.Close();
                return;
            }
            if (e.KeyCode == Keys.I) { ui.Toggle(); return; }
            if (e.KeyCode == Keys.E && Game.Player.Character!=null && Game.Player.Character.Exists())
            {
                var pos=Game.Player.Character.Position;
                if (trap.TryInteract(Game.Player.Character)) return;
                if (shops.IsNearBank(pos)) { ui.OpenBankApp(); return; }
                if (weed.TryInteract(Game.Player.Character)) return;
                if (shops.IsNearDealer(pos) || shops.IsNearWeedDealer(pos) || shops.IsNearCityHall(pos) || shops.GetNearbyType(pos)==ShopType.GasStation || shops.GetNearbyType(pos)==ShopType.Ammu)
                { ui.Toggle(); return; }
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            loot.Tick();
            DrugEffects.Tick(); shops.Tick(); weed.Tick(); trap.Tick(); needs.Tick(); shops.DrawWorldPrompts(ui.Open);
            var player=Game.Player.Character; if(player!=null&&player.Exists()){ if(shops.IsNearBlockedShop(player.Position)) Function.Call(Hash.DISABLE_CONTROL_ACTION,0,51,true); }
            if (DateTime.UtcNow >= nextSync)
            {
                store.SyncFromGame();
                nextSync = DateTime.UtcNow.AddMilliseconds(250);
            }
            if (ui.Open) ui.Draw();
            else loot.DrawNearbyPrompt();
            needs.DrawHud();
            weed.Draw();
            trap.Draw();
            Notifications.Draw();
        }

        private void OnAborted(object sender, EventArgs e)
        {
            store.Save();
            ui.Dispose(); shops.Dispose(); trap.Dispose();
        }
    }

    public sealed class InventoryConfig
    {
        public int PlayerColumns = 5;
        public int PlayerRows = 5;
        public int VehicleColumns = 5;
        public int VehicleRows = 2;
        public int NearbyColumns = 5;
        public int NearbyRows = 2;
        public float LootRadius = 1.524f;
        public int RareVehicleChancePercent = 10;
        public int EmptyVehicleChancePercent = 35;
        public int DeadPedWeaponChancePercent = 100;
        public int DeadPedMoneyChancePercent = 55;
        public int DeadPedMiscChancePercent = 35;
        public int SaveIntervalSeconds = 10;
        public int BankDepositLimit = 50000;
        public List<ShopItem> Ammu = new List<ShopItem>();
        public List<ShopItem> GasStation = new List<ShopItem>();
        public List<ShopItem> Dealer = new List<ShopItem>();
        public List<ShopItem> GasStationUnderCounter = new List<ShopItem>();
        public List<ShopItem> CityHall = new List<ShopItem>();
        public List<ShopItem> WeedDealer = new List<ShopItem>();
        public List<LootDefinition> NormalVehicleLoot = new List<LootDefinition>();
        public List<LootDefinition> RareVehicleLoot = new List<LootDefinition>();
        public List<LootDefinition> PedLoot = new List<LootDefinition>();

        public static InventoryConfig Default()
        {
            var c = new InventoryConfig();
            c.Ammu.AddRange(new[] {
                new ShopItem("pistol",25000), new ShopItem("combat_pistol",30000),
                new ShopItem("machine_pistol",35000), new ShopItem("smg",45000),
                new ShopItem("pump_shotgun",50000), new ShopItem("carbine_rifle",65000),
                new ShopItem("assault_rifle",75000), new ShopItem("heavy_pistol",40000),
                new ShopItem("knife",8000), new ShopItem("armor",12000),
                new ShopItem("medkit",1500), new ShopItem("pistol_ammo",50),
                new ShopItem("pistol_ammo",50), new ShopItem("shotgun_ammo",50),
                new ShopItem("rifle_ammo",50), new ShopItem("mg_ammo",50), new ShopItem("snp_ammo",50),
                new ShopItem("suppressor",5000)
            });
            c.GasStation.AddRange(new[] {
                new ShopItem("water_bottle",60), new ShopItem("sandwich",90),
                new ShopItem("burger",140), new ShopItem("chips_bag",85),
                new ShopItem("chocolate_bar",75), new ShopItem("energy_drink",120),
                new ShopItem("coffee",80), new ShopItem("donut",70),
                new ShopItem("firstaid",500), new ShopItem("bandage",120), new ShopItem("repairkit",1500),
                new ShopItem("backpack",2500), new ShopItem("lockpick",300), new ShopItem("food",100),
                new ShopItem("cigarette_pack",180), new ShopItem("wrench",450), new ShopItem("knife",1200),
                new ShopItem("flashlight",600), new ShopItem("lighter",40), new ShopItem("jerry_can",1200), new ShopItem("whiskey",180), new ShopItem("wine",220), new ShopItem("oxy",6500)
            });
            c.CityHall.Add(new ShopItem("id_card",300));
            c.WeedDealer.AddRange(new[] { new ShopItem("joint",100), new ShopItem("weed_seed",350), new ShopItem("baggie",3) });
            c.GasStationUnderCounter.AddRange(new[] {
                new ShopItem("joint",1800), new ShopItem("weed_bud",2500), new ShopItem("cokebaggy",6500),
                new ShopItem("crack_baggy",5500), new ShopItem("meth_bag",8000), new ShopItem("xtcbaggy",7000),
                new ShopItem("oxy",5000), new ShopItem("morphine",6000)
            });
            c.NormalVehicleLoot.AddRange(new[] {
                new LootDefinition("cash",20,250,42), new LootDefinition("knife",1,1,10),
                new LootDefinition("medkit",1,1,8), new LootDefinition("armor",1,1,4),
                new LootDefinition("pistol",1,1,4), new LootDefinition("pistol_ammo",8,30,12),
                new LootDefinition("water_bottle",1,2,10), new LootDefinition("sandwich",1,2,10)
            });
            c.RareVehicleLoot.AddRange(new[] {
                new LootDefinition("cash",250,1500,25), new LootDefinition("pistol",1,1,8),
                new LootDefinition("smg",1,1,6), new LootDefinition("carbine_rifle",1,1,3),
                new LootDefinition("armor",1,1,8), new LootDefinition("medkit",1,2,10),
                new LootDefinition("rifle_ammo",20,90,12), new LootDefinition("coke_small_brick",1,1,2)
            });
            c.PedLoot.AddRange(new[] {
                new LootDefinition("cash",5,180,50), new LootDefinition("medkit",1,1,7),
                new LootDefinition("armor",1,1,2), new LootDefinition("knife",1,1,8),
                new LootDefinition("cigarette_pack",1,1,8), new LootDefinition("phone",1,1,6),
                new LootDefinition("water_bottle",1,1,8), new LootDefinition("sandwich",1,1,8)
            });
            return c;
        }
    }

    public sealed class ShopItem
    {
        public string Id;
        public int Price;
        public ShopItem() { }
        public ShopItem(string id, int price) { Id = id; Price = price; }
    }

    public sealed class LootDefinition
    {
        public string Id;
        public int Min;
        public int Max;
        public int Weight;
        public LootDefinition() { }
        public LootDefinition(string id, int min, int max, int weight) { Id=id;Min=min;Max=max;Weight=weight; }
    }

    public static class Config
    {
        public static void Save(string path, InventoryConfig config)
        {
            try { File.WriteAllText(path, new JavaScriptSerializer().Serialize(config)); } catch { }
        }
        public static InventoryConfig Load(string path)
        {
            try
            {
                var defaults = InventoryConfig.Default();
                if (!File.Exists(path))
                {
                    File.WriteAllText(path, new JavaScriptSerializer().Serialize(defaults));
                    return defaults;
                }
                var c = new JavaScriptSerializer().Deserialize<InventoryConfig>(File.ReadAllText(path));
                if (c == null) return defaults;
                // Older builds used a single "Shop" array. Keep loading resilient so an old config
                // cannot silently make every shop empty.
                if (c.Ammu == null || c.Ammu.Count == 0) c.Ammu = defaults.Ammu;
                if (c.GasStation == null || c.GasStation.Count == 0) c.GasStation = defaults.GasStation;
                if (c.Dealer == null || c.Dealer.Count == 0) c.Dealer = defaults.Dealer;
                if (c.GasStationUnderCounter == null || c.GasStationUnderCounter.Count == 0) c.GasStationUnderCounter = defaults.GasStationUnderCounter;
                if (c.CityHall == null || c.CityHall.Count == 0) c.CityHall = defaults.CityHall;
                if (c.WeedDealer == null || c.WeedDealer.Count == 0) c.WeedDealer = defaults.WeedDealer;
                if (c.NormalVehicleLoot == null || c.NormalVehicleLoot.Count == 0) c.NormalVehicleLoot = defaults.NormalVehicleLoot;
                if (c.RareVehicleLoot == null || c.RareVehicleLoot.Count == 0) c.RareVehicleLoot = defaults.RareVehicleLoot;
                if (c.PedLoot == null || c.PedLoot.Count == 0) c.PedLoot = defaults.PedLoot;
                EnsureStock(c.GasStation, defaults.GasStation);
                EnsureStock(c.GasStationUnderCounter, defaults.GasStationUnderCounter);
                EnsureStock(c.Ammu, defaults.Ammu);
                c.Ammu.RemoveAll(x=>x!=null&&string.Equals(x.Id,"smg_ammo",StringComparison.OrdinalIgnoreCase));
                EnsureStock(c.CityHall, defaults.CityHall);
                EnsureStock(c.WeedDealer, defaults.WeedDealer);
                if (c.PlayerColumns != 5) c.PlayerColumns = defaults.PlayerColumns;
                if (c.PlayerRows != 5) c.PlayerRows = defaults.PlayerRows;
                if (c.VehicleColumns != 5) c.VehicleColumns = defaults.VehicleColumns;
                if (c.VehicleRows != 2) c.VehicleRows = defaults.VehicleRows;
                if (c.NearbyColumns != 5) c.NearbyColumns = defaults.NearbyColumns;
                if (c.NearbyRows != 2) c.NearbyRows = defaults.NearbyRows;
                return c;
            }
            catch { return InventoryConfig.Default(); }
        }
        private static void EnsureStock(List<ShopItem> target, List<ShopItem> defaults)
        {
            if(target==null)return;
            foreach(var d in defaults) if(!target.Any(x=>x!=null&&string.Equals(x.Id,d.Id,StringComparison.OrdinalIgnoreCase))) target.Add(new ShopItem(d.Id,d.Price));
        }
    }

    public sealed class Item
    {
        public string Id;
        public string Name;
        public int Amount;
        public int W;
        public int H;
        public bool Rotated;
        public string Weapon;
        public string AmmoType;
        public string Icon;
        public bool SyncedWeapon;
        public bool SyncedAmmo;
        public bool Usable;

        public int Width { get { return Rotated ? H : W; } }
        public int Height { get { return Rotated ? W : H; } }
        public Item Clone() { return (Item)MemberwiseClone(); }
    }

    public static class ItemCatalog
    {
        private static readonly Dictionary<string,Item> defs = new Dictionary<string,Item>(StringComparer.OrdinalIgnoreCase);
        static ItemCatalog()
        {
            Add("cash","DIRTY MONEY",1,1,"cash.png",usable:false);
            Add("armor","ARMOR",2,2,"armor.png",usable:true);
            Add("medkit","MEDKIT",2,2,"medkit.png",usable:true);
            Add("firstaid","FIRST AID",1,2,"firstaid.png",usable:true);
            Add("bandage","BANDAGE",1,1,"bandage.png",usable:true);
            Add("repairkit","VEHICLE REPAIR KIT",2,2,"repairkit.png",usable:true);
            Add("id_card","Weapon License",1,1,"id_card.png",usable:false);
            Add("lockpick","LOCKPICK",1,1,"lockpick.png",usable:true);
            Add("food","FOOD",1,1,"burger.png",usable:true);
            Add("wrench","WRENCH",1,2,"wrench.png",usable:true);
            Add("backpack","BACKPACK",2,2,"backpack.png",usable:true);
            Add("water_bottle","WATER BOTTLE",1,1,"water_bottle.png",usable:true);
            Add("sandwich","SANDWICH",1,1,"sandwich.png",usable:true);
            Add("burger","BURGER",2,1,"burger.png",usable:true);
            Add("chips_bag","CHIPS",1,2,"chips_bag.png",usable:true);
            Add("chocolate_bar","CHOCOLATE",1,1,"chocolate_bar.png",usable:true);
            Add("energy_drink","ENERGY DRINK",1,1,"energy_drink.png",usable:true);
            Add("coffee","COFFEE",1,1,"coffee.png",usable:true);
            Add("donut","DONUT",1,1,"donut.png",usable:true);
            Add("lighter","LIGHTER",1,1,"lighter.png",usable:true);
            Add("jerry_can","JERRY CAN",1,1,"jerry_can.png",usable:true);
            Add("whiskey","WHISKEY",1,1,"whiskey.png",usable:true);
            Add("wine","WINE",1,1,"wine.png",usable:true);
            Add("suppressor","SUPPRESSOR",1,1,"suppressor_attachment.png",usable:true);
            Add("flashlight","FLASHLIGHT",1,2,"flashlight.png",usable:true);
            Add("joint","JOINT",1,1,"joint.png",usable:true);
            Add("weed_seed","WEED SEED",1,1,"weed_seed.png",usable:true);
            Add("weed_bud","WEED BUD",1,1,"weed_bud.png",usable:false);
            Add("baggie","BAGGIES",1,1,"empty_weed_bag.png",usable:true);
            Add("weed_brick","BAGGED BUD",1,1,"weed_brick.png",usable:true);
            Add("cokebaggy","COCAINE",1,1,"cokebaggy.png",usable:true);
            Add("coke_small_brick","COCAINE BRICK",2,2,"coke_small_brick.png",usable:false);
            Add("crack_baggy","CRACK",1,1,"crack_baggy.png",usable:true);
            Add("meth_bag","METH BAGGIE",1,1,"meth_bag.png",usable:true);
            Add("xtcbaggy","ECSTASY",1,1,"xtcbaggy.png",usable:true);
            Add("oxy","OXY",1,1,"oxy.png",usable:true);
            Add("morphine","MORPHINE",1,1,"morphine.png",usable:true);
            Add("cigarette_pack","CIGARETTES",1,1,"cigarette_pack.png",usable:true);
            Add("phone","PHONE",1,2,"phone.png",usable:true);
            Add("pistol_ammo","9x19mm",1,1,"pistol_ammo.png",ammo:"AMMO_PISTOL");
            Add("smg_ammo","9x19mm",1,1,"smg_ammo.png",ammo:"AMMO_PISTOL");
            Add("shotgun_ammo","SHOTGUN SHELLS",1,1,"shotgun_ammo.png",ammo:"AMMO_SHOTGUN");
            Add("rifle_ammo","7.62x39mm",1,1,"rifle_ammo.png",ammo:"AMMO_RIFLE");
            Add("mg_ammo","7.62x51mm NATO",1,1,"mg_ammo.png",ammo:"AMMO_MG");
            Add("snp_ammo","7.62mm SNIPER",1,1,"snp_ammo.png",ammo:"AMMO_SNIPER");

            Weapon("pistol","Glock 17","WEAPON_PISTOL","AMMO_PISTOL",2,2,"WEAPON_PISTOL.png");
            Weapon("pistol_mk2","PISTOL MK II","WEAPON_PISTOL_MK2","AMMO_PISTOL",2,2,"WEAPON_PISTOL_MK2.png");
            Weapon("combat_pistol","Glock 41","WEAPON_COMBATPISTOL","AMMO_PISTOL",2,2,"WEAPON_COMBATPISTOL.png");
            Weapon("heavy_pistol","Glock 20","WEAPON_HEAVYPISTOL","AMMO_PISTOL",2,2,"WEAPON_HEAVYPISTOL.png");
            Weapon("machine_pistol","MACHINE PISTOL","WEAPON_MACHINEPISTOL","AMMO_PISTOL",2,2,"WEAPON_MACHINEPISTOL.png");
            Weapon("smg","SMG","WEAPON_SMG","AMMO_PISTOL",3,2,"WEAPON_SMG.png");
            Weapon("mini_smg","MINI SMG","WEAPON_MINISMG","AMMO_PISTOL",2,2,"WEAPON_MINISMG.png");
            Weapon("assault_smg","ASSAULT SMG","WEAPON_ASSAULTSMG","AMMO_PISTOL",3,2,"WEAPON_ASSAULTSMG.png");
            Weapon("pump_shotgun","PUMP SHOTGUN","WEAPON_PUMPSHOTGUN","AMMO_SHOTGUN",3,2,"WEAPON_PUMPSHOTGUN.png");
            Weapon("sawn_off","SAWED-OFF SHOTGUN","WEAPON_SAWNOFFSHOTGUN","AMMO_SHOTGUN",3,2,"WEAPON_SAWNOFFSHOTGUN.png");
            Weapon("assault_shotgun","ASSAULT SHOTGUN","WEAPON_ASSAULTSHOTGUN","AMMO_SHOTGUN",3,2,"WEAPON_ASSAULTSHOTGUN.png");
            Weapon("carbine_rifle","M4A1","WEAPON_CARBINERIFLE","AMMO_RIFLE",4,2,"WEAPON_CARBINERIFLE.png");
            Weapon("assault_rifle","AR Pistol","WEAPON_ASSAULTRIFLE","AMMO_RIFLE",4,2,"WEAPON_ASSAULTRIFLE.png");
            Weapon("advanced_rifle","ADVANCED RIFLE","WEAPON_ADVANCEDRIFLE","AMMO_RIFLE",4,2,"WEAPON_ADVANCEDRIFLE.png");
            Weapon("bullpup_rifle","BULLPUP RIFLE","WEAPON_BULLPUPRIFLE","AMMO_RIFLE",4,2,"WEAPON_BULLPUPRIFLE.png");
            Weapon("combat_mg","COMBAT MG","WEAPON_COMBATMG","AMMO_MG",4,2,"WEAPON_COMBATMG.png");
            Weapon("mg","MG","WEAPON_MG","AMMO_MG",4,2,"WEAPON_MG.png");
            Weapon("sniper_rifle","SNIPER RIFLE","WEAPON_SNIPERRIFLE","AMMO_SNIPER",4,2,"WEAPON_SNIPERRIFLE.png");
            Weapon("heavy_sniper","HEAVY SNIPER","WEAPON_HEAVYSNIPER","AMMO_SNIPER",4,2,"WEAPON_HEAVYSNIPER.png");
            Weapon("knife","SWITCHBLADE","WEAPON_KNIFE",null,1,2,"WEAPON_KNIFE.png");
            Weapon("switchblade","SWITCHBLADE","WEAPON_SWITCHBLADE",null,1,2,"WEAPON_SWITCHBLADE.png");
            Weapon("bat","BAT","WEAPON_BAT",null,1,3,"WEAPON_BAT.png");
            Weapon("crowbar","CROWBAR","WEAPON_CROWBAR",null,1,3,"WEAPON_CROWBAR.png");
            Weapon("machete","MACHETE","WEAPON_MACHETE",null,1,3,"WEAPON_MACHETE.png");
            Weapon("nightstick","NIGHTSTICK","WEAPON_NIGHTSTICK",null,1,3,"WEAPON_NIGHTSTICK.png");
            Weapon("stungun","TASER","WEAPON_STUNGUN",null,2,2,"WEAPON_STUNGUN.png");
            Weapon("molotov","MOLOTOV","WEAPON_MOLOTOV",null,1,2,"WEAPON_MOLOTOV.png");
            Weapon("grenade","GRENADE","WEAPON_GRENADE",null,1,1,"WEAPON_GRENADE.png");
            Weapon("smoke_grenade","SMOKE GRENADE","WEAPON_SMOKEGRENADE",null,1,1,"WEAPON_SMOKEGRENADE.png");
            Weapon("firework","FIREWORK","WEAPON_FIREWORK",null,1,2,"WEAPON_FIREWORK.png");
            Weapon("rpg","RPG","WEAPON_RPG","AMMO_RPG",3,2,"WEAPON_RPG.png");
        }

        private static void Add(string id,string name,int w,int h,string icon,string weapon=null,string ammo=null,bool usable=false)
        {
            // SPCore uses a uniform one-slot inventory: every item occupies exactly one cell.
            defs[id]=new Item{Id=id,Name=name,W=1,H=1,Icon=icon,Weapon=weapon,AmmoType=ammo,Amount=1,Usable=usable};
        }
        private static void Weapon(string id,string name,string weapon,string ammo,int w,int h,string icon)
        { Add(id,name,w,h,icon,weapon,ammo,false); }
        public static Item Create(string id,int amount=1)
        { Item d; if(!defs.TryGetValue(id,out d)) return null; var x=d.Clone(); x.Amount=amount; return x; }
        public static IEnumerable<Item> All(){return defs.Values;}
        public static Item FindByWeapon(string weapon){return defs.Values.FirstOrDefault(x=>string.Equals(x.Weapon,weapon,StringComparison.OrdinalIgnoreCase));}
        public static string AmmoItemFor(string ammo){var x=defs.Values.FirstOrDefault(i=>i.AmmoType==ammo);return x==null?null:x.Id;}
    }

    public static class AmmoBridge
    {
        public static void AddToPed(Item item)
        {
            if(item==null||item.AmmoType==null||Game.Player.Character==null||!Game.Player.Character.Exists())return;
            var weapon=ItemCatalog.All().FirstOrDefault(x=>x.Weapon!=null&&x.AmmoType==item.AmmoType&&Function.Call<bool>(Hash.HAS_PED_GOT_WEAPON,Game.Player.Character.Handle,unchecked((int)Game.GenerateHash(x.Weapon)),false));
            if(weapon!=null)Function.Call(Hash.ADD_AMMO_TO_PED,Game.Player.Character.Handle,unchecked((int)Game.GenerateHash(weapon.Weapon)),Math.Max(0,item.Amount));
        }
    }

    public sealed class InventoryGrid
    {
        public int Columns { get; private set; }
        public int Rows { get; private set; }
        public readonly List<Item> Items = new List<Item>();
        public InventoryGrid(int columns,int rows){Columns=columns;Rows=rows;}
        public bool CanPlace(Item item,int x,int y,Item ignore=null)
        {
            if(item==null || x<0 || y<0 || x+item.Width>Columns || y+item.Height>Rows) return false;
            foreach(var other in Items)
            {
                if(other==ignore) continue;
                Point p=ItemPositions.Get(other);
                if(Overlap(x,y,item.Width,item.Height,p.X,p.Y,other.Width,other.Height)) return false;
            }
            return true;
        }
        private static bool Overlap(int ax,int ay,int aw,int ah,int bx,int by,int bw,int bh)
        { return ax<bx+bw && ax+aw>bx && ay<by+bh && ay+ah>by; }
        public bool Add(Item item)
        {
            if(item==null) return false;
            bool r=item.Rotated;
            for(int y=0;y<Rows;y++) for(int x=0;x<Columns;x++)
            {
                item.Rotated=false;
                if(CanPlace(item,x,y)){Set(item,x,y);Items.Add(item);return true;}
                item.Rotated=true;
                if(CanPlace(item,x,y)){Set(item,x,y);Items.Add(item);return true;}
            }
            item.Rotated=r; return false;
        }
        public bool Move(Item item,int x,int y){if(!CanPlace(item,x,y,item))return false;Set(item,x,y);return true;}
        public void Set(Item item,int x,int y){ItemPositions.Set(item,x,y);}
        public void Remove(Item item){if(item!=null){Items.Remove(item);ItemPositions.Remove(item);}}
        public void Clear(){foreach(var i in Items)ItemPositions.Remove(i);Items.Clear();}
        public void Expand(int columns, int rows) { Columns = Math.Max(Columns, columns); Rows = Math.Max(Rows, rows); }
    }

    public static class ItemPositions
    {
        private static readonly Dictionary<Item,Point> map=new Dictionary<Item,Point>();
        public static void Set(Item i,int x,int y){map[i]=new Point(x,y);}
        public static Point Get(Item i){Point p;return map.TryGetValue(i,out p)?p:new Point(-1,-1);}
        public static void Remove(Item i){map.Remove(i);}
    }

    public sealed class InventoryStore
    {
        public InventoryGrid Player;
        public InventoryGrid Backpack;
        public int BankBalance { get; private set; }
        public float Hunger { get; set; } = 100f;
        public float Thirst { get; set; } = 100f;
        private string root;
        private string savePath;
        private InventoryConfig cfg;
        private int lastGameMoney=-1;
        private DateTime nextSave=DateTime.UtcNow.AddSeconds(10);
        public bool MoneySyncSuspended { get; set; }
        public int Cash { get { return Game.Player != null ? Math.Max(0, Game.Player.Money) : 0; } }

        public void Initialize(string r,InventoryConfig c)
        {
            root=r;cfg=c;savePath=Path.Combine(root,"inventory_save_spcore.json");
            Player=new InventoryGrid(c.PlayerColumns,c.PlayerRows); Backpack=new InventoryGrid(5,4); Load(); SyncFromGame();
        }
        public void SyncFromGame()
        {
            if(Game.Player==null || Game.Player.Character==null || !Game.Player.Character.Exists()) return;
            if(MoneySyncSuspended) return;
            int money=Math.Max(0, Game.Player.Money);
            var cash = Player.Items.FirstOrDefault(i => i.Id == "cash");
            if (money <= 0)
            {
                if(cash!=null) Player.Remove(cash);
            }
            else
            {
                if(cash==null) { cash=ItemCatalog.Create("cash", money); if(cash!=null) PlaceCash(cash); }
                else cash.Amount=money;
                if(!Player.Items.Contains(cash)) PlaceCash(ItemCatalog.Create("cash", money));
            }
            lastGameMoney=money;
            if(DateTime.UtcNow>=nextSave){Save();nextSave=DateTime.UtcNow.AddSeconds(Math.Max(2,cfg.SaveIntervalSeconds));}
        }
        public bool SetMoney(int amount)
        {
            amount=Math.Max(0,amount);
            Game.Player.Money=amount;
            var cash = Player.Items.FirstOrDefault(i => i.Id == "cash");
            if (cash == null) { cash = ItemCatalog.Create("cash", amount); if (cash != null) PlaceCash(cash); }
            else if (amount <= 0) Player.Remove(cash);
            else cash.Amount = amount;
            if (amount > 0 && !Player.Items.Any(i => i.Id == "cash")) PlaceCash(ItemCatalog.Create("cash", amount));
            lastGameMoney=amount;return true;
        }
        private void PlaceCash(Item cash)
        {
            if (cash == null) return;
            // Reserve a predictable 1x1 cell for money, falling back to normal placement.
            int x = Player.Columns - 1, y = Player.Rows - 1;
            if (Player.CanPlace(cash, x, y)) { Player.Set(cash, x, y); Player.Items.Add(cash); return; }
            Player.Add(cash);
        }
        public bool Spend(int amount)
        {
            if(amount<=0) return true;
            int cash=Cash;
            if(cash>=amount){ SetMoney(cash-amount); return true; }
            int remaining=amount-cash;
            if(BankBalance<remaining) return false;
            if(cash>0) SetMoney(0);
            BankBalance-=remaining;
            Save();
            return true;
        }
        public bool RemoveCash(int amount){return Spend(amount);}
        public bool HasItem(string id){return Player.Items.Any(i=>i.Id.Equals(id,StringComparison.OrdinalIgnoreCase));}
        public void DepositToBank(int amount){if(amount<=0||Cash<amount)return;SetMoney(Cash-amount);BankBalance+=amount;Save();}
        public void AddBank(int amount){if(amount>0){BankBalance+=amount;Save();}}
        public void ClearBank(){BankBalance=0;Save();}
        public void AddCash(int amount){if(amount>0)SetMoney(Cash+amount);}
        public void ExpandForBackpack() { Backpack.Expand(7,6); }
        public bool AddToPlayer(Item item)
        {
            if(item==null)return false;
            if(item.Id=="cash"){AddCash(item.Amount);return true;}
            if(item.AmmoType!=null) AmmoBridge.AddToPed(item);
            if(!item.SyncedAmmo && !item.SyncedWeapon && item.Weapon==null)
            {
                var s=Player.Items.FirstOrDefault(i=>i.Id==item.Id&&!i.SyncedAmmo&&!i.SyncedWeapon&&i.Weapon==null);
                if(s!=null){s.Amount+=item.Amount;return true;}
            }
            return Player.Add(item);
        }
        public void Save()
        {
            try
            {
                var data=new SaveData{PlayerColumns=Player.Columns,PlayerRows=Player.Rows,BankBalance=BankBalance,Hunger=Hunger,Thirst=Thirst,BackpackColumns=Backpack.Columns,BackpackRows=Backpack.Rows,Player=Player.Items.Where(i=>!i.SyncedWeapon&&!i.SyncedAmmo&&i.Id!="cash").Select(SaveItem.From).ToList(),Backpack=Backpack.Items.Where(i=>!i.SyncedWeapon&&!i.SyncedAmmo).Select(SaveItem.From).ToList()};
                File.WriteAllText(savePath,new JavaScriptSerializer().Serialize(data));
            }catch{}
        }
        private void Load()
        {
            if(!File.Exists(savePath))return;
            try{var d=new JavaScriptSerializer().Deserialize<SaveData>(File.ReadAllText(savePath));if(d!=null){ BankBalance=Math.Max(0,d.BankBalance); Hunger=Math.Max(0,Math.Min(100,d.Hunger<=0?100:d.Hunger)); Thirst=Math.Max(0,Math.Min(100,d.Thirst<=0?100:d.Thirst)); if(d.BackpackColumns>Backpack.Columns || d.BackpackRows>Backpack.Rows) Backpack.Expand(Math.Max(Backpack.Columns,d.BackpackColumns),Math.Max(Backpack.Rows,d.BackpackRows)); if(d.PlayerColumns>Player.Columns || d.PlayerRows>Player.Rows) Player.Expand(Math.Max(Player.Columns,d.PlayerColumns),Math.Max(Player.Rows,d.PlayerRows)); if(d.Player!=null) foreach(var s in d.Player){if(string.Equals(s.Id,"smg_ammo",StringComparison.OrdinalIgnoreCase))s.Id="pistol_ammo";var i=s.ToItem();if(i!=null)Player.Add(i);} if(d.Backpack!=null) foreach(var s in d.Backpack){if(string.Equals(s.Id,"smg_ammo",StringComparison.OrdinalIgnoreCase))s.Id="pistol_ammo";var i=s.ToItem();if(i!=null)Backpack.Add(i);} }}catch{}
        }
    }
    public sealed class SaveData{public int PlayerColumns;public int PlayerRows;public int BankBalance;public float Hunger=100f;public float Thirst=100f;public int BackpackColumns;public int BackpackRows;public List<SaveItem> Player=new List<SaveItem>();public List<SaveItem> Backpack=new List<SaveItem>();}
    public sealed class SaveItem
    {
        public string Id;public int Amount;public bool Rotated;
        public static SaveItem From(Item i){return new SaveItem{Id=i.Id,Amount=i.Amount,Rotated=i.Rotated};}
        public Item ToItem(){var i=ItemCatalog.Create(Id,Amount);if(i!=null)i.Rotated=Rotated;return i;}
    }

    public sealed class LootContainer
    {
        public string Key;public Vector3 Position;public InventoryGrid Grid;public bool IsVehicle;public bool IsPed;
        public LootContainer(string key,Vector3 pos,int cols,int rows){Key=key;Position=pos;Grid=new InventoryGrid(cols,rows);}
    }

    public sealed class LootManager
    {
        private readonly InventoryConfig cfg;private readonly InventoryStore store;private readonly Random rng=new Random();
        private readonly Dictionary<string,LootContainer> vehicles=new Dictionary<string,LootContainer>();
        private readonly Dictionary<int,LootContainer> peds=new Dictionary<int,LootContainer>();
        private readonly List<LootContainer> dropped=new List<LootContainer>();
        public LootManager(InventoryConfig c,InventoryStore s){cfg=c;store=s;}
        public void Tick(){if(Game.IsPaused)return;ScanDeadPeds();Cleanup();}
        private void Cleanup(){dropped.RemoveAll(c=>c.Grid.Items.Count==0||c.Position.DistanceTo(Game.Player.Character.Position)>80f);}
        public LootContainer GetVehicle(Vehicle v)
        {
            if(v==null||!v.Exists())return null;
            string plate=(Function.Call<string>(Hash.GET_VEHICLE_NUMBER_PLATE_TEXT,v.Handle)??"").Trim().ToUpperInvariant();
            string key=v.Model.Hash+"_"+plate+"_"+v.Handle;
            LootContainer c;if(!vehicles.TryGetValue(key,out c)){c=new LootContainer(key,v.Position,cfg.VehicleColumns,cfg.VehicleRows){IsVehicle=true};GenerateVehicleLoot(c);vehicles[key]=c;}
            c.Position=v.Position;return c;
        }
        private void GenerateVehicleLoot(LootContainer c)
        {
            Vehicle vehicle=null;
            try
            {
                int handle=int.Parse(c.Key.Split('_').Last());
                vehicle=World.GetAllVehicles().FirstOrDefault(x=>x!=null&&x.Exists()&&x.Handle==handle);
            }
            catch {}

            // Police vehicles use the same randomized glovebox model in this beta.
            // This keeps every vehicle predictable at the minimum while avoiding guaranteed guns.

            // Every vehicle has at least one basic consumable.
            AddMerged(c.Grid,rng.Next(2)==0 ? "water_bottle" : "food",1);

            // Vehicle weapon odds are deliberately simple and readable:
            // 10% baseline, reduced in affluent districts and increased in poorer districts.
            int gunChance=VehicleGunChance(vehicle==null?c.Position:vehicle.Position);
            if(rng.Next(100)<gunChance)
            {
                int roll=rng.Next(100);
                string gun=roll<60 ? "combat_pistol" : (roll<80 ? "pistol" : "machine_pistol");
                AddMerged(c.Grid,gun,1);

                // A vehicle containing a gun always contains 3-23 rounds for it.
                string ammo="pistol_ammo";
                AddMerged(c.Grid,ammo,rng.Next(3,24));
            }

            // A little extra civilian loot, without ever making the glovebox empty.
            if(rng.Next(100)<45)AddMerged(c.Grid,"cigarette_pack",rng.Next(1,2));
            if(rng.Next(100)<25)AddMerged(c.Grid,"sandwich",1);
        }

        private int VehicleGunChance(Vector3 pos)
        {
            // Baseline 10%. Poorer areas get a modest boost; affluent areas get a reduction.
            // These are intentionally broad world-space bands rather than exact neighborhoods.
            bool poor =
                (pos.X > -300f && pos.X < 450f && pos.Y > -2100f && pos.Y < -1500f) || // South LS / Grove-Davis-Forum
                (pos.X > 900f && pos.X < 1800f && pos.Y > -1700f && pos.Y < -900f) || // East LS
                (pos.X > -1200f && pos.X < -500f && pos.Y > -1900f && pos.Y < -900f);   // Strawberry / Rancho
            bool rich =
                (pos.X > -1500f && pos.X < -300f && pos.Y > -900f && pos.Y < -100f) ||  // Rockford / Vinewood west
                (pos.X > -500f && pos.X < 650f && pos.Y > -700f && pos.Y < 250f);        // Vinewood / Burton / Downtown north
            if(poor)return 15;
            if(rich)return 5;
            return 10;
        }

        private void AddMerged(InventoryGrid grid,string id,int amount)
        {
            if(grid==null || amount<=0)return;
            var existing=grid.Items.FirstOrDefault(i=>i.Id==id && !i.SyncedAmmo && !i.SyncedWeapon);
            if(existing!=null){existing.Amount+=amount;return;}
            var item=ItemCatalog.Create(id,amount);
            if(item!=null && !grid.Add(item)){}
        }
        private void GenerateWeighted(LootContainer c,List<LootDefinition> defs,int min,int max)
        {
            int count=rng.Next(min,max+1);for(int n=0;n<count;n++){var d=Pick(defs);if(d==null)continue;var i=ItemCatalog.Create(d.Id,rng.Next(d.Min,d.Max+1));if(i!=null&&!c.Grid.Add(i))break;}
        }
        private LootDefinition Pick(List<LootDefinition> defs){if(defs==null||defs.Count==0)return null;int total=defs.Sum(d=>Math.Max(0,d.Weight));if(total<=0)return null;int r=rng.Next(total);foreach(var d in defs){r-=Math.Max(0,d.Weight);if(r<0)return d;}return defs.Last();}
        private void ScanDeadPeds()
        {
            var me=Game.Player.Character;if(me==null||!me.Exists())return;
            foreach(var ped in World.GetNearbyPeds(me.Position, Math.Max(12f, cfg.LootRadius + 6f)))
            {
                if(ped==null||!ped.Exists()||ped==me||ped.IsAlive)continue;
                if(ped.Position.DistanceTo(me.Position)>Math.Max(12f, cfg.LootRadius+6f))continue;
                if(peds.ContainsKey(ped.Handle))continue;
                var c=new LootContainer("ped_"+ped.Handle,ped.Position,cfg.NearbyColumns,cfg.NearbyRows){IsPed=true};
                GeneratePedLoot(c,ped);
                peds[ped.Handle]=c;
                MergeNearbyLoot(c);
            }
        }
        private void GeneratePedLoot(LootContainer c,Ped ped)
        {
            var weaponDefs=ItemCatalog.All().Where(i=>i.Weapon!=null).ToList();
            var armed=weaponDefs.Where(d=>Function.Call<bool>(Hash.HAS_PED_GOT_WEAPON,ped.Handle,unchecked((int)Game.GenerateHash(d.Weapon)),false)).ToList();

            // Armed peds can drop the weapon they actually carried, plus a small matching ammo stack.
            // Unarmed peds use the same 5/10/15% area-based gun logic as gloveboxes.
            if(armed.Count>0)
            {
                var d=armed.FirstOrDefault(x=>unchecked((int)Game.GenerateHash(x.Weapon))==Function.Call<int>(Hash.GET_SELECTED_PED_WEAPON,ped.Handle)) ?? armed[0];
                var weaponItem=ItemCatalog.Create(d.Id);
                if(weaponItem!=null)AddMerged(c.Grid,weaponItem.Id,1);
                if(d!=null&&!string.IsNullOrEmpty(d.AmmoType))
                {
                    string ammoId=ItemCatalog.AmmoItemFor(d.AmmoType);
                    if(ammoId!=null)
                    {
                        int ammo=Math.Max(0,Function.Call<int>(Hash.GET_AMMO_IN_PED_WEAPON,ped.Handle,unchecked((int)Game.GenerateHash(d.Weapon))));
                        if(ammo>0)AddMerged(c.Grid,ammoId,Math.Min(ammo,23));
                    }
                }
            }
            else if(rng.Next(100)<PedGunChance(c.Position))
            {
                int roll=rng.Next(100);
                string gun=roll<60 ? "combat_pistol" : (roll<80 ? "pistol" : "machine_pistol");
                AddMerged(c.Grid,gun,1);
                AddMerged(c.Grid,"pistol_ammo",rng.Next(3,24));
            }

            // Every dead pedestrian gets at least one ordinary consumable.
            AddMerged(c.Grid,rng.Next(2)==0 ? "water_bottle" : "food",1);

            bool stripper=IsStripper(ped.Model.Hash);
            if(stripper && rng.Next(100)<45)AddMerged(c.Grid,"meth_bag",DrugAmount());

            bool thug=IsThug(ped.Model.Hash);
            bool gunDeath=weaponDefs.Any(w=>w.Weapon!=null&&unchecked((int)Game.GenerateHash(w.Weapon))==Function.Call<int>(Hash.GET_PED_CAUSE_OF_DEATH,ped.Handle));
            if(thug)
            {
                // Each drug type can appear at most once in a corpse's loot.
                // 2-4 is the normal result; large stacks are increasingly rare and cap at 30.
                string[] drugs={"xtcbaggy","morphine","oxy","meth_bag","baggie","weed_bud","joint"};
                foreach(var id in drugs)
                {
                    if(rng.Next(100)<rng.Next(4,11))
                        AddMerged(c.Grid,id,DrugAmount());
                }

                string[] harder={"cokebaggy","crack_baggy"};
                foreach(var id in harder)
                {
                    if(rng.Next(100)<4)
                        AddMerged(c.Grid,id,DrugAmount());
                }

                if(rng.Next(100)<65)AddMerged(c.Grid,"cash",rng.Next(500,9001));
                if(rng.Next(100)<18 && armed.Count==0)
                {
                    var d=weaponDefs[rng.Next(Math.Min(weaponDefs.Count,18))];
                    if(d!=null)AddMerged(c.Grid,d.Id,1);
                }
            }
            else
            {
                if(gunDeath && rng.Next(100)<60)AddMerged(c.Grid,"cash",rng.Next(500,9001));
                if(rng.Next(100)<12)AddMerged(c.Grid,"cigarette_pack",1);
                if(rng.Next(100)<10)AddMerged(c.Grid,"water_bottle",1);
                if(rng.Next(100)<8)AddMerged(c.Grid,"sandwich",1);
                if(rng.Next(100)<cfg.DeadPedMiscChancePercent)AddMerged(c.Grid,"phone",1);
                if(rng.Next(100)<cfg.DeadPedMoneyChancePercent)AddMerged(c.Grid,"cash",rng.Next(20,251));
            }
            if(c.Grid.Items.Count==0)AddMerged(c.Grid,"water_bottle",1);
        }

        private int PedGunChance(Vector3 pos)
        {
            bool poor =
                (pos.X > -300f && pos.X < 450f && pos.Y > -2100f && pos.Y < -1500f) ||
                (pos.X > 900f && pos.X < 1800f && pos.Y > -1700f && pos.Y < -900f) ||
                (pos.X > -1200f && pos.X < -500f && pos.Y > -1900f && pos.Y < -900f);
            bool rich =
                (pos.X > -1500f && pos.X < -300f && pos.Y > -900f && pos.Y < -100f) ||
                (pos.X > -500f && pos.X < 650f && pos.Y > -700f && pos.Y < 250f);
            if(poor)return 15;
            if(rich)return 5;
            return 10;
        }

        private void MergeNearbyLoot(LootContainer c)
        {
            if(c==null || c.Grid.Items.Count==0)return;
            const float mergeRadius=1.524f; // 5 feet
            LootContainer target=null;
            foreach(var other in peds.Values)
            {
                if(other==null||ReferenceEquals(other,c)||other.Grid.Items.Count==0)continue;
                if(other.Position.DistanceTo(c.Position)<=mergeRadius){target=other;break;}
            }
            if(target==null)
            {
                foreach(var other in dropped)
                {
                    if(other==null||ReferenceEquals(other,c)||other.Grid.Items.Count==0)continue;
                    if(other.Position.DistanceTo(c.Position)<=mergeRadius){target=other;break;}
                }
            }
            if(target==null)return;
            foreach(var item in c.Grid.Items.ToList())
            {
                var same=target.Grid.Items.FirstOrDefault(i=>i.Id==item.Id&&!i.SyncedAmmo&&!i.SyncedWeapon);
                if(same!=null)same.Amount+=item.Amount;
                else if(!target.Grid.Add(item)){}
            }
            c.Grid.Clear();
        }

        private int DrugAmount()
        {
            int r=rng.Next(1000);
            if(r<700)return rng.Next(2,5);      // 2-4 most common
            if(r<925)return rng.Next(5,11);     // 5-10
            if(r<990)return rng.Next(11,21);    // 11-20
            return rng.Next(21,31);             // rare 21-30 maximum
        }
        private static bool IsStripper(int hash)
        {
            int[] models={unchecked((int)Game.GenerateHash("s_f_y_stripper_01")),unchecked((int)Game.GenerateHash("s_f_y_stripper_02")),unchecked((int)Game.GenerateHash("s_f_y_stripperlite"))};
            return models.Contains(hash);
        }
        private static bool IsThug(int hash)
        {
            string[] names={"g_m_y_ballaeast_01","g_m_y_ballasout_01","g_m_y_ballaorig_01","g_m_y_famca_01","g_m_y_famdnf_01","g_m_y_famfor_01","g_m_y_lost_01","g_m_y_lost_02","g_m_y_lost_03","g_m_y_mexgoon_01","g_m_y_mexgoon_02","g_m_y_mexgoon_03","g_m_y_mexgang_01","g_m_y_mexgang_02"};
            foreach(var n in names)if(hash==unchecked((int)Game.GenerateHash(n)))return true;
            return false;
        }
        public IEnumerable<LootContainer> GetNearby(Vector3 pos,float radius)
        {
            var list=new List<LootContainer>();
            foreach(var c in peds.Values)if(c.Position.DistanceTo(pos)<=radius&&c.Grid.Items.Count>0)list.Add(c);
            foreach(var c in dropped.ToList())if(c.Position.DistanceTo(pos)<=radius&&c.Grid.Items.Count>0)list.Add(c);
            return list.OrderBy(c=>c.Position.DistanceTo(pos));
        }
        public LootContainer FindNearestLoot(Vector3 pos,float radius){return GetNearby(pos,radius).FirstOrDefault();}
        public void AddDropped(LootContainer c)
        {
            if(c==null)return;
            const float mergeRadius=1.524f; // 5 feet
            var existing=dropped.FirstOrDefault(x=>x!=null&&x.Grid.Items.Count>0&&x.Position.DistanceTo(c.Position)<=mergeRadius);
            if(existing!=null)
            {
                foreach(var item in c.Grid.Items.ToList())
                {
                    var same=existing.Grid.Items.FirstOrDefault(i=>i.Id==item.Id&&!i.SyncedAmmo&&!i.SyncedWeapon);
                    if(same!=null)same.Amount+=item.Amount;
                    else if(!existing.Grid.Add(item)){}
                }
                return;
            }
            dropped.Add(c);
        }
        public void DrawNearbyPrompt()
        {
            var p=Game.Player.Character; if(p==null||!p.Exists())return;
            var nearest=FindNearestLoot(p.Position,cfg.LootRadius);
            if(nearest==null)return;
            // Give dropped/dead-body loot a visible world-space anchor.  Previously the
            // container existed only in memory, so there was nothing on the floor to tell
            // the player where the vicinity inventory was.
            Function.Call(Hash.DRAW_MARKER, 2, nearest.Position.X, nearest.Position.Y, nearest.Position.Z + 0.12f,
                0f,0f,0f, 0f,0f,0f, 0.28f,0.28f,0.18f, 70,170,255,190, false,true,2,false,null,null,false);
            new TextElement("~b~I~s~  SEARCH NEARBY",new PointF(640,665),0.32f,Color.White,GTA.UI.Font.ChaletLondon,Alignment.Center).ScaledDraw();
        }
    }

    public enum ShopType { None,Ammu,GasStation,Dealer,CityHall,WeedDealer }
    public sealed class ShopManager
    {
        private readonly InventoryConfig cfg;private readonly InventoryStore store;private readonly LootManager loot;
        private readonly List<Vector3> ammu=new List<Vector3>{new Vector3(22.1f,-1107.2f,29.8f),new Vector3(252.8f,-50f,69.9f),new Vector3(-662f,-934.3f,21.8f),new Vector3(810.2f,-2157.3f,29.6f),new Vector3(1693.4f,3760.4f,34.7f),new Vector3(-330f,6083.6f,31.4f),new Vector3(-1118.3f,2698.8f,18.5f)};
        private readonly List<Vector3> gas=new List<Vector3>{new Vector3(26.2f,-1347.3f,29.5f),new Vector3(-48.5f,-1757.5f,29.4f),new Vector3(-707.5f,-914.3f,19.2f),new Vector3(1135.8f,-982.3f,46.4f),new Vector3(1163.4f,-323.8f,69.2f),new Vector3(373.9f,325.9f,103.6f),new Vector3(-1820.1f,792.4f,138.1f),new Vector3(-2967.8f,390.9f,15.0f),new Vector3(-3241.9f,1001.5f,12.8f),new Vector3(547.4f,2671.7f,42.2f),new Vector3(1961.2f,3740.5f,32.3f),new Vector3(2678.9f,3280.7f,55.2f),new Vector3(1729.2f,6414.9f,35f),new Vector3(1392.6f,3604.6f,34.9f),new Vector3(2556.2f,382.1f,108.6f),new Vector3(49.5f,2778.8f,58f),new Vector3(264.1f,2607.3f,44.9f),new Vector3(1208.7f,2669.1f,37.8f),new Vector3(-1799.6f,803.6f,138.7f),new Vector3(-2554.9f,2334.5f,33.1f),new Vector3(-94.1f,6419.5f,31.5f),new Vector3(1701.7f,6416.3f,32.8f),new Vector3(620.8f,269.1f,103f),new Vector3(-724.5f,-935.9f,19.2f)};
        private Vector3 dealerPos; private Ped dealerPed; private Ped cityHallPed; private Vector3 weedDealerPos; private Ped weedDealerPed; private int dealerBlipHandle; private int cityHallBlipHandle; private int weedDealerBlipHandle; private readonly List<int> gasBlips=new List<int>(); private readonly List<int> ammuBlips=new List<int>(); private int bankBlipHandle;
        private readonly List<Vector3> greenhousePoints=new List<Vector3>{new Vector3(759.6f,-824.7f,25.3f)}; private readonly List<int> greenhouseBlips=new List<int>();
        // Fixed, clearly marked interaction points. These are intentionally not tied to a
        // GTA interior ID because interior IDs can differ between game/build combinations.
        private readonly Vector3 cityHall=new Vector3(-544.7f,-204.0f,38.2f); private readonly Vector3 bankPoint=new Vector3(241.8f,222.4f,106.3f);
        private readonly List<Vector3> atms=new List<Vector3>{new Vector3(150.266f,-1040.203f,29.374f),new Vector3(-1212.98f,-330.841f,37.787f),new Vector3(-2962.582f,482.627f,15.703f),new Vector3(-112.202f,6469.295f,31.626f)};
        public ShopManager(InventoryConfig c,InventoryStore s,LootManager l){cfg=c;store=s;loot=l;SpawnShadyDealer();}
        public Vector3 DealerPosition { get { return dealerPos; } }
        private void SpawnShadyDealer()
        {
            dealerPos=new Vector3(112.7f,-1960.2f,20.96f);
            weedDealerPos=new Vector3(93.8f,-1930.8f,20.9f);
            SpawnNpc(ref dealerPed, "g_m_y_mexgoon_02", dealerPos, 145f);
            SpawnNpc(ref weedDealerPed, "g_m_y_lost_01", weedDealerPos, 275f);
            SpawnNpc(ref cityHallPed, "s_m_m_highsec_01", cityHall + new Vector3(1.4f,0.4f,0f), 180f);
            dealerBlipHandle=CreateBlip(dealerPos,110,1,"Shady Gun Dealer");
            cityHallBlipHandle=CreateBlip(cityHall,419,3,"City Hall");
            weedDealerBlipHandle=CreateBlip(weedDealerPos,496,2,"Weed Dealer");
            foreach(var p in greenhousePoints.Distinct()) greenhouseBlips.Add(CreateBlip(p,496,2,"Greenhouse"));
            foreach(var p in UniquePoints(gas,8f)) gasBlips.Add(CreateBlip(p,361,2,"24/7 Gas Station"));
            foreach(var p in UniquePoints(ammu,8f)) ammuBlips.Add(CreateBlip(p,110,1,"Ammu-Nation")); bankBlipHandle=CreateBlip(bankPoint,108,2,"Pacific Standard Bank");
        }
        private void SpawnNpc(ref Ped ped,string modelName,Vector3 pos,float heading)
        {
            try
            {
                var model=new Model(modelName);
                model.Request(1500);
                if(!model.IsInCdImage || !model.IsValid) return;
                ped=World.CreatePed(model,pos);
                if(ped!=null&&ped.Exists())
                {
                    ped.IsPersistent=true; ped.IsInvincible=true; ped.BlockPermanentEvents=true;
                    ped.CanRagdoll=false; ped.Heading=heading; ped.Task.StandStill(-1);
                }
                model.MarkAsNoLongerNeeded();
            }
            catch { }
        }
        private IEnumerable<Vector3> UniquePoints(IEnumerable<Vector3> points,float minDistance)
        {
            var result=new List<Vector3>();
            foreach(var p in points)
            {
                if(!result.Any(x=>x.DistanceTo(p)<minDistance))result.Add(p);
            }
            return result;
        }
        private int CreateBlip(Vector3 pos,int sprite,int colour,string name)
        {
            try
            {
                int h=Function.Call<int>(Hash.ADD_BLIP_FOR_COORD,pos.X,pos.Y,pos.Z);
                Function.Call(Hash.SET_BLIP_SPRITE,h,sprite); Function.Call(Hash.SET_BLIP_COLOUR,h,colour);
                Function.Call(Hash.SET_BLIP_AS_SHORT_RANGE,h,false);
                Function.Call(Hash.BEGIN_TEXT_COMMAND_SET_BLIP_NAME,"STRING");
                Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME,name);
                Function.Call(Hash.END_TEXT_COMMAND_SET_BLIP_NAME,h);
                return h;
            }
            catch { return 0; }
        }
        public ShopType GetNearbyType(Vector3 p)
        {
            float best=9f; ShopType type=ShopType.None;
            foreach(var x in ammu){float d=x.DistanceTo(p);if(d<best && IsAtShopInterior(p,x)){best=d;type=ShopType.Ammu;}}
            foreach(var x in gas){float d=x.DistanceTo(p);if(d<best && IsAtShopInterior(p,x)){best=d;type=ShopType.GasStation;}}
            float dd=dealerPos.DistanceTo(p);if(dd<5f){best=dd;type=ShopType.Dealer;}
            float wd=weedDealerPos.DistanceTo(p);if(wd<5f && wd<best){best=wd;type=ShopType.WeedDealer;}
            float dh=cityHall.DistanceTo(p);if(dh<5f && dh<best){best=dh;type=ShopType.CityHall;}
            return type;
        }
        private bool IsAtShopInterior(Vector3 p, Vector3 shopPoint)
        {
            var ped=Game.Player.Character;
            if(ped==null||!ped.Exists()) return false;
            int currentInterior=Function.Call<int>(Hash.GET_INTERIOR_FROM_ENTITY,ped.Handle);
            if(currentInterior!=0) return p.DistanceTo(shopPoint) <= 35f;
            if (p.DistanceTo(shopPoint) > 9f) return false;

            // Some 24/7/Ammu interiors report zero in SHVDN; the tighter doorway radius
            // keeps the interaction tied to the storefront rather than the whole block.
            return p.DistanceTo(shopPoint) <= 4.5f;
        }
        public bool IsNearBlockedShop(Vector3 p){return GetNearbyType(p)==ShopType.Ammu || GetNearbyType(p)==ShopType.GasStation;}
        public bool IsNearDealer(Vector3 p){return dealerPos.DistanceTo(p)<5f;}
        public bool IsNearWeedDealer(Vector3 p){return weedDealerPos.DistanceTo(p)<5f;}
        public bool IsNearGreenhouse(Vector3 p){return greenhousePoints.Any(x=>x.DistanceTo(p)<4.5f);}
        public bool IsNearCityHall(Vector3 p){return cityHall.DistanceTo(p)<5f;}
        public bool IsNearATM(Vector3 p){return atms.Any(x=>x.DistanceTo(p)<3.0f);} public bool IsNearBank(Vector3 p){return bankPoint.DistanceTo(p)<4.0f;}
        public Vector3? NearestGasPump(Vector3 p){if(gas.Count==0)return null;var n=gas.OrderBy(x=>x.DistanceTo(p)).First();return n;}
        public void Tick()
        {
            if(dealerPed!=null&&dealerPed.Exists()){dealerPed.IsInvincible=true;dealerPed.BlockPermanentEvents=true;dealerPed.Task.StandStill(-1);}
            if(cityHallPed!=null&&cityHallPed.Exists()){cityHallPed.IsInvincible=true;cityHallPed.BlockPermanentEvents=true;cityHallPed.Task.StandStill(-1);}
        }
        public void DrawWorldPrompts(bool uiOpen)
        {
            var p=Game.Player.Character; if(p==null||!p.Exists()||uiOpen)return;
            float dd=p.Position.DistanceTo(dealerPos), dh=p.Position.DistanceTo(cityHall);
            if(dd<14f)
            {
                Function.Call(Hash.DRAW_MARKER,2,dealerPos.X,dealerPos.Y,dealerPos.Z+0.05f,0f,0f,0f,0f,0f,0f,0.45f,0.45f,0.25f,255,90,60,190,false,true,2,false,null,null,false);
                if(dd<5f)new TextElement("~b~E~s~  TALK TO SHADY GUN DEALER",new PointF(640,665),0.30f,Color.White,GTA.UI.Font.ChaletLondon,Alignment.Center).ScaledDraw();
            }
            if(dh<14f)
            {
                Function.Call(Hash.DRAW_MARKER,2,cityHall.X,cityHall.Y,cityHall.Z+0.05f,0f,0f,0f,0f,0f,0f,0.45f,0.45f,0.25f,75,165,255,190,false,true,2,false,null,null,false);
                if(dh<5f)new TextElement("~b~E~s~  TALK TO CITY HALL",new PointF(640,665),0.30f,Color.White,GTA.UI.Font.ChaletLondon,Alignment.Center).ScaledDraw();
            float bd=p.Position.DistanceTo(bankPoint); if(bd<12f){ Function.Call(Hash.DRAW_MARKER,2,bankPoint.X,bankPoint.Y,bankPoint.Z+0.05f,0f,0f,0f,0f,0f,0f,0.45f,0.45f,0.25f,70,150,255,190,false,true,2,false,null,null,false); if(bd<4f)new TextElement("~b~E~s~  USE PACIFIC STANDARD BANK",new PointF(640,665),0.30f,Color.White,GTA.UI.Font.ChaletLondon,Alignment.Center).ScaledDraw(); }
            }
            float wd=p.Position.DistanceTo(weedDealerPos);
            if(wd<14f)
            {
                Function.Call(Hash.DRAW_MARKER,2,weedDealerPos.X,weedDealerPos.Y,weedDealerPos.Z+0.05f,0f,0f,0f,0f,0f,0f,0.45f,0.45f,0.25f,80,200,100,190,false,true,2,false,null,null,false);
                if(wd<5f)new TextElement("~b~E~s~  TALK TO WEED DEALER",new PointF(640,665),0.30f,Color.White,GTA.UI.Font.ChaletLondon,Alignment.Center).ScaledDraw();
            }
            foreach(var gp in greenhousePoints) if(p.Position.DistanceTo(gp)<12f)
            {
                Function.Call(Hash.DRAW_MARKER,1,gp.X,gp.Y,gp.Z-1f,0f,0f,0f,0f,0f,0f,2.0f,2.0f,0.35f,70,190,90,150,false,true,2,false,null,null,false);
                if(p.Position.DistanceTo(gp)<4.5f)new TextElement("~b~E~s~  PLANT WEED SEED",new PointF(640,665),0.30f,Color.White,GTA.UI.Font.ChaletLondon,Alignment.Center).ScaledDraw();
            }
            var gasPoint=gas.OrderBy(x=>x.DistanceTo(p.Position)).FirstOrDefault();
            if(gasPoint!=default(Vector3) && gasPoint.DistanceTo(p.Position)<5f)
                new TextElement("~b~E~s~  OPEN 24/7 MARKET",new PointF(640,665),0.30f,Color.White,GTA.UI.Font.ChaletLondon,Alignment.Center).ScaledDraw();
        }

        public List<ShopItem> Stock(ShopType t, bool underCounter=false)
        {
            if(t==ShopType.Ammu) return cfg.Ammu;
            if(t==ShopType.GasStation) return underCounter ? cfg.GasStationUnderCounter : cfg.GasStation;
            if(t==ShopType.Dealer) return cfg.Ammu.Select(x=>new ShopItem(x.Id,(int)Math.Round(x.Price*1.65))).ToList();
            if(t==ShopType.CityHall) return cfg.CityHall;
            if(t==ShopType.WeedDealer) return cfg.WeedDealer;
            return new List<ShopItem>();
        }
        public string Name(ShopType t, bool underCounter=false)
        {
            if(t==ShopType.Ammu) return "AMMU-NATION";
            if(t==ShopType.GasStation) return underCounter ? "24/7 MARKET  /  UNDER COUNTER" : "24/7 MARKET";
            if(t==ShopType.Dealer) return "SHADY GUN DEALER";
            if(t==ShopType.CityHall) return "CITY HALL";
            if(t==ShopType.WeedDealer) return "WEED DEALER";
            return "";
        }
        public bool Buy(ShopItem s)
        {
            if(s==null) return false;
            var item=ItemCatalog.Create(s.Id); if(item==null)return false;
            if(item.Weapon!=null && !store.HasItem("id_card")){Notifications.Push("WEAPON LICENSE REQUIRED",true);return false;}
            if(store.Cash+store.BankBalance<s.Price) return false;
            if(item.Id.EndsWith("_ammo",StringComparison.OrdinalIgnoreCase)) return QuickBuyAmmo(s,1);
            if(!store.Player.Add(item)){Notifications.Push("INVENTORY FULL",true);return false;}
            store.Spend(s.Price);
            if(item.Weapon!=null){item.SyncedWeapon=true;GiveWeapon(item);}
            Notifications.PushItem("REMOVED",s.Price,ItemCatalog.Create("cash",s.Price));
            Notifications.PushItem("ADDED",1,item);
            return true;
        }

        public bool PurchaseAt(ShopItem s, InventoryGrid target, int x, int y, ShopType type, int count=1)
        {
            if(s==null || target==null) return false;
            count=Math.Max(1,count);
            var item=ItemCatalog.Create(s.Id,count); if(item==null)return false;
            if(item.Weapon!=null || item.Id=="suppressor") count=1;
            item.Amount=count;
            if(type==ShopType.Ammu && item.Weapon!=null && !store.HasItem("id_card")){Notifications.Push("WEAPON LICENSE REQUIRED",true);return false;}
            int total=s.Price*count; if(store.Cash+store.BankBalance<total){Notifications.Push("INSUFFICIENT FUNDS",true);return false;}
            Point targetPoint=new Point(Math.Max(0,Math.Min(target.Columns-1,x)),Math.Max(0,Math.Min(target.Rows-1,y)));
            var atCell=target.Items.FirstOrDefault(i=>{Point p=ItemPositions.Get(i);return p.X==targetPoint.X&&p.Y==targetPoint.Y;});
            if(atCell!=null)
            {
                if(atCell.Id!=item.Id || atCell.SyncedAmmo || atCell.SyncedWeapon || atCell.Weapon!=null){Notifications.Push("SLOT OCCUPIED",true);return false;}
                atCell.Amount+=count;
            }
            else
            {
                if(!target.CanPlace(item,targetPoint.X,targetPoint.Y)){Notifications.Push("SLOT OCCUPIED",true);return false;}
                target.Set(item,targetPoint.X,targetPoint.Y);target.Items.Add(item);
            }
            store.Spend(total);
            if(item.AmmoType!=null) AmmoBridge.AddToPed(item);
            if(item.Weapon!=null){item.SyncedWeapon=true;GiveWeapon(item);}
            Notifications.PushItem("REMOVED",total,ItemCatalog.Create("cash",total));
            Notifications.PushItem("ADDED",count,item);
            return true;
        }
        public bool QuickBuyAmmo(ShopItem s,int count){if(s==null||!s.Id.EndsWith("_ammo",StringComparison.OrdinalIgnoreCase)||count<1)return false;int total=s.Price*count;if(store.Cash+store.BankBalance<total){Notifications.Push("INSUFFICIENT FUNDS",true);return false;}var item=ItemCatalog.Create(s.Id,count);if(item==null)return false;var existing=store.Player.Items.FirstOrDefault(i=>i.Id==item.Id&&!i.SyncedAmmo&&!i.SyncedWeapon);if(existing!=null)existing.Amount+=count;else if(!store.Player.Add(item)){Notifications.Push("INVENTORY FULL",true);return false;}store.Spend(total);AmmoBridge.AddToPed(item);Notifications.PushItem("REMOVED",total,ItemCatalog.Create("cash",total));Notifications.PushItem("ADDED",count,item);return true;}
                public int SellWeapon(Item item){if(item==null||item.Weapon==null)return 0;var shop=cfg.Ammu.FirstOrDefault(x=>x.Id.Equals(item.Id,StringComparison.OrdinalIgnoreCase));int basePrice=shop==null?50000:shop.Price;int pct=new Random().Next(30,76);return Math.Max(1,basePrice*pct/100);}
        public void Dispose()
        {
            foreach(var h0 in gasBlips.Concat(ammuBlips).ToList()) { int h=h0; if(h!=0) { try { Function.Call(Hash.REMOVE_BLIP, h); } catch {} } }
            if(bankBlipHandle!=0){try{Function.Call(Hash.REMOVE_BLIP,bankBlipHandle);}catch{}}
            if(dealerBlipHandle!=0){try{Function.Call(Hash.REMOVE_BLIP,dealerBlipHandle);}catch{}}
            if(cityHallBlipHandle!=0){try{Function.Call(Hash.REMOVE_BLIP,cityHallBlipHandle);}catch{}}
            if(weedDealerBlipHandle!=0){try{Function.Call(Hash.REMOVE_BLIP,weedDealerBlipHandle);}catch{}}
            foreach(var h0 in greenhouseBlips.ToList()){int h=h0;if(h!=0){try{Function.Call(Hash.REMOVE_BLIP,h);}catch{}}}
            try{if(dealerPed!=null&&dealerPed.Exists())dealerPed.Delete();}catch{}
            try{if(cityHallPed!=null&&cityHallPed.Exists())cityHallPed.Delete();}catch{}
            try{if(weedDealerPed!=null&&weedDealerPed.Exists())weedDealerPed.Delete();}catch{}
        }
        private void GiveWeapon(Item item){var ped=Game.Player.Character;int h=unchecked((int)Game.GenerateHash(item.Weapon));Function.Call(Hash.GIVE_WEAPON_TO_PED,ped.Handle,h,0,false,true);}
        private bool HasMatchingWeapon(Item item)
        {
            var ped=Game.Player.Character;return ItemCatalog.All().Any(x=>x.Weapon!=null&&x.AmmoType==item.AmmoType&&Function.Call<bool>(Hash.HAS_PED_GOT_WEAPON,ped.Handle,unchecked((int)Game.GenerateHash(x.Weapon)),false));
        }
        private void AddAmmo(Item item)
        {
            var weapon=ItemCatalog.All().FirstOrDefault(x=>x.Weapon!=null&&x.AmmoType==item.AmmoType&&Function.Call<bool>(Hash.HAS_PED_GOT_WEAPON,Game.Player.Character.Handle,unchecked((int)Game.GenerateHash(x.Weapon)),false));
            if(weapon==null)return;Function.Call(Hash.ADD_AMMO_TO_PED,Game.Player.Character.Handle,unchecked((int)Game.GenerateHash(weapon.Weapon)),item.Amount);
        }
    }

    public static class Notifications
    {
        private sealed class N
        {
            public string Text; public string Icon; public DateTime Until; public bool Blue;
            public N(string text, string icon = null, bool blue = false) { Text = text; Icon = icon; Blue = blue; Until = DateTime.UtcNow.AddSeconds(2.6); }
        }
        private static readonly List<N> list = new List<N>();
        public static void Push(string text) { list.Add(new N(text)); Trim(); }
        public static void Push(string text, bool blue) { list.Add(new N(text, null, blue)); Trim(); }
        public static void PushItem(string verb, int amount, Item item)
        {
            if (item == null) { Push(verb + " " + amount + "x"); return; }
            string amountText = item.Id == "cash" ? "$" + amount.ToString("N0") : amount + "X";
            list.Add(new N(verb + " " + amountText + "  " + item.Name, item.Icon));
            Trim();
        }
        private static void Trim() { while (list.Count > 4) list.RemoveAt(0); }
        public static void Draw()
        {
            float sw = GTA.UI.Screen.Resolution.Width, sh = GTA.UI.Screen.Resolution.Height;
            float width = 108f, height = 108f, x = (sw-width)*0.5f, y = Math.Min(sh-height-18f, sh*0.88f);
            foreach (var n in list.ToList())
            {
                if (n.Until < DateTime.UtcNow) { list.Remove(n); continue; }
                float remain=(float)(n.Until-DateTime.UtcNow).TotalSeconds;
                float a=Math.Min(1f,remain/0.45f);
                // Dark gray body with a solid black header strip so the header always stays
                // above the transparent icon, matching the supplied removal notification.
                Function.Call(Hash.DRAW_RECT,(x+width/2f)/sw,(y+height/2f)/sh,width/sw,height/sh,28,31,35,(int)(242*a));
                Function.Call(Hash.DRAW_RECT,(x+width/2f)/sw,(y+14f)/sh,width/sw,28f/sh,4,5,7,(int)(250*a));
                string header = n.Blue ? "" : n.Text;
                string body = n.Blue ? n.Text : "";
                int split = header.IndexOf("  ",StringComparison.Ordinal);
                if(split>0){ body=header.Substring(split+2); header=header.Substring(0,split); }
                // TextElement uses the 1280x720 design surface.
                float ux = (x + width/2f) / sw * 1280f;
                float uy = (y + 8f) / sh * 720f;
                new TextElement(header,new PointF(ux,uy),0.22f,Color.FromArgb((int)(255*a),245,245,245),GTA.UI.Font.RockstarTag,Alignment.Center,true,false).Draw();
                if(!string.IsNullOrEmpty(n.Icon))
                {
                    string path=IconPath(n.Icon);
                    if(path!=null) try
                    {
                        float iw=46f, ih=46f;
                        float bx=(x+width/2f-iw/2f)/sw*1280f, by=(y+30f)/sh*720f;
                        // Create the sprite for this draw call instead of retaining a native
                        // texture wrapper between frames. This is safer with SHVDN nightly + RPH.
                        var sprite=new CustomSprite(path,new SizeF(iw/sw*1280f,ih/sh*720f),new PointF(bx,by),Color.FromArgb((int)(255*a),255,255,255),0f,false);
                        sprite.Draw();
                    } catch {}
                }
                if(!string.IsNullOrEmpty(body))
                {
                    float textY=y+height-15f;
                    new TextElement(body,new PointF((x+width/2f)/sw*1280f,textY/sh*720f),0.22f,n.Blue?Color.FromArgb((int)(255*a),75,165,255):Color.FromArgb((int)(255*a),245,245,245),GTA.UI.Font.RockstarTag,Alignment.Center,true,false).Draw();
                }
                y -= height + 12f;
            }
        }
        private static string IconPath(string icon)
        {
            var candidates = new[] { Path.Combine(Path.GetDirectoryName(typeof(Main).Assembly.Location) ?? "", "inventory_icons", icon), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts", "inventory_icons", icon), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "inventory_icons", icon) };
            return candidates.FirstOrDefault(File.Exists);
        }
    }

    public sealed class InventoryUI : IDisposable
    {
        private readonly InventoryConfig cfg; private readonly InventoryStore store; private readonly LootManager loot; private readonly ShopManager shops; private readonly string configPath;
        private bool open; private Item held; private InventoryGrid heldOrigin; private bool leftWasDown; private bool rightWasDown;
        private Item contextItem; private bool contextOpen; private float contextX; private float contextY; private float contextRenderX; private float contextRenderY; private bool contextIsLoot; private InventoryGrid contextGrid;
        private ShopItem shopHeld; private Item shopHeldPreview; private ShopType shopType; private bool gasUnderCounter; private bool backpackOpen; private bool bankOpen;
        private bool wasReloading;
        private string amountInput;
        private readonly Dictionary<int,int> lastWeaponAmmoTotal = new Dictionary<int,int>();
        private int reloadWeaponHash;
        private string reloadAmmoItem;
        private DateTime openedAt=DateTime.MinValue;
        private float OpenAlpha { get { return Math.Min(1f,(float)(DateTime.UtcNow-openedAt).TotalSeconds/0.22f); } }

        // UI is laid out in screen-relative pixels, matching the attached QBCore/Horizon reference.
        // This avoids the old letterboxed 1664x936 canvas that caused placement/text drift on ultrawide displays.
        private float CellW, CellH, Gap;
        private float ScreenW, ScreenH;

        [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int vKey);
        [DllImport("user32.dll")] private static extern bool GetCursorPos(out POINT lpPoint);
        [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] private static extern bool GetClientRect(IntPtr hWnd, out RECT rect);
        [DllImport("user32.dll")] private static extern bool ClientToScreen(IntPtr hWnd, ref POINT point);
        [StructLayout(LayoutKind.Sequential)] private struct POINT { public int X; public int Y; }
        [StructLayout(LayoutKind.Sequential)] private struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
        public bool Open { get { return open; } }

        public bool HandleAmountKey(System.Windows.Forms.Keys key)
        {
            if (!open) return false;
            if (key >= System.Windows.Forms.Keys.D0 && key <= System.Windows.Forms.Keys.D9)
            {
                char c = (char)('0' + ((int)key - (int)System.Windows.Forms.Keys.D0));
                if (amountInput == "0") amountInput = c.ToString();
                else if (amountInput.Length < 6) amountInput += c;
                return true;
            }
            if (key >= System.Windows.Forms.Keys.NumPad0 && key <= System.Windows.Forms.Keys.NumPad9)
            {
                char c = (char)('0' + ((int)key - (int)System.Windows.Forms.Keys.NumPad0));
                if (amountInput == "0") amountInput = c.ToString();
                else if (amountInput.Length < 6) amountInput += c;
                return true;
            }
            if (key == System.Windows.Forms.Keys.Back)
            {
                if (!string.IsNullOrEmpty(amountInput)) amountInput = amountInput.Substring(0, amountInput.Length - 1);
                if (string.IsNullOrEmpty(amountInput)) amountInput = "1";
                return true;
            }
            if (key == System.Windows.Forms.Keys.Delete)
            {
                amountInput = "0";
                return true;
            }
            return false;
        }

        private int RequestedAmount()
        {
            int value;
            if (!int.TryParse(amountInput, out value)) return 1;
            return Math.Max(0, value);
        }

        public InventoryUI(InventoryConfig c, InventoryStore s, LootManager l, ShopManager sh)
        { cfg = c; store = s; loot = l; shops = sh; }

        private void UpdateCanvas()
        {
            ScreenW = GTA.UI.Screen.Resolution.Width;
            ScreenH = GTA.UI.Screen.Resolution.Height;
            // Same 5x5 + 5x2 grids, but twice the reference slot size for readability.
            // On unusually small resolutions, scale down only enough to keep both panels visible.
            float desired = 128f;
            float maxScale = Math.Min((ScreenW - 90f) / (5f * desired * 2f + 120f), (ScreenH - 120f) / (5f * desired));
            float k = Math.Min(1f, Math.Max(0.72f, maxScale));
            CellW = desired * k;
            CellH = desired * k;
            Gap = 110f * k;
        }
        private float X(float v) { return v; }
        private float Y(float v) { return v; }
        private float S(float v) { return v; }
        private PointF MouseDesign()
        {
            POINT cursor; if (!GetCursorPos(out cursor)) return new PointF(-9999f, -9999f);

            // GetCursorPos() is in desktop coordinates, while our inventory hit-tests are
            // in the GTA client/render surface.  This matters in bordered/windowed mode and
            // was the main reason dragging items from the right panel could miss completely.
            IntPtr hwnd = GetForegroundWindow();
            RECT rect;
            if (hwnd != IntPtr.Zero && GetClientRect(hwnd, out rect))
            {
                POINT origin = new POINT { X = 0, Y = 0 };
                if (ClientToScreen(hwnd, ref origin))
                {
                    float clientW = Math.Max(1, rect.Right - rect.Left);
                    float clientH = Math.Max(1, rect.Bottom - rect.Top);
                    float x = (cursor.X - origin.X) * ScreenW / clientW;
                    float y = (cursor.Y - origin.Y) * ScreenH / clientH;
                    return new PointF(Math.Max(0, Math.Min(ScreenW - 1, x)), Math.Max(0, Math.Min(ScreenH - 1, y)));
                }
            }

            return new PointF(Math.Max(0, Math.Min(ScreenW - 1, cursor.X)), Math.Max(0, Math.Min(ScreenH - 1, cursor.Y)));
        }
        private static bool MouseDown(int vk)
        {
            bool async = (GetAsyncKeyState(vk) & 0x8000) != 0;
            if (async) return true;
            if (vk == 0x01) return (System.Windows.Forms.Control.MouseButtons & MouseButtons.Left) != 0;
            if (vk == 0x02) return (System.Windows.Forms.Control.MouseButtons & MouseButtons.Right) != 0;
            return false;
        }

        public void Toggle()
        {
            if (open) { Close(); return; }
            open = true; openedAt=DateTime.UtcNow; amountInput="1"; backpackOpen=false; bankOpen=false; contextOpen = false; contextItem = null; contextGrid = null; contextIsLoot = false; shopHeld = null; shopHeldPreview = null; gasUnderCounter = false;
            shopType = shops.GetNearbyType(Game.Player.Character.Position);
        }
        public void Close()
        {
            if (held != null) CancelDrag(); contextOpen = false; shopHeld = null; shopHeldPreview = null; open = false; backpackOpen=false; bankOpen=false;
            shopType = ShopType.None; gasUnderCounter = false; store.Save();
        }
        public void OpenBankApp(){bankOpen=true;open=true;contextOpen=false;shopHeld=null;held=null;}
        public void ToggleGasUnderCounter()
        { if (open && shopType == ShopType.GasStation) { gasUnderCounter = !gasUnderCounter; contextOpen = false; shopHeld = null; shopHeldPreview = null; } }
        public void RotateHeld() { if (held != null) held.Rotated = !held.Rotated; }
        public void QuickTakeNearest()
        {
            var n = loot.FindNearestLoot(Game.Player.Character.Position, cfg.LootRadius); if (n == null) return;
            var i = n.Grid.Items.FirstOrDefault(); if (i != null) TakeFromLoot(n.Grid, i);
        }

        public void Draw()
        {
            UpdateCanvas();
            SyncWeapons(); TrackReloadAndAmmo(); shops.Tick();
            if(bankOpen){DrawBankApp(); DisableGameplay(); HandleBankKeys(); return;}
            shopType = shops.GetNearbyType(Game.Player.Character.Position);
            if (shopType != ShopType.GasStation) gasUnderCounter = false;
            if (backpackOpen && !store.HasItem("backpack")) backpackOpen=false;
            DisableGameplay();

            bool inVehicle = Game.Player.Character.IsInVehicle();
            var nearby = !inVehicle ? loot.FindNearestLoot(Game.Player.Character.Position, cfg.LootRadius) : null;
            bool hasRight = shopType != ShopType.None || inVehicle || nearby != null;

            float playerW = cfg.PlayerColumns * CellW, playerH = cfg.PlayerRows * CellH;
            float rightW = cfg.NearbyColumns * CellW, rightH = cfg.NearbyRows * CellH;
            float totalW = playerW + Gap + rightW;
            float leftX = Math.Max(10f, (ScreenW - totalW) * 0.5f);
            float rightX = leftX + playerW + Gap;
            float gridY = Math.Max(45f, (ScreenH - playerH) * 0.5f);

            // No full-screen overlay: the game world, minimap, and HUD remain visible.
            // Only the individual inventory slots receive translucent fills.
            DrawPanelHeader("INVENTORY", leftX, gridY - CellH * 0.34f, playerW);
            DrawGrid(store.Player, leftX, gridY, MouseDesign());
            DrawCapacity(store.Player.Items.Count, leftX + playerW - 2f, gridY - CellH * 0.42f);
            DrawCash(leftX + playerW - 76f, gridY - CellH * 0.43f);

            // The secondary panel is always present. On foot it is VICINITY; in a vehicle
            // it is the vehicle GLOVEBOX; at a shop it temporarily becomes the shop stock.
            if (backpackOpen) DrawContainer("BACKPACK STORAGE", store.Backpack, rightX, gridY, MouseDesign());
            else if (shopType != ShopType.None) DrawShop(shopType, rightX, gridY);
            else if (inVehicle)
            {
                var v = loot.GetVehicle(Game.Player.Character.CurrentVehicle);
                if (v != null) DrawContainer("GLOVEBOX", v.Grid, rightX, gridY, MouseDesign());
            }
            else
            {
                if (nearby != null) DrawContainer("VICINITY", nearby.Grid, rightX, gridY, MouseDesign());
                else DrawEmptyContainer("VICINITY", rightX, gridY, cfg.NearbyColumns, cfg.NearbyRows, MouseDesign());
            }

            if (contextOpen) DrawContextMenu();
            {
                float bx=(ScreenW-64f)/2f, by=(ScreenH-64f)/2f;
                DrawRect(bx,by,64f,64f,Color.FromArgb(55,20,24,28));
                DrawRect(bx,by,64f,1.0f,Color.FromArgb(90,150,160,168)); DrawRect(bx,by+63f,64f,1.0f,Color.FromArgb(60,150,160,168));
                TextDesign("AMOUNT",bx+32f,by+8f,0.12f,Color.FromArgb(185,220,220,220),GTA.UI.Font.RockstarTag,Alignment.Center);
                TextDesign(string.IsNullOrEmpty(amountInput)?"1":amountInput,bx+32f,by+27f,0.23f,Color.White,GTA.UI.Font.RockstarTag,Alignment.Center);
            }
            if (shopHeld != null && shopHeldPreview != null)
            {
                var sm = MouseDesign();
                if (sm.X > -1000f)
                {
                    DrawIcon(shopHeldPreview, sm.X-CellW*0.42f, sm.Y-CellH*0.42f, CellW*0.84f, CellH*0.84f);
                    DrawRect(sm.X-54f, sm.Y+CellH*0.45f, 108f, 22f, Color.FromArgb(230,4,5,7));
                    TextDesign("BUY  $"+(shopHeld.Price*Math.Max(1,shopHeldPreview.Amount)).ToString("N0"), sm.X, sm.Y+CellH*0.48f, 0.17f, Color.White, GTA.UI.Font.RockstarTag, Alignment.Center);
                }
            }
            if (held != null)
            {
                var m = MouseDesign();
                if (m.X > -1000f)
                    DrawIcon(held, m.X - (held.Width * CellW * 0.45f), m.Y - (held.Height * CellH * 0.45f), Math.Max(42f, held.Width * CellW * 0.9f), Math.Max(42f, held.Height * CellH * 0.9f));
            }
            // Hit-testing must use the exact same origins as rendering.  The previous
            // build added arbitrary offsets here, making the right-side loot panel and
            // some left-side item clicks land in the wrong cells.
            HandleMouse(leftX, gridY, rightX);
        }

        private void DrawPanel(float x, float y, float w, float h)
        {
            DrawRect(x, y, w, h, Color.FromArgb(218, 9, 12, 16));
            DrawRect(x, y, w, 1.5f, Color.FromArgb(130, 75, 80, 86));
            DrawRect(x, y + h - 1.5f, w, 1.5f, Color.FromArgb(70, 0, 0, 0));
        }
        private void DrawPanelHeader(string title, float x, float y, float width)
        {
            TextDesign(title, x, y, 0.30f, Color.FromArgb(245, 245, 245, 245), GTA.UI.Font.RockstarTag);
        }
        private void DrawCapacity(int count, float x, float y)
        { TextDesign(count.ToString(), x, y, 0.18f, Color.FromArgb(170, 210, 210, 210), GTA.UI.Font.RockstarTag, Alignment.Right); }
        private void DrawCash(float x, float y)
        {
            // Plain text only: no opaque badge behind the amount, so transparent icons and cash values remain clean.
            TextDesign("DIRTY CASH  $" + store.Cash.ToString("N0"), x + 70f, y+1f, 0.21f, Color.FromArgb(245, 245, 245, 245), GTA.UI.Font.RockstarTag, Alignment.Center);
        }

        private void DrawContainer(string title, InventoryGrid grid, float x, float y, PointF mouse)
        {
            if (grid == null) return;
            TextDesign(title, x, y - CellH * 0.34f, 0.30f, Color.FromArgb(245, 245, 245, 245), GTA.UI.Font.RockstarTag);
            DrawGrid(grid, x, y, mouse);
        }

        private void DrawEmptyContainer(string title, float x, float y, int cols, int rows, PointF mouse)
        {
            TextDesign(title, x, y - CellH * 0.34f, 0.30f, Color.FromArgb(245, 245, 245, 245), GTA.UI.Font.RockstarTag);
            int hoverCol = -1, hoverRow = -1;
            if (mouse.X >= x && mouse.X < x + cols * CellW && mouse.Y >= y && mouse.Y < y + rows * CellH)
            {
                hoverCol = (int)((mouse.X - x) / CellW);
                hoverRow = (int)((mouse.Y - y) / CellH);
            }
            for (int r = 0; r < rows; r++) for (int c = 0; c < cols; c++)
            {
                bool hover = c == hoverCol && r == hoverRow;
                float px=x+c*CellW, py=y+r*CellH, sw=CellW-3f, sh=CellH-3f;
                DrawRect(px,py,sw,sh,hover?Color.FromArgb(125,65,70,78):Color.FromArgb(78,10,13,18));
                DrawRect(px,py,sw,1.2f,hover?Color.FromArgb(235,220,225,230):Color.FromArgb(105,95,101,110));
            }
        }

        private void DrawGrid(InventoryGrid grid, float x, float y, PointF mouse)
        {
            if (grid == null) return;

            int hoverCol = -1, hoverRow = -1;
            if (mouse.X >= x && mouse.X < x + grid.Columns * CellW &&
                mouse.Y >= y && mouse.Y < y + grid.Rows * CellH)
            {
                hoverCol = (int)((mouse.X - x) / CellW);
                hoverRow = (int)((mouse.Y - y) / CellH);
            }

            // Empty slots are the only backgrounds. Items themselves are transparent PNGs
            // sitting over the slots, matching the QBCore/Horizon reference.
            for (int r = 0; r < grid.Rows; r++)
                for (int c = 0; c < grid.Columns; c++)
                {
                    bool hover = c == hoverCol && r == hoverRow;
                    float px = x + c * CellW, py = y + r * CellH;
                    float sw = CellW - 3f, sh = CellH - 3f;
                    DrawRect(px, py, sw, sh, hover ? Color.FromArgb(155, 55, 60, 66) : Color.FromArgb(112, 28, 31, 36));
                    DrawRect(px, py, sw, 1.2f, hover ? Color.FromArgb(235, 220, 225, 230) : Color.FromArgb(105, 95, 101, 110));
                    DrawRect(px, py + sh - 1.2f, sw, 1.2f, hover ? Color.FromArgb(220, 205, 210, 216) : Color.FromArgb(70, 70, 75, 82));
                    if (hover)
                    {
                        DrawRect(px, py, 1.2f, sh, Color.FromArgb(215, 205, 210, 218));
                        DrawRect(px + sw - 1.2f, py, 1.2f, sh, Color.FromArgb(215, 205, 210, 218));
                    }
                }

            foreach (var item in grid.Items)
            {
                Point p = ItemPositions.Get(item); if (p.X < 0) continue;
                float px = x + p.X * CellW + 3f, py = y + p.Y * CellH + 3f;
                float w = item.Width * CellW - 8f, h = item.Height * CellH - 8f;
                bool itemHover = hoverCol >= p.X && hoverCol < p.X + item.Width &&
                                  hoverRow >= p.Y && hoverRow < p.Y + item.Height;

                // Never draw an opaque item rectangle. This was the source of the dark grey
                // blocks behind guns/money in the previous build.
                if (itemHover)
                {
                    DrawRect(px, py, w, 1.4f, Color.FromArgb(210, 220, 225, 230));
                    DrawRect(px, py + h - 1.4f, w, 1.4f, Color.FromArgb(160, 200, 205, 212));
                }

                float iconW = Math.Max(26f, w - 10f);
                float iconH = Math.Max(26f, h - 18f);
                DrawIcon(item, px + (w - iconW) * 0.5f, py + 2f, iconW, iconH);

                // Labels are drawn AFTER the transparent PNG and get their own dark backing
                // so quantities/names can never disappear underneath a large icon.
                if (item.Amount > 1 || item.Id == "cash")
                {
                    string amountText = item.Id == "cash" ? "$" + item.Amount.ToString("N0") : item.Amount.ToString();
                    TextDesign(amountText, px + w - 7f, py + 5f, item.Id == "cash" ? 0.22f : 0.17f, Color.White, GTA.UI.Font.RockstarTag, Alignment.Right);
                }

                DrawRect(px + 2f, py + h - 17f, w - 4f, 16f, Color.FromArgb(205, 3, 5, 7));
                TextDesign(item.Name, px + w * 0.5f, py + h - 15f, 0.16f,
                    Color.FromArgb(240, 235, 238, 240), GTA.UI.Font.RockstarTag, Alignment.Center);
            }
        }

        private void DrawShop(ShopType type, float x, float y)
        {
            string title = shops.Name(type, gasUnderCounter);
            TextDesign(title, x, y - CellH * 0.34f, 0.30f, Color.FromArgb(245, 245, 245, 245), GTA.UI.Font.RockstarTag);
            if (type == ShopType.GasStation)
                TextDesign(gasUnderCounter ? "UNDER COUNTER [U]" : "SHOP [U]", x + 5f * CellW, y - CellH * 0.34f, 0.17f, Color.FromArgb(185, 205, 205, 205), GTA.UI.Font.RockstarTag, Alignment.Right);

            var stock = shops.Stock(type, gasUnderCounter);
            int col = 0, row = 0;
            foreach (var s in stock)
            {
                float px = x + col * CellW, py = y + row * CellH;
                PointF mouse = MouseDesign();
                bool hover = mouse.X >= px && mouse.X < px + CellW && mouse.Y >= py && mouse.Y < py + CellH;
                DrawRect(px, py, CellW - 3f, CellH - 3f, hover ? Color.FromArgb(155, 55, 60, 66) : Color.FromArgb(112, 28, 31, 36));
                DrawRect(px, py, CellW - 3f, 1.2f, hover ? Color.FromArgb(235, 220, 225, 230) : Color.FromArgb(105, 95, 101, 110));
                var item = ItemCatalog.Create(s.Id);
                if (item != null)
                {
                    DrawIcon(item, px + 6f, py + 3f, CellW - 15f, CellH - 17f);
                    TextDesign(item.Name, px + CellW * 0.5f, py + CellH - 17f, 0.15f, Color.FromArgb(220, 230, 230, 230), GTA.UI.Font.RockstarTag, Alignment.Center);
                    TextDesign("$" + s.Price.ToString("N0"), px + CellW - 5f, py + 4f, 0.15f, Color.FromArgb(230, 235, 235, 235), GTA.UI.Font.RockstarTag, Alignment.Right);
                }
                col++; if (col >= 5) { col = 0; row++; }
            }
            if(type==ShopType.CityHall) { TextDesign("WEAPON LICENSE REQUIRED FOR LEGAL FIREARMS",x+2.5f*CellW,y+3*CellH+CellH*0.42f,0.15f,Color.FromArgb(190,210,215,220),GTA.UI.Font.RockstarTag,Alignment.Center); }
            if(type==ShopType.Ammu)
            {
                float sy=y+3*CellH;
                DrawRect(x,sy,5*CellW-3f,CellH-3f,Color.FromArgb(125,35,38,44));
                TextDesign("SELL WEAPONS - DRAG GUN HERE",x+2.5f*CellW,sy+CellH*0.34f,0.20f,Color.FromArgb(235,110,175,255),GTA.UI.Font.RockstarTag,Alignment.Center);
                TextDesign("30% - 75% OF STORE VALUE",x+2.5f*CellW,sy+CellH*0.65f,0.15f,Color.FromArgb(190,210,215,220),GTA.UI.Font.RockstarTag,Alignment.Center);
            }
        }

        private void DrawContextMenu()
        {
            bool usable=contextItem!=null&&contextItem.Usable;
            float w=156f, h=contextIsLoot?48f:(usable?76f:48f);
            float x=Math.Max(6f,Math.Min(ScreenW-w-6f,contextX+8f));
            float y=Math.Max(6f,Math.Min(ScreenH-h-6f,contextY+8f));
            DrawRect(x,y,w,h,Color.FromArgb(218,24,27,31));
            DrawRect(x,y,w,18f,Color.FromArgb(225,4,5,7));
            if(contextIsLoot)
            {
                DrawRect(x+6f,y+23f,w-12f,20f,Color.FromArgb(80,4,5,7));
                TextDesign("PICK UP",x+14f,y+27f,0.19f,Color.White,GTA.UI.Font.RockstarTag);
            }
            else if(usable)
            {
                DrawRect(x+6f,y+22f,w-12f,24f,Color.FromArgb(80,4,5,7));
                TextDesign("USE",x+14f,y+27f,0.19f,Color.White,GTA.UI.Font.RockstarTag);
                DrawRect(x+6f,y+50f,w-12f,20f,Color.FromArgb(80,4,5,7));
                TextDesign("DROP",x+14f,y+54f,0.19f,Color.White,GTA.UI.Font.RockstarTag);
            }
            else
            {
                DrawRect(x+6f,y+22f,w-12f,20f,Color.FromArgb(80,4,5,7));
                TextDesign("DROP",x+14f,y+27f,0.19f,Color.White,GTA.UI.Font.RockstarTag);
            }
            contextRenderX=x; contextRenderY=y;
        }

        private void HandleMouse(float gx, float gy, float rightSideX)
        {
            var m = MouseDesign(); if (m.X < -1000f) return;
            bool left=MouseDown(0x01); bool right=MouseDown(0x02); if(held!=null)AdjustHeldAmount();

            if (right && !rightWasDown)
            {
                contextItem=HitGrid(store.Player,m.X,m.Y,gx,gy);contextGrid=contextItem==null?null:store.Player;contextIsLoot=false;
                if(contextItem==null&&shopType==ShopType.None){if(Game.Player.Character.IsInVehicle()){var v=loot.GetVehicle(Game.Player.Character.CurrentVehicle);if(v!=null){contextItem=HitGrid(v.Grid,m.X,m.Y,rightSideX,gy);contextGrid=contextItem==null?null:v.Grid;contextIsLoot=contextItem!=null;}}else{var n=loot.FindNearestLoot(Game.Player.Character.Position,cfg.LootRadius);if(n!=null){contextItem=HitGrid(n.Grid,m.X,m.Y,rightSideX,gy);contextGrid=contextItem==null?null:n.Grid;contextIsLoot=contextItem!=null;}}}
                if(contextItem==null)contextOpen=false;contextX=m.X;contextY=m.Y;contextOpen=contextItem!=null;
            }
            if (contextOpen && left && !leftWasDown)
            {
                bool usable=contextItem!=null&&contextItem.Usable;
                float rx=contextRenderX, ry=contextRenderY;
                bool inside=m.X>=rx && m.X<=rx+156f && m.Y>=ry && m.Y<=ry+(contextIsLoot?48f:(usable?76f:48f));
                if(inside)
                {
                    if(contextIsLoot && m.Y>=ry+20f){TakeFromLoot(contextGrid,contextItem);contextOpen=false;}
                    else if(!contextIsLoot && usable && m.Y>=ry+20f && m.Y<ry+48f){UseItem(contextItem);contextOpen=false;}
                    else if(!contextIsLoot && ((!usable && m.Y>=ry+20f)||(usable&&m.Y>=ry+48f))){DropItem(contextItem);contextOpen=false;}
                }
                else contextOpen=false;
                leftWasDown=left; rightWasDown=right; return;
            }

            if (left && !leftWasDown && !contextOpen)
            {
                if (shopType == ShopType.GasStation && m.X >= rightSideX + 4f * CellW && m.X <= rightSideX + 5f * CellW && m.Y >= gy - CellH * 0.45f && m.Y <= gy - 4f)
                { gasUnderCounter = !gasUnderCounter; shopHeld = null; shopHeldPreview = null; }
                else if(shopType!=ShopType.None)
                {
                    shopHeld=HitShop(m.X,m.Y,rightSideX,gy);
                    if(shopHeld!=null)
                    {
                        if(shopHeld.Id.EndsWith("_ammo",StringComparison.OrdinalIgnoreCase)&&(GetAsyncKeyState(0x10)&0x8000)!=0)
                        { shops.QuickBuyAmmo(shopHeld,10); shopHeld=null; shopHeldPreview=null; }
                        else { int req=RequestedAmount(); if(req<=0) req=Math.Max(1,(store.Cash+store.BankBalance)/Math.Max(1,shopHeld.Price)); int max=shopHeld.Id=="pistol"||shopHeld.Id=="combat_pistol"||shopHeld.Id=="machine_pistol"||shopHeld.Id=="smg"||shopHeld.Id=="pump_shotgun"||shopHeld.Id=="carbine_rifle"||shopHeld.Id=="assault_rifle"||shopHeld.Id=="heavy_pistol"||shopHeld.Id=="knife"||shopHeld.Id=="suppressor"?1:req; shopHeldPreview=ItemCatalog.Create(shopHeld.Id,Math.Min(req,max)); }
                    }
                }

                if (shopHeld == null)
                {
                    var hit = HitGrid(store.Player, m.X, m.Y, gx, gy);
                    if(hit!=null){ BeginDrag(store.Player,hit,RequestedAmount()); }
                    else if (shopType == ShopType.None)
                    {
                        InventoryGrid source = null;
                        if (backpackOpen) source=store.Backpack;
                        else if (Game.Player.Character.IsInVehicle())
                        {
                            var v = loot.GetVehicle(Game.Player.Character.CurrentVehicle);
                            if (v != null && InGrid(m.X,m.Y,rightSideX,gy,v.Grid.Columns,v.Grid.Rows)) source=v.Grid;
                        }
                        else
                        {
                            var near = loot.FindNearestLoot(Game.Player.Character.Position, cfg.LootRadius);
                            if (near != null && InGrid(m.X,m.Y,rightSideX,gy,near.Grid.Columns,near.Grid.Rows)) source=near.Grid;
                        }
                        if (source != null)
                        {
                            var lootItem = HitGrid(source, m.X, m.Y, rightSideX, gy);
                            if (lootItem != null) { BeginDrag(source,lootItem,RequestedAmount()); }
                        }
                    }
                }
            }
            if (!left && leftWasDown)
            {
                if(shopHeld!=null){TryBuyDrop(shopHeld,m.X,m.Y,gx,gy,Math.Max(1,shopHeldPreview==null?(RequestedAmount()<=0?99999:RequestedAmount()):shopHeldPreview.Amount));shopHeld=null;shopHeldPreview=null;}else if(held!=null){if(shopType==ShopType.Ammu&&held.Weapon!=null&&IsShopSellZone(m.X,m.Y,rightSideX,gy)){int sale=shops.SellWeapon(held);RemoveWeaponFromPed(held);store.AddCash(sale);Notifications.PushItem("ADDED",sale,ItemCatalog.Create("cash",sale));held=null;store.MoneySyncSuspended=false;}else{if(!TryPlaceHeld(m.X,m.Y,gx,gy,rightSideX))CancelDrag();held=null;store.MoneySyncSuspended=false;}}
            }
            leftWasDown = left; rightWasDown = right;
        }

        private void AdjustHeldAmount(){if(held==null||held.Amount<=0||heldOrigin==null)return;bool up=Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED,0,14);bool down=Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED,0,15);if(!up&&!down)return;var source=heldOrigin.Items.FirstOrDefault(i=>i!=held&&i.Id==held.Id&&!i.SyncedAmmo&&!i.SyncedWeapon);if(up&&source!=null&&source.Amount>0){source.Amount--;if(source.Amount<=0)heldOrigin.Remove(source);held.Amount++;}else if(down&&held.Amount>1){held.Amount--;if(source!=null)source.Amount++;else {var back=ItemCatalog.Create(held.Id,1);if(back!=null)heldOrigin.Add(back);}}}
        private void BeginDrag(InventoryGrid source, Item item, int amount=1)
        {
            if(source==null||item==null)return;
            if(amount<=0) amount=item.Amount;
            amount=Math.Max(1,Math.Min(amount,item.Amount));
            if(source==store.Player && item.Id=="cash") store.MoneySyncSuspended=true;
            heldOrigin=source;
            if(amount<item.Amount){item.Amount-=amount;held=item.Clone();held.Amount=amount;held.Rotated=item.Rotated;}
            else {held=item;source.Remove(item);}
        }
        private bool IsShopSellZone(float mx,float my,float x,float y){return shopType==ShopType.Ammu&&mx>=x&&mx<x+5*CellW&&my>=y+3*CellH&&my<y+4*CellH;}
        private ShopItem HitShop(float mx, float my, float x, float y)
        {
            int col = 0, row = 0;
            foreach (var s in shops.Stock(shopType, gasUnderCounter))
            {
                float px = x + col * CellW, py = y + row * CellH;
                if (mx >= px && mx <= px + CellW && my >= py && my <= py + CellH) return s;
                col++; if (col >= 5) { col = 0; row++; }
            }
            return null;
        }
        private Item HitGrid(InventoryGrid g, float mx, float my, float x, float y)
        {
            if (g == null) return null;
            foreach (var i in g.Items)
            {
                Point p = ItemPositions.Get(i); if (p.X < 0) continue;
                float ix = x + p.X * CellW, iy = y + p.Y * CellH;
                if (mx >= ix && mx <= ix + i.Width * CellW && my >= iy && my <= iy + i.Height * CellH) return i;
            }
            return null;
        }
        private bool TryBuyDrop(ShopItem s, float mx, float my, float gx, float gy, int amount)
        {
            if(!InGrid(mx,my,gx,gy,store.Player.Columns,store.Player.Rows)) return false;
            Point c=ToCell(mx,my,gx,gy);
            return shops.PurchaseAt(s,store.Player,c.X,c.Y,shopType,Math.Max(1,amount));
        }
        private bool TryPlaceHeld(float mx, float my, float gx, float gy, float rightX)
        {
            if (held == null) return false;
            var me = Game.Player.Character;
            InventoryGrid target = null; float x = gx, y = gy;
            bool targetIsPlayer = false;

            if (InGrid(mx, my, gx, gy, store.Player.Columns, store.Player.Rows))
            {
                target = store.Player; targetIsPlayer = true;
            }
            else if (backpackOpen)
            { target = store.Backpack; x = rightX; }
            else if (me.IsInVehicle())
            {
                var v = loot.GetVehicle(me.CurrentVehicle);
                if (v != null && InGrid(mx, my, rightX, gy, v.Grid.Columns, v.Grid.Rows)) { target = v.Grid; x = rightX; }
            }
            else
            {
                var n = loot.FindNearestLoot(me.Position, cfg.LootRadius);
                if (n != null && InGrid(mx, my, rightX, gy, n.Grid.Columns, n.Grid.Rows)) { target = n.Grid; x = rightX; }
            }

            // Dropping outside either inventory panel means dropping into the world.
            if (target == null) { DropToWorld(held); return true; }
            if (!targetIsPlayer && held.Id == "cash") return false;

            Point cell = ToCell(mx, my, x, y);
            var atCell = target.Items.FirstOrDefault(i => { Point p = ItemPositions.Get(i); return p.X == cell.X && p.Y == cell.Y; });
            if (atCell == null && !target.CanPlace(held, cell.X, cell.Y)) return false;
            if (atCell != null && (atCell.Id != held.Id || atCell.SyncedAmmo || atCell.SyncedWeapon || atCell.Weapon != null)) return false;
            if (targetIsPlayer && held.Id == "cash" && heldOrigin != store.Player)
            {
                // Cash picked up from the ground updates the actual GTA character balance and
                // merges into the existing MONEY stack instead of creating a second cash item.
                store.AddCash(held.Amount);
                if (heldOrigin != null) heldOrigin.Remove(held);
                Notifications.PushItem("ADDED", held.Amount, ItemCatalog.Create("cash", held.Amount));
                return true;
            }

            if (targetIsPlayer)
            {
                if (held.Weapon != null)
                {
                    held.SyncedWeapon = true;
                    target.Set(held, cell.X, cell.Y); target.Items.Add(held);
                    GiveWeaponToPlayer(held);
                    Notifications.PushItem("ADDED", held.Amount, held);
                    return true;
                }
                if (atCell != null) { atCell.Amount += held.Amount; Notifications.PushItem("ADDED",held.Amount,held); return true; }
            }

            target.Set(held, cell.X, cell.Y); target.Items.Add(held);
            if (target != store.Player && held.Weapon != null) { held.SyncedWeapon = false; RemoveWeaponFromPed(held); }
            Notifications.PushItem("ADDED",held.Amount,held);
            return true;
        }
        private Point FindNearestPlacement(InventoryGrid grid, Item item, int desiredX, int desiredY)
        {
            Point best=new Point(-1,-1); int bestDist=int.MaxValue;
            for(int y=0;y<grid.Rows;y++) for(int x=0;x<grid.Columns;x++)
            {
                if(!grid.CanPlace(item,x,y)) continue;
                int d=Math.Abs(x-desiredX)+Math.Abs(y-desiredY);
                if(d<bestDist){bestDist=d;best=new Point(x,y);}
            }
            return best;
        }

        private void DropToWorld(Item item)
        {
            if (item == null) return;
            if(item.Id=="cash" && heldOrigin==store.Player)store.SetMoney(Math.Max(0,store.Cash-item.Amount));
            if (item.SyncedWeapon) { RemoveWeaponFromPed(item); item.SyncedWeapon=false; }
            var c = new LootContainer("drop_" + Guid.NewGuid(), Game.Player.Character.Position + Game.Player.Character.ForwardVector * 1.2f, cfg.NearbyColumns, cfg.NearbyRows);
            c.Grid.Add(item); loot.AddDropped(c); Notifications.PushItem("REMOVED", item.Amount, item);
        }
        private void CancelDrag(){if(held==null||heldOrigin==null){store.MoneySyncSuspended=false;return;}if(heldOrigin==store.Player){if(held.Id=="cash")store.SetMoney(store.Cash+held.Amount);else store.AddToPlayer(held);}else heldOrigin.Add(held);store.MoneySyncSuspended=false;}
        private void GiveWeaponToPlayer(Item item)
        { if(item==null||item.Weapon==null)return; Function.Call(Hash.GIVE_WEAPON_TO_PED,Game.Player.Character.Handle,unchecked((int)Game.GenerateHash(item.Weapon)),0,false,true); }
        private void RemoveWeaponFromPed(Item item)
        {
            if (item == null || item.Weapon == null) return;
            Function.Call(Hash.REMOVE_WEAPON_FROM_PED, Game.Player.Character.Handle, unchecked((int)Game.GenerateHash(item.Weapon)));
        }
        private void TakeFromLoot(InventoryGrid from, Item item)
        {
            if (item == null) return;
            if (item.Id == "cash") { store.AddCash(item.Amount); from.Remove(item); Notifications.PushItem("ADDED", item.Amount, ItemCatalog.Create("cash", item.Amount)); return; }
            if(item.Weapon!=null){var gun=item.Clone();gun.SyncedWeapon=true;if(store.AddToPlayer(gun)){GiveWeaponToPlayer(gun);from.Remove(item);Notifications.PushItem("ADDED",1,gun);}else Notifications.Push("INVENTORY FULL",true);return;}
            if (store.AddToPlayer(item.Clone())) { from.Remove(item); Notifications.PushItem("ADDED", item.Amount, item); }
        }
        private void UseItem(Item item)
        {
            if (item == null || !store.Player.Items.Contains(item)) return;
            if (item.Id == "backpack") { store.ExpandForBackpack(); backpackOpen = true; contextOpen = false; Notifications.Push("BACKPACK STORAGE OPEN"); return; }
            if (item.Id == "armor") { Game.Player.Character.Armor = 100; RemoveOne(item); Notifications.PushItem("REMOVED", 1, item); }
            else if (item.Id == "water_bottle" || item.Id == "energy_drink" || item.Id == "coffee")
            {
                SurvivalNeeds.Drink(item.Id);
                RemoveOne(item);
                Notifications.PushItem("REMOVED", 1, item);
            }
            else if(item.Id=="medkit"||item.Id=="firstaid"||item.Id=="bandage"){Game.Player.Character.Health=Math.Min(Game.Player.Character.MaxHealth,Game.Player.Character.Health+(item.Id=="medkit"?75:30));RemoveOne(item);Notifications.PushItem("REMOVED",1,item);}
            else if(item.Id=="food"||item.Id=="sandwich"||item.Id=="burger"||item.Id=="chips_bag"||item.Id=="chocolate_bar"||item.Id=="donut"){SurvivalNeeds.Eat(item.Id);RemoveOne(item);Notifications.PushItem("REMOVED",1,item);}
            else if(item.Id=="backpack"){backpackOpen=true;Notifications.Push("BACKPACK STORAGE OPEN");}
            else if (item.Id == "repairkit") { var v=Game.Player.Character.IsInVehicle()?Game.Player.Character.CurrentVehicle:World.GetNearbyVehicles(Game.Player.Character.Position,4f).FirstOrDefault(); if(v==null){Notifications.Push("NEAR A VEHICLE REQUIRED",true);return;} v.Repair(); RemoveOne(item); Notifications.PushItem("REMOVED",1,item); }
            else if(item.Id=="baggie")
            {
                var bud=store.Player.Items.FirstOrDefault(i=>i.Id=="weed_bud"&&i.Amount>0);
                if(bud==null){Notifications.Push("NEED WEED BUD",true);return;}
                RemoveOne(item); RemoveOne(bud); var bagged=ItemCatalog.Create("weed_brick",1); if(store.AddToPlayer(bagged)) Notifications.PushItem("ADDED",1,bagged); else Notifications.Push("INVENTORY FULL",true);
            }
            else if(item.Id=="suppressor")
            {
                int wh=Function.Call<int>(Hash.GET_SELECTED_PED_WEAPON,Game.Player.Character.Handle);
                string comp=SuppressorComponentFor(wh);
                if(string.IsNullOrEmpty(comp)){Notifications.Push("HOLD A COMPATIBLE GUN",true);return;}
                Function.Call(Hash.GIVE_WEAPON_COMPONENT_TO_PED,Game.Player.Character.Handle,wh,unchecked((int)Game.GenerateHash(comp))); RemoveOne(item); Notifications.Push("SUPPRESSOR ATTACHED");
            }
            else if(item.Id=="jerry_can") { Notifications.Push("JERRY CAN READY"); }
            else if(item.Id=="whiskey"||item.Id=="wine") { DrugEffects.Start("alcohol"); RemoveOne(item); Notifications.PushItem("REMOVED",1,item); }
            else if(item.Id=="phone"){bankOpen=true;Notifications.Push("BANK APP OPEN");}
            else if (IsDrug(item.Id)) { DrugEffects.Start(item.Id); RemoveOne(item); Notifications.PushItem("REMOVED", 1, item); }
            else { RemoveOne(item); Notifications.PushItem("REMOVED", 1, item); }
        }
        private void HandleBankKeys()
        {
            if(Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED,0,200)||Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED,0,177)){bankOpen=false;return;}
            if(Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED,0,29)){ if(store.Cash<=0){Notifications.Push("NO CASH",true);return;} int amount=store.Cash; if(amount>cfg.BankDepositLimit){store.SetMoney(0);store.ClearBank();Function.Call(Hash.SET_PLAYER_WANTED_LEVEL,Game.Player.Handle,5,false);Function.Call(Hash.SET_PLAYER_WANTED_LEVEL_NOW,Game.Player.Handle,false);Notifications.Push("BANK FRAUD DETECTED",true);} else {store.DepositToBank(amount);Notifications.Push("DEPOSITED $"+amount.ToString("N0"));} bankOpen=false;}
        }
        private void DrawBankApp()
        {
            float w=420f,h=250f,x=(ScreenW-w)/2f,y=(ScreenH-h)/2f;DrawRect(x,y,w,h,Color.FromArgb(240,18,21,25));DrawRect(x,y,w,38f,Color.FromArgb(250,4,5,7));TextDesign("BANK APP",x+w/2f,y+10f,0.28f,Color.White,GTA.UI.Font.RockstarTag,Alignment.Center);TextDesign("BANK BALANCE: $"+store.BankBalance.ToString("N0"),x+w/2f,y+72f,0.23f,Color.White,GTA.UI.Font.RockstarTag,Alignment.Center);TextDesign("CASH: $"+store.Cash.ToString("N0"),x+w/2f,y+105f,0.23f,Color.White,GTA.UI.Font.RockstarTag,Alignment.Center);DrawRect(x+55f,y+145f,w-110f,45f,Color.FromArgb(250,4,5,7));TextDesign("DEPOSIT ALL CASH  [B]",x+w/2f,y+158f,0.21f,Color.White,GTA.UI.Font.RockstarTag,Alignment.Center);TextDesign("ESC TO CLOSE",x+w/2f,y+h-28f,0.17f,Color.FromArgb(180,210,210,210),GTA.UI.Font.RockstarTag,Alignment.Center);}
        private static string SuppressorComponentFor(int weaponHash)
        {
            string[] pi={"WEAPON_PISTOL","WEAPON_PISTOL_MK2","WEAPON_COMBATPISTOL","WEAPON_HEAVYPISTOL","WEAPON_MACHINEPISTOL","WEAPON_SMG","WEAPON_MINISMG","WEAPON_ASSAULTSMG"};
            foreach(var w in pi) if(unchecked((int)Game.GenerateHash(w))==weaponHash) return "COMPONENT_AT_PI_SUPP";
            string[] ar={"WEAPON_CARBINERIFLE","WEAPON_ASSAULTRIFLE","WEAPON_ADVANCEDRIFLE","WEAPON_BULLPUPRIFLE","WEAPON_COMBATMG","WEAPON_MG"};
            foreach(var w in ar) if(unchecked((int)Game.GenerateHash(w))==weaponHash) return "COMPONENT_AT_AR_SUPP_02";
            string[] sr={"WEAPON_SNIPERRIFLE","WEAPON_HEAVYSNIPER"};
            foreach(var w in sr) if(unchecked((int)Game.GenerateHash(w))==weaponHash) return "COMPONENT_AT_AR_SUPP_02";
            return null;
        }
        private static bool IsDrug(string id)
        { return id == "joint" || id == "weed_bud" || id == "cokebaggy" || id == "crack_baggy" || id == "meth_bag" || id == "xtcbaggy" || id == "oxy" || id == "morphine"; }
        private void DropItem(Item item)
        {
            if (item == null) return;
            // Only player inventory items are removed here. A vicinity item right-clicked in the
            // secondary panel is simply left in place until it is dragged/taken.
            if (!store.Player.Items.Contains(item)) return;
            if (item.Id == "cash")
            {
                int amount = item.Amount;
                store.SetMoney(0);
                store.Player.Remove(item);
                var cashDrop = ItemCatalog.Create("cash", amount);
                var cashContainer = new LootContainer("drop_" + Guid.NewGuid(), Game.Player.Character.Position + Game.Player.Character.ForwardVector * 1.2f, cfg.NearbyColumns, cfg.NearbyRows);
                if (cashDrop != null) cashContainer.Grid.Add(cashDrop);
                loot.AddDropped(cashContainer);
                Notifications.PushItem("REMOVED", amount, ItemCatalog.Create("cash", amount));
                return;
            }
            int removedAmount = item.Amount;
            store.Player.Remove(item); if (item.SyncedWeapon) { RemoveWeaponFromPed(item); item.SyncedWeapon=false; }
            var c = new LootContainer("drop_" + Guid.NewGuid(), Game.Player.Character.Position + Game.Player.Character.ForwardVector * 1.2f, cfg.NearbyColumns, cfg.NearbyRows);
            c.Grid.Add(item); loot.AddDropped(c); Notifications.PushItem("REMOVED", removedAmount, item);
        }
        private void RemoveOne(Item item) { item.Amount--; if (item.Amount <= 0) store.Player.Remove(item); }
        private bool InGrid(float mx, float my, float x, float y, int cols, int rows)
        { return mx >= x && mx <= x + cols * CellW && my >= y && my <= y + rows * CellH; }
        private Point ToCell(float mx, float my, float x, float y)
        { return new Point((int)((mx - x) / CellW), (int)((my - y) / CellH)); }

        private void DrawIcon(Item item, float x, float y, float w, float h)
        {
            string path = IconPath(item == null ? null : item.Icon);
            if (path == null) return;
            try
            {
                // CustomSprite is the SHVDN v3 supported loader for external PNGs.
                // Its Draw() API uses the 1280x720 UI design space, so convert our
                // actual-resolution pixel rectangle before drawing.
                float bx = x / ScreenW * 1280f;
                float by = y / ScreenH * 720f;
                float bw = w / ScreenW * 1280f;
                float bh = h / ScreenH * 720f;
                // Do not retain CustomSprite instances across frames. SHVDN's sprite wrapper
                // owns native texture state; recreating it per draw avoids stale/native handles.
                var sprite = new CustomSprite(path, new SizeF(bw,bh), new PointF(bx,by), Color.FromArgb((int)(255f*OpenAlpha),255,255,255), item != null && item.Rotated ? 90f : 0f, false);
                sprite.Draw();
            }
            catch { }
        }
        private static string IconPath(string icon)
        {
            if (string.IsNullOrEmpty(icon)) return null;
            string dllDir = Path.GetDirectoryName(typeof(Main).Assembly.Location) ?? "";
            var candidates = new[] { Path.Combine(dllDir, "inventory_icons", icon), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts", "inventory_icons", icon), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "inventory_icons", icon) };
            return candidates.FirstOrDefault(File.Exists);
        }
        private void DrawRect(float x, float y, float w, float h, Color c)
        { int a=(int)(c.A*OpenAlpha); Function.Call(Hash.DRAW_RECT, (X(x) + S(w) / 2f) / GTA.UI.Screen.Resolution.Width, (Y(y) + S(h) / 2f) / GTA.UI.Screen.Resolution.Height, S(w) / GTA.UI.Screen.Resolution.Width, S(h) / GTA.UI.Screen.Resolution.Height, c.R, c.G, c.B, a); }
        private void TextDesign(string s, float x, float y, float scaleText, Color c, GTA.UI.Font f, Alignment a = Alignment.Left)
        {
            if (string.IsNullOrEmpty(s)) return;
            // SHVDN TextElement.Draw() uses its documented 1280x720 design space.
            // Our layout is calculated in physical pixels, so convert exactly once.
            float ux = x / Math.Max(1f, ScreenW) * GTA.UI.Screen.Width;
            float uy = y / Math.Max(1f, ScreenH) * GTA.UI.Screen.Height;
            new TextElement(s, new PointF(ux, uy), scaleText, Color.FromArgb((int)(c.A*OpenAlpha),c.R,c.G,c.B), f, a, true, false).Draw();
        }
        private void DisableGameplay()
        { Function.Call(Hash.DISABLE_ALL_CONTROL_ACTIONS, 0); Function.Call(Hash.SET_MOUSE_CURSOR_THIS_FRAME); }
        public void Dispose() { }

        private void SyncWeapons()
        {
            var ped = Game.Player.Character; if (ped == null || !ped.Exists()) return;
            var owned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int selectedWeapon = Function.Call<int>(Hash.GET_SELECTED_PED_WEAPON, ped.Handle);
            var selectedDef = ItemCatalog.All().FirstOrDefault(x => x.Weapon != null && unchecked((int)Game.GenerateHash(x.Weapon)) == selectedWeapon);
            if (selectedDef != null) owned.Add(selectedDef.Id);
            foreach (var d in ItemCatalog.All().Where(x => x.Weapon != null))
            {
                int h = unchecked((int)Game.GenerateHash(d.Weapon));
                if (!Function.Call<bool>(Hash.HAS_PED_GOT_WEAPON, ped.Handle, h, false)) continue;
                owned.Add(d.Id);
                if (held != null && held.Weapon != null && held.Id.Equals(d.Id, StringComparison.OrdinalIgnoreCase)) continue;
                var existing = store.Player.Items.FirstOrDefault(x => x.SyncedWeapon && x.Id.Equals(d.Id, StringComparison.OrdinalIgnoreCase));
                if (existing == null) { var w = ItemCatalog.Create(d.Id); w.SyncedWeapon = true; if(!store.Player.Add(w)) continue; }

            }
            foreach (var old in store.Player.Items.Where(x => x.SyncedWeapon && !owned.Contains(x.Id)).ToList()) store.Player.Remove(old);

            // Ammo is independent inventory stock. Shots do not remove inventory ammo;
            // when the player begins a reload we charge only for bullets fired since the last reload.
        }

        private int reloadStartClip;
        private void TrackReloadAndAmmo()
        {
            var ped=Game.Player.Character; if(ped==null||!ped.Exists()) return;
            int wh=Function.Call<int>(Hash.GET_SELECTED_PED_WEAPON,ped.Handle);
            var def=ItemCatalog.All().FirstOrDefault(x=>x.Weapon!=null && unchecked((int)Game.GenerateHash(x.Weapon))==wh);
            if(def==null || string.IsNullOrEmpty(def.AmmoType)) return;
            bool reloading=Function.Call<bool>(Hash.IS_PED_RELOADING,ped.Handle);
            if(reloading && !wasReloading)
            {
                reloadWeaponHash=wh;
                reloadAmmoItem=ItemCatalog.AmmoItemFor(def.AmmoType);
                reloadStartClip=Math.Max(0,Function.Call<int>(Hash.GET_AMMO_IN_CLIP,ped.Handle,wh));
            }
            if(!reloading && wasReloading && reloadWeaponHash==wh)
            {
                int clipAfter=Math.Max(0,Function.Call<int>(Hash.GET_AMMO_IN_CLIP,ped.Handle,wh));
                int loaded=Math.Max(0,clipAfter-reloadStartClip);
                if(loaded>0 && !string.IsNullOrEmpty(reloadAmmoItem))
                {
                    var stock=store.Player.Items.FirstOrDefault(i=>i.Id==reloadAmmoItem&&!i.SyncedAmmo&&!i.SyncedWeapon);
                    int removed=Math.Min(loaded,stock==null?0:stock.Amount);
                    if(removed>0)
                    {
                        stock.Amount-=removed;
                        if(stock.Amount<=0)store.Player.Remove(stock);
                        Notifications.PushItem("REMOVED",removed,ItemCatalog.Create(reloadAmmoItem,removed));
                    }
                }
                reloadWeaponHash=0; reloadAmmoItem=null; reloadStartClip=0;
            }
            wasReloading=reloading;
        }
    }

    public sealed class TrapManager
    {
        private readonly InventoryStore store;
        private readonly LootManager loot;
        private readonly ShopManager shops;
        private readonly Random rng=new Random();
        private readonly Vector3 trapPoint=new Vector3(132.5f,-1300.6f,29.2f);
        private int blip;
        private Ped client;
        private bool working;
        private DateTime nextClient=DateTime.MinValue;
        private DateTime handoffUntil=DateTime.MinValue;

        public TrapManager(InventoryStore s,LootManager l,ShopManager sh){store=s;loot=l;shops=sh;blip=CreateBlip();}
        private int CreateBlip()
        {
            try{int h=Function.Call<int>(Hash.ADD_BLIP_FOR_COORD,trapPoint.X,trapPoint.Y,trapPoint.Z);Function.Call(Hash.SET_BLIP_SPRITE,h,280);Function.Call(Hash.SET_BLIP_COLOUR,h,2);Function.Call(Hash.BEGIN_TEXT_COMMAND_SET_BLIP_NAME,"STRING");Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME,"Trap");Function.Call(Hash.END_TEXT_COMMAND_SET_BLIP_NAME,h);return h;}catch{return 0;}
        }
        private bool HasDrugs(){return store.Player.Items.Any(i=>IsDrug(i.Id)&&i.Amount>0);}
        private static bool IsDrug(string id){return id=="joint"||id=="weed_bud"||id=="weed_brick"||id=="cokebaggy"||id=="crack_baggy"||id=="meth_bag"||id=="xtcbaggy"||id=="oxy"||id=="morphine";}
        public bool TryInteract(Ped ped)
        {
            if(ped==null||!ped.Exists()||ped.Position.DistanceTo(trapPoint)>4.5f)return false;
            if(working){working=false;DeleteClient();Notifications.Push("TRAP WORK STOPPED");return true;}
            if(!HasDrugs()){Notifications.Push("NO DRUGS TO SELL",true);return true;}
            working=true;nextClient=DateTime.UtcNow.AddSeconds(1);Notifications.Push("TRAP WORK STARTED");return true;
        }
        public void Tick()
        {
            var ped=Game.Player.Character;if(ped==null||!ped.Exists())return;
            if(!working)return;
            if(!HasDrugs()){working=false;DeleteClient();Notifications.Push("OUT OF DRUGS");return;}
            if(client!=null&&client.Exists())
            {
                float d=client.Position.DistanceTo(ped.Position);
                if(handoffUntil==DateTime.MinValue && d<2.4f) BeginHandoff(ped);
                if(handoffUntil!=DateTime.MinValue && DateTime.UtcNow>=handoffUntil) FinishHandoff(ped);
                return;
            }
            if(DateTime.UtcNow>=nextClient) SpawnClient(ped);
        }
        private void SpawnClient(Ped ped)
        {
            try
            {
                string modelName=new[]{"a_m_m_skater_01","a_m_y_hipster_01","a_f_y_hipster_01","a_m_m_business_01"}[rng.Next(4)];
                var model=new Model(modelName);model.Request(1200);if(!model.IsInCdImage||!model.IsValid){nextClient=DateTime.UtcNow.AddSeconds(3);return;}
                double a=rng.NextDouble()*Math.PI*2;Vector3 pos=trapPoint+new Vector3((float)Math.Cos(a)*12f,(float)Math.Sin(a)*12f,0f);
                client=World.CreatePed(model,pos);model.MarkAsNoLongerNeeded();
                if(client==null||!client.Exists()){nextClient=DateTime.UtcNow.AddSeconds(3);return;}
                client.IsPersistent=true;client.BlockPermanentEvents=true;client.CanRagdoll=true;Function.Call(Hash.TASK_GO_TO_ENTITY,client.Handle,ped.Handle,-1,2.0f,2.0f,1073741824,0);
            }catch{nextClient=DateTime.UtcNow.AddSeconds(3);}
        }
        private void BeginHandoff(Ped ped)
        {
            handoffUntil=DateTime.UtcNow.AddSeconds(2.0);
            try
            {
                Function.Call(Hash.REQUEST_ANIM_DICT,"mp_common");
                Function.Call(Hash.TASK_PLAY_ANIM,ped.Handle,"mp_common","givetake1_a",8f,-8f,2000,0,0f,false,false,false);
                if(client!=null&&client.Exists()) Function.Call(Hash.TASK_PLAY_ANIM,client.Handle,"mp_common","givetake1_a",8f,-8f,2000,0,0f,false,false,false);
            }catch{}
        }
        private void FinishHandoff(Ped ped)
        {
            handoffUntil=DateTime.MinValue;
            var choices=store.Player.Items.Where(i=>IsDrug(i.Id)&&i.Amount>0).ToList();
            if(choices.Count==0){working=false;DeleteClient();return;}
            var item=choices[rng.Next(choices.Count)];
            int desired=rng.Next(1,9);int sold=Math.Min(desired,item.Amount);
            item.Amount-=sold;if(item.Amount<=0)store.Player.Remove(item);
            int unitPrice=item.Id=="weed_bud"?150:item.Id=="weed_brick"?500:item.Id=="joint"?100:item.Id=="oxy"?450:item.Id=="morphine"?550:item.Id=="xtcbaggy"?600:item.Id=="meth_bag"?700:item.Id=="crack_baggy"?650:800;
            int payout=sold*unitPrice;if(rng.Next(100)<12)payout*=2;
            store.AddCash(payout);Notifications.PushItem("REMOVED",sold,item);Notifications.PushItem("ADDED",payout,ItemCatalog.Create("cash",payout));
            DeleteClient();nextClient=DateTime.UtcNow.AddSeconds(rng.Next(2,6));
        }
        private void DeleteClient(){try{if(client!=null&&client.Exists())client.Delete();}catch{}client=null;handoffUntil=DateTime.MinValue;}
        public void Draw()
        {
            var ped=Game.Player.Character;if(ped==null||!ped.Exists())return;
            float d=ped.Position.DistanceTo(trapPoint);
            if(d<16f)Function.Call(Hash.DRAW_MARKER,1,trapPoint.X,trapPoint.Y,trapPoint.Z-1f,0f,0f,0f,0f,0f,0f,2f,2f,0.4f,70,190,90,160,false,true,2,false,null,null,false);
            if(d<5f && !working && HasDrugs())new TextElement("~b~E~s~  START TRAP WORK",new PointF(640,665),0.30f,Color.White,GTA.UI.Font.ChaletLondon,Alignment.Center).ScaledDraw();
            else if(d<5f && working)new TextElement("~b~E~s~  STOP TRAP WORK",new PointF(640,665),0.30f,Color.White,GTA.UI.Font.ChaletLondon,Alignment.Center).ScaledDraw();
        }
        public void Dispose(){DeleteClient();if(blip!=0){try{int h=blip;Function.Call(Hash.REMOVE_BLIP,h);}catch{}}}
    }

    public sealed class SurvivalNeeds
    {
        private readonly InventoryStore store;
        private DateTime nextDecay = DateTime.UtcNow.AddSeconds(45);
        private static SurvivalNeeds instance;
        private static DateTime animationUntil = DateTime.MinValue;

        public SurvivalNeeds(InventoryStore s){store=s;instance=this;}

        public void Tick()
        {
            var ped=Game.Player.Character;
            if(ped==null||!ped.Exists()) return;
            var now=DateTime.UtcNow;
            if(now>=nextDecay)
            {
                // Gentle survival drain: thirst falls slightly faster than hunger.
                store.Hunger=Math.Max(0f,store.Hunger-0.75f);
                store.Thirst=Math.Max(0f,store.Thirst-1.0f);
                nextDecay=now.AddSeconds(45);
                if(store.Hunger<=0f || store.Thirst<=0f)
                    ped.Health=Math.Max(1,ped.Health-1);
                if(now.Second%15==0) store.Save();
            }
            if(animationUntil!=DateTime.MinValue && now>=animationUntil)
            {
                Function.Call(Hash.CLEAR_PED_TASKS, ped.Handle);
                animationUntil=DateTime.MinValue;
            }
        }

        public static void Eat(string id)
        {
            if(instance==null) return;
            float gain = id=="burger"?24f:id=="sandwich"?20f:id=="chips_bag"?12f:id=="chocolate_bar"?10f:id=="donut"?14f:18f;
            instance.store.Hunger=Math.Min(100f,instance.store.Hunger+gain);
            StartScenario("WORLD_HUMAN_EATING",2.2f);
        }
        public static void Drink(string id)
        {
            if(instance==null) return;
            float gain = id=="energy_drink"?30f:id=="coffee"?18f:28f;
            instance.store.Thirst=Math.Min(100f,instance.store.Thirst+gain);
            StartScenario("WORLD_HUMAN_DRINKING",2.0f);
        }
        private static void StartScenario(string scenario,float seconds)
        {
            var ped=Game.Player.Character;if(ped==null||!ped.Exists())return;
            try { Function.Call(Hash.TASK_START_SCENARIO_IN_PLACE,ped.Handle,scenario,0,true); animationUntil=DateTime.UtcNow.AddSeconds(seconds); } catch { }
        }
        public void DrawHud()
        {
            float sw=GTA.UI.Screen.Resolution.Width, sh=GTA.UI.Screen.Resolution.Height;
            float x=sw*0.018f, y=sh*0.035f, w=190f, h=10f;
            DrawBar(x,y,w,h,store.Hunger,"HUNGER",70,190,85);
            DrawBar(x,y+23f,w,h,store.Thirst,"THIRST",75,160,240);
        }
        private static void DrawBar(float x,float y,float w,float h,float value,string label,int r,int g,int b)
        {
            float sw=GTA.UI.Screen.Resolution.Width,sh=GTA.UI.Screen.Resolution.Height;
            float pct=Math.Max(0f,Math.Min(1f,value/100f));
            Function.Call(Hash.DRAW_RECT,(x+w/2f)/sw,(y+h/2f)/sh,w/sw,h/sh,5,7,9,205);
            Function.Call(Hash.DRAW_RECT,(x+(w*pct)/2f)/sw,(y+h/2f)/sh,(w*pct)/sw,h/sh,r,g,b,235);
            new TextElement(label+"  "+Math.Round(value)+"%",new PointF(x/sw*1280f,(y-11f)/sh*720f),0.16f,Color.White,GTA.UI.Font.RockstarTag,Alignment.Left,true,false).Draw();
        }
    }

    public sealed class WeedManager
    {
        private readonly InventoryStore store; private readonly ShopManager shops; private readonly LootManager loot;
        private DateTime plantUntil=DateTime.MinValue; private bool planting;
        public WeedManager(InventoryStore s,ShopManager sh,LootManager l){store=s;shops=sh;loot=l;}
        public bool TryInteract(Ped ped)
        {
            if(ped==null||!ped.Exists()||!shops.IsNearGreenhouse(ped.Position)) return false;
            if(planting){Notifications.Push("ALREADY PLANTING",true);return true;}
            var seed=store.Player.Items.FirstOrDefault(i=>i.Id=="weed_seed"&&i.Amount>0);
            if(seed==null){Notifications.Push("YOU NEED A WEED SEED",true);return true;}
            seed.Amount--; if(seed.Amount<=0)store.Player.Remove(seed);
            planting=true; plantUntil=DateTime.UtcNow.AddMinutes(1);
            Notifications.Push("PLANTING WEED  /  1 MINUTE"); return true;
        }
        public void Tick()
        {
            if(!planting)return;
            if(DateTime.UtcNow>=plantUntil)
            {
                planting=false; plantUntil=DateTime.MinValue;
                var buds=ItemCatalog.Create("weed_bud",8);
                if(!store.AddToPlayer(buds))
                {
                    var c=new LootContainer("weed_harvest_"+Guid.NewGuid(),Game.Player.Character.Position,5,2); c.Grid.Add(buds); loot.AddDropped(c);
                    Notifications.Push("INVENTORY FULL - HARVEST DROPPED",true);
                }
                else Notifications.PushItem("ADDED",8,buds);
            }
        }
        public void Draw()
        {
            if(!planting)return;
            float sw=GTA.UI.Screen.Resolution.Width,sh=GTA.UI.Screen.Resolution.Height;
            double total=60.0, left=Math.Max(0,(plantUntil-DateTime.UtcNow).TotalSeconds); float progress=(float)Math.Max(0,Math.Min(1,1-left/total));
            float w=420f,h=18f,x=(sw-w)/2f,y=sh-72f;
            Function.Call(Hash.DRAW_RECT,(x+w/2f)/sw,(y+h/2f)/sh,w/sw,h/sh,8,12,10,205);
            Function.Call(Hash.DRAW_RECT,(x+(w*progress)/2f)/sw,(y+h/2f)/sh,(w*progress)/sw,h/sh,70,190,95,235);
            new TextElement("GROWING WEED  "+Math.Ceiling(left)+"s",new PointF((x+w/2f)/sw*1280f,(y-18f)/sh*720f),0.19f,Color.White,GTA.UI.Font.RockstarTag,Alignment.Center,true,false).Draw();
        }
    }

    public static class DrugEffects
    {
        private static DateTime until=DateTime.MinValue; private static string active; private static int lastHealth; private static bool smoking;
        public static void Start(string id)
        {
            active=id;until=DateTime.UtcNow.AddSeconds(id=="meth_bag"?30:id=="oxy"||id=="morphine"?30:id=="alcohol"?25:8);lastHealth=Game.Player.Character.Health;
            if(id=="joint"||id=="weed_bud"){Function.Call(Hash.SET_TIMECYCLE_MODIFIER,"stoned"); smoking=true; Function.Call(Hash.TASK_START_SCENARIO_IN_PLACE,Game.Player.Character.Handle,"WORLD_HUMAN_SMOKING",0,true);}
            else if(id=="cokebaggy"||id=="crack_baggy")Function.Call(Hash.SET_TIMECYCLE_MODIFIER,"drug_wobbly");
            else if(id=="meth_bag")Function.Call(Hash.SET_TIMECYCLE_MODIFIER,"spectator5");
            else if(id=="alcohol")Function.Call(Hash.SET_TIMECYCLE_MODIFIER,"drug_wobbly");
            else if(id=="xtcbaggy")Function.Call(Hash.SET_TIMECYCLE_MODIFIER,"spectator5");
            else Function.Call(Hash.SET_TIMECYCLE_MODIFIER,"drug_flying_base");
            Function.Call(Hash.SHAKE_GAMEPLAY_CAM,"DRUNK_SHAKE",0.15f);
        }
        public static void Tick()
        {
            if(string.IsNullOrEmpty(active))return;
            if(DateTime.UtcNow>=until){Function.Call(Hash.CLEAR_TIMECYCLE_MODIFIER);Function.Call(Hash.STOP_GAMEPLAY_CAM_SHAKING,true);if(smoking){Function.Call(Hash.CLEAR_PED_TASKS,Game.Player.Character.Handle);smoking=false;}if(active=="oxy"||active=="morphine")Function.Call(Hash.SET_RUN_SPRINT_MULTIPLIER_FOR_PLAYER,Game.Player.Handle,1.0f);active=null;return;}
            if(active=="oxy"||active=="morphine"){Function.Call(Hash.SET_RUN_SPRINT_MULTIPLIER_FOR_PLAYER,Game.Player.Handle,1.35f);int hp=Game.Player.Character.Health;if(hp<lastHealth)Game.Player.Character.Health=Math.Min(Game.Player.Character.MaxHealth,lastHealth-(lastHealth-hp)/2);lastHealth=Game.Player.Character.Health;}
        }
    }
}
