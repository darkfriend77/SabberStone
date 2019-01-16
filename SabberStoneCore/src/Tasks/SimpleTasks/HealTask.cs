﻿using SabberStoneCore.Model;
using SabberStoneCore.Model.Entities;

namespace SabberStoneCore.Tasks.SimpleTasks
{
	public class HealTask : SimpleTask
	{
		public HealTask(int amount, EntityType entityType)
		{
			Amount = amount;
			Type = entityType;
		}

		public int Amount { get; set; }

		public EntityType Type { get; set; }

		public override TaskState Process(in Game game, in Controller controller, in IEntity source, in IEntity target,
			in TaskStack stack = null)
		{
			if (Amount < 1) return TaskState.STOP;

			var playable = source as IPlayable;
			//System.Collections.Generic.List<IPlayable> entities = IncludeTask.GetEntities(Type, in controller, source, target, stack?.Playables);
			//entities.ForEach(p =>
			foreach (IPlayable p in IncludeTask.GetEntities(Type, in controller, playable, target, stack?.Playables))
			{
				var character = p as ICharacter;
				character?.TakeHeal(playable, Amount);
			}
			return TaskState.COMPLETE;
		}
	}
}
