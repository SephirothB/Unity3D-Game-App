﻿using Svelto.ECS;
using ECS.Component.Piece;
using ECS.Component.SharedComponent;
using ECS.Component.Piece.Move;
using ECS.Component.Player;
using ECS.Component.Visibility;
using ECS.Component.Piece.Tower;

namespace ECS.EntityView.Piece
{
    public struct PieceEV : IEntityViewStruct
    {
        public EGID ID { get; set; }

        public IPieceComponent piece;
        public IMovePieceComponent movePiece;
        public ILocationComponent location;
        public ITowerTierComponent tier;
        public IHighlightComponent highlight;
        public IChangeColorComponent changeColor;
        public IPlayerComponent playerOwner;
        public IVisibilityComponent visibility;
    }
}
