
namespace Parse.Combinators
{
    public static partial class Extensions
    {
        public static Parser<T> Not<T, V>(this Parser<T, V> parser)
        {
            return (input) =>
            {
                return parser(input).Visit(
                    (success) => Result.Fail<T>(success.Remaining),
                    (failure) => Result.Match<T>(input));
            };
        }

        public static Parser<T> Not<T>(this Parser<T> parser)
        {
            return (input) =>
            {
                return parser(input).Visit(
                    (success) => Result.Fail<T>(success.Remaining),
                    (failure) => Result.Match<T>(input));
            };
        }
    }
}
