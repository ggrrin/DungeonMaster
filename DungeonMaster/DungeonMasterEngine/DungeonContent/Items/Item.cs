﻿using DungeonMasterEngine.DungeonContent.Tiles;
using DungeonMasterEngine.Graphics;
using DungeonMasterEngine.Interfaces;
using Microsoft.Xna.Framework;

namespace DungeonMasterEngine.DungeonContent.Items
{
    public abstract class Item : WorldObject,  IItem
    {
        private Tile location;
        public Graphic Graphics { get; set; }
        public bool Visible { get; set; } = true;
        public bool AcceptMessages { get; set; }

        public virtual BoundingBox Bounding => new BoundingBox(Position, Position + Graphics.Scale);
        public sealed override IGraphicProvider GraphicsProvider => Visible ? Graphics : null;

        public override Vector3 Position
        {
            get
            {
                return base.Position;
            }

            set
            {
                base.Position = value;
                if (Graphics != null)
                    Graphics.Position = value;
            }
        }

        public Tile Location
        {
            get
            {
                return location;
            }

            set
            {
                //old
                location?.OnObjectLeft(this);

                location = value;
                
                location?.OnObjectEntered(this);
                OnLocationChanged();
            }
        }

        protected Item(Vector3 position) : base(position)
        {
            Graphics = new CubeGraphic
            {
                Position = position,
                Scale = new Vector3(0.25f),
                DrawFaces = CubeFaces.All ^ CubeFaces.Floor,
                Outter = true
            };
        }

        public virtual GrabableItem ExchangeItems(GrabableItem item)
        {
            return item;
        }

        protected virtual void OnLocationChanged()
        {
            Position = location?.Position ?? Vector3.Zero;
        }

        public virtual void Update(GameTime gameTime)
        { }
    }
}
