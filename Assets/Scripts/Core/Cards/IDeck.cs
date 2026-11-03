using System;

namespace PokerGame.Core.Cards
{
    /// <summary>
    /// Contract for a deck of 52 playing cards. Depend on this interface in game logic
    /// so the deck can be mocked in tests.
    /// </summary>
    public interface IDeck
    {
        /// <summary>Number of cards still available to deal.</summary>
        int RemainingCount { get; }

        /// <summary>Number of cards already dealt since the last reset.</summary>
        int DealtCount { get; }

        /// <summary>Deals the top card.</summary>
        /// <exception cref="InvalidOperationException">The deck is empty.</exception>
        Card Deal();

        /// <summary>Tries to deal the top card without throwing.</summary>
        /// <returns>False if the deck is empty.</returns>
        bool TryDeal(out Card card);

        /// <summary>
        /// Deals exactly <c>destination.Length</c> cards atomically (all or nothing).
        /// </summary>
        /// <exception cref="InvalidOperationException">Not enough cards remain.</exception>
        void DealInto(Span<Card> destination);

        /// <summary>Discards the top card (poker "burn" card).</summary>
        /// <exception cref="InvalidOperationException">The deck is empty.</exception>
        void Burn();

        /// <summary>Shuffles the undealt cards using Fisher-Yates.</summary>
        void Shuffle();

        /// <summary>Returns all 52 cards to the deck in canonical (unshuffled) order.</summary>
        void Reset();

        /// <summary>Equivalent to <see cref="Reset"/> followed by <see cref="Shuffle"/>, atomically.</summary>
        void ResetAndShuffle();
    }
}