using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;


namespace ElRengar
{
    /// <summary>
    ///    ElRengar Menu handler
    ///    Goodguy FluxySenpai for Smite stuff ._.
    /// </summary>
    public class ElRengarMenu
    {

        public static Menu _menu;

        public static void Initialize()
        {
            _menu = new Menu("ElRengar", "menu", true);

            //ElRengar.Orbwalker
            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            Rengar.Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
           _menu.AddSubMenu(orbwalkerMenu);

            //ElRengar.TargetSelector
            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);
            _menu.AddSubMenu(targetSelector);

            //ElRengar.Menu
            var comboMenu = _menu.AddSubMenu(new LeagueSharp.Common.Menu("Combo", "Combo"));
            comboMenu.AddItem(new MenuItem("ElRengar.Combo.Q", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("ElRengar.Combo.W", "Use W").SetValue(true));
            comboMenu.AddItem(new MenuItem("ElRengar.Combo.E", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("ElRengar.Combo.Prio", "Prioritize").SetValue(new StringList(new[] { "Q", "W", "E" }, 2)));
            comboMenu.AddItem(new MenuItem("ElRengar.Combo.EOOR", "Use E when out of range").SetValue(true));
            comboMenu.AddItem(new MenuItem("ElRengar.Combo.separator", ""));
            comboMenu.AddItem(new MenuItem("ElRengar.Combo.Ignite", "Use Ignite").SetValue(true));
            comboMenu.AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            //ElRengar.Submenu
            //comboMenu.SubMenu("Combo").AddItem(new MenuItem("ElRengar.Combo.QQQ", "Triple Q").SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Press)));
            
            //ElRengar.Items
            comboMenu.SubMenu("Items").AddItem(new MenuItem("ElRengar.Combo.Tiamat", "Use Tiamat").SetValue(true));
            comboMenu.SubMenu("Items").AddItem(new MenuItem("ElRengar.Combo.Hydra", "Use Ravenous Hydra").SetValue(true));
            comboMenu.SubMenu("Items").AddItem(new MenuItem("ElRengar.Combo.Youmuu", "Use Youmuu's Ghostblade").SetValue(true));
            comboMenu.SubMenu("Items").AddItem(new MenuItem("ElRengar.Combo.Cutlass", "Use Bilgewater Cutlass").SetValue(true));
            comboMenu.SubMenu("Items").AddItem(new MenuItem("ElRengar.Combo.Blade", "Use Blade of the Ruined King").SetValue(true));

            //ElRengar.Clear
            var clearMenu = _menu.AddSubMenu(new Menu("Jungle and laneclear", "JLC"));
            clearMenu.AddItem(new MenuItem("ElRengar.Clear.Prio", "Prioritize").SetValue(new StringList(new[] { "W", "Q", "E" }, 2)));
            clearMenu.AddItem(new MenuItem("ElRengar.Clear.Q", "Use Q").SetValue(true));
            clearMenu.AddItem(new MenuItem("ElRengar.Clear.W", "Use W").SetValue(true));
            clearMenu.AddItem(new MenuItem("ElRengar.Clear.E", "Use E").SetValue(true));
            clearMenu.AddItem(new MenuItem("ElRengar.Clear.Save", "Save Ferocity").SetValue(false));
            clearMenu.AddItem(new MenuItem("422442fsaafsf", ""));

            //ElRengar.SmiteSettinsg
            clearMenu.SubMenu("Smite Settings").AddItem(new MenuItem("ElRengar.smiteEnabled", "Auto smite enabled").SetValue(new KeyBind("M".ToCharArray()[0], KeyBindType.Toggle)));
            clearMenu.SubMenu("Smite Settings").AddItem(new MenuItem("422442fsaafsf", ""));
            clearMenu.SubMenu("Smite Settings").AddItem(new MenuItem("Selected Smite Targets", "Selected Smite Targets:"));

            clearMenu.SubMenu("Smite Settings").AddItem(new MenuItem("SRU_Red", "Red Buff").SetValue(true));
            clearMenu.SubMenu("Smite Settings").AddItem(new MenuItem("SRU_Blue", "Blue Buff").SetValue(true));
            clearMenu.SubMenu("Smite Settings").AddItem(new MenuItem("SRU_Dragon", "Dragon").SetValue(true));
            clearMenu.SubMenu("Smite Settings").AddItem(new MenuItem("SRU_Baron", "Baron").SetValue(true));
            clearMenu.SubMenu("Smite Settings").AddItem(new MenuItem("ElRengar.normalSmite", "Normal Smite").SetValue(true));

            clearMenu.SubMenu("Smite Settings").AddItem(new MenuItem("422442fsaafsf11", ""));
            clearMenu.SubMenu("Smite Settings").AddItem(new MenuItem("Smite Save Settings", "Smite Save Settings:"));

            clearMenu.SubMenu("Smite Settings").AddItem(new MenuItem("ElRengar.smiteSave", "Smite Save Active").SetValue(true));
            clearMenu.SubMenu("Smite Settings").AddItem(new MenuItem("hpPercentSM", "WWSmite on x%").SetValue(new Slider(10, 1)));
            clearMenu.SubMenu("Smite Settings").AddItem(new MenuItem("param1", "Dont Smite if near and hp = x%")); 
            clearMenu.SubMenu("Smite Settings").AddItem(new MenuItem("dBuffs", "Buffs").SetValue(true));
            clearMenu.SubMenu("Smite Settings").AddItem(new MenuItem("hpBuffs", "HP %").SetValue(new Slider(30, 1)));
            clearMenu.SubMenu("Smite Settings").AddItem(new MenuItem("dEpics", "Epics").SetValue(true));
            clearMenu.SubMenu("Smite Settings").AddItem(new MenuItem("hpEpics", "HP %").SetValue(new Slider(10, 1)));

            //ElRengar.Clear.Item
            clearMenu.SubMenu("Items").AddItem(new MenuItem("ElRengar.Clear.Hydra", "Use Ravenous Hydra").SetValue(true));
            clearMenu.SubMenu("Items").AddItem(new MenuItem("ElRengar.Clear.Tiamat", "Use tiamat").SetValue(true));
            clearMenu.AddItem(new MenuItem("ElRengar.WaveClear.Active", "WaveClear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            //ElRengar.Harass
            var harassMenu = _menu.AddSubMenu(new Menu("Harass", "Harass"));
            harassMenu.AddItem(new MenuItem("ElRengar.Harass.Q", "Use Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("ElRengar.Harass.W", "Use W").SetValue(true));
            harassMenu.AddItem(new MenuItem("ElRengar.Harass.E", "Use E").SetValue(true));
            harassMenu.AddItem(new MenuItem("ElRengar.Harass.Prio", "Prioritize").SetValue(new StringList(new[] { "Q", "E", "W" }, 2)));
            harassMenu.AddItem(new MenuItem("ElRengar.Harass.Active", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            //ElRengar.Healing
            var healMenu = _menu.AddSubMenu(new Menu("Heal", "SH"));
            healMenu.AddItem(new MenuItem("ElRengar.Heal.AutoHeal", "Auto heal yourself").SetValue(true));
            healMenu.AddItem(new MenuItem("ElRengar.Heal.HP", "Self heal at >= ").SetValue(new Slider(25, 1, 100)));

            //ElRengar.Misc
            var miscMenu = _menu.AddSubMenu(new Menu("Drawings", "Misc"));
            miscMenu.AddItem(new MenuItem("ElRengar.Draw.off", "[Drawing] Drawings off").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElRengar.Draw.W", "Draw W").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElRengar.Draw.E", "Draw E").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElRengar.Draw.R", "Draw R").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElRengar.Draw.Minimap", "Draw R on minimap").SetValue(true));


            var dmgAfterE = new MenuItem("ElRengar.DrawComboDamage", "Draw combo damage").SetValue(false);
            var drawFill = new MenuItem("ElRengar.DrawColour", "Fill colour", true).SetValue(new Circle(true, Color.FromArgb(204, 204, 0, 0)));
            miscMenu.AddItem(drawFill);
            miscMenu.AddItem(dmgAfterE);

            DrawDamage.DamageToUnit = Rengar.GetComboDamage;
            DrawDamage.Enabled = dmgAfterE.GetValue<bool>();
            DrawDamage.Fill = drawFill.GetValue<Circle>().Active;
            DrawDamage.FillColor = drawFill.GetValue<Circle>().Color;

            dmgAfterE.ValueChanged += delegate (object sender, OnValueChangeEventArgs eventArgs)
            {
                DrawDamage.Enabled = eventArgs.GetNewValue<bool>();
            };

            drawFill.ValueChanged += delegate (object sender, OnValueChangeEventArgs eventArgs)
            {
                DrawDamage.Fill = eventArgs.GetNewValue<Circle>().Active;
                DrawDamage.FillColor = eventArgs.GetNewValue<Circle>().Color;
            };


            //Here comes the moneyyy, money, money, moneyyyy
            var credits = _menu.AddSubMenu(new Menu("Credits", "jQuery"));
            credits.AddItem(new MenuItem("ElRengar.Paypal", "if you would like to donate via paypal:"));
            credits.AddItem(new MenuItem("ElRengar.Email", "info@zavox.nl"));

            _menu.AddItem(new MenuItem("422442fsaafs4242f", ""));
            _menu.AddItem(new MenuItem("422442fsaafsf", "Version: 2.0.0.2"));
            _menu.AddItem(new MenuItem("fsasfafsfsafsa", "Made By jQuery"));

            _menu.AddToMainMenu();

            Console.WriteLine("Menu Loaded");
        }
    }
}