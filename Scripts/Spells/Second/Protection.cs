using System;
using System.Collections;
using Server.Targeting;
using Server.Network;

namespace Server.Spells.Second
{
	public class ProtectionSpell : MagerySpell
	{
		private static Hashtable m_Registry = new Hashtable();
		public static Hashtable Registry { get { return m_Registry; } }

		private static SpellInfo m_Info = new SpellInfo(
				"Protection", "Uus Sanct",
				236,
				9011,
				Reagent.Garlic,
				Reagent.Ginseng,
				Reagent.SulfurousAsh
			);

		public override SpellCircle Circle { get { return SpellCircle.Second; } }

		public ProtectionSpell( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}
		
		public override void OnCast()
		{
			Caster.Target = new InternalTarget( this );
		}

		// public override bool CheckCast()
		// {
			// if ( Core.AOS )
				// return true;
// 
			// if ( m_Registry.ContainsKey( Caster ) )
			// {
				// Caster.SendLocalizedMessage( 1005559 ); // This spell is already in effect.
				// return false;
			// }
			// /*else if ( !Caster.CanBeginAction( typeof( DefensiveSpell ) ) )
			// {
				// Caster.SendLocalizedMessage( 1005385 ); // The spell will not adhere to you at this time.
				// return false;
			// }*/
// 
			// return true;
		// }

		private static Hashtable m_Table = new Hashtable();

		public static void Toggle( Mobile caster, Mobile target )
		{
			/* Players under the protection spell effect can no longer have their spells "disrupted" when hit.
			 * Players under the protection spell have decreased physical resistance stat value,
			 * a decreased "resisting spells" skill value by -35,
			 * and a slower casting speed modifier (technically, a negative "faster cast speed") of 2 points.
			 * The protection spell has an indefinite duration, becoming active when cast, and deactivated when re-cast.
			 * Reactive Armor, Protection, and Magic Reflection will stay on�even after logging out,
			 * even after dying�until you �turn them off� by casting them again.
			 */

			object[] mods = (object[])m_Table[target];

			if ( mods == null )
			{
				target.PlaySound( 0x1E9 );
				target.FixedParticles( 0x375A, 9, 20, 5016, EffectLayer.Waist );

				mods = new object[2]
					{
						new ResistanceMod( ResistanceType.Physical, -15 + Math.Min( (int)(caster.Skills[SkillName.Inscribe].Value / 20), 15 ) ),
						new DefaultSkillMod( SkillName.MagicResist, true, -35 + Math.Min( (int)(caster.Skills[SkillName.Inscribe].Value / 20), 35 ) )
					};

				m_Table[target] = mods;
				Registry[target] = 100.0;

				target.AddResistanceMod( (ResistanceMod)mods[0] );
				target.AddSkillMod( (SkillMod)mods[1] );
			}
			else
			{
				target.PlaySound( 0x1ED );
				target.FixedParticles( 0x375A, 9, 20, 5016, EffectLayer.Waist );

				m_Table.Remove( target );
				Registry.Remove( target );

				target.RemoveResistanceMod( (ResistanceMod)mods[0] );
				target.RemoveSkillMod( (SkillMod)mods[1] );
			}
		}

		public void Target(Mobile m)
		{
			if ( Core.AOS )
			{
				if ( CheckSequence() )
					Toggle( Caster, Caster );

				FinishSequence();
			}
			else
			{
				if(CheckSequence())
				{
					if ( m.BeginAction( typeof( ProtectionSpell ) ) )
					{
						int val = (int)(Caster.Skills[SkillName.Magery].Value/10.0 + 1);
						Caster.DoBeneficial( m );
						m.VirtualArmorMod += val;
						new InternalTimer( m, Caster, val ).Start();
	
						m.FixedParticles( 0x375A, 9, 20, 5016, EffectLayer.Waist );
						m.PlaySound( 0x1ED );
					} else {
						Caster.SendLocalizedMessage( 1005559 ); // This spell is already in effect.
					}
				}
				// if ( m_Registry.ContainsKey( m ) )
				// {
					// Caster.SendLocalizedMessage( 1005559 ); // This spell is already in effect.
				// }
				// /*else if ( !Caster.CanBeginAction( typeof( DefensiveSpell ) ) )
				// {
					// Caster.SendLocalizedMessage( 1005385 ); // The spell will not adhere to you at this time.
				// }*/
				// else if ( CheckSequence() )
				// {
					// // if ( Caster.BeginAction( typeof( DefensiveSpell ) ) )
					// // {
						// double value = (int)(Caster.Skills[SkillName.EvalInt].Value + Caster.Skills[SkillName.Meditation].Value);
						// value /= 4;
// 
						// if ( value < 0 )
							// value = 0;
						// else if ( value > 75 )
							// value = 75.0;
// 
						// Registry.Add( m, value );
						// new InternalTimer( Caster, m ).Start();
// 
						// Caster.FixedParticles( 0x375A, 9, 20, 5016, EffectLayer.Waist );
						// Caster.PlaySound( 0x1ED );
					// // }
					// // else
					// // {
						// // Caster.SendLocalizedMessage( 1005385 ); // The spell will not adhere to you at this time.
					// // }
				// }

				FinishSequence();
			}
		}
		private class InternalTimer : Timer
		{
			private Mobile m_Owner;
			private int m_Val;

			public InternalTimer( Mobile target, Mobile caster, int val ) : base( TimeSpan.FromSeconds( 0 ) )
			{
				double time = caster.Skills[SkillName.Magery].Value * 1.2;
				if ( time > 144 )
					time = 144;
				Delay = TimeSpan.FromSeconds( time );
				Priority = TimerPriority.OneSecond;

				m_Owner = target;
				m_Val = val;
			}

			protected override void OnTick()
			{
				m_Owner.EndAction( typeof( ProtectionSpell ) );
				m_Owner.VirtualArmorMod -= m_Val;
				if ( m_Owner.VirtualArmorMod < 0 )
					m_Owner.VirtualArmorMod = 0;
			}
		}

		/*private class InternalTimer : Timer
		{
			private Mobile m_Caster;

			public InternalTimer( Mobile caster, Mobile m ) : base( TimeSpan.FromSeconds( 0 ) )
			{
				double val = caster.Skills[SkillName.Magery].Value * 2.0;
				if ( val < 15 )
					val = 15;
				else if ( val > 240 )
					val = 240;

				m_Caster = caster;
				Delay = TimeSpan.FromSeconds( val );
				Priority = TimerPriority.OneSecond;
			}

			protected override void OnTick()
			{
				ProtectionSpell.Registry.Remove( m_Caster );
				DefensiveSpell.Nullify( m_Caster );
			}
		}*/
		
		public class InternalTarget : Target
		{
			private ProtectionSpell m_Owner;

			public InternalTarget( ProtectionSpell owner ) : base( 12, false, TargetFlags.Beneficial )
			{
				m_Owner = owner;
			}

			protected override void OnTarget( Mobile from, object o )
			{
				if ( o is Mobile )
				{
					m_Owner.Target( (Mobile)o );
				}
			}

			protected override void OnTargetFinish( Mobile from )
			{
				m_Owner.FinishSequence();
			}
		}
	}
}
