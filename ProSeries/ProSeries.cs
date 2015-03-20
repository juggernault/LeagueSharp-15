﻿using System;
using System.Linq;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using ProSeries.Utils;
using ProSeries.Utils.Drawings;

namespace ProSeries
{
    internal static class ProSeries
    {
        internal static Menu Config;
        internal static Orbwalking.Orbwalker Orbwalker;
        internal static Obj_AI_Hero Player;

        internal static void Load()
        {
            try
            {
                Player = ObjectManager.Player;

                //Print the welcome message
                Game.PrintChat("Pro Series Loaded!");

                //Load the menu.
                Config = new Menu("ProSeries", "ProSeries", true);

                //Add the target selector.
                TargetSelector.AddToMenu(Config.SubMenu("Selector"));

                //Add the orbwalking.
                Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker"));

                //Load the crosshair
                Crosshair.Load();

                //Check if the champion is supported
                try
                {
                    Type.GetType("ProSeries.Champions." + Player.ChampionName).GetMethod("Load").Invoke(null, null);
                }
                catch (NullReferenceException)
                {
                    Game.PrintChat(Player.ChampionName + " is not supported yet! however the orbwalking will work");
                }

                //Add ADC items usage.
                ItemManager.Load();

                //Add the menu as main menu.
                Config.AddToMainMenu();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        internal static bool CanCombo()
        {
            // "usecombo" keybind required
            // "combomana" slider required
            return Config.Item("usecombo").GetValue<KeyBind>().Active &&
                   Player.Mana / Player.MaxMana * 100 > Config.Item("combomana").GetValue<Slider>().Value;
        }

        internal static bool CanHarass()

        {   // "harasscombo" keybind required
            // "harassmana" slider required
            return Config.Item("useharass").GetValue<KeyBind>().Active &&
                  Player.Mana / Player.MaxMana * 100 > Config.Item("harassmana").GetValue<Slider>().Value;           
        }

        internal static bool CanClear()
        {            
            // "clearcombo" keybind required
            // "clearmana" slider required
            return Config.Item("useclear").GetValue<KeyBind>().Active &&
                  Player.Mana / Player.MaxMana * 100 > Config.Item("clearmana").GetValue<Slider>().Value;               
        }

        internal static IEnumerable<Obj_AI_Minion> JungleMobsInRange(float range)
        {
            var names = new[]
            {
                "SRU_Razorbeak", "SRU_Krug", "Sru_Crab",
                "SRU_Baron", "SRU_Dragon", "SRU_Blue", "SRU_Red", "SRU_Murkwolf", "SRU_Gromp"
            };

            var minions = from minion in ObjectManager.Get<Obj_AI_Minion>()
                where minion.IsValidTarget(range) && !minion.Name.Contains("Mini")
                where names.Any(name => minion.Name.StartsWith(name))
                select minion;

            return minions;
        }

        internal static string[] Creeps =
        {
            "SRU_Razorbeak", "SRU_Krug", "Sru_Crab",
            "SRU_Baron", "SRU_Dragon", "SRU_Blue", "SRU_Red", "SRU_Murkwolf", "SRU_Gromp"
        };
    }
}