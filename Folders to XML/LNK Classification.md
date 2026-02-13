Ah — *that* combination (empty `TargetParsingPath`, empty `Arguments`, empty `TargetIDList`) is extremely distinctive.
It tells us you’ve hit one of the **pure virtual shell objects** where Explorer stores **only a PIDL** in the `.lnk`, and none of the property-system fields can represent it.

This is one of those corners of the Shell Link format where the Windows Property System simply gives up.

Let me walk you through what’s really happening.

---

# 🧠 Why all three fields are empty

When you see:

- `System.Link.TargetParsingPath` → `""`
- `System.Link.Arguments` → `""`
- `System.Link.TargetIDList` → `""`

…it means:

> The `.lnk` contains **only a PIDL**, and that PIDL does **not** correspond to a filesystem object **and** cannot be expressed as a GUID path.

This is typical for:

- **Run…**
- **Search…**
- **Some Control Panel items**
- **Some legacy shell verbs**
- **Certain taskbar-pinned items**
- **Deep shell namespace objects with no canonical parsing path**

These objects exist *only* as PIDLs.
Explorer knows how to interpret them, but the Property System does not.

---

# ✔️ How to detect this case reliably

At this point, the only reliable indicator is:

### **`$item.IsFileSystem` is false**

Because for any normal shortcut:

- Filesystem target → `IsFileSystem = True`
- Shell object with GUID → `Arguments` or `TargetParsingPath` contains a GUID
- Pure PIDL-only shell object → **everything empty**, but `IsFileSystem = False`

So your detection rule becomes:

```powershell
$isSpecialShell = -not $item.IsFileSystem
```

This is the simplest and most accurate test.

---

# 🧩 Why PowerShell can’t show the PIDL

The PIDL *is* present in the `.lnk`, but:

- `Shell.Application` does not expose it
- The Property System does not expose it
- `WScript.Shell` does not expose it
- PowerShell cannot marshal `IShellLinkW.GetIDList` without embedded C#

So PowerShell sees:

> “I can’t represent this PIDL as a path, GUID, or string → return empty.”

Explorer, however, reads the PIDL directly from the `.lnk` and knows exactly what to do.

---

# 🧠 What this means for your detection logic

You now have three categories of `.lnk`:

| Type of target | ParsingPath | Arguments | IDList | IsFileSystem | Meaning |
|----------------|-------------|-----------|--------|--------------|---------|
| Normal file | path | maybe args | filesystem PIDL | True | Regular shortcut |
| Shell object with GUID | GUID | or GUID | empty | False | Namespace object |
| **Pure PIDL shell object** | **empty** | **empty** | **empty** | **False** | **Special shell verb** |

Your case is the **third** one.

---

# ✔️ The cleanest PowerShell classifier

Here’s the version I’d use:

```powershell
function Get-LinkType {
    param([string]$Path)

    $shell  = New-Object -ComObject Shell.Application
    $folder = $shell.Namespace((Split-Path $Path))
    $item   = $folder.ParseName((Split-Path $Path -Leaf))

    $parsing = $item.ExtendedProperty("System.Link.TargetParsingPath")
    $args    = $item.ExtendedProperty("System.Link.Arguments")

    [PSCustomObject]@{
        Path              = $Path
        TargetParsingPath = $parsing
        Arguments         = $args
        IsFileSystem      = $item.IsFileSystem
        IsSpecialShell    = -not $item.IsFileSystem
        Classification    = if ($item.IsFileSystem) {
                                "Filesystem shortcut"
                            }
                            elseif ($parsing -match '^::\{') {
                                "Shell GUID shortcut"
                            }
                            else {
                                "Pure PIDL shell shortcut"
                            }
    }
}
```

This will correctly classify:

- Normal shortcuts
- GUID-based shell shortcuts
- Pure PIDL shortcuts (your case)

---

# 🎯 Summary

If all fields are empty:

- The `.lnk` contains a **PIDL-only** shell object
- PowerShell cannot expose the PIDL
- Explorer can interpret it
- `IsFileSystem = False` is your definitive indicator

This is exactly how the **Run…** shortcut behaves.

---

If you want, I can show you how to embed a tiny C# snippet in PowerShell to call `IShellLinkW.GetIDList` and dump the raw PIDL bytes — that’s the only way to see the actual structure.