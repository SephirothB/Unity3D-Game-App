﻿using Data.Enums.Modal;
using Data.Enums.Move;
using Data.Step.Piece.Ability.Substitution;
using Data.Step.Piece.Ability.TierExchange;
using Data.Step.Piece.Capture;
using Data.Step.Piece.Click;
using Data.Step.Piece.Move;
using ECS.EntityView.Board.Tile;
using ECS.EntityView.Modal;
using ECS.EntityView.Piece;
using Service.Board.Tile;
using Service.Modal;
using Service.Piece.Find;
using Svelto.ECS;
using System;

namespace ECS.Engine.Modal.CaptureStack
{
    class CaptureStackModalAnswerEngine : SingleEntityEngine<ModalEV>, IQueryingEntitiesEngine
    {
        private ModalService modalService = new ModalService();
        private PieceFindService pieceFindService = new PieceFindService();
        private TileService tileService = new TileService();

        private readonly ISequencer captureStackModalAnswerSequence;

        public IEntitiesDB entitiesDB { private get; set; }

        public CaptureStackModalAnswerEngine(ISequencer captureStackModalAnswerSequence)
        {
            this.captureStackModalAnswerSequence = captureStackModalAnswerSequence;
        }

        public void Ready()
        { }

        protected override void Add(ref ModalEV entityView)
        {
            entityView.CaptureOrStack.Answer.NotifyOnValueSet(OnPressed);
        }

        protected override void Remove(ref ModalEV entityView)
        {
            entityView.CaptureOrStack.Answer.StopNotify(OnPressed);
        }

        private void OnPressed(int entityId, ModalQuestionAnswer answer)
        {
            switch (answer)
            {
                case ModalQuestionAnswer.CAPTURE:
                    NextActionCapture();
                    break;
                case ModalQuestionAnswer.STACK:
                    NextActionStack();
                    break;
                case ModalQuestionAnswer.SUBSTITUTION:
                    NextActionSubstitution();
                    break;
                case ModalQuestionAnswer.TIER_1_3_EXCHANGE:
                    NextActionTierExchange();
                    break;
                case ModalQuestionAnswer.CLICK:
                    NextActionClick();
                    break;
                default:
                    throw new InvalidOperationException("Unsupported ModalQuestionAnswer value");
            }
        }

        private void NextActionCapture()
        {
            ModalEV modal = modalService.FindModalEV(entitiesDB);
            TileEV destinationTile = FindDestinationTile(modal);
            PieceEV topPieceAtDestinationTile = pieceFindService.FindTopPieceByLocation(
                destinationTile.Location.Location, entitiesDB).Value;
            PieceEV pieceToMove = FindPieceToMove(destinationTile);

            var captureToken = new CapturePieceStepState
            {
                PieceToCapture = topPieceAtDestinationTile,
                PieceToMove = pieceToMove,
                DestinationTile = destinationTile
            };

            captureStackModalAnswerSequence.Next(this, ref captureToken, (int)MoveState.MOBILE_CAPTURE);
        }

        private void NextActionStack()
        {
            ModalEV modal = modalService.FindModalEV(entitiesDB);
            TileEV destinationTile = FindDestinationTile(modal);
            PieceEV pieceToMove = FindPieceToMove(destinationTile);

            var movePieceStepState = new MovePieceStepState
            {
                DestinationTile = destinationTile,
                PieceToMove = pieceToMove
            };

            captureStackModalAnswerSequence.Next(this, ref movePieceStepState, (int)MoveState.MOVE_PIECE);
        }

        private void NextActionSubstitution()
        {
            ModalEV modal = modalService.FindModalEV(entitiesDB);
            TileEV clickedTile = FindDestinationTile(modal);
            PieceEV ninjaPiece = FindTopPiece(clickedTile);

            var token = new SubstitutionStepState
            {
                SubstitutionPiece = ninjaPiece,
                TileReferenceEV = clickedTile
            };

            captureStackModalAnswerSequence.Next(this, ref token, (int)MoveState.SUBSTITUTION);
        }

        private void NextActionTierExchange()
        {
            ModalEV modal = modalService.FindModalEV(entitiesDB);
            TileEV clickedTile = FindDestinationTile(modal);

            var token = new TierExchangeStepState
            {
                ReferenceTile = clickedTile
            };

            captureStackModalAnswerSequence.Next(this, ref token, (int)MoveState.TIER_1_3_EXCHANGE);
        }

        private void NextActionClick()
        {
            ModalEV modal = modalService.FindModalEV(entitiesDB);
            TileEV clickedTile = FindDestinationTile(modal);
            PieceEV clickedPiece = FindTopPiece(clickedTile);

            var token = new ClickPieceStepState
            {
                ClickedPiece = clickedPiece
            };

            captureStackModalAnswerSequence.Next(this, ref token, (int)MoveState.CLICK);
        }

        private TileEV FindDestinationTile(ModalEV modal)
        {
            return tileService.FindTileEV(modal.CaptureOrStack.TileReferenceId, entitiesDB);
        }

        private PieceEV FindPieceToMove(TileEV destinationTile)
        {
            return pieceFindService.FindPieceEVById(
                destinationTile.Tile.PieceRefEntityId.Value, entitiesDB).Value;
        }

        private PieceEV FindTopPiece(TileEV destinationTile)
        {
            return pieceFindService.FindTopPieceByLocation(
                destinationTile.Location.Location, entitiesDB).Value;
        }
    }
}
