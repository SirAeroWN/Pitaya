# Pitaya
Pitaya enables a strongly typed `Main` method in C# console applications. No more hand rolling a `string[]` parser, dealing with overly verbose builders, or finicky attributes. Pitaya even supports custom types and native AOT compilation!


## Features
Pitaya allows you to declare a strongly typed `Main` method with required and optional parameters, including default values:

```c#
public static void Main(int required, FileInfo? optionalFile, string hasDefault = "My great default")
{
    Console.WriteLine($"required: {required}");
    Console.WriteLine($"optionalFile was {(optionalFile == null ? "not passed" : "passed")}");
    Console.WriteLine($"hasDefault: {hasDefault}");
}
```

Everything just works:
```
$ ./app --required 42
required: 42
optionalFile was not passed
hasDefault: My great default
```

With standard xml method comments, you even get a nice help message:
```
$ ./app --help
Description:
  My sample program

Usage:
  app [options]

Options:
  --required <int>                       A value you must provide
  --optional-file <System.IO.FileInfo?>  An optional file
  --has-default <string>                 A string with a default
  -?, -h, --help                         Show help and usage information
```

Array parameters work as well, even in the middle of a list of options:
```c#
public static void Main(string[] values, string somethingElse)
{
    Console.WriteLine("values: " + string.Join(", ", values));
    Console.WriteLine($"somethingElse: {somethingElse}");
}
```

```
$ ./app --values 1 2 3 --something-else "hello there"
values: 1, 2, 3
somethingElse: hello there
```

You can even use your own types! Types must have either a static `Parse` method returning the parameter type or a constructor that accepts a `string` as the first parameter. Any additional parameters must be optional. Parse methods will be favored over constructors.

Here is an example of a custom negative type:

```c#
class MyNegativeInt
{
    public int Value { get; private set; }

    private MyNegativeInt(int value)
    {
        this.Value = value;
    }

    public static MyNegativeInt Parse(string value)
    {
        int valueAsInt = int.Parse(value);

        if (valueAsInt >= 0)
        {
            throw new ArgumentException($"{valueAsInt} must be negative");
        }

        return new MyNegativeInt(valueAsInt);
    }

    public override string ToString()
    {
        return $"MyNegativeInt has a value of {this.Value}";
    }
}
```

Our main method can simply use our custom type like any other:

```c#
/// <summary>
/// My sample program
/// </summary>
/// <param name="negative">A negative integer</param>
public static void Main(MyNegativeInt negative)
{
    Console.WriteLine(negative);
}
```

Running the program produces the expected result:

```
$ ./app --negative -1
MyNegativeInt has a value of -1
```

The help specifies the type:

```
$ ./app --help
Description:
  My sample program

Usage:
  app [options]

Options:
  --negative <ConsoleApp8.MyNegativeInt>  A negative integer
  -?, -h, --help                          Show help and usage information
```