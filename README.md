Génération Procédurale
---
Ce Projet Contient une génération de Grid et 4 Logique de génération procédurales implémentée dans Unity

Réalisé lors de l'intervetion de RUTKOWSKI Yona a Gaming Campus pour la classe G.Tech 3
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

---
## 3. Cellular Automata
Le Cellular Automata (Utilisé dans le jeu de la vie) est un algorithme qui suit une logique assez simple appliqué a une grande échèle
regarder les entourages de ma case, et si X case sont d'un type ma case deviendras du type
dans ce cas si, si ma case est entouré de X case d'herbe la case deviendras (ou resteras) de l'herbe



(pour des raison de performance la génération basique de grid a été modifié pour changer le sprite et la template de la cell plutot que d'insantier un Gameobject)


L'algorithme peut etre utile pour de la génération de terrain basique
<img src="Documentation/CA.png?raw=true"/>
---
## 4. Noise Generator
Le noise generator utilise FastNoiseLite pour générer des bruits qui vont ensuite être utilisé dans un code pour générer un terrain en fonction de la valeur des pixel de "L'image" généré

Dans le projet un script customise aussi l'inspector pour permettre de choisir des valeures plus visuellement qu'avec des sliders
<img src="Documentation/FNLEditor.png?raw=true"/>

L'algorithme peut etre utile pour de la génération de terrain
<img src="Documentation/FNL.png?raw=true"/>
---

Merci a Yona RUTKOWSKI pour ses cours biens structurées qui viennent de mettre utile pour réaliser [Planet Flipper](https://github.com/Aggresive-Gamblers/PlanetFlipper) et qui j'en suis sur me serons utile pour mes futurs projet procéduraux
---
