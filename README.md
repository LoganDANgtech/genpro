Génération Procédurale
---
Ce Projet Contient une génération de Grid et 4 Logique de génération procédurales implémentée dans Unity
---
## Table of Content

<details>
<summary>Liens</summary>

  - 1. [Getting Started](#1-getting-started)
  - 2. [Simple Room Placement](#2-simple-room-placement)
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
Le Simple Room Placement est un algorithme assez basique de placement de salles qui instantie des rectangle dans l'espace avec des tailles minimum est maximum

Il peut etre utile pour générer un Donjon
<img src="Documentation/SRP.png?raw=true"/>
---
## 3. BSP (Binary Space Partition)
Le BSP est un algorithme qui place des salles dans l'espace aussi mais de manière plus intélligente en coupant un espace en 2 parcelles aléatoirement proportionées qui seront à leur tour divisées en 2 jusqu'a que l'espace disponible ne puisse plus contenir une salle.
Puis des salles sont générées aléatoirement dans les espace dédiés,
et des couloirs sont créés entre les salles reliées dans des parcelles puis entre salles reliées entre les parents de leur parcelles

Il peut etre plus utile que le SRP pour générer un Donjon
<img src="Documentation/BSP.png?raw=true"/>
--
