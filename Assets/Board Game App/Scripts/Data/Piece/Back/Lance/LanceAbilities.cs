﻿using Data.Enum.Piece.Drop;
using Data.Enum.Piece.PostMove;
using System.Collections.Generic;

namespace Data.Piece.Back.Lance
{
    class LanceAbilities : IAbilities
    {
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
                return new List<PostMoveAbility>(new PostMoveAbility[] { PostMoveAbility.FORCED_RECOVERY, PostMoveAbility.FORCED_REARRANGEMENT });
            }
        }
    }
}