﻿using DungeonMasterEngine.GameConsoleContent.Base;
using DungeonMasterEngine.Player;
using System.Threading.Tasks;
using DungeonMasterEngine.DungeonContent.Actuators.Wall;

namespace DungeonMasterEngine.GameConsoleContent
{
    public class ChampionCommand : Interpreter
    {

        public ChampoinActuator Actuator {get; set;}

        public override async Task Run()
        {
            if (Actuator != null)
                await ChampoinReincarnation();
            else
                await ChampoinInfo();
        }

        private async Task ChampoinInfo()
        {
            if (Parameters.Length > 0)
            {
                string parameter = Parameters[0];
                switch(parameter)
                {
                    case "list":
                        await GetFromItemIndex(ConsoleContext.AppContext.Theron.PartyGroup, false);
                        break;


                }
            }
            else
                Output.WriteLine("Invalid paramter");
        }

        private async Task ChampoinReincarnation()
        {
            Output.WriteLine("Champoin builder:");
            if (ConsoleContext.AppContext.Theron.PartyGroup.Count == 4)
            {
                Output.WriteLine("Group is full!!");
                return;
            }

            Output.WriteLine(Actuator.Champoin);

            var getOption = new string[] { "Reincarnate", "rescue" };
            var res = await GetFromItemIndex(getOption);

            Champoin champoin = null;
            if (res == getOption[0])
                champoin = await ReincarnateInterpreter();
            else if (res == getOption[1])
                champoin = Actuator.Champoin;
            else            
                return;
            

            if(champoin != null)
            {
                Actuator.RemoveChampoin();
                ConsoleContext.AppContext.Theron.AddChampoinToGroup(champoin);
                Output.WriteLine("Champoin succesfully added to group.");
            }
        }

        private async Task<Champoin> ReincarnateInterpreter()
        {
            Output.WriteLine("Write first name.");
            string firstName = await Input.ReadLineAsync();

            Output.WriteLine("Write last name.");
            string lastName = await Input.ReadLineAsync();

            Output.WriteLine("Write title.");
            string title = await Input.ReadLineAsync();

            Actuator.Champoin.Name = $"{firstName} {lastName}";
            Actuator.Champoin.Title = title;
            return Actuator.Champoin;

        }
    }
}
