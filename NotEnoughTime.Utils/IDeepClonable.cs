
namespace NotEnoughTime.Utils
{
    public interface IDeepClonable<out T>
    {
        T Clone();
    }
}
