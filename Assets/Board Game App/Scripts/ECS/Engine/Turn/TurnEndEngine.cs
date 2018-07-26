﻿using Data.Step.Drop;
using Data.Step.Piece.Move;
using Data.Step.Turn;
using Service.Turn;
using Svelto.ECS;

namespace ECS.Engine.Turn
{
    class TurnEndEngine : IStep<MovePieceStepState>, IStep<DropStepState>, IQueryingEntitiesEngine
    {
        private Sequencer turnStartSequencer;

        public IEntitiesDB entitiesDB { private get; set; }

        public TurnEndEngine(Sequencer turnStartSequencer)
        {
            this.turnStartSequencer = turnStartSequencer;
        }

        public void Ready()
        { }

        public void Step(ref MovePieceStepState token, int condition)
        {
            EndTurn();
        }

        public void Step(ref DropStepState token, int condition)
        {
            EndTurn();
        }

        private void EndTurn()
        {
            SwitchTurnPlayer();

            var turnStartStepState = new TurnStartStepState();
            turnStartSequencer.Next<TurnStartStepState>(this, ref turnStartStepState);
        }

        private void SwitchTurnPlayer()
        {
            TurnService.SwitchTurnPlayer(entitiesDB);
        }
    }
}
