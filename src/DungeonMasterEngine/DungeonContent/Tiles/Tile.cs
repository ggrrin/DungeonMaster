﻿using System;
using System.Collections.Generic;
using DungeonMasterEngine.DungeonContent.Entity;
using DungeonMasterEngine.DungeonContent.Entity.GroupSupport.Base;
using DungeonMasterEngine.DungeonContent.Tiles.Renderers;
using DungeonMasterEngine.DungeonContent.Tiles.Sides;
using DungeonMasterEngine.DungeonContent.Tiles.Support;
using DungeonMasterEngine.Helpers;
using DungeonMasterEngine.Interfaces;
using Microsoft.Xna.Framework;

namespace DungeonMasterEngine.DungeonContent.Tiles
{
    public abstract class Tile<TMessage> : Tile, IMessageAcceptor<TMessage> where TMessage : Message
    {
        protected Tile(TileInitializer initializer) : base(initializer) { }

        public abstract override bool IsAccessible { get; }

        public sealed override void AcceptMessageBase(Message message)
        {

            if(message is TMessage)
                AcceptMessage(tMessage: (TMessage)message);
        }

        public virtual void AcceptMessage(TMessage tMessage)
        {
            base.AcceptMessageBase(tMessage);
        }

    }

    public abstract class Tile :  ITile
    {
        public abstract IEnumerable<object> SubItems { get; }
        public bool IsInitialized => Level != null; 

        protected Tile(TileInitializer initializer)
        {
            initializer.Initializing += Initialize;
            initializer.Initialized += AfterInitialized;
        }

        private void AfterInitialized(TileInitializer initializer)
        {
            Renderer?.Initialize();

            initializer.Initialized -= AfterInitialized;
            Initialized?.Invoke(this, new EventArgs());
        }


        private void Initialize(TileInitializer initializer)
        {
            GridPosition = initializer.GridPosition;
            Level = initializer.Level;
            Neighbors = initializer.Neighbors;


            initializer.Initializing -= Initialize;
        }

        public virtual LayoutManager<ILiveEntity> LayoutManager { get; } = new LayoutManager<ILiveEntity>();

        public virtual TileNeighbors Neighbors { get; protected set; }

        INeighbors<ITile> INeighbourable<ITile>.Neighbors => Neighbors;

        public event EventHandler Initialized;
        public abstract bool IsAccessible { get; }
        public virtual bool CanFlyItems => true;
        public virtual bool IsTransparent => true;

        public Vector3 Position => new Vector3(GridPosition.X, -LevelIndex, GridPosition.Y);

        public int LevelIndex => Level.LevelIndex;

        public Point GridPosition { get; private set; }

        public DungeonLevel Level { get; private set; }

        public virtual Vector3 StayPoint => Position + new Vector3(0.5f);

        public virtual bool ContentActivated { get; protected set; }
        public ICollection<IRenderable> Drawables { get; } = new HashSet<IRenderable>();
        public virtual bool IsDangerous => false; 

        public virtual void ActivateTileContent()
        {
            ContentActivated = true;
            //$"Activating message received at {GridPosition}".Dump();
        }

        public virtual void DeactivateTileContent()
        {
            ContentActivated = false;
            //$"Deactivating message recived at {GridPosition}".Dump();
        }

        public virtual void OnObjectEntered(object localizable)
        {
            ObjectEntered?.Invoke(this, localizable);
        }

        public virtual void OnObjectLeft(object localizable)
        {
            ObjectLeft?.Invoke(this, localizable);
        }

        public virtual void OnObjectEntering(object localizable) { }

        public virtual void OnObjectLeaving(object localizable) { }

        public event EventHandler<object> ObjectEntered;

        public event EventHandler<object> ObjectLeft;

        public virtual void AcceptMessageBase(Message message)
        {
            switch (message.Action)
            {
                case MessageAction.Set:
                    ActivateTileContent();
                    break;
                case MessageAction.Clear:
                    DeactivateTileContent();
                    break;
                case MessageAction.Toggle:
                    if (ContentActivated)
                        DeactivateTileContent();
                    else
                        ActivateTileContent();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            foreach (var tileSide in Sides)
            {
                tileSide.AcceptMessage(message);
            }
        }

        public abstract IEnumerable<ITileSide> Sides { get; }
        public IRenderer Renderer { get; set; }
    }


 

}