﻿using Data.Enums.Piece.Drop;
using Data.Enums.Piece.OtherMove;
using Data.Enums.Piece.PostMove;
using Data.Enums.Piece.PreMove;
using System.Collections.Generic;

namespace Data.Piece.Front.Captain
{
    class CaptainAbilities : IAbilities
    {
        public OtherMoveAbility? OtherMove
        {
            get
            {
                return OtherMoveAbility.TIER_1_EXCHANGE;
            }
        }

        public List<PreMoveAbility> PreMove
        {
            get
            {
                return new List<PreMoveAbility>();
            }
        }

        public List<DropAbility> Drop
        {
            get
            {
                return new List<DropAbility>();
            }
        }

        public List<PostMoveAbility> PostMove
        {
            get
            {
                return new List<PostMoveAbility>();
            }
        }
    }
}
