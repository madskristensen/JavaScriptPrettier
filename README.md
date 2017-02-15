# JavaScript Prettier

[![Build status](https://ci.appveyor.com/api/projects/status/t38jbrjn8akd2jla?svg=true)](https://ci.appveyor.com/project/madskristensen/javascriptprettier)

Download this extension from the [Marketplace](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.JavaScriptPrettier)
or get the [CI build](http://vsixgallery.com/extension/J1da7ad9e-85b3-4a0c-8e45-b2ae59a575a7/).

---------------------------------------

Prettier is an opinionated JavaScript formatter inspired by refmt with advanced support for language features from ES2017, JSX, and Flow. It removes all original styling and ensures that all outputted JavaScript conforms to a consistent style.

See the [change log](CHANGELOG.md) for changes and road map.

## Features

- Prettifies JavaScript
- Uses [prettier](https://github.com/jlongster/prettier) node module

### Prettify
This extension calls the [prettier](https://github.com/jlongster/prettier) node module behind the scenes to format any JavaScript document to its standards.

For example, take the following code:

```js
foo(arg1, arg2, arg3, arg4);
```

That looks like the right way to format it. However, we've all run
into this situation:

```js
foo(reallyLongArg(), omgSoManyParameters(), IShouldRefactorThis(), isThereSeriouslyAnotherOne());
```

Suddenly our previous format for calling function breaks down because
this is too long. What you would probably do is this instead:

```js
foo(
  reallyLongArg(),
  omgSoManyParameters(),
  IShouldRefactorThis(),
  isThereSeriouslyAnotherOne()
);
```

Invoke the command from the context menu in the JavaScript editor.

![Context Menu](art/context-menu.png)

## Contribute
Check out the [contribution guidelines](.github/CONTRIBUTING.md)
if you want to contribute to this project.

For cloning and building this project yourself, make sure
to install the
[Extensibility Tools 2015](https://visualstudiogallery.msdn.microsoft.com/ab39a092-1343-46e2-b0f1-6a3f91155aa6)
extension for Visual Studio which enables some features
used by this project.

## License
[Apache 2.0](LICENSE)