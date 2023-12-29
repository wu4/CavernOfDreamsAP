
using System;
using System.Linq;

namespace CoDArchipelago.WhackableLabels
{
    static class WhackableTypes
    {
        static readonly (Access.Field<Whackable, bool> Field, Func<Whackable, string> GetName)[] whackableTypes = new (Access.Field<Whackable, bool>, Func<Whackable, string>)[] {
            (new("attackWorks"),     whackable => "Tail"),
            (new("diveWorks"),       whackable => "Horn"),
            (new("rollWorks"),       whackable => "Roll"),
            (new("projectileWorks"), whackable => "Bubble"),
            (new("throwWorks"),      whackable => "Throwables"),
            (new("specialWorks"),    whackable => WhackTypeAsString(whackable.specialWhackType))
        };
        
        static string WhackTypeAsString(Whackable.WhackType whackType)
        {
            return whackType switch
            {
                Whackable.WhackType.ATTACK => "Tail",
                Whackable.WhackType.ROLL => "Roll",
                Whackable.WhackType.DIVE => "Horn",
                Whackable.WhackType.PROJECTILE => "Bubble",
                Whackable.WhackType.THROW => "Throwables",

                Whackable.WhackType.SEED => "Apple",
                Whackable.WhackType.TORPEDO => "Bubble Conch",
                Whackable.WhackType.POTION => "Potion",
                Whackable.WhackType.PAINTING_ITEM_PRINCESS => "Lady Opal's Head",
                Whackable.WhackType.PAINTING_ITEM_KAPPA => "Shelnert's Fish",
                Whackable.WhackType.PAINTING_ITEM_MONSTER => "Mr. Kerrington's Wings",
                Whackable.WhackType.PAINTING_ITEM_SAGE => "Sage's Gloves",
                Whackable.WhackType.CUTSCENE => "Cutscene",

                _ => throw new ArgumentException("unknown whack type " + whackType),
            };
        }
        
        public static bool HasValidWhackableMethods(Whackable whackable) =>
            whackableTypes.Any(type => type.Field.Get(whackable));

        public static string ValidWhackableMethods(Whackable whackable) =>
            string.Join(
                "\n",
                whackableTypes
                    .Where(type => type.Field.Get(whackable))
                    .Select(type => type.GetName(whackable))
            );
    }
}