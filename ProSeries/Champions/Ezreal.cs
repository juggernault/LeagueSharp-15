﻿using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using ProSeries.Utils.Drawings;

namespace ProSeries.Champions
{
    public static class Ezreal
    {
        internal static Spell Q;
        internal static Spell W;
        internal static Spell R;

        public static void Load()
        {
            // Load spells
            Q = new Spell(SpellSlot.Q, 1190);
            Q.SetSkillshot(0.25f, 60f, 2000f, true, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W, 800);
            W.SetSkillshot(0.25f, 80f, 1600f, false, SkillshotType.SkillshotLine);

            R = new Spell(SpellSlot.R, 2500);
            R.SetSkillshot(1f, 160f, 2000f, false, SkillshotType.SkillshotLine);

            // Drawings
            Circles.Add("Q Range", Q);

            // Spell usage
            var cMenu = new Menu("Combo", "combo");
            cMenu.AddItem(new MenuItem("combomana", "Minimum mana %")).SetValue(new Slider(5));
            cMenu.AddItem(new MenuItem("usecomboq", "Use Mystic Shot", true).SetValue(true));
            cMenu.AddItem(new MenuItem("usecombow", "Use Essence Flux", true).SetValue(true));
            cMenu.AddItem(new MenuItem("usecombor", "Use Trueshot Barrage", true).SetValue(true));;
            cMenu.AddItem(new MenuItem("usecombo", "Combo (active)")).SetValue(new KeyBind(32, KeyBindType.Press));
            ProSeries.Config.AddSubMenu(cMenu);

            var hMenu = new Menu("Harass", "harass");
            hMenu.AddItem(new MenuItem("harassmana", "Minimum mana %")).SetValue(new Slider(55));
            hMenu.AddItem(new MenuItem("useharassq", "Use Mystic Shot", true).SetValue(true));
            hMenu.AddItem(new MenuItem("useharassw", "Use Essence Flux", false).SetValue(true));
            hMenu.AddItem(new MenuItem("useharass", "Harass (active)")).SetValue(new KeyBind(67, KeyBindType.Press));
            ProSeries.Config.AddSubMenu(hMenu);

            var fMenu = new Menu("Farming", "farming");
            fMenu.AddItem(new MenuItem("clearmana", "Minimum mana %")).SetValue(new Slider(35));
            fMenu.AddItem(new MenuItem("useclearq", "Use Mystic Shot", true).SetValue(true));
            fMenu.AddItem(new MenuItem("useclear", "Wave/Jungle (active)")).SetValue(new KeyBind(86, KeyBindType.Press));
            ProSeries.Config.AddSubMenu(fMenu);

            var mMenu = new Menu("Misc", "misc");
            mMenu.AddItem(new MenuItem("maxrdist", "Max R distance", true)).SetValue(new Slider(1500, 0, 3000));
            mMenu.AddItem(new MenuItem("useqimm", "Use Q on Immobile", true)).SetValue(true);
            mMenu.AddItem(new MenuItem("useqdash", "Use Q on Dashing", true)).SetValue(true);
            ProSeries.Config.AddSubMenu(mMenu);

            // Events
            Game.OnUpdate += Game_OnUpdate;
        }

        internal static void Game_OnUpdate(EventArgs args)
        { 

            if (ProSeries.CanCombo())
            {
                var qtarget = TargetSelector.GetTargetNoCollision(Q);
                if (qtarget.IsValidTarget() && Q.IsReady())
                {
                    if (ProSeries.Config.Item("usecomboq", true).GetValue<bool>())
                        Q.CastIfHitchanceEquals(qtarget, HitChance.Medium);
                }

                var wtarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
                if (wtarget.IsValidTarget() && W.IsReady())
                {
                    if (ProSeries.Config.Item("usecombow", true).GetValue<bool>())
                        W.CastIfHitchanceEquals(wtarget, HitChance.Medium);
                }
            }

            if (ProSeries.CanHarass())
            {
                var qtarget = TargetSelector.GetTargetNoCollision(Q);
                if (qtarget.IsValidTarget() && Q.IsReady())
                {
                    if (ProSeries.Config.Item("usecomboq", true).GetValue<bool>())
                        Q.CastIfHitchanceEquals(qtarget, HitChance.Medium);
                }

                var wtarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
                if (wtarget.IsValidTarget() && W.IsReady())
                {
                    if (ProSeries.Config.Item("useharassw", true).GetValue<bool>())
                        W.Cast(wtarget.ServerPosition);
                }
            }

            if (Q.IsReady())
            {
                foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget(Q.Range)))
                {
                    if (ProSeries.Config.Item("useqimm", true).GetValue<bool>())
                        Q.CastIfHitchanceEquals(target, HitChance.Immobile);

                    if (ProSeries.Config.Item("useqdash", true).GetValue<bool>())
                        Q.CastIfHitchanceEquals(target, HitChance.Dashing);
                }
            }

            if (Q.IsReady())
            {
                if (ProSeries.CanClear())
                {
                    foreach (
                        var jmin in
                            ObjectManager.Get<Obj_AI_Minion>()
                                .Where(
                                    m =>
                                        m.IsValidTarget(600) &&
                                        ProSeries.Creeps.Any(name => m.Name.StartsWith(name)) &&
                                        !m.Name.Contains("Mini")))
                    {
                        if (ProSeries.Config.Item("useclearq", true).GetValue<bool>())
                            Q.CastIfHitchanceEquals(jmin, HitChance.Low);
                    }

                    foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(m => m.IsValidTarget(Q.Range)))
                    {
                        if (ProSeries.Player.GetSpellDamage(minion, Q.Slot) >= minion.Health && !minion.IsDead)
                        {
                            if (ProSeries.Player.GetAutoAttackDamage(minion) >= minion.Health &&
                                ProSeries.Player.Spellbook.IsAutoAttacking)
                            {
                                return;
                            }

                            if (ProSeries.Config.Item("useclearq", true).GetValue<bool>())
                                Q.CastIfHitchanceEquals(minion, HitChance.Low);
                        }
                    }
                }
            }

            if (ProSeries.Config.Item("usecombor", true).GetValue<bool>())
            {
                var maxDistance = ProSeries.Config.Item("maxrdist", true).GetValue<Slider>().Value;
                foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget(maxDistance)))
                {
                    var aaDamage = Orbwalking.InAutoAttackRange(target)
                        ? ProSeries.Player.GetAutoAttackDamage(target, true)
                        : 0;

                    if (target.Health - aaDamage <= ProSeries.Player.GetSpellDamage(target, SpellSlot.R))
                    {
                        R.CastIfHitchanceEquals(target, HitChance.Medium);
                    }
                }
            }
        }
    }
}
