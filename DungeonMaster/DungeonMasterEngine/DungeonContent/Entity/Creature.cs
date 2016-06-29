﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DungeonMasterEngine.Builders;
using DungeonMasterEngine.DungeonContent.Actuators;
using DungeonMasterEngine.DungeonContent.Entity.BodyInventory.Base;
using DungeonMasterEngine.DungeonContent.Entity.GroupSupport.Base;
using DungeonMasterEngine.DungeonContent.Entity.Properties;
using DungeonMasterEngine.DungeonContent.Entity.Properties.Base;
using DungeonMasterEngine.DungeonContent.Entity.Relations;
using DungeonMasterEngine.DungeonContent.Entity.Skills.Base;
using DungeonMasterEngine.DungeonContent.Tiles;
using DungeonMasterEngine.DungeonContent.Tiles.Renderers;
using DungeonMasterEngine.DungeonContent.Tiles.Support;
using DungeonMasterEngine.Graphics;
using DungeonMasterEngine.Graphics.ResourcesProvides;
using DungeonMasterEngine.Helpers;
using DungeonMasterEngine.Interfaces;
using Microsoft.Xna.Framework;

namespace DungeonMasterEngine.DungeonContent.Entity
{
    public class Creature : LiveEntity, IRenderable
    {
        public CreatureFactory Factory { get; }
        private static readonly Random random = new Random();
        private static readonly BreadthFirstSearch<ITile, object> globalSearcher = new BreadthFirstSearch<ITile, object>();
        private readonly Animator<Creature, ISpaceRouteElement> animator = new Animator<Creature, ISpaceRouteElement>();
        private readonly BreadthFirstSearch<ITile, uint> watchAroundArea = new BreadthFirstSearch<ITile, uint>();
        private ITile watchAroungOrigin;

        public override IRelationManager RelationManager { get; }
        public override IBody Body { get; }

        private readonly Dictionary<IPropertyFactory, IProperty> properties;

        public IRenderer Renderer { get; set; }

        public int DetectRange => Factory.DetectRange;

        public int SightRange => Factory.SightRange;

        public bool hounting => hountingPath != null;

        public int MoveDuration => (int)(Factory.MoveDuration * random.Next(9, 11) / 10f);

        public override float TranslationVelocity => 4;

        public int watchAroundRadius { get; protected set; } = 3;

        public override IGroupLayout GroupLayout => Factory.Layout;

        protected ISpaceRouteElement location = null;

        protected int attackDuration = 1000;

        protected IReadOnlyList<ITile> hountingPath = null;

        protected bool gettingHome => homeRoute != null;

        protected IReadOnlyList<ITile> homeRoute = null;

        public bool Alive { get; private set; } = true;

        public override bool Activated
        {
            get { return base.Activated; }
            set
            {
                base.Activated = value;
                if (Activated == value)
                    Live();
            }
        }

        public override ISpaceRouteElement Location
        {
            get { return location; }
            set
            {
                if (location == value)
                    throw new InvalidOperationException("Cannot move from space to itself.");

                bool alreadyOnTile = location?.Tile == value.Tile;

                if (!alreadyOnTile)
                    location?.Tile?.OnObjectLeft(this);

                location = value;
                Position = location.StayPoint;

                if (!alreadyOnTile)
                    location?.Tile?.OnObjectEntered(this);
            }
        }

        public Creature(ICreatureInitializer initializer, CreatureFactory factory)
        {
            Factory = factory;
            location = initializer.Location;
            if (!location.Tile.LayoutManager.TryGetSpace(this, location.Space))
                throw new ArgumentException("Location is not accessable!");

            watchAroungOrigin = location.Tile;
            RelationManager = new DefaultRelationManager(initializer.RelationToken, initializer.EnemiesTokens);

            HealthProperty health;
            properties = new Dictionary<IPropertyFactory, IProperty>
            {
                {PropertyFactory<HealthProperty>.Instance, health = new HealthProperty(initializer.HitPoints) },
                {PropertyFactory<ExperienceProperty>.Instance, new ExperienceProperty(Factory.Experience)},
            };

            health.ValueChanged += async (sender, value) =>
            {
                if (Alive && value <= 0)
                {
                    Alive = false;

                    await animator.AnimatingTask;
                    location.Tile.LayoutManager.FreeSpace(this, location.Space);
                    location.Tile.OnObjectLeft(this);
                }
            };
        }

