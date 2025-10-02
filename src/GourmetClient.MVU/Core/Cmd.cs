namespace GourmetClient.MVU.Core;

public abstract record Cmd<TMsg> {
  public record None : Cmd<TMsg>;
  public record OfFunc(Func<Task<TMsg>> AsyncFunc) : Cmd<TMsg>;
  public record OfTask(Task<TMsg> Task) : Cmd<TMsg>;
  public record Batch(IReadOnlyList<Cmd<TMsg>> Commands) : Cmd<TMsg>;
}

public static class Cmd {
  public static Cmd<TMsg> None<TMsg>() => new Cmd<TMsg>.None();

  public static Cmd<TMsg> OfFunc<TMsg>(Func<Task<TMsg>> asyncFunc) =>
    new Cmd<TMsg>.OfFunc(asyncFunc);

  public static Cmd<TMsg> OfTask<TMsg>(Task<TMsg> task) =>
    new Cmd<TMsg>.OfTask(task);

  public static Cmd<TMsg> OfTask<TMsg>(Func<Task<TMsg>> taskFactory) =>
    new Cmd<TMsg>.OfFunc(taskFactory);

  public static Cmd<TMsg> Batch<TMsg>(params Cmd<TMsg>[] commands) =>
    new Cmd<TMsg>.Batch(commands.Where(c => c is not Cmd<TMsg>.None).ToList());

  public static Cmd<TMsg> Batch<TMsg>(IEnumerable<Cmd<TMsg>> commands) =>
    new Cmd<TMsg>.Batch(commands.Where(c => c is not Cmd<TMsg>.None).ToList());
}