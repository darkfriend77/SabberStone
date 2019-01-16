﻿using System.Collections.Generic;
using SabberStoneCore.Actions;
using SabberStoneCore.Enchants;
using SabberStoneCore.Enums;
using SabberStoneCore.Model;
using SabberStoneCore.Model.Entities;

namespace SabberStoneCore.Tasks.SimpleTasks
{
	public class RitualTask : SimpleTask
	{
		public enum RitualType
		{
			Check,
			Buff,
			Taunt,
			Blade
		}

		private static readonly Card BuffEnchantmentCard = Cards.FromId("OG_281e");
		private static readonly Card BladeofCThunEnchantmentCard = Cards.FromId("OG_282e");
		private static readonly Card TauntEnchantmentCard = Cards.FromId("OG_284e");
		private readonly int _amount;

		private readonly RitualType _type;

		public RitualTask(RitualType type = RitualType.Check)
		{
			_type = type;
		}

		public RitualTask(int amount)
		{
			_type = RitualType.Buff;
			_amount = amount;
		}

		public override TaskState Process(in Game game, in Controller controller, in IEntity source, in IEntity target,
			in TaskStack stack = null)
		{
			//[irc] Patashu @darkfriend77 yeah, that's the general idea. 
			// there's two kinds of triggers, one when a c'thun is summoned or 
			// generated in hand or moved to hand that copies the ritual buffs 
			// on the proxy to that c'thun, one when a new c'thun buff is made 
			// (or maybe it's a card effect not a trigger? or even an aura style 
			// effect ??) that copies the additional effect to c'thuns in your 
			// hand and board that aren't silenced

			IPlayable proxyCthun;
			if (!controller.SeenCthun)
			{
				proxyCthun = Entity.FromCard(in controller, Cards.FromId("OG_279"));
				proxyCthun[GameTag.REVEALED] = 1;
				controller.SetasideZone.Add(proxyCthun);
				controller.ProxyCthun = proxyCthun.Id;
				controller.SeenCthun = true;
			}
			else
			{
				proxyCthun = game.IdEntityDic[controller.ProxyCthun];
			}

			var entities = new List<IPlayable> {proxyCthun};
			entities.AddRange(controller.BoardZone.GetAll(p => p.Card.Id.Equals("OG_280")));
			entities.AddRange(controller.HandZone.GetAll(p => p.Card.Id.Equals("OG_280")));

			switch (_type)
			{
				case RitualType.Buff:
					if (proxyCthun.OngoingEffect == null)
					{
						foreach (IPlayable p in entities)
						{
							Generic.AddEnchantmentBlock.Invoke(controller, BuffEnchantmentCard, (IPlayable) source, p,
								0, 0, false);

							((OngoingEnchant) p.OngoingEffect).Count += _amount - 1;
						}

						break;
					}

					foreach (IPlayable p in entities)
						if (p.OngoingEffect == null)
						{
							Generic.AddEnchantmentBlock.Invoke(controller, BuffEnchantmentCard, (IPlayable) source, p,
								0, 0, false);
							((OngoingEnchant) p.OngoingEffect).Count += _amount - 1;
						}
						else
						{
							((OngoingEnchant) p.OngoingEffect).Count += _amount;
						}

					break;

				case RitualType.Taunt:
					if (proxyCthun[GameTag.TAUNT] == 1) break;
					foreach (IPlayable p in entities)
						Generic.AddEnchantmentBlock.Invoke(controller, TauntEnchantmentCard, (IPlayable) source, p, 0,
							0, false);

					break;

				case RitualType.Blade:
					foreach (IPlayable p in entities)
						Generic.AddEnchantmentBlock.Invoke(controller, BladeofCThunEnchantmentCard, (IPlayable) source,
							p, stack.Number,
							stack.Number1, false);

					break;
			}

			return TaskState.COMPLETE;
		}
	}
}
