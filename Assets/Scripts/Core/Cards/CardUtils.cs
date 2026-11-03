using System;
using System.Collections.Generic;

namespace PokerGame.Core.Cards
{
    /// <summary>
    /// Stateless helpers for <see cref="Card"/>: text conversion, validation and comparison.
    /// Allocation-free on hot paths (string names are cached, parsing works on spans).
    /// </summary>
    public static class CardUtils
    {
        /// <summary>Number of cards in a standard deck.</summary>
        public const int DECK_SIZE = 52;

        /// <summary>Number of ranks per suit.</summary>
        public const int RANKS_PER_SUIT = 13;

        /// <summary>Number of suits.</summary>
        public const int SUIT_COUNT = 4;

        private const string INVALID_CARD_TEXT = "??";
        private const string RANK_CHARS = "23456789TJQKA";
        private const string SUIT_CHARS = "cdhs";
        private const string SUIT_SYMBOLS = "\u2663\u2666\u2665\u2660"; // ♣ ♦ ♥ ♠

        private static readonly string[] _shortNames = BuildNames(useSymbols: false);
        private static readonly string[] _displayNames = BuildNames(useSymbols: true);

        /// <summary>Comparer by rank only (ascending, Ace high). Suits are ignored.</summary>
        public static IComparer<Card> RankComparer { get; } = new RankOnlyComparer();

        /// <summary>Comparer by rank, then suit (ascending). Gives a total order.</summary>
        public static IComparer<Card> RankThenSuitComparer { get; } = new RankThenSuitComparer_();

        #region Validation

        /// <summary>Returns true if <paramref name="rank"/> is a defined rank (2..Ace).</summary>
        public static bool IsValid(Rank rank) => rank >= Rank.Two && rank <= Rank.Ace;

        /// <summary>Returns true if <paramref name="suit"/> is a defined suit.</summary>
        public static bool IsValid(Suit suit) => suit >= Suit.Clubs && suit <= Suit.Spades;

        /// <summary>Returns true if both rank and suit of the card are valid.</summary>
        public static bool IsValid(Card card) => card.IsValid;

        #endregion

        #region String conversion

        /// <summary>
        /// Converts a card to short notation: rank char + suit letter ("As", "Td", "7h").
        /// Returns a cached string, so no allocation occurs.
        /// </summary>
        /// <returns>The notation, or "??" for an invalid card.</returns>
        public static string ToString(Card card)
        {
            int index = card.Index;
            return index < 0 ? INVALID_CARD_TEXT : _shortNames[index];
        }

        /// <summary>
        /// Converts a card to a display string with a suit symbol ("A♠", "T♦").
        /// Intended for UI/logging; use <see cref="ToString(Card)"/> for protocols.
        /// </summary>
        public static string ToDisplayString(Card card)
        {
            int index = card.Index;
            return index < 0 ? INVALID_CARD_TEXT : _displayNames[index];
        }

        /// <summary>
        /// Parses text such as "As", "td", "10h" or "K♠" (case-insensitive,
        /// surrounding whitespace ignored). Never throws and never allocates.
        /// </summary>
        /// <param name="text">Text to parse.</param>
        /// <param name="card">Parsed card, or <c>default</c> on failure.</param>
        /// <returns>True on success.</returns>
        public static bool TryParse(ReadOnlySpan<char> text, out Card card)
        {
            card = default;
            text = text.Trim();

            if (text.Length < 2 || text.Length > 3)
            {
                return false;
            }

            Rank rank;
            int suitPosition;

            if (text.Length == 3)
            {
                if (text[0] != '1' || text[1] != '0')
                {
                    return false;
                }

                rank = Rank.Ten;
                suitPosition = 2;
            }
            else
            {
                if (!TryParseRank(text[0], out rank))
                {
                    return false;
                }

                suitPosition = 1;
            }

            if (!TryParseSuit(text[suitPosition], out Suit suit))
            {
                return false;
            }

            card = new Card(rank, suit);
            return true;
        }

