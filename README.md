# DisposableComponent

Tools for more convenient use of IDisposable interface.

# Feature

Easy to know if Dispose() has been called!

```c#
var disposable = new SampleDisposable();

// false
Console.WriteLine(disposable.IsDisposed); 

disposable.Dispose();

// true
Console.WriteLine(disposable.IsDisposed); 
```

Notification by event is also possible.

```c#
var disposable = new SampleDisposable();

disposable.Disposing += (s, e) =>
{
    Console.WriteLine("Disposing!");
};

disposable.Disposed += (s, e) =>
{
    Console.WriteLine("Disposed!");
};

disposable.Dispose();
```

Just inherit!  
This adds the definition of the IDisposable interface and the IsDisposed property,   
which can determine if an object has been disposed of.

```c#
public class SampleDisposable : DisposableComponent
{
    private readonly IDisposable _something;

    public SampleDisposable()
    {
        _something = new DummyDisposable();
        
        // If registered to this property, it is destroyed at the same time as the call to Dispose().
        // This is due to DisposableCollection (see below)
        Disposable.Add(_something);
    }

    protected override void OnDisposing()
    {
        // When this object is destroyed (when Dispose() is called), additional processing can be performed.
    }
    
    protected override void OnDisposed()
    {
        // When this object is destroyed (when Dispose() is called), additional processing can be performed.
    }
}
```

A DisposableCollection is also available to dispose of multiple registered IDisposables at once.

```c#
var disposableCollection = new DisposableCollection();

disposableCollection.Add(somethingDisposable1);
disposableCollection.Add(somethingDisposable2);
disposableCollection.Add(somethingDisposable3);

// All at once!
disposableCollection.Dispose();
```

# Installation

```shell
# See NuGet or GitHub for the latest version.
Install-Package DisposableComponent -Version 1.0.21
```

# Licence

"DisposableComponent" is under [MIT license](https://github.com/samunohito/disposable_component/blob/develop/LICENSE.md)
.

# Note

This repository was created with the help of the following third-party tools.

- [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning)
    - Copyright (c) .NET Foundation and Contributors
    - [MIT license](https://github.com/dotnet/Nerdbank.GitVersioning/blob/master/LICENSE)
