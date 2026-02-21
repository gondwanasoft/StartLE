# StartLE

*A low-effort start menu for Microsoft Windows.*

## Description

*StartLE* is an adjunct to the Windows start menu. It adds an icon to the taskbar's system tray (notification area). Left-clicking or touching *StartLE*'s icon displays a simple menu of apps and/or documents. Selecting an item from the menu launches it. You can customise the menu to include any items that you want.

*StartLE* doesn't replace the Windows start menu. It can be useful to launch commonly-used apps and documents without having to see the Windows desktop or search in Windows' start menu.

## Advantages

* *StartLE* is simple and unobtrusive.
* You have complete control over the structure and contents of the menu; it won't be changed by Windows or apps.
* You are not limited to only two levels of hierarchy; *i.e.*, you can use as many levels of sub-menu as you like.

## Disadvantages

* Customising *StartLE*'s menu requires editing an [XML](https://en.wikipedia.org/wiki/XML) file.
* If you install or uninstall apps, the menu won't automatically add or remove relevant items.
* Icons and accelerator keys aren't currently supported.

## Download

* Microsoft Store (coming soon – maybe)
* [github releases](https://github.com/gondwanasoft/StartLE/releases)

## Installation

If you obtain an installation package from here, unzip it and run `Install.ps1`. This will guide you through installation of the security certificate, which is used to verify that the app comes from its original creator (Gondwana Software). If you don't trust this process, install the Microsoft Store version instead.

> [!NOTE]
> You may need to adjust Windows Settings (`System > Advanced > Terminal > PowerShell`) to allow `Install.ps1` to run.

## Editing the Menu