        /// <summary>String overload of <see cref="TryParse(ReadOnlySpan{char}, out Card)"/>. Null returns false.</summary>
        public static bool TryParse(string text, out Card card)
        {
            if (text == null)
            {
                card = default;
                return false;
            }

            return TryParse(text.AsSpan(), out card);
        }

        /// <summary>
        /// Parses a card, throwing on failure.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is null.</exception>
        /// <exception cref="FormatException">The text is not a valid card.</exception>
        public static Card Parse(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (!TryParse(text.AsSpan(), out Card card))
            {
                throw new FormatException($"'{text}' is not a valid card.");
            }

            return card;
        }

        #endregion

        #region Comparison

        /// <summary>
        /// Compares by rank only (Ace high). Returns negative, zero or positive.
        /// This is the comparison that matters for poker hand strength.
        /// </summary>
        public static int CompareByRank(Card a, Card b) => ((int)a.Rank).CompareTo((int)b.Rank);

        /// <summary>Compares by rank, then by suit (Clubs &lt; Diamonds &lt; Hearts &lt; Spades).</summary>
        public static int CompareByRankThenSuit(Card a, Card b)
        {
            int byRank = CompareByRank(a, b);
            return byRank != 0 ? byRank : ((int)a.Suit).CompareTo((int)b.Suit);
        }

        /// <summary>True if both cards have the same rank.</summary>
        public static bool HaveSameRank(Card a, Card b) => a.Rank == b.Rank;

        /// <summary>True if both cards have the same suit.</summary>
        public static bool HaveSameSuit(Card a, Card b) => a.Suit == b.Suit;

        /// <summary>
        /// Sorts cards in place by rank, highest first, without allocating
        /// (stable insertion sort; ideal for 2-7 card hands).
        /// </summary>
        /// <param name="cards">Cards to sort.</param>
        public static void SortByRankDescending(Span<Card> cards)
        {
            for (int i = 1; i < cards.Length; i++)
            {
                Card key = cards[i];
                int j = i - 1;

                while (j >= 0 && CompareByRank(cards[j], key) < 0)
                {
                    cards[j + 1] = cards[j];
                    j--;
                }

                cards[j + 1] = key;
            }
        }

        #endregion

        #region Private helpers

        private static bool TryParseRank(char c, out Rank rank)
        {
            int position = RANK_CHARS.IndexOf(char.ToUpperInvariant(c));

            if (position < 0)
            {
                rank = default;
                return false;
            }

            rank = (Rank)((int)Rank.Two + position);
            return true;
        }

        private static bool TryParseSuit(char c, out Suit suit)
        {
            int position = SUIT_CHARS.IndexOf(char.ToLowerInvariant(c));

            if (position < 0)
            {
                position = SUIT_SYMBOLS.IndexOf(c);
            }

            if (position < 0)
            {
                suit = default;
                return false;
            }

            suit = (Suit)position;
            return true;
        }

        private static string[] BuildNames(bool useSymbols)
        {
            var names = new string[DECK_SIZE];

            for (int i = 0; i < DECK_SIZE; i++)
            {
                Card card = Card.FromIndex(i);
                char rankChar = RANK_CHARS[(int)card.Rank - (int)Rank.Two];
                char suitChar = useSymbols ? SUIT_SYMBOLS[(int)card.Suit] : SUIT_CHARS[(int)card.Suit];
                names[i] = new string(new[] { rankChar, suitChar });
            }

            return names;
        }

        private sealed class RankOnlyComparer : IComparer<Card>
        {
            public int Compare(Card x, Card y) => CompareByRank(x, y);
        }

        private sealed class RankThenSuitComparer_ : IComparer<Card>
        {
            public int Compare(Card x, Card y) => CompareByRankThenSuit(x, y);
        }

        #endregion
    }
}