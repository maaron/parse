
namespace Parse
{
    public partial class Combinators
    {
        public static Parser<T> Not<T, V>(Parser<T, V> parser, T value)
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
