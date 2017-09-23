﻿using System;
using System.Diagnostics;
using DungeonMasterEngine.GameConsoleContent.Base;
using System.Threading.Tasks;
using DungeonMasterEngine.DungeonContent.Actuators.Sensors.WallSensors;
using DungeonMasterEngine.DungeonContent.Entity;

namespace DungeonMasterEngine.GameConsoleContent
{
    public class ChampionCommand : Interpreter
    {
        public bool GameOver { get; set; }
        public Sensor127 Actuator { get; set; }

        public override async Task Run()
        {
            if (GameOver)
            {
                Output.WriteLine("GAME OVER!!! --> send anything to exit");
                await Input.ReadLineAsync();
                Environment.Exit(0);
            }
            else if (Actuator != null)
            {
                await ChampoinReincarnation();
            }
            else
            {
                await ChampoinInfo();
            }
        }

        private async Task ChampoinInfo()
        {
            if (Parameters.Length > 0)
            {
                string parameter = Parameters[0];
                switch (parameter)
                {
                    case "list":
                        var champion = await GetFromItemIndex(ConsoleContext.AppContext.Leader.PartyGroup);
                        if (champion != null)
                        {
                            foreach (var skill in champion.Skills)
                                Output.WriteLine($"{skill.GetType().Name}: {skill.SkillLevel}");
                        }
                        break;
                    case "sleep":
                        await Sleep();
                        break;
                    case "switch":
                        await SwitchChampions();
                        break;
                }
            }
            else
                Output.WriteLine("Invalid parameter");
        }

        private async Task SwitchChampions()
        {
            Output.WriteLine("Specify first champion.");
            var champion1 = await GetFromItemIndex(ConsoleContext.AppContext.Leader.PartyGroup);
            Output.WriteLine("Specify second champion.");
            var champion2 = await GetFromItemIndex(ConsoleContext.AppContext.Leader.PartyGroup);

            if (champion1 == null || champion2 == null)
            {
                Output.WriteLine("Invalid champion selection.");
            }
            else
            {
                var firstLocation = champion1.Location;
                var secondLocation = champion2.Location;
                champion1.Location = null;
                champion2.Location = firstLocation;
                champion1.Location = secondLocation;
            }
        }

        private async Task Sleep()
        {
            foreach (var champion in ConsoleContext.AppContext.Leader.PartyGroup)
                champion.Sleeping = true;

            while (true)
            {
                Output.WriteLine("Write 'wake' to wake the group.");
                var text = await Input.ReadLineAsync();
                if (text.Trim() == "wake")
                {
                    foreach (var champion in ConsoleContext.AppContext.Leader.PartyGroup)
                        champion.Sleeping = false;

                    Output.WriteLine("Party had just woken up.");
                    break;
                }
            }
        }

        private async Task ChampoinReincarnation()
        {
            Output.WriteLine("Champion builder:");
            if (ConsoleContext.AppContext.Leader.PartyGroup.Count == 4)
            {
                Output.WriteLine("Group is full!!");
                return;
            }

            Output.WriteLine(Actuator.Champion);

            if (Actuator.Champion == null)
                return;

            foreach (var skill in Actuator.Champion.Skills)
                Output.WriteLine($"{skill.GetType().Name}: {skill.SkillLevel}");

            var getOption = new string[] { "Reincarnate", "rescue" };
            var res = await GetFromItemIndex(getOption);

            Champion champion = null;
            if (res == getOption[0])
                champion = await ReincarnateInterpreter();
            else if (res == getOption[1])
                champion = Actuator.Champion;
            else
                return;

            if (champion != null)
            {
                Actuator.Champion = null;
                champion.Rebirth();
                ConsoleContext.AppContext.Leader.AddChampoinToGroup(champion);
                Output.WriteLine("Champion successfully added to group.");
            }
        }

        private async Task<Champion> ReincarnateInterpreter()
        {
            Output.WriteLine("Write first name.");
            string firstName = await Input.ReadLineAsync();

            Output.WriteLine("Write last name.");
            string lastName = await Input.ReadLineAsync();

            Output.WriteLine("Write title.");
            string title = await Input.ReadLineAsync();

            Actuator.Champion.Name = $"{firstName} {lastName}";
            Actuator.Champion.Title = title;
            return Actuator.Champion;

        }
    }
}
