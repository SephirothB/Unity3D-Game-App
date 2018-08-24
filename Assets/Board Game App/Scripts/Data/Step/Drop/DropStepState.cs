﻿using ECS.EntityView.Board.Tile;
using ECS.EntityView.Hand;

namespace Data.Step.Drop
{
    public struct DropStepState
    {
        public HandPieceEV HandPiece;
        public TileEV DestinationTile;
    }
}