        public override IProperty GetProperty(IPropertyFactory propertyType)
        {
            IProperty res;
            properties.TryGetValue(propertyType, out res);
            return res ?? new EmptyProperty();
        }

        public override ISkill GetSkill(ISkillFactory skillType)
        {
            throw new NotImplementedException();
        }

        #region Simple AI
        protected virtual async void Live()
        {
            while (Activated && Alive)
            {
                if (hounting)
                    await Hount();
                else if (gettingHome)
                    await GoHome();
                else
                    await WatchAround();

                await Task.Delay(100);
            }
        }

        protected virtual async Task GoHome()
        {
            foreach (var tile in homeRoute.Skip(1))//first tile is current tile
            {
                if (!await MoveToNeighbourTile(tile, findEnemies: true))
                    break;
            }

            homeRoute = null;
        }

        protected virtual async Task WatchAround()
        {
            var nextRoute = FindNextWatchLocation();
            foreach (var tile in nextRoute.Skip(1))//first tile is current tile
            {
                if (!await MoveToNeighbourTile(tile, findEnemies: true))
                    break;
            }
        }

        protected virtual async Task<bool> MoveToNeighbourTile(ITile tile, bool findEnemies)
        {
            if (tile.IsAccessible) //TODO other conditions
            {
                if (await MoveThroughSpaces(tile, findEnemies))
                {
                    if (!hounting && !gettingHome)
                        watchAroundArea.SetBundle(tile, watchAroundArea.GetBundle(tile) + 1);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        protected virtual async Task<bool> MoveThroughSpaces(ITile targetTile, bool findEnemies)
        {
            var spaceRoute = GroupLayout.GetToNeighbour(this, Location.Tile, targetTile);

            if (spaceRoute == null)
                return false;

            foreach (var space in spaceRoute.Skip(1))
            {
                if (findEnemies && FindEnemies())
                    return false;

                var duration = Task.Delay(MoveDuration);
                if (!await MoveToSpace(space))
                    return false;

                await duration;
            }
            return true;
        }

        protected virtual async Task<bool> MoveToSpace(ISpaceRouteElement destination)
        {
            if (destination.Tile.LayoutManager.TryGetSpace(this, destination.Space))
            {
                //free previous location
                location?.Tile.LayoutManager.FreeSpace(this, location.Space);
                await animator.MoveToAsync(this, destination, setLocation: true);
                return true;
            }
            else
                return false;
        }

        protected virtual IReadOnlyList<ITile> FindNextWatchLocation()
        {
            var maxTravelDistance = random.Next(2, 2 * watchAroundRadius + 1);
            ITile destTile = Location.Tile;
            uint destTileUsages = 0;
            int desDist = 0;
            watchAroundArea.StartSearch(watchAroungOrigin, Location.Tile, watchAroundRadius, (tile, distance, bundle) =>
            {
                if (tile == null)
                    throw new Exception();

                if (distance > maxTravelDistance)
                    watchAroundArea.StopSearch();

                if ((desDist < distance) || (desDist == distance && destTileUsages < bundle))
                {
                    destTile = tile;
                    destTileUsages = bundle;
                    desDist = distance;
                }
            });

            if (destTile == null)
                throw new Exception();

            return watchAroundArea.GetShortestRoute(destTile);
        }

        protected virtual bool FindEnemies()
        {
            ILiveEntity enemy = null;
            globalSearcher.StartSearch(Location.Tile, Location.Tile, Math.Max(DetectRange, SightRange), (tile, layer, bundle) =>
            {
                enemy = tile.LayoutManager.Entities.FirstOrDefault(e => RelationManager.IsEnemy(e.RelationManager.RelationToken));
                if (enemy != null)
                    globalSearcher.StopSearch();
            });
            if (enemy != null)
            {
                hountingPath = globalSearcher.GetShortestRoute(enemy.Location.Tile);
                $"{this} found enemies at {hountingPath.Last().GridPosition}".Dump();
                return true;
            }
            else
            {
                hountingPath = null;
                return false;
            }
        }

        protected virtual async Task Hount()
        {
            if (hountingPath != null)
            {
                if (hountingPath.Count == 2)
                {
                    await PrepareForFight(hountingPath[1]);
                }
                else
                {
                    await MoveToNeighbourTile(hountingPath.Skip(1).First(), findEnemies: false);
                }

                if (!FindEnemies())
                {
                    if (!GetPathHome())
                        EstablishNewBase();
                }
            }
        }

        protected virtual bool GetPathHome()
        {
            var distance = watchAroungOrigin.GridPosition - Location.Tile.GridPosition;//TODO calculate appropriate distance
            var maxDistance = Math.Max(Math.Abs(distance.X), Math.Abs(distance.Y));
            ITile destTile = null;
            globalSearcher.StartSearch(Location.Tile, Location.Tile, maxDistance, (tile, layer, bundle) =>
            {
                if (tile == watchAroungOrigin)
                {
                    globalSearcher.StopSearch();
                    destTile = tile;
                }
            });
            if (destTile == null) //lost
            {
                $"{this} lost".Dump();
                homeRoute = null;
                return false;
            }
            else
            {
                $"{this} going home.".Dump();
                homeRoute = globalSearcher.GetShortestRoute(destTile);
                return true;
            }
        }

        protected virtual void EstablishNewBase()
        {

            $"{this} reestablishing base at {Location}.".Dump();
            watchAroundArea.ClearBundles();
            watchAroungOrigin = Location.Tile;
        }

        protected virtual async Task PrepareForFight(ITile enemyTile)
        {
            var moveDirection = Location.Tile.Neighbors.Single(t => t.Item1 == enemyTile).Item2;

            while (true)
            {
                var routeToSide = GroupLayout.GetToSide(this, Location.Tile, moveDirection);
                if (routeToSide != null)
                {
                    foreach (var space in routeToSide.Skip(1))
                    {
                        if (!await MoveToSpace(space))
                        {
                            goto whileLoop;
                        }
                        await Task.Delay(MoveDuration);
                    }

                    await Fight(enemyTile, moveDirection);
                    break;
                }

                whileLoop:
                await Task.Delay(100);
            }
        }

        protected virtual async Task Fight(ITile enemyTile, MapDirection moveDirection)
        {
            var sortedEnemyLocation = GroupLayout.AllSpaces
                .Where(s => s.Sides.Contains(moveDirection.Opposite))
                .Concat(GroupLayout.AllSpaces
                    .Where(s => s.Sides.Contains(moveDirection)))
                .Where(s => !enemyTile.LayoutManager.IsFree(s));

            var locEnum = sortedEnemyLocation.GetEnumerator();


            while (locEnum.MoveNext() && (Activated && Alive))
            {
                await Task.Delay(100);

                if (enemyTile.LayoutManager.WholeTileEmpty)
                    break;

                ILiveEntity enemy;
                do
                {
                    enemy = enemyTile.LayoutManager.GetEntities(locEnum.Current).FirstOrDefault();
                    if (enemy != null)
                    {
                        //TODO $"{enemy} hitted.".Dump();
                        await Task.Delay(attackDuration);
                        //TODO if killed reset enumerator
                        if (false)
                        {
                            locEnum.Reset();
                            break;
                        }
                    }
                }
                while (Activated && Alive && enemy != null);
            }
        }

        #endregion

        public override void MoveTo(ITile newLocation, bool setNewLocation)
        {

        }

        public override void Update(GameTime time)
        {
            animator.Update(time);
        }

        public override string ToString()
        {
            return "creature";
        }

    }
}