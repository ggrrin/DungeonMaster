using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DungeonMasterEngine.DungeonContent.Actuators;
using DungeonMasterEngine.DungeonContent.Actuators.Sensors.Initializers;
using DungeonMasterEngine.DungeonContent.Actuators.Sensors.WallSensors;
using DungeonMasterEngine.DungeonContent.Tiles.Initializers;
using DungeonMasterParser.Items;

namespace DungeonMasterEngine.Builders.ActuatorCreators
{
    public interface ILogicActuatorCreator
    {
        void SetLogicActuator(LogicTileInitializer initializer, IEnumerable<ActuatorItemData> data);
    }
        
    public class LogicActuatorCreator : ActuatorCreatorBase, ILogicActuatorCreator
    {
        public async void  SetLogicActuator(LogicTileInitializer initializer, IEnumerable<ActuatorItemData> data)
        {
            initializer.LogicActuator = new LogicActuator(await Task.WhenAll(data.Select(async x => await ParseLogicSensor(x))));
        }

        protected virtual async Task<SensorX> ParseLogicSensor(ActuatorItemData data)
        {
            switch (data.ActuatorType)
            {
                case 5:
                    var logicGateInitializer = new LogicGateInitializer
                    {
                        RefBit0 = (data.Data & 0x10) == 0x10,
                        RefBit1 = (data.Data & 0x20) == 0x20,
                        RefBit2 = (data.Data & 0x40) == 0x40,
                        RefBit3 = (data.Data & 0x80) == 0x80
                    };
                    await SetupInitializer(logicGateInitializer, data);
                    return new LogicGateSensor(logicGateInitializer);
                case 6:
                    var counterInitializer = new CounterIntializer()
                    {
                        Count = data.Data,
                    };
                    await SetupInitializer(counterInitializer, data);
                    return new CounterSensor(counterInitializer);
                default:
                    throw new InvalidOperationException();
            }
        }

        public LogicActuatorCreator(LegacyMapBuilder builder) : base(builder) {}
    }
}