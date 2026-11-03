using System;

namespace PokerGame.Core.Cards
{
    /// <summary>
    /// An immutable playing card. Implemented as a readonly struct (2 bytes of data),
    /// so arrays of cards are contiguous in memory and never allocate per-card objects.
    /// <para>
    /// <c>default(Card)</c> is an invalid card (rank 0); check <see cref="IsValid"/>
    /// for data coming from untrusted sources such as the network.
    /// </para>
    /// </summary>
    public readonly struct Card : IEquatable<Card>, IComparable<Card>
    {
        /// <summary>The rank of the card.</summary>
        public Rank Rank { get; }

        /// <summary>The suit of the card.</summary>
        public Suit Suit { get; }

        /// <summary>
        /// Creates a card.
        /// </summary>
        /// <param name="rank">A defined <see cref="Rank"/> value.</param>
        /// <param name="suit">A defined <see cref="Suit"/> value.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="rank"/> or <paramref name="suit"/> is not a defined value.
        /// </exception>
        public Card(Rank rank, Suit suit)
        {
            if (!CardUtils.IsValid(rank))
            {
                throw new ArgumentOutOfRangeException(nameof(rank), rank, "Invalid rank.");
            }

            if (!CardUtils.IsValid(suit))
            {
                throw new ArgumentOutOfRangeException(nameof(suit), suit, "Invalid suit.");
            }

            Rank = rank;
            Suit = suit;
        }

        /// <summary>
        /// True when both rank and suit are defined values.
        /// False for <c>default(Card)</c> or cards built from corrupted data.
        /// </summary>
        public bool IsValid => CardUtils.IsValid(Rank) && CardUtils.IsValid(Suit);

        /// <summary>
        /// A unique index in [0, 51] (suit-major, rank-minor), or -1 if the card is invalid.
        /// Handy for lookup tables, bitmasks and compact network serialization (1 byte).
        /// </summary>
        public int Index => IsValid
            ? ((int)Suit * CardUtils.RANKS_PER_SUIT) + ((int)Rank - (int)Rank.Two)
            : -1;

        /// <summary>
        /// Creates the card for a given index in [0, 51].
        /// </summary>
        /// <param name="index">Index as produced by <see cref="Index"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">Index is outside [0, 51].</exception>
        public static Card FromIndex(int index)
        {
            if (!TryFromIndex(index, out Card card))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index), index, "Card index must be in [0, 51].");
            }

            return card;
        }

        /// <summary>
        /// Attempts to create the card for a given index. Never throws.
        /// Use this for data received from the network.
        /// </summary>
        /// <param name="index">Candidate index.</param>
        /// <param name="card">The resulting card, or <c>default</c> on failure.</param>
        /// <returns>True if <paramref name="index"/> is within [0, 51].</returns>
        public static bool TryFromIndex(int index, out Card card)
        {
            if (index < 0 || index >= CardUtils.DECK_SIZE)
            {
                card = default;
                return false;
            }

            var suit = (Suit)(index / CardUtils.RANKS_PER_SUIT);
            var rank = (Rank)((int)Rank.Two + (index % CardUtils.RANKS_PER_SUIT));
            card = new Card(rank, suit);
            return true;
        }

        /// <inheritdoc />
        public bool Equals(Card other) => Rank == other.Rank && Suit == other.Suit;

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is Card other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => ((int)Rank << 2) | (int)Suit;

        /// <summary>
        /// Orders by rank, then by suit. Note: suit order has no meaning in poker
        /// hand strength. Use <see cref="CardUtils.CompareByRank"/> for that.
        /// </summary>
        public int CompareTo(Card other) => CardUtils.CompareByRankThenSuit(this, other);

        /// <summary>Returns the short notation, e.g. "As", "Td", "2c" ("??" if invalid).</summary>
        public override string ToString() => CardUtils.ToString(this);

        /// <summary>Equality operator.</summary>
        public static bool operator ==(Card left, Card right) => left.Equals(right);

        /// <summary>Inequality operator.</summary>
        public static bool operator !=(Card left, Card right) => !left.Equals(right);
    }
}