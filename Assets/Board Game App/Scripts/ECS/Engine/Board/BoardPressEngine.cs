﻿using Data.Enum;
using Data.Step;
using Data.Step.Board;
using Data.Step.Drop;
using Data.Step.Piece.Capture;
using Data.Step.Piece.Move;
using ECS.EntityView.Board.Tile;
using ECS.EntityView.Hand;
using ECS.EntityView.Piece;
using ECS.EntityView.Turn;
using Scripts.Data.Board;
using Service.Board;
using Service.Turn;
using Svelto.ECS;
using System;

namespace ECS.Engine.Board
{
    class BoardPressEngine : IStep<BoardPressStepState>, IQueryingEntitiesEngine
    {
        private PieceTileService pieceTileService = new PieceTileService();

        private readonly ISequencer boardPressSequence;

        public IEntitiesDB entitiesDB { private get; set; }

        public BoardPressEngine(ISequencer boardPressSequence)
        {
            this.boardPressSequence = boardPressSequence;
        }

        public void Ready()
        { }

        public void Step(ref BoardPressStepState token, int condition)
        {
            ConstraintCheck(ref token);

            BoardPressStateInfo stateInfo = pieceTileService.FindBoardPressStateInfo(entitiesDB, ref token);
            TurnEV currentTurn = TurnService.GetCurrentTurnEV(entitiesDB);

            BoardPress action = BoardPressService.DecideAction(stateInfo, currentTurn);
            ExecuteNextAction(action, stateInfo);
        }

        /// <exception cref="InvalidOperationException">Both token member variables are null/zero.</exception>
        private void ConstraintCheck(ref BoardPressStepState token)
        {
            if (token.pieceEntityId.GetValueOrDefault() == 0
                && token.tileEntityId.GetValueOrDefault() == 0)
            {
                throw new InvalidOperationException("BoardPressEngine: Both piece and tile id are null");
            }
        }

        private void ExecuteNextAction(BoardPress action, BoardPressStateInfo stateInfo)
        {
            switch(action)
            {
                case BoardPress.CLICK_HIGHLIGHT:
                    NextActionHighlight(stateInfo.piece.Value);
                    break;
                case BoardPress.MOVE_PIECE:
                    NextActionMovePiece(stateInfo.piece.Value, stateInfo.tile.Value);
                    break;
                case BoardPress.MOBILE_CAPTURE:
                    NextActionMobileCapturePiece(
                        stateInfo.piece.Value, stateInfo.tile.Value, stateInfo.pieceAtDestination.Value);
                    break;
                case BoardPress.DROP:
                    NextActionDropPiece(stateInfo.handPiece.Value, stateInfo.tile.Value);
                    break;
                case BoardPress.NOTHING:
                    break;
                default:
                    throw new InvalidOperationException("Invalid or unsupported BoardPress state");
            }
        }

        private void NextActionHighlight(PieceEV pieceEV)
        {
            // Give desired state, up to later engines to make changes accordingly
            var pressState = new PressStepState
            {
                pieceEntityId = pieceEV.ID.entityID,
                piecePressState = pieceEV.highlight.IsHighlighted ? PiecePressState.UNCLICKED : PiecePressState.CLICKED,
                affectedTiles = DestinationTileService.CalcDestinationTileLocations(pieceEV.ID.entityID, entitiesDB)
            };

            boardPressSequence.Next(this, ref pressState, (int)BoardPress.CLICK_HIGHLIGHT);
        }

        private void NextActionMovePiece(PieceEV pieceEV, TileEV tileEV)
        {
            var movePieceInfo = new MovePieceStepState
            {
                pieceToMove = pieceEV,
                destinationTile = tileEV
            };

            boardPressSequence.Next(this, ref movePieceInfo, (int)BoardPress.MOVE_PIECE);
        }

        private void NextActionMobileCapturePiece(PieceEV pieceToMove, TileEV destinationTile, PieceEV pieceToCapture)
        {
            // TODO If the user clicks the piece, then the pieceToMove and pieceToCapture are both the pieceToCapture
            var capturePieceInfo = new CapturePieceStepState
            {
                pieceToMove = pieceToMove,
                destinationTile = destinationTile,
                pieceToCapture = pieceToCapture
            };

            boardPressSequence.Next(this, ref capturePieceInfo, (int)BoardPress.MOBILE_CAPTURE);
        }

        private void NextActionDropPiece(HandPieceEV handPiece, TileEV destinationTile)
        {
            var dropInfo = new DropStepState
            {
                handPiece = handPiece,
                destinationTile = destinationTile
            };

            boardPressSequence.Next(this, ref dropInfo, (int)BoardPress.DROP);
        }
    }
}
