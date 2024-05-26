# Pitaya
Pitaya enables a strongly typed `Main` method in C# console applications. No more hand rolling a `string[]` parser, dealing with overly verbose builders, or finicky attributes. Pitaya allows you to declare a strongly typed `Main` method with required and optional parameters, including default values:

```c#
public static void Main(int required, FileInfo? optionalFile, string hasADefault = "My great default")
{
    Console.WriteLine($"required: {required}");
    Console.WriteLine($"optionalFile was {(optionalFile == null ? "not passed" : "passed")}");
    Console.WriteLine($"hasADefault: {hasADefault}");
}
```

Everything just works:
```
$ ./app --required 42

```