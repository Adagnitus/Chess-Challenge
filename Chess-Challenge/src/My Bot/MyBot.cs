using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


public class MyBot : IChessBot
{
    int[] pieceVal = { 0, 100, 300, 300, 500, 900, 0 };
    
    public Move Think(Board board, Timer timer)
    {
        bool isWhite = board.IsWhiteToMove;
        Move[] moves = board.GetLegalMoves();
        Move bestMove = moves[0];
        int bestVal = int.MinValue;

        foreach (Move move in moves) 
        {
            board.MakeMove(move);
            int val = -Negamax(board, int.MinValue, int.MaxValue, 5);
            board.UndoMove(move);
            if (val > bestVal)
            {
                bestVal = val;
                bestMove = move;
            }
        }
        Console.WriteLine($"time spent thinking: {timer.MillisecondsElapsedThisTurn} ms");
        return bestMove;
    }

    private int Eval(Board board)
    {
        if (board.IsInCheckmate()) return board.IsWhiteToMove ? int.MinValue : int.MaxValue;
        if (board.IsDraw()) return 0;

        List<PieceList> pieces = board.GetAllPieceLists().ToList();
        List<PieceList> whitePieces = pieces.Where((list) => list.IsWhitePieceList == true).ToList();
        List<PieceList> blackPieces = pieces.Where((list) => list.IsWhitePieceList == false).ToList();

        int totalWhiteVal = whitePieces.Sum((p) => pieceVal[(int)p.TypeOfPieceInList] * p.Count);
        int totalBlackVal = blackPieces.Sum((p) => pieceVal[(int)p.TypeOfPieceInList] * p.Count);

        int moves = board.IsWhiteToMove ? board.GetLegalMoves().Length : -board.GetLegalMoves().Length;
        int bonus = 0;
        if (board.IsInCheck())
        {
            bonus += board.IsWhiteToMove ? -700 : 700;
        }
        if (board.IsRepeatedPosition())
        {
            bonus -= 500;
        }
        
        return bonus + moves + totalWhiteVal - totalBlackVal;
    }

    private int Negamax(Board board, int alpha, int beta, int depth)
    {
        Move[] moves = board.GetLegalMoves();

        if (depth == 0 || moves.Length == 0) 
        {
            return board.IsWhiteToMove ? Eval(board) : -Eval(board);
        }
        moves = orderMoves(moves, board)[..Math.Min(5, moves.Length)];

        int value = int.MinValue;

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            value = Math.Max(value, -Negamax(board, -beta, -alpha, depth -1));
            board.UndoMove(move);
            alpha = Math.Max(alpha, value);
            if (alpha >= beta)
            {
                break;
            }
        }
        return value;
    }

    private Move[] orderMoves(Move[] moves, Board board)
    {
        return moves.Select(move => {
            board.MakeMove(move);
            int val = board.IsWhiteToMove ? Eval(board) : -Eval(board);
            board.UndoMove(move);
            return (move, val);
        }).OrderBy(t => -t.Item2).Select(t => t.Item1).ToArray();
    }

}


/*
 * 
 * Where(t => {
            if (!board.IsWhiteToMove && t.Item2 > 800) return false;
            if (board.IsWhiteToMove && t.Item2 < -800) return false;
            return true;
        })
 * 
 * 
 */