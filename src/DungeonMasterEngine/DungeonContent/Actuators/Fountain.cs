using DungeonMasterEngine.DungeonContent.Tiles.Renderers;
using DungeonMasterEngine.DungeonContent.Tiles.Support;
using DungeonMasterEngine.Interfaces;

namespace DungeonMasterEngine.DungeonContent.Actuators
{
    public class Fountain: IActuatorX
    {
        public IFactories Factories { get; }
        public Fountain(IFactories factories)
        {
            Factories = factories;
        }

        public void AcceptMessage(Message message)
        {
        }


        public IRenderer Renderer { get; set; }

        public bool Trigger(ILeader leader)
        {
            var jar = leader.Hand as IWaterJar;
            if (jar != null)
            {
                jar.Fill(Factories);
                return true;
            }
            return false;
        }

    }
}