using System;

namespace PokerGame.Core.Cards
{
    /// <summary>
    /// A standard 52-card deck with a cryptographically secure Fisher-Yates shuffle.
    /// <para>
    /// <b>Thread safety:</b> every public member is guarded by a single lock, so one instance
    /// can be shared across threads. Multi-card operations (<see cref="DealInto"/>,
    /// <see cref="ResetAndShuffle"/>) are atomic.
    /// </para>
    /// <para>
    /// <b>Memory:</b> one 52-element array is allocated at construction; Deal, Shuffle
    /// and Reset allocate nothing.
    /// </para>
    /// </summary>
    public sealed class Deck : IDeck
    {
        private readonly object _syncRoot = new object();
        private readonly Card[] _cards = new Card[CardUtils.DECK_SIZE];
        private readonly IRandomSource _random;
        private int _nextIndex;

        /// <summary>
        /// Creates a shuffled deck using the shared <see cref="CryptoRandomSource"/>.
        /// </summary>
        public Deck() : this(CryptoRandomSource.Shared, true)
        {
        }

        /// <summary>
        /// Creates a deck with a custom random source.
        /// </summary>
        /// <param name="random">Random source used for shuffling (inject a fake in tests).</param>
        /// <param name="shuffleOnCreate">If true, the deck is shuffled immediately.</param>
        /// <exception cref="ArgumentNullException"><paramref name="random"/> is null.</exception>
        public Deck(IRandomSource random, bool shuffleOnCreate = true)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
            FillInOrder();

            if (shuffleOnCreate)
            {
                ShuffleRemaining();
            }
        }

        /// <inheritdoc />
        public int RemainingCount
        {
            get
            {
                lock (_syncRoot)
                {
                    return CardUtils.DECK_SIZE - _nextIndex;
                }
            }
        }

        /// <inheritdoc />
        public int DealtCount
        {
            get
            {
                lock (_syncRoot)
                {
                    return _nextIndex;
                }
            }
        }

        /// <inheritdoc />
        public Card Deal()
        {
            lock (_syncRoot)
            {
                if (_nextIndex >= CardUtils.DECK_SIZE)
                {
                    throw new InvalidOperationException("The deck is empty.");
                }

                return _cards[_nextIndex++];
            }
        }

        /// <inheritdoc />
        public bool TryDeal(out Card card)
        {
            lock (_syncRoot)
            {
                if (_nextIndex >= CardUtils.DECK_SIZE)
                {
                    card = default;
                    return false;
                }

                card = _cards[_nextIndex++];
                return true;
            }
        }

        /// <inheritdoc />
        public void DealInto(Span<Card> destination)
        {
            lock (_syncRoot)
            {
                int remaining = CardUtils.DECK_SIZE - _nextIndex;

                if (destination.Length > remaining)
                {
                    throw new InvalidOperationException(
                        $"Cannot deal {destination.Length} cards; only {remaining} remain.");
                }

                new ReadOnlySpan<Card>(_cards, _nextIndex, destination.Length).CopyTo(destination);
                _nextIndex += destination.Length;
            }
        }

        /// <inheritdoc />
        public void Burn()
        {
            lock (_syncRoot)
            {
                if (_nextIndex >= CardUtils.DECK_SIZE)
                {
                    throw new InvalidOperationException("The deck is empty.");
                }

                _nextIndex++;
            }
        }

        /// <summary>
        /// Shuffles the cards that have not been dealt yet; dealt cards are not touched.
        /// Call <see cref="Reset"/> first (or use <see cref="ResetAndShuffle"/>)
        /// to shuffle all 52 cards.
        /// </summary>
        public void Shuffle()
        {
            lock (_syncRoot)
            {
                ShuffleRemaining();
            }
        }

        /// <inheritdoc />
        public void Reset()
        {
            lock (_syncRoot)
            {
                FillInOrder();
            }
        }

        /// <inheritdoc />
        public void ResetAndShuffle()
        {
            lock (_syncRoot)
            {
                FillInOrder();
                ShuffleRemaining();
            }
        }

        /// <summary>
        /// Copies the undealt cards, in dealing order, into <paramref name="destination"/>.
        /// <b>Server-side / test use only</b>: exposing this to clients reveals the deck.
        /// </summary>
        /// <param name="destination">Must hold at least <see cref="RemainingCount"/> cards.</param>
        /// <returns>The number of cards copied.</returns>
        /// <exception cref="ArgumentException">Destination is too small.</exception>
        public int CopyRemainingTo(Span<Card> destination)
        {
            lock (_syncRoot)
            {
                int remaining = CardUtils.DECK_SIZE - _nextIndex;

                if (destination.Length < remaining)
                {
                    throw new ArgumentException("Destination is too small.", nameof(destination));
                }

                new ReadOnlySpan<Card>(_cards, _nextIndex, remaining).CopyTo(destination);
                return remaining;
            }
        }

        /// <summary>Restores canonical order and returns every card to the deck. Caller must hold the lock.</summary>
        private void FillInOrder()
        {
            for (int i = 0; i < CardUtils.DECK_SIZE; i++)
            {
                _cards[i] = Card.FromIndex(i);
            }

            _nextIndex = 0;
        }

        /// <summary>
        /// Fisher-Yates (Durstenfeld) over [_nextIndex, 52). Caller must hold the lock.
        /// Each of the n! orderings is produced with equal probability, given an unbiased source.
        /// </summary>
        private void ShuffleRemaining()
        {
            for (int i = CardUtils.DECK_SIZE - 1; i > _nextIndex; i--)
            {
                int bound = i - _nextIndex + 1;
                int offset = _random.NextInt(bound);

                if ((uint)offset >= (uint)bound)
                {
                    throw new InvalidOperationException(
                        $"Random source returned {offset}, outside [0, {bound}).");
                }

                int j = _nextIndex + offset;
                (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
            }
        }
    }
}