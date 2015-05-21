using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ElRengar
{


    enum Spells
    {
        Q, W, E, R
    }

    internal class Rengar
    {

        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public static Orbwalking.Orbwalker Orbwalker;

        private static SpellSlot _ignite;
        private static SpellSlot _smiteSlot;
        private static bool _checkSmite = false;
        public static Obj_AI_Base Minionerimo;


        private const float Jqueryluckynumber = 400f;
        private static int _leapRange = 775;
        private static bool _visible = true;
        private static Items.Item _youmuu, _cutlass, _blade, _tiamat, _hydra;

        private static readonly string[] Epics =
        {
            "SRU_Baron", "SRU_Dragon"
        };
        private static readonly string[] Buffs =
        {
            "SRU_Red", "SRU_Blue"
        };
        private static readonly string[] Buffandepics =
        {
            "SRU_Red", "SRU_Blue", "SRU_Dragon", "SRU_Baron"
        };

        public static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>()
        {
            { Spells.Q, new Spell(SpellSlot.Q, 0)},
            { Spells.W, new Spell(SpellSlot.W, 500)},
            { Spells.E, new Spell(SpellSlot.E, 1000)},
            { Spells.R, new Spell(SpellSlot.R, 2000)}
        };

        #region Gameloaded 

        public static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != "Rengar")
                return;

            _youmuu = new Items.Item(3142, 0f);
            _cutlass = new Items.Item(3144, 450f);
            _blade = new Items.Item(3153, 450f);

            _tiamat = new Items.Item(3077, 400f);
            _hydra = new Items.Item(3074, 400f);
            _ignite = Player.GetSpellSlot("summonerdot");

            Notifications.AddNotification("ElRengar by jQuery v2.0.0.2", 5000);
            spells[Spells.E].SetSkillshot(0.25f, 70f, 1500f, true, SkillshotType.SkillshotLine);

            ElRengarMenu.Initialize();
            Game.OnUpdate += OnGameUpdate;
            Drawing.OnDraw += Drawings.Drawing_OnDraw;
            Drawing.OnEndScene += Drawings.OnDrawEndScene;
            Orbwalking.BeforeAttack += OrbwalkingBeforeAttack;
            new AssassinManager();
        }

        private static void OrbwalkingBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region OnGameUpdate

        private static void OnGameUpdate(EventArgs args)
        {
            _smiteSlot = Player.GetSpellSlot(Smitetype());
            spells[Spells.R].Range = 1000 + spells[Spells.R].Level * 1000;

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    JungleClear();
                break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                break;
            }
            SelfHealing();
            CalcRange();
            Visibility();

            if (ElRengarMenu._menu.Item("ElRengar.smiteEnabled").GetValue<KeyBind>().Active) Smiter();
            if (ParamBool("ElRengar.smiteSave")) SaveMe();
        }

        #endregion

        #region Visibility

        // UNSEEN PREDATOR: While in brush or in stealth, Rengar gains bonus attack range 
        // and his basic attacks cause him to leap at the target's location.
        private static void CalcRange()
        {
            if (Player.AttackRange > 150 && Player.AttackRange < 700)
            {
                _leapRange = (int) (Player.AttackRange + 175);
            }

            if (Player.AttackRange > 150 && Player.AttackRange >= 700)
            {
                _leapRange = (int) (Player.AttackRange + 75);
            }

            if (Player.AttackRange < 150)
            {
                _leapRange = 125;
            }

            spells[Spells.Q].Range = _leapRange + 25;
        }

        private static void Visibility()
        {
            if(_leapRange > 150)
            {
                _visible = false;
            }
            else
            {
                _visible = true;
            }
        }

        #endregion

        private static void OrbwalkingBeforeAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo || Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Mixed)
                return;

            if (unit.IsMe)
            {
                spells[Spells.Q].Cast();
            }
        }

        #region itemusage

        private static void FightItems()
        {
            var target = GetEnemy(spells[Spells.W].Range);

            var tiamatItem = ElRengarMenu._menu.Item("ElRengar.Combo.Tiamat").GetValue<bool>();
            var hydraItem = ElRengarMenu._menu.Item("ElRengar.Combo.Hydra").GetValue<bool>();

            if (Items.CanUseItem(3074) && hydraItem && Player.Distance(target) <= Jqueryluckynumber)
                Items.UseItem(3074);

            if (Items.CanUseItem(3077) && tiamatItem && Player.Distance(target) <= Jqueryluckynumber)
                Items.UseItem(3077);
        }

        #endregion

        #region ComboDamage

        public static float GetComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (spells[Spells.Q].IsReady())
            {
                damage += spells[Spells.Q].GetDamage(enemy);
            }

            if (spells[Spells.W].IsReady())
            {
                damage += spells[Spells.W].GetDamage(enemy);
            }

            if (spells[Spells.E].IsReady())
            {
                damage += spells[Spells.E].GetDamage(enemy);
            }

            if (_ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(_ignite) != SpellState.Ready)
            {
                damage += (float)Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            }

            return (float)(damage + Player.GetAutoAttackDamage(enemy));
        }

        #endregion

        private static void Combo()
        {
            var target = GetEnemy(spells[Spells.W].Range);
            if (target == null || !target.IsValid)
            {
                return;
            }
            var qCombo = ElRengarMenu._menu.Item("ElRengar.Combo.Q").GetValue<bool>();
            var wCombo = ElRengarMenu._menu.Item("ElRengar.Combo.W").GetValue<bool>();
            var eCombo = ElRengarMenu._menu.Item("ElRengar.Combo.E").GetValue<bool>();
            var eComboOor = ElRengarMenu._menu.Item("ElRengar.Combo.EOOR").GetValue<bool>();
            var cutlassItem = ElRengarMenu._menu.Item("ElRengar.Combo.Cutlass").GetValue<bool>();
            var useYoumuu = ElRengarMenu._menu.Item("ElRengar.Combo.Youmuu").GetValue<bool>();
            var bladeItem = ElRengarMenu._menu.Item("ElRengar.Combo.Blade").GetValue<bool>();
            var prioCombo = ElRengarMenu._menu.Item("ElRengar.Combo.Prio").GetValue<StringList>();

            FightItems();

            // cast Q
            if (Player.Mana <= 4)
            {
                if (qCombo && Player.Distance(target) <= Player.AttackRange && spells[Spells.Q].IsReady())
                {
                    CastQ(target);
                }

                if (wCombo && spells[Spells.W].IsReady() && Player.Distance(target) <= spells[Spells.W].Range)
                {
                    spells[Spells.W].CastOnUnit(Player);
                }

                if (_visible && eCombo && spells[Spells.E].IsReady())
                {
                    spells[Spells.E].Cast(target);
                }
            }

            if (Player.Mana == 5)
            {
                if (spells[Spells.Q].IsReady() && qCombo &&
                    prioCombo.SelectedIndex == 0 && Player.Distance(target) <= Player.AttackRange)
                {
                   CastQ(target);
                }

                if (Player.HasBuff("RengarR"))
                    return;

                if (spells[Spells.W].IsReady() && wCombo && prioCombo.SelectedIndex == 1 && target.IsValidTarget(spells[Spells.W].Range)
                    )
                {
                    spells[Spells.W].CastOnUnit(Player);
                }

                if (_visible && eCombo &&
                    prioCombo.SelectedIndex == 2 && Player.Distance(target) <= spells[Spells.E].Range)
                {
                    spells[Spells.E].Cast(target);
                }

                if (eComboOor && Player.Distance(target) > Player.AttackRange + 100) 
                {
                    spells[Spells.E].Cast(target);
                }  
            }

            if (useYoumuu && _youmuu.IsReady() && target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player) + (Player.HasBuff("RengarR") ? Player.MoveSpeed / 2 : 0)))
            {
                _youmuu.Cast(Player);
            }

            if (cutlassItem && Player.Distance(target) <= 450 && _cutlass.IsReady())
            {
                _cutlass.Cast(target);
            }

            if (bladeItem && Player.Distance(target) <= 450 && _blade.IsReady())
            {
                _blade.Cast(target);
            }
               
            if (Player.Distance(target) <= 600 && IgniteDamage(target) >= target.Health && ElRengarMenu._menu.Item("ElRengar.Combo.Ignite").GetValue<bool>())
            {
                Player.Spellbook.CastSpell(_ignite, target);
            }
        }

        #region CastQ

   
        private static void CastQ(Obj_AI_Base target)
        {
            if (spells[Spells.Q].IsReady() && target.IsValidTarget())
            {
                spells[Spells.Q].Cast();
            }
        }

        #endregion

        #region harass

        private static void Harass()
        {
            var target = GetEnemy(spells[Spells.R].Range);
            if (target == null || !target.IsValid)
                return;

            var qHarass = ElRengarMenu._menu.Item("ElRengar.Harass.Q").GetValue<bool>();
            var wHarass = ElRengarMenu._menu.Item("ElRengar.Harass.W").GetValue<bool>();
            var eHarass = ElRengarMenu._menu.Item("ElRengar.Harass.E").GetValue<bool>();
            var prioHarass = ElRengarMenu._menu.Item("ElRengar.Harass.Prio").GetValue<StringList>();

            if (Player.Mana <= 4)
            {
                if (qHarass && Player.Distance(target) <= Player.AttackRange && spells[Spells.Q].IsReady())
                {
                    spells[Spells.Q].Cast();
                }

                //_visible
                if (_visible && wHarass && target.IsValidTarget(spells[Spells.W].Range) && spells[Spells.W].CanCast(Player) && spells[Spells.W].IsReady())
                {
                    spells[Spells.W].CastOnUnit(Player);
                }

                if (eHarass && spells[Spells.E].IsReady() && target.IsValidTarget(spells[Spells.E].Range))
                {
                    spells[Spells.E].Cast(target);
                }
            }

            if (Player.Mana >= 5)
            {
                if (qHarass && Player.Distance(target) <= Player.AttackRange && spells[Spells.Q].IsReady())
                {
                    spells[Spells.Q].Cast();
                }

                if (wHarass &&
                    prioHarass.SelectedIndex == 0 && target.IsValidTarget(spells[Spells.W].Range) && spells[Spells.W].IsReady())
                {
                    spells[Spells.W].CastOnUnit(Player);
                }

                if (eHarass &&
                    prioHarass.SelectedIndex == 1 && target.IsValidTarget(spells[Spells.E].Range))
                {
                    spells[Spells.E].CastIfHitchanceEquals(target, HitChance.Medium);
                }
            }  
        }
        #endregion

        #region jungle

        private static void JungleClear()
        {
            var qWaveClear = ElRengarMenu._menu.Item("ElRengar.Clear.Q").GetValue<bool>();
            var wWaveClear = ElRengarMenu._menu.Item("ElRengar.Clear.W").GetValue<bool>();
            var eWaveClear = ElRengarMenu._menu.Item("ElRengar.Clear.E").GetValue<bool>();
            var hydraClear = ElRengarMenu._menu.Item("ElRengar.Clear.Hydra").GetValue<bool>();
            var tiamatClear = ElRengarMenu._menu.Item("ElRengar.Clear.Tiamat").GetValue<bool>();
            var saveClear = ElRengarMenu._menu.Item("ElRengar.Clear.Save").GetValue<bool>();
            var prioClear = ElRengarMenu._menu.Item("ElRengar.Clear.Prio").GetValue<StringList>();

            var Target = MinionManager.GetMinions(
                Player.Position, 700, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault();

            if (Player.Mana <= 4)
            {
                if (qWaveClear && spells[Spells.Q].IsReady() &&
                    Target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)))
                {
                    spells[Spells.Q].Cast();
                }
                if (wWaveClear && spells[Spells.W].IsReady() 
                    && Target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) 
                    && Player.Distance(Target) <= spells[Spells.W].Range)
                {
                    spells[Spells.W].Cast();
                }
                if (eWaveClear && spells[Spells.E].IsReady() && Target.IsValidTarget(spells[Spells.E].Range))
                {
                    spells[Spells.E].Cast(Target);
                }
            }

            if (Player.Mana == 5)
            {
                if (saveClear)
                    return;

                if (prioClear.SelectedIndex == 0 && qWaveClear && spells[Spells.Q].IsReady() &&
                   Target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)))
                {
                    spells[Spells.Q].Cast();
                }
                if (prioClear.SelectedIndex == 1 && wWaveClear && spells[Spells.W].IsReady() &&
                    Target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) &&
                    Player.Distance(Target) <= spells[Spells.W].Range)
                {
                    spells[Spells.W].Cast();
                }
                if (prioClear.SelectedIndex == 2 && eWaveClear && spells[Spells.E].IsReady() && Target.IsValidTarget(spells[Spells.E].Range))
                {
                    spells[Spells.E].Cast(Target);
                }
            }

            if (Items.CanUseItem(3074) && hydraClear && Target.IsValidTarget(400f))
                Items.UseItem(3074);

            if (Items.CanUseItem(3077) && tiamatClear && Target.IsValidTarget(400f))
                Items.UseItem(3077);
        }

        #endregion

        #region laneclear

        private static void LaneClear()
        {
            var minion = MinionManager.GetMinions(Rengar.Player.ServerPosition, spells[Spells.W].Range).FirstOrDefault();
            if (minion == null || minion.Name.ToLower().Contains("ward")) return;

            var qWaveClear = ElRengarMenu._menu.Item("ElRengar.Clear.Q").GetValue<bool>();
            var wWaveClear = ElRengarMenu._menu.Item("ElRengar.Clear.W").GetValue<bool>();
            var eWaveClear = ElRengarMenu._menu.Item("ElRengar.Clear.E").GetValue<bool>();
            var hydraClear = ElRengarMenu._menu.Item("ElRengar.Clear.Hydra").GetValue<bool>();
            var tiamatClear = ElRengarMenu._menu.Item("ElRengar.Clear.Tiamat").GetValue<bool>();
            var saveClear = ElRengarMenu._menu.Item("ElRengar.Clear.Save").GetValue<bool>();
            var prioClear = ElRengarMenu._menu.Item("ElRengar.Clear.Prio").GetValue<StringList>();

            var bestFarmLocation = MinionManager.GetBestCircularFarmLocation(MinionManager.GetMinions(spells[Spells.W].Range, MinionTypes.All, MinionTeam.Enemy).Select(m => m.ServerPosition.To2D()).ToList(), spells[Spells.W].Width, spells[Spells.W].Range);
            var minions = MinionManager.GetMinions(Player.ServerPosition, spells[Spells.R].Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);

            if (Player.Mana <= 4)
            {
                if (wWaveClear && minion.IsValidTarget() && spells[Spells.W].IsReady())
                {
                    spells[Spells.W].Cast(bestFarmLocation.Position);
                }

                if (qWaveClear && minion.IsValidTarget() && spells[Spells.Q].IsReady())
                {
                    spells[Spells.Q].Cast();
                }

                if (eWaveClear && minion.IsValidTarget() && spells[Spells.E].IsReady())
                {
                    spells[Spells.E].Cast(minion);
                }
            }

            if (Player.Mana == 5)
            {
                if (saveClear)
                    return;

                if (prioClear.SelectedIndex == 0 && wWaveClear && minion.IsValidTarget() && spells[Spells.W].IsReady())
                {
                    spells[Spells.W].Cast();
                }

                if (prioClear.SelectedIndex == 1 && qWaveClear && minion.IsValidTarget() && spells[Spells.Q].IsReady())
                {
                    spells[Spells.Q].Cast();
                }

                if (prioClear.SelectedIndex == 2 && eWaveClear && minion.IsValidTarget() && spells[Spells.E].IsReady())
                {
                    spells[Spells.E].Cast(minion);
                }
            }

            if (Items.CanUseItem(3074) && hydraClear && minion.IsValidTarget(400f) && minions.Count() > 1)
                Items.UseItem(3074);

            if (Items.CanUseItem(3077) && tiamatClear && minion.IsValidTarget(400f) && minions.Count() > 1)
                Items.UseItem(3077);
        }

        #endregion

        #region selfheal

        private static void SelfHealing()
        {
            if (Player.IsRecalling() || Player.InFountain() || Player.Mana <= 4)
                return;

            var useHeal = ElRengarMenu._menu.Item("ElRengar.Heal.AutoHeal").GetValue<bool>();
            var healPercentage = ElRengarMenu._menu.Item("ElRengar.Heal.HP").GetValue<Slider>().Value;

            if (useHeal && (Player.Health / Player.MaxHealth) * 100 <= healPercentage && spells[Spells.W].IsReady())
            {
                spells[Spells.W].Cast(Player);
            }
        }

        #endregion

        #region Ignite

        private static float IgniteDamage(Obj_AI_Hero target)
        {
            if (_ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(_ignite) != SpellState.Ready)
            {
                return 0f;
            }
            return (float)Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }

        #endregion

        #region AssassinManager
        private static Obj_AI_Hero GetEnemy(float vDefaultRange = 0, TargetSelector.DamageType vDefaultDamageType = TargetSelector.DamageType.Physical)
        {
            if (Math.Abs(vDefaultRange) < 0.00001)
                vDefaultRange = spells[Spells.R].Range;

            if (!ElRengarMenu._menu.Item("AssassinActive").GetValue<bool>())
                return TargetSelector.GetTarget(vDefaultRange, vDefaultDamageType);

            var assassinRange = ElRengarMenu._menu.Item("AssassinSearchRange").GetValue<Slider>().Value;

            var vEnemy =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        enemy =>
                            enemy.Team != ObjectManager.Player.Team && !enemy.IsDead && enemy.IsVisible &&
                            ElRengarMenu._menu.Item("Assassin" + enemy.ChampionName) != null &&
                            ElRengarMenu._menu.Item("Assassin" + enemy.ChampionName).GetValue<bool>() &&
                            ObjectManager.Player.Distance(enemy) < assassinRange);

            if (ElRengarMenu._menu.Item("AssassinSelectOption").GetValue<StringList>().SelectedIndex == 1)
            {
                vEnemy = (from vEn in vEnemy select vEn).OrderByDescending(vEn => vEn.MaxHealth);
            }

            Obj_AI_Hero[] objAiHeroes = vEnemy as Obj_AI_Hero[] ?? vEnemy.ToArray();

            Obj_AI_Hero t = !objAiHeroes.Any()
                ? TargetSelector.GetTarget(vDefaultRange, vDefaultDamageType)
                : objAiHeroes[0];

            return t;
        }

        #endregion

        private static bool HpLowerParam(Obj_AI_Base obj, String paramName)
        {
            return ((obj.Health / obj.MaxHealth) * 100) <= ElRengarMenu._menu.Item(paramName).GetValue<Slider>().Value;
        }

        #region Autosmite
        private static double SmiteDmg()
        {
            int[] dmg =
            {
                20*Player.Level + 370, 30*Player.Level + 330, 40*+Player.Level + 240, 50*Player.Level + 100
            };
            return Player.Spellbook.CanUseSpell(_smiteSlot) == SpellState.Ready ? dmg.Max() : 0;
        }

        private static bool ParamBool(String paramName)
        {
            return (ElRengarMenu._menu.Item(paramName).GetValue<bool>());
        }

        private static void Smiter()
        {
            var minion = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(a => Buffandepics.Contains(a.BaseSkinName) && a.Distance(Player) <= 1300);
            if (minion != null)
            {
                if (ElRengarMenu._menu.Item(minion.BaseSkinName).GetValue<bool>())
                {
                    Minionerimo = minion;
                    if (SmiteDmg() > minion.Health && minion.IsValidTarget(780) && ParamBool("ElRengar.normalSmite")) Player.Spellbook.CastSpell(_smiteSlot, minion);
                    if (minion.Distance(Player) < 100 && _checkSmite)
                    {
                        _checkSmite = false;
                        Player.Spellbook.CastSpell(_smiteSlot, minion);
                    }

                }
            }
        }

        #endregion

        #region SmiteSaver
        private static void SaveMe()
        {
            if ((Player.Health / Player.MaxHealth * 100) > ElRengarMenu._menu.Item("hpPercentSM").GetValue<Slider>().Value || Player.Spellbook.CanUseSpell(_smiteSlot) != SpellState.Ready) return;
            var epicSafe = false;
            var buffSafe = false;
            foreach (
                var minion in
                    MinionManager.GetMinions(Player.Position, 1100f, MinionTypes.All, MinionTeam.Neutral,
                        MinionOrderTypes.None))
            {
                foreach (var minionName in Epics)
                {
                    if (minion.BaseSkinName == minionName && HpLowerParam(minion, "hpEpics") && ParamBool("dEpics"))
                    {
                        epicSafe = true;
                        break;
                    }
                }
                foreach (var minionName in Buffs)
                {
                    if (minion.BaseSkinName == minionName && HpLowerParam(minion, "hpBuffs") && ParamBool("dBuffs"))
                    {
                        buffSafe = true;
                        break;
                    }
                }
            }

            if (epicSafe || buffSafe) return;
        }
        #endregion

        //Start Credits to Kurisu
        private static readonly int[] SmitePurple = { 3713, 3726, 3725, 3726, 3723 };
        private static readonly int[] SmiteGrey = { 3711, 3722, 3721, 3720, 3719 };
        private static readonly int[] SmiteRed = { 3715, 3718, 3717, 3716, 3714 };
        private static readonly int[] SmiteBlue = { 3706, 3710, 3709, 3708, 3707 };

        private static string Smitetype()
        {
            if (SmiteBlue.Any(a => Items.HasItem(a)))
            {
                return "s5_summonersmiteplayerganker";
            }
            if (SmiteRed.Any(a => Items.HasItem(a)))
            {
                return "s5_summonersmiteduel";
            }
            if (SmiteGrey.Any(a => Items.HasItem(a)))
            {
                return "s5_summonersmitequick";
            }
            if (SmitePurple.Any(a => Items.HasItem(a)))
            {
                return "itemsmiteaoe";
            }
            return "summonersmite";
        }
        //End credits
    }
}
