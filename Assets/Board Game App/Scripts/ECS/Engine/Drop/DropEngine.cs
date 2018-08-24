﻿using Data.Constants.Board;
using Data.Step.Drop;
using Data.Step.Turn;
using ECS.EntityView.Hand;
using ECS.EntityView.Piece;
using ECS.EntityView.Turn;
using Service.Check;
using Service.Drop;
using Service.Hand;
using Service.Piece.Find;
using Service.Turn;
using Svelto.ECS;

namespace ECS.Engine.Drop
{
    class DropEngine : IStep<DropStepState>, IQueryingEntitiesEngine
    {
        private CheckService checkService = new CheckService();
        private DropService dropService = new DropService();
        private HandService handService = new HandService();
        private PieceFindService pieceFindService = new PieceFindService();
        private TurnService turnService = new TurnService();

        private readonly ISequencer dropSequence;

        public IEntitiesDB entitiesDB { private get; set; }

        public DropEngine(ISequencer dropSequence)
        {
            this.dropSequence = dropSequence;
        }

        public void Ready()
        { }

        public void Step(ref DropStepState token, int condition)
        {
            TurnEV currentTurn = turnService.GetCurrentTurnEV(entitiesDB);
            PieceEV pieceToDrop = pieceFindService.FindFirstPieceByLocationAndType(
                BoardConst.HAND_LOCATION, token.handPiece.HandPiece.PieceType, entitiesDB);

            if (!currentTurn.Check.CommanderInCheck
                || checkService.DropReleasesCheck(pieceToDrop, token.destinationTile.Location.Location, currentTurn, entitiesDB))
            {
                dropService.DropPiece(ref pieceToDrop, ref token.destinationTile, token.handPiece.PlayerOwner.PlayerColor, entitiesDB);
                UpdateHandPiece(ref token.handPiece);
                GotoTurnEndStep();
            }
        }

        private void UpdateHandPiece(ref HandPieceEV handPiece)
        {
            handService.DecrementHandPiece(ref handPiece);
        }

        private void GotoTurnEndStep()
        {
            var turnEndToken = new TurnEndStepState();
            dropSequence.Next(this, ref turnEndToken);
        }
    }
}
