﻿using System.Collections.Generic;
using System.Text;
using SabberStoneCore.Enchants;
using SabberStoneCore.Enums;

namespace SabberStoneCore.Model.Entities
{
	/// <summary>
	/// The entity representing the player.
	/// </summary>
	/// <seealso cref="Character{Hero}" />
	/// <autogeneratedoc />
	public partial class Hero : Character
	{
		/// <summary>Gets or sets the hero power entity.</summary>
		/// <value><see cref="Entities.HeroPower"/></value>
		public HeroPower HeroPower { get; set; }

		/// <summary>Gets or sets the weapon entity equipped on the Hero.</summary>
		/// <value><see cref="Entities.Weapon"/></value>
		public Weapon Weapon { get; set; }

		public List<Aura> Auras { get; }

		/// <summary>Initializes a new instance of the <see cref="Hero"/> class.</summary>
		/// <param name="controller">Owner of the character; not specifically limited to players.</param>
		/// <param name="card">The card which this character embodies.</param>
		/// <param name="tags">Properties of this entity.</param>
		/// <autogeneratedoc />
		public Hero(Controller controller, Card card, IDictionary<GameTag, int> tags)
			: base(controller, card, tags)
		{
			Auras = new List<Aura>();
			Game.Log(LogLevel.VERBOSE, BlockType.PLAY, "Hero", !Game.Logging? "":$"{card.Name} ({card.Class}) was created.");
		}

		/// <summary>
		/// A copy constructor.
		/// </summary>
		/// <param name="controller">The target <see cref="T:SabberStoneCore.Model.Entities.Controller" /> instance.</param>
		/// <param name="hero">The source <see cref="T:SabberStoneCore.Model.Entities.Hero" />.</param>
		private Hero(Controller controller, Hero hero) : base(controller, hero)
		{
			Auras = new List<Aura>(hero.Auras.Count);
			IsDamagedThisTurn = hero.IsDamagedThisTurn;
		}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		public override int AttackDamage => base.AttackDamage + (Game.CurrentPlayer != Controller ? 0 : Weapon?.AttackDamage ?? 0);

		public int TotalAttackDamage => AttackDamage/* + (Weapon?.AttackDamage ?? 0)*/;

		public override bool CanAttack => TotalAttackDamage > 0 && base.CanAttack;

		public override bool HasWindfury => Weapon != null && Weapon.HasWindfury;

		public override bool HasLifeSteal => Weapon?.HasLifeSteal ?? false;

		public void AddWeapon(Weapon weapon)
		{
			RemoveWeapon();
			weapon.OrderOfPlay = Game.NextOop;
			Weapon = weapon;
			Weapon[GameTag.ZONE] = (int)Enums.Zone.PLAY;
			Weapon[GameTag.ZONE_POSITION] = 0;
			EquippedWeapon = weapon.Id;
			if (weapon.HasWindfury && IsExhausted && NumAttacksThisTurn == 1)
				IsExhausted = false;

			Game.TriggerManager.OnEquipWeaponTrigger(weapon);
		}

		/// <summary>
		/// Removing a weapon to the graveyard. Triggering deathrattle events on the weapon.
		/// </summary>
		public void RemoveWeapon()
		{
			if (Weapon == null)
				return;

			if (Weapon.HasDeathrattle)
				Weapon.ActivateTask(PowerActivation.DEATHRATTLE);

			Game.TriggerManager.OnDeathTrigger(Weapon);

			Game.Log(LogLevel.INFO, BlockType.PLAY, "Hero", !Game.Logging? "":$"Butcher's knife incoming to graveyard, say 'gugus' to {Weapon}");
			Controller.GraveyardZone.Add(Weapon);

			ClearWeapon();
		}

		/// <summary>
		/// Clears weapon information on Hero.
		/// </summary>
		public void ClearWeapon()
		{
			Weapon.ActivatedTrigger?.Remove();
			Weapon.OngoingEffect?.Remove();
			if (Weapon.AppliedEnchantments != null && Weapon[GameTag.KEEP_ENCHANTMENTS] != 1)
				for (int i = Weapon.AppliedEnchantments.Count - 1; i >= 0; i--)
					Weapon.AppliedEnchantments[i].Remove();
			Weapon = null;
			EquippedWeapon = 0;
		}

		/// <inheritdoc cref="Playable{T}.Clone(Controller)" />
		public override IPlayable Clone(Controller controller)
		{
			return new Hero(controller, this);
		}

		public string FullPrint()
		{
			var str = new StringBuilder();
			string mStr = Weapon != null ? $"[{Weapon.Card.Name}[{Weapon.AttackDamage}/{Weapon.Durability}]]" : "[NO WEAPON]";
			str.Append($"[HERO][{this}][ATK{AttackDamage}/AR{Armor}/HP{Health}][{mStr}][SP{Controller.CurrentSpellPower}]");
			//str.Append($"[ENCH {OldEnchants.Count}]");
			//str.Append($"[TRIG {Triggers.Count}]");
			return str.ToString();
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}

	public partial class Hero
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		//public int SpellPowerDamage => this[GameTag.CURRENT_SPELLPOWER];

		public int EquippedWeapon
		{
			get { return this[GameTag.WEAPON]; }
			set { this[GameTag.WEAPON] = value; }
		}

		public int HeroPowerDamage
		{
			get { return this[GameTag.HEROPOWER_DAMAGE]; }
			set { this[GameTag.HEROPOWER_DAMAGE] = value; }
		}

		public int Fatigue
		{
			get { return this[GameTag.FATIGUE]; }
			set { this[GameTag.FATIGUE] = value; }
		}

		public bool IsDamagedThisTurn { get; set; }

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}
