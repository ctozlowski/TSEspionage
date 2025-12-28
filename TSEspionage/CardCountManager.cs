/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;

namespace TSEspionage
{
    /**
     * Counts of cards in each hand/pile.
     */
    public class CardCounts
    {
        public readonly TwilightLibWrapper.Players Players;
        public readonly EPlayer ChinaCardHolder;
        public readonly bool ChinaCardFaceUp;
        public readonly ushort UsaHandCount;
        public readonly ushort UssrHandCount;
        public readonly ushort DrawPileCount;
        public readonly ushort DiscardPileCount;
        public readonly ushort RemovedPileCount;

        private CardCounts(TwilightLibWrapper.Players players, EPlayer chinaCardHolder, bool chinaCardFaceUp, ushort usaHandCount,
            ushort ussrHandCount, ushort drawPileCount, ushort discardPileCount, ushort removedPileCount)
        {
            Players = players;
            ChinaCardHolder = chinaCardHolder;
            ChinaCardFaceUp = chinaCardFaceUp;
            UsaHandCount = usaHandCount;
            UssrHandCount = ussrHandCount;
            DrawPileCount = drawPileCount;
            DiscardPileCount = discardPileCount;
            RemovedPileCount = removedPileCount;
        }

        public class Builder
        {
            public TwilightLibWrapper.Players Players;
            public EPlayer ChinaCardHolder = EPlayer.NONE;
            public bool ChinaCardFaceUp;
            public ushort UsaHandCount;
            public ushort UssrHandCount;
            public ushort DrawPileCount;
            public ushort DiscardPileCount;
            public ushort RemovedPileCount;

            public CardCounts Build()
            {
                return new CardCounts(Players, ChinaCardHolder, ChinaCardFaceUp, UsaHandCount, UssrHandCount,
                    DrawPileCount, DiscardPileCount, RemovedPileCount);
            }
        }
    }

    /**
     * Listens for events that might have changed the counts of cards in hands or piles. Recalculates the counts and
     * sends them to listeners.
     */
    public class CardCountManager : MonoBehaviour
    {
        // IL2CPP requires this constructor for injected types
        public CardCountManager(IntPtr ptr) : base(ptr) { }

        // Store callbacks manually since IL2CPP UnityEvent<T> has limitations with generic types
        private readonly List<Action<CardCounts>> _listeners = new List<Action<CardCounts>>();

        [HideFromIl2Cpp]
        public void UpdateCardCounts()
        {
            var players = TwilightLibWrapper.GetPlayers();
            if (players.LocalSuperpower == EPlayer.NONE || players.OpposingSuperpower == EPlayer.NONE)
            {
                // Players have not yet been assigned sides
                return;
            }

            var usaHandState = TwilightLibWrapper.GetHand(players.GetUsaPlayerId());
            var ussrHandState = TwilightLibWrapper.GetHand(players.GetUssrPlayerId());

            var cardCounts = new CardCounts.Builder
            {
                Players = players,
                UsaHandCount = usaHandState.handCardCount,
                UssrHandCount = ussrHandState.handCardCount,
            };

            if (usaHandState.chinaCardFaceUp > 0)
            {
                cardCounts.ChinaCardHolder = EPlayer.US;
                cardCounts.ChinaCardFaceUp = true;
            }
            else if (usaHandState.chinaCardFaceDown > 0)
            {
                cardCounts.ChinaCardHolder = EPlayer.US;
                cardCounts.ChinaCardFaceUp = false;
            }
            else if (ussrHandState.chinaCardFaceUp > 0)
            {
                cardCounts.ChinaCardHolder = EPlayer.USSR;
                cardCounts.ChinaCardFaceUp = true;
            }
            else if (ussrHandState.chinaCardFaceDown > 0)
            {
                cardCounts.ChinaCardHolder = EPlayer.USSR;
                cardCounts.ChinaCardFaceUp = false;
            }

            var gameDeckCounts = TwilightLibWrapper.GetDeckCounts();

            cardCounts.DrawPileCount = (ushort)gameDeckCounts.draw_pile_count;
            cardCounts.DiscardPileCount = (ushort)gameDeckCounts.discard_pile_count;
            cardCounts.RemovedPileCount = (ushort)gameDeckCounts.removed_pile_count;

            // Invoke all registered listeners
            var counts = cardCounts.Build();
            foreach (var listener in _listeners)
            {
                listener?.Invoke(counts);
            }
        }

        [HideFromIl2Cpp]
        public void AddListener(Action<CardCounts> callback)
        {
            if (callback != null && !_listeners.Contains(callback))
            {
                _listeners.Add(callback);
            }
        }

        [HideFromIl2Cpp]
        public void RemoveListener(Action<CardCounts> callback)
        {
            _listeners.Remove(callback);
        }
    }
}