﻿using ECS.EntityView.Board.Tile;
using Service.Common;
using Svelto.ECS;
using System;
using UnityEngine;

namespace Service.Board.Tile
{
    public class TileService
    {
        public TileEV FindTileEV(int entityId, IEntitiesDB entitiesDB)
        {
            return CommonService.FindEntity<TileEV>(entityId, entitiesDB);
        }

        public TileEV FindTileEV(Vector2 location, IEntitiesDB entitiesDB)
        {
            var tiles = FindAllTileEVs(entitiesDB);

            for (int i = 0; i < tiles.Length; ++i)
            {
                if (tiles[i].Location.Location == location)
                {
                    return tiles[i];
                }
            }

            throw new ArgumentOutOfRangeException("No matching location for finding TilEV");
        }

        public TileEV[] FindAllTileEVs(IEntitiesDB entitiesDB)
        {
            return CommonService.FindAllEntities<TileEV>(entitiesDB);
        }

        public TileEV? FindTileEVById(int? entityId, IEntitiesDB entitiesDB)
        {
            TileEV returnValue = CommonService.FindEntityById<TileEV>(entityId, entitiesDB);

            return returnValue.ID.entityID != 0 ? (TileEV?)returnValue : null;
        }
    }
}
