
namespace Parse
{
    public partial class Combinators
    {
        public static Parser<T> Not<T, V>(Parser<T, V> parser)
        {
            return (input) =>
            {
                return parser(input).Visit(
                    (success) => Result.Fail<T>(success.Remaining),
                    (failure) => Result.Match<T>(input));
            };
        }

        public static Parser<T> Not<T>(Parser<T> parser)
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
