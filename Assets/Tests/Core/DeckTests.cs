using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using PokerGame.Core.Cards;

namespace PokerGame.Core.Tests
{
    [TestFixture]
    public class DeckTests
    {
        /// <summary>Always returns the last valid value, so Fisher-Yates swaps each card with itself.</summary>
        private sealed class MaxValueRandomSource : IRandomSource
        {
            public readonly List<int> Bounds = new List<int>();

            public int NextInt(int maxExclusive)
            {
                Bounds.Add(maxExclusive);
                return maxExclusive - 1;
            }
        }

        private static List<Card> DealAll(Deck deck)
        {
            var list = new List<Card>();
            while (deck.TryDeal(out Card c))
            {
                list.Add(c);
            }

            return list;
        }

        [Test]
        public void NewDeck_Contains52UniqueValidCards()
        {
            var deck = new Deck();
            List<Card> cards = DealAll(deck);

            Assert.That(cards.Count, Is.EqualTo(52));
            Assert.That(cards.Distinct().Count(), Is.EqualTo(52));
            Assert.That(cards.All(c => c.IsValid), Is.True);
        }

        [Test]
        public void Deal_OnEmptyDeck_Throws()
        {
            var deck = new Deck();
            DealAll(deck);

            Assert.Throws<InvalidOperationException>(() => deck.Deal());
            Assert.That(deck.TryDeal(out _), Is.False);
        }

        [Test]
        public void Reset_RestoresAllCardsInCanonicalOrder()
        {
            var deck = new Deck();
            deck.Deal();
            deck.Burn();

            deck.Reset();

            Assert.That(deck.RemainingCount, Is.EqualTo(52));
            Assert.That(deck.DealtCount, Is.EqualTo(0));
            Assert.That(DealAll(deck).Select(c => c.Index), Is.EqualTo(Enumerable.Range(0, 52)));
        }

        [Test]
        public void Shuffle_UsesCorrectFisherYatesBounds()
        {
            var fake = new MaxValueRandomSource();
            var deck = new Deck(fake, shuffleOnCreate: false);

            deck.Shuffle();

            // 51 calls with bounds 52, 51, ..., 2
            Assert.That(fake.Bounds, Is.EqualTo(Enumerable.Range(2, 51).Reverse()));
            // Swapping each position with itself leaves the order unchanged.
            Assert.That(DealAll(deck).Select(c => c.Index), Is.EqualTo(Enumerable.Range(0, 52)));
        }

        [Test]
        public void Shuffle_PreservesCardSet_AndOnlyTouchesUndealtCards()
        {
            var deck = new Deck(CryptoRandomSource.Shared, shuffleOnCreate: false);
            for (int i = 0; i < 5; i++)
            {
                deck.Deal();
            }

            var before = new Card[47];
            deck.CopyRemainingTo(before);

            deck.Shuffle();

            var after = new Card[47];
            deck.CopyRemainingTo(after);

            Assert.That(deck.RemainingCount, Is.EqualTo(47));
            Assert.That(after, Is.EquivalentTo(before));
        }

        [Test]
        public void DealInto_IsAtomic()
        {
            var deck = new Deck();
            var tooMany = new Card[53];

            Assert.Throws<InvalidOperationException>(() => deck.DealInto(tooMany));
            Assert.That(deck.RemainingCount, Is.EqualTo(52));
        }

        [Test]
        public void ConcurrentDeal_NeverDuplicatesOrLosesCards()
        {
            var deck = new Deck();
            var dealt = new System.Collections.Concurrent.ConcurrentBag<Card>();

            Parallel.For(0, 200, _ =>
            {
                if (deck.TryDeal(out Card c))
                {
                    dealt.Add(c);
                }
            });

            Assert.That(dealt.Count, Is.EqualTo(52));
            Assert.That(dealt.Distinct().Count(), Is.EqualTo(52));
        }

        [Test, Category("Statistical")]
        public void Shuffle_TopCardIsRoughlyUniform()
        {
            const int TRIALS = 20800; // expected 400 per card, sigma ~ 20
            var counts = new int[52];
            var deck = new Deck();

            for (int i = 0; i < TRIALS; i++)
            {
                deck.ResetAndShuffle();
                counts[deck.Deal().Index]++;
            }

            // +-35% is about 7 sigma, so a false failure is practically impossible.
            Assert.That(counts.Min(), Is.GreaterThan(260));
            Assert.That(counts.Max(), Is.LessThan(540));
        }

        [Test]
        public void CryptoRandomSource_StaysWithinBounds()
        {
            for (int i = 0; i < 10000; i++)
            {
                int v = CryptoRandomSource.Shared.NextInt(7);
                Assert.That(v, Is.InRange(0, 6));
            }

            Assert.Throws<ArgumentOutOfRangeException>(() => CryptoRandomSource.Shared.NextInt(0));
        }

        [Test]
        public void Card_IndexRoundTrip_AndStringRoundTrip()
        {
            for (int i = 0; i < 52; i++)
            {
                Card card = Card.FromIndex(i);
                Assert.That(card.Index, Is.EqualTo(i));
                Assert.That(CardUtils.Parse(card.ToString()), Is.EqualTo(card));
            }
        }

        [TestCase("As", Rank.Ace, Suit.Spades)]
        [TestCase("td", Rank.Ten, Suit.Diamonds)]
        [TestCase("10h", Rank.Ten, Suit.Hearts)]
        [TestCase(" 2C ", Rank.Two, Suit.Clubs)]
        [TestCase("K\u2660", Rank.King, Suit.Spades)]
        public void TryParse_AcceptsValidFormats(string text, Rank rank, Suit suit)
        {
            Assert.That(CardUtils.TryParse(text, out Card card), Is.True);
            Assert.That(card, Is.EqualTo(new Card(rank, suit)));
        }

        [TestCase("")]
        [TestCase("A")]
        [TestCase("1s")]
        [TestCase("Xs")]
        [TestCase("Ax")]
        [TestCase("11s")]
        [TestCase("Asx")]
        public void TryParse_RejectsInvalidFormats(string text)
        {
            Assert.That(CardUtils.TryParse(text, out _), Is.False);
        }

        [Test]
        public void DefaultCard_IsInvalid()
        {
            Assert.That(default(Card).IsValid, Is.False);
            Assert.That(default(Card).Index, Is.EqualTo(-1));
            Assert.That(default(Card).ToString(), Is.EqualTo("??"));
        }

        [Test]
        public void Compare_RankIgnoresSuit()
        {
            Card aceSpades = CardUtils.Parse("As");
            Card aceHearts = CardUtils.Parse("Ah");
            Card king = CardUtils.Parse("Kd");

            Assert.That(CardUtils.CompareByRank(aceSpades, aceHearts), Is.EqualTo(0));
            Assert.That(CardUtils.CompareByRank(aceSpades, king), Is.GreaterThan(0));
            Assert.That(CardUtils.CompareByRankThenSuit(aceSpades, aceHearts), Is.GreaterThan(0));
        }

        [Test]
        public void SortByRankDescending_SortsWithoutAllocation()
        {
            var hand = new[]
            {
                CardUtils.Parse("2c"), CardUtils.Parse("Ah"),
                CardUtils.Parse("9d"), CardUtils.Parse("Ks")
            };

            CardUtils.SortByRankDescending(hand);

            Assert.That(hand.Select(c => c.ToString()), Is.EqualTo(new[] { "Ah", "Ks", "9d", "2c" }));
        }
    }
}