# Hello SW Add-in

A minimal SOLIDWORKS add-in in C# (.NET Framework 4.8). It adds a **Hello SW Add-in** menu
with one command, **Say Hello**, which shows the active document's type, title and path.

## Requirements

- Windows with SOLIDWORKS installed (64-bit)
- Visual Studio 2022 or the .NET SDK (`dotnet build`)

## Build

```bat
dotnet build src\HelloSwAddin\HelloSwAddin.csproj
```

The project references the interop DLLs in
`C:\Program Files\SOLIDWORKS Corp\SOLIDWORKS\api\redist`. If SOLIDWORKS is installed elsewhere:

```bat
dotnet build src\HelloSwAddin\HelloSwAddin.csproj -p:SwApiDir="D:\SOLIDWORKS\api\redist"
```

## Register (load into SOLIDWORKS)

From an **Administrator** command prompt, with SOLIDWORKS closed:

```bat
scripts\register.bat          :: Debug build
scripts\register.bat Release  :: Release build
```

Start SOLIDWORKS. The add-in is listed under **Tools > Add-Ins** and loads at startup;
its menu appears once a document is open (and under **Tools** when none is).

To remove it: `scripts\unregister.bat`.

## Debugging

In Visual Studio, set the project's debug launch to start
`C:\Program Files\SOLIDWORKS Corp\SOLIDWORKS\SLDWORKS.exe`, then F5. Breakpoints in
`OnHello` are hit when you click the menu item. Rebuilding requires closing SOLIDWORKS first
because it locks the DLL.

## Where to go next

- `SwAddin.cs` → `AddCommands()`: add more `AddCommandItem2` calls (give each a new ID and
  add it to `knownIds`) with matching public callback methods.
- Toolbar/CommandManager tab: set `group.IconList` (PNG strips) and `HasToolbar = true`.
- Events (file open, save, rebuild): subscribe to `SldWorks` events in `ConnectToSW`.
- Change `AddinGuid` if you base a new add-in on this project.
