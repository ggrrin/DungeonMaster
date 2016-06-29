﻿using System;
using System.Threading.Tasks;
using DungeonMasterEngine.DungeonContent.Tiles;
using DungeonMasterEngine.DungeonContent.Tiles.Support;
using DungeonMasterEngine.Helpers;
using Microsoft.Xna.Framework;

namespace DungeonMasterEngine.DungeonContent.Magic.Spells
{
    public class OpenDoorSpell : Spell
    {
        private readonly MapDirection startDirection;
        private readonly Animator<OpenDoorSpell, ITile> animator = new Animator<OpenDoorSpell, ITile>();

        public override float TranslationVelocity { get; protected set; } = 2 * 2.2f;

        public OpenDoorSpell(ITile location, MapDirection startDirection) : base(location.Position)
        {
            this.startDirection = startDirection;
            Location = location;
        }


        public override async void Run()
        {
            ITile moveTile = null; 
            while (!TryFinishSpell() && null != (moveTile = Location.Neighbors.GetTile(startDirection)))
            {
                await animator.MoveToAsync(this, moveTile, setLocation: true);
            }
            FinishSpell();
        }

        protected override void OnSpellUpdate(GameTime gameTime)
        {
            animator.Update(gameTime);
        }

        private bool TryFinishSpell()
        {
            $"{GetType().Name} : {Location.Position}".Dump();
            var door = Location as DoorTile;
            if (door != null && door.HasButton)
            {
                door.ActivateTileContent();
                return true;
            }
            else if(!Location.IsAccessible)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}