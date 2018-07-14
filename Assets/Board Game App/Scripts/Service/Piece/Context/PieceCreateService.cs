﻿using Data.Enum;
using Data.Enum.Player;
using ECS.EntityDescriptor.Piece;
using ECS.Implementor;
using ECS.Implementor.Piece;
using PrefabUtil;
using Service.Common;
using Svelto.ECS;
using UnityEngine;

namespace Service.Piece.Context
{
    class PieceCreateService
    {
        private IEntityFactory entityFactory;
        private PrefabsDictionary prefabsDictionary;

        public PieceCreateService(IEntityFactory entityFactory)
        {
            this.entityFactory = entityFactory;
            prefabsDictionary = new PrefabsDictionary();
        }

        public void CreatePiece(PlayerColor playerOwner, int fileNum, int rankNum)
        {
            var piece = prefabsDictionary.Instantiate("Pawn");
            var pieceImpl = piece.GetComponent<PieceImpl>();
            var pieceHighlightMoveImpl = piece.GetComponent<PieceHighlightMoveImpl>();
            var pieceOwnerImpl = piece.GetComponent<PieceOwnerImpl>();
            entityFactory.BuildEntity<PieceED>(piece.GetInstanceID(), piece.GetComponents<IImplementor>());

            pieceImpl.Direction = playerOwner.Equals(PlayerColor.BLACK) ? Direction.UP : Direction.DOWN;
            pieceOwnerImpl.PlayerColor = playerOwner;

            piece.transform.position = CommonService.CalcTransformPosition(fileNum, rankNum, 1);
            pieceHighlightMoveImpl.Location = new Vector3(fileNum, rankNum, 1);
        }
    }
}
