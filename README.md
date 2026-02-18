# MyDarlingIsARogueLikeCharacter

A cute roguelike visual novel game for desktop and mobile devices where you have a crazy for you lover.

## Run Guide

From the repository root:

```bash
cd RoguelikeDarling
```

### Desktop (Linux/Windows/macOS via DesktopGL)

Normal run:

```bash
dotnet run --project RoguelikeDarling.DesktopGL/RoguelikeDarling.DesktopGL.csproj
```

Collision debug mode (default debug color):

```bash
dotnet run --project RoguelikeDarling.DesktopGL/RoguelikeDarling.DesktopGL.csproj -- --debug-collision
```

Collision debug mode (custom RGB color):

```bash
dotnet run --project RoguelikeDarling.DesktopGL/RoguelikeDarling.DesktopGL.csproj -- --debug-collision 0,120,30
```

You can also pass color as `--debug-collision=0,120,30`.

### Android

Build:

```bash
dotnet build RoguelikeDarling.Android/RoguelikeDarling.Android.csproj
```

Run/deploy should be done from an IDE/device workflow (Visual Studio or VS Code + Android tooling), because Android does not use Desktop-style command-line args at app launch.

### iOS

Build:

```bash
dotnet build RoguelikeDarling.iOS/RoguelikeDarling.iOS.csproj
```

Run/deploy should be done from an IDE/device/simulator workflow on macOS, because iOS app launch does not use Desktop-style command-line args.

## Collision Debug on Mobile (Android/iOS)

`--debug-collision` is currently wired for `DesktopGL` startup args.

For Android/iOS, enable collision debug manually by passing the same values in the platform bootstrap when creating `RoguelikeDarlingGame`:

- Android: `RoguelikeDarling.Android/MainActivity.cs`
- iOS: `RoguelikeDarling.iOS/Program.cs`

Example constructor usage:

```csharp
new RoguelikeDarlingGame(isCollisionDebugEnabled: true, collisionDebugColor: new Color(0, 120, 30));
```
