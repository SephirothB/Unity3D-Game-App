﻿using Data.Constants.Board;
using Data.Enum;
using Data.Enum.Player;
using ECS.EntityView.Piece;
using Service.Piece.Factory;
using Service.Piece.Find;
using Svelto.ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Service.Board
{
    public class DestinationTileService
    {
        private PieceFactory pieceFactory = new PieceFactory();
        private PieceFindService pieceFindService = new PieceFindService();
        /**
         * This will return values exceeding board boundaries (such as below the zero-th rank).  It is the
         * responsibility of the client to not abuse this data.
         */
        public List<Vector2> CalcDestinationTileLocations(
            PieceEV pieceEV, IEntitiesDB entitiesDB, List<PieceEV> allPieces = null)
        {
            List<Vector2> returnValue = new List<Vector2>();

            if (allPieces == null)
            {
                // Current piece included, doesn't matter for current calc
                allPieces = pieceFindService.FindAllBoardPieces(entitiesDB).ToList();
            }

            returnValue.AddRange(CalcSingleDestinations(pieceEV, allPieces));

            return returnValue;
        }

        public HashSet<Vector2> CalcDestinationTileLocations(PieceEV[] pieces, IEntitiesDB entitiesDB)
        {
            HashSet<Vector2> returnValue = new HashSet<Vector2>();
            List<PieceEV> allPieces = pieceFindService.FindAllBoardPieces(entitiesDB).ToList();

            for (int i = 0; i < pieces.Length; ++i)
            {
                returnValue.UnionWith(CalcDestinationTileLocations(pieces[i], entitiesDB, allPieces));
            }

            return returnValue;
        }

        private List<Vector2> CalcSingleDestinations(PieceEV pieceEV, List<PieceEV> allPieces)
        {
            bool useGoldMovement = IsOpponentPieceDirectlyBelow(pieceEV, allPieces);
            List<Vector2> returnValue = GetRawSingleDestinationLocations(pieceEV, useGoldMovement);
            AdjustRawDataWithPieceLocationAndDirection(pieceEV, returnValue);
            ExcludeOutOfBoard(returnValue);
            ExcludeDestinationsWithFriendlyTier3Tower(pieceEV, returnValue, allPieces);
            // Do NOT allow destinations other pieces in the way
            ExcludeDestinationsWithObstructingPieces(pieceEV, returnValue, allPieces);

            return returnValue;
        }

        private bool IsOpponentPieceDirectlyBelow(PieceEV pieceEV, List<PieceEV> allPieces)
        {
            return pieceEV.Tier.Tier != 1
                && allPieces.Where(piece =>
                    piece.Location.Location == pieceEV.Location.Location
                    && piece.PlayerOwner.PlayerColor != pieceEV.PlayerOwner.PlayerColor
                    && piece.Tier.Tier + 1 == pieceEV.Tier.Tier)
                .Count() > 0;
        }

        private List<Vector2> GetRawSingleDestinationLocations(PieceEV pieceEV, bool useGoldMovement)
        {
            PieceType pieceToCreate = !useGoldMovement ? pieceEV.Piece.PieceType : PieceType.GOLD;

            return pieceFactory.CreateIPieceData(pieceToCreate).Tiers[pieceEV.Tier.Tier - 1].Single;
        }

        private void AdjustRawDataWithPieceLocationAndDirection(
            PieceEV pieceEV, List<Vector2> rawLocationData)
        {
            // Add piece's location to value
            for (int i = 0; i < rawLocationData.Count; ++i)
            {
                rawLocationData[i] = new Vector2(
                    pieceEV.Location.Location.x + (rawLocationData[i].x * (int)pieceEV.Piece.Direction),
                    pieceEV.Location.Location.y + (rawLocationData[i].y * (int)pieceEV.Piece.Direction));
            }
        }

        /**
         * If destinations are outside board, such as (-2, 1), then exclude them.
         */
        private void ExcludeOutOfBoard(List<Vector2> returnValue)
        {
            returnValue.RemoveAll(location =>
                location.x < 0 || location.x >= BoardConst.NUM_FILES_RANKS
                || location.y < 0 || location.y >= BoardConst.NUM_FILES_RANKS
            );
        }

        /**
         * If destination contains a tier 3 tower with a friendly piece on top,
         * then that tile is not a valid destination tile.
         */
        private void ExcludeDestinationsWithFriendlyTier3Tower(
            PieceEV pieceToCalc, List<Vector2> destinations, List<PieceEV> allPieces)
        {
            List<Vector2> destinationsToRemove = new List<Vector2>();

            foreach (Vector2 destination in destinations)
            {
                if (HasFriendlyTier3Tower(pieceToCalc, destination, allPieces))
                {
                    destinationsToRemove.Add(destination);
                }
            }

            foreach (Vector2 removeDestination in destinationsToRemove)
            {
                destinations.Remove(removeDestination);
            }
        }

        private bool HasFriendlyTier3Tower(
            PieceEV pieceToCalc, Vector2 destination, List<PieceEV> allPieces)
        {
            PlayerColor friendlyColor = pieceToCalc.PlayerOwner.PlayerColor;

            int numPiecesBarringPath = allPieces.Where(piece =>
                piece.Tier.Tier == 3
                && piece.PlayerOwner.PlayerColor == friendlyColor
                && destination == piece.Location.Location).Count();

            return numPiecesBarringPath > 0;
        }

        /**
         * Potentially modify param destinations, by removing locations with piece(s) in the way
         * 
         * Some destinations are in different rank AND file as pieceToCalc's current location
         */
        private void ExcludeDestinationsWithObstructingPieces(
            PieceEV pieceToCalc, List<Vector2> destinations, List<PieceEV> allPieces)
        {
            List<Vector2> destinationsToRemove = new List<Vector2>();
            /*
             * Types of destinations
             * * One tile away, horizontally, vertically, or diagonally
             * More than one tile away,
             * * Same rank, different file
             * * Same file, different rank
             * * Different rank, different file
             */

            // for loop hopLocations, since that count will often be less than allPiece's count
            foreach (Vector2 destination in destinations)
            {
                if (ShouldRemoveDestination(pieceToCalc, destination, allPieces))
                {
                    destinationsToRemove.Add(destination);
                }
            }

            foreach (Vector2 removeDestination in destinationsToRemove)
            {
                destinations.Remove(removeDestination);
            }
        }

        private bool ShouldRemoveDestination(
            PieceEV pieceToCalc, Vector2 destination, List<PieceEV> allPieces)
        {
            bool returnValue = false;
            Vector2 pieceLocation = pieceToCalc.Location.Location;

            Vector2 increment = new Vector2(
                pieceLocation.x == destination.x ?
                    0 : (pieceLocation.x - destination.x) / Math.Abs(pieceLocation.x - destination.x),
                pieceLocation.y == destination.y ?
                    0 : (pieceLocation.y - destination.y) / Math.Abs(pieceLocation.y - destination.y)
                );

            Vector2 evalLocation = pieceLocation - increment;

            while (evalLocation != destination)
            {
                int numPiecesBarringPath = allPieces.Where(piece =>
                    evalLocation == piece.Location.Location).Count();

                if (numPiecesBarringPath > 0)
                {
                    returnValue = true;
                    break;
                }

                evalLocation -= increment;
            }

            return returnValue;
        }
    }
}
