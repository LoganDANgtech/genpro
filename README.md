Génération Procédurale
---
Ce Projet Contient une génération de Grid et 4 Logique de génération procédurales implémentée dans Unity
---
## Table of Content

<details>
<summary>Liens</summary>

  - 1. [Getting Started](#1-getting-started)
  - 2. [Simple Room Placement](#SimpleRoomPlacement)
  - 3. [BSP](#BSP)
  - 4. [Cellular Automata](#CellularAutomata)
  - 5. [Noise Generator](#NoiseGenerator)

</details>

## 1. Getting started

Comment installer le Grid:

- Créer un projet Unity 6
- Installer Unitask avec le l'url [https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask] Dans l'Asset Manager

- (Créer une scene) et y Rajouter le ```Procedural Grid Generator```
<img src="Documentation/GridInInspector.png?raw=true"/>

Dériver de ```ProceduralGenerationMethod```
```
namespace Components.ProceduralGeneration.[Nom de l'algo]
{
    [CreateAssetMenu(menuName = "Procedural Generation Method/[Nom de l'algo]")]
    public class [Nom de l'algo] : ProceduralGenerationMethod
    {
```
Il vous faudra rentrer votre logique dans cette fonction :  
```
protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
{
    //Définition des variables

    // Check for cancellation
    cancellationToken.ThrowIfCancellationRequested();

    // Logique du code

    // Waiting between steps to see the result.
    await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
}
```
Créer une Procedural Generation method du script et ensuite la mettre dans Generation Method du Grid Générator

---
## 2. Simple Room Placement
---
