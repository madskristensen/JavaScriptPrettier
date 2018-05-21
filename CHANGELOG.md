# Road map

- [ ] Document code in comments
- [ ] Move strings to resource file
- [ ] Execute on document save
- [ ] Execute on document format
- [ ] Use your own version of Prettier

Features that have a checkmark are complete and available for
download in the
[CI build](http://vsixgallery.com/extension/J1da7ad9e-85b3-4a0c-8e45-b2ae59a575a7/).

# Change log

These are the changes to each version that has been released
on the official Visual Studio extension gallery.

## 2.0
- [x] Updated tp prettier@1.12.1
- [x] Ability to read and use the [prettier configuration](https://prettier.io/docs/en/configuration.html) specified in your project
- [x] Tries to maintain your scroll position when formatting
- [x] Disables the Visual Studio Formatting after using Prettier - so make your results consistent.

## 1.1

- [x] Updated to prettier@1.4.2
- [x] Support for TypeScript (.ts and .tsx)

## 1.0

- [x] Added UTF-8 encoding support
- [x] Upgraded to [prettier](https://github.com/jlongster/prettier) version 1.0.2

## 0.6

- [x] Call *FormatDocument* command after run
- [x] New logo and command icon
- [x] Scoped keyboard shortcut to JS editor

## 0.5

- [x] Initial release
- [x] Install npm modules
- [x] Command on JavaScript context menu
- [x] Updated readme.md