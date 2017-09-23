﻿using System;
using DungeonMasterParser.Enums;
using DungeonMasterParser.Items;

namespace DungeonMasterParser.Tiles
{
    public class DoorTileData : TileData
    {
        //Bit 2 - 0: State
        //    '000' Open
        //    '001' 1 / 4 closed
        //    '010' 1 / 2 closed
        //    '011' 3 / 4 closed
        //    '100' Closed
        //    '101' Bashed
        //    '110' Invalid
        //    '111' Invalid
        public DoorState State { get; set; }

        //Bit 3: Orientation
        //    '0' West - East
        //    '1' North - South
        public Orientation Orientation { get; set; }

        public DoorItem Door { get; set; }

        public override T GetTile<T>(ITileCreator<T> t)
        {
            return t.GetTile(this);
        }

        public override void SetupDecorations(DungeonMap map, Random rand) { }
    }
}