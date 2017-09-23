﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DungeonMasterEngine.DungeonContent;

namespace DungeonMasterEngine.GameConsoleContent.Base
{
    /// <summary>
    /// Represents main interpreter to recognize client commands.
    /// </summary>
    public class BaseInterpreter : Interpreter
    {
        private readonly CommandParser<ConsoleContext<Dungeon>> parser;

        private readonly KeyboardStream inputStream;

        /// <summary>
        /// Initialize interpreter.
        /// </summary>
        /// <param name="factories">The factories.</param>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public BaseInterpreter(IEnumerable<ICommandFactory<ConsoleContext<Dungeon>>> factories, TextReader input, TextWriter output, KeyboardStream inputStream)
        {
            if (factories == null || input == null || output == null || inputStream == null)
                throw new ArgumentNullException();

            Input = input;
            Output = output;
            this.inputStream = inputStream;

            parser = new CommandParser<ConsoleContext<Dungeon>>(factories);
        }

        public bool Running { get; set; } = true;

        public bool ExecutingCommand => RunningCommand != null;

        public IInterpreter<ConsoleContext<Dungeon>> RunningCommand { get; private set; }

        /// <summary>
        /// Runs the interpreter.
        /// </summary>
        /// <returns>Return s false whether application end is requested.</returns>
        public override async Task Run()
        {
            Output.WriteLine("Welcome! Write a command. (Consider using \"help\" command if U R lost.)");

            while (Running)
            {
                RunningCommand = await GetInterpreter();

                if (RunningCommand != null)
                {
                    RunningCommand.Input = Input;
                    RunningCommand.Output = Output;
                    RunningCommand.ConsoleContext = ConsoleContext;

                    await RunningCommand.Run();
                    RunningCommand = null;
                    Output.WriteLine();
                }
                else
                {
                    Output.WriteLine("Unrecognized command!");
                }
            }

            Output.WriteLine("Have a nice day.");
        }


        private async Task<IInterpreter<ConsoleContext<Dungeon>>> GetInterpreter()
        {
            var queueTask = WaitForImplicitCommand();
            var readLineTask = WaitForInput();

            var winTask = await Task.WhenAny(queueTask, readLineTask);
            if(winTask == queueTask)
            {
                inputStream.WriteLineToInput("Implicit command execution:");//To complete another task in order to be able to read in sub interpreter
                await readLineTask;   
            }

            return await winTask;
        }

        private async Task<IInterpreter<ConsoleContext<Dungeon>>> WaitForInput()
        {
            string command;

            do
            {
                command = await Input.ReadLineAsync();
            }
            while (string.IsNullOrWhiteSpace(command));

            return parser.ParseCommand(command);
        }


        TaskCompletionSource<IInterpreter<ConsoleContext<Dungeon>>> interpreterPromise = new TaskCompletionSource<IInterpreter<ConsoleContext<Dungeon>>>();

        private async Task<IInterpreter<ConsoleContext<Dungeon>>> WaitForImplicitCommand()
        {
            var res = await interpreterPromise.Task;
            interpreterPromise = new TaskCompletionSource<IInterpreter<ConsoleContext<Dungeon>>>();
            return res;
        }

        public void RunCommand(IInterpreter<ConsoleContext<Dungeon>> interpreter)
        {
            interpreterPromise.SetResult(interpreter);
        }

    }
}

