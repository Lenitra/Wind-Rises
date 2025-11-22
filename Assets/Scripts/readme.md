# ✈️ Wind Rises - Architecture du Code

## Vue d'ensemble

Wind Rises est un jeu d'avion open world cozy construit avec une architecture modulaire et événementielle. Le code est organisé en 6 systèmes indépendants qui communiquent via un **EventBus** central, garantissant un faible couplage et une maintenabilité élevée.

## Principes d'architecture

- **Séparation des responsabilités** : Chaque module a une responsabilité unique et bien définie
- **Communication événementielle** : Les systèmes communiquent exclusivement par événements, évitant les dépendances directes
- **Modularité** : Les systèmes peuvent être développés, testés et modifiés indépendamment
- **Scalabilité** : Facilité d'ajout de nouvelles fonctionnalités sans affecter le code existant

## Vue d'ensemble des systèmes

| Système         | Responsabilité                                  | Composants clés                                    |
| --------------- | ----------------------------------------------- | -------------------------------------------------- |
| **Core**        | Coordination centrale et gestion des événements | GameManager, EventBus                              |
| **Flight**      | Contrôles de l'avion et simulation physique     | PlaneInput, FlightController, PlaneData            |
| **Camera**      | Système de caméra dynamique suivant l'avion     | CameraFollow                                       |
| **Environment** | Météo, cycle jour/nuit et effets atmosphériques | WeatherManager, TimeOfDay                          |
| **Gameplay**    | Gestion des défis, collectibles et événements   | GameplayManager, Collectible, RaceChallenge        |
| **UI**          | Affichage tête haute et interface utilisateur   | UIManager, SpeedUI, AltitudeUI, GameplayUI         |

## Structure du projet

```
Scripts/
├─ Core/
│  ├─ GameManager.cs        # Contrôleur principal du jeu
│  └─ EventBus.cs           # Système d'événements pour la communication inter-modules
│
├─ Flight/
│  ├─ PlaneInput.cs         # Gestion des entrées (clavier, manette, etc.)
│  ├─ FlightController.cs   # Simulation physique et dynamique de vol
│  └─ PlaneData.cs          # Configuration et paramètres de l'avion
│
├─ Camera/
│  └─ CameraFollow.cs       # Système de suivi de caméra fluide
│
├─ Environment/
│  ├─ WeatherManager.cs     # Système météo et conditions
│  └─ TimeOfDay.cs          # Cycle jour/nuit et éclairage
│
├─ Gameplay/
│  ├─ GameplayManager.cs    # Gestionnaire central des défis
│  ├─ GameplayData.cs       # Données et configuration des défis
│  ├─ Collectibles/
│  │  ├─ Collectible.cs     # Classe de base pour les collectibles
│  │  ├─ CollectibleTrigger.cs  # Détection de collecte
│  │  └─ CollectibleSpawner.cs  # Placement des collectibles
│  ├─ Races/
│  │  ├─ RaceChallenge.cs   # Logique de course
│  │  ├─ Checkpoint.cs      # Points de passage
│  │  └─ RaceTimer.cs       # Chronométrage
│  └─ Events/
│     ├─ WorldEvent.cs      # Événements mondiaux
│     └─ EventTrigger.cs    # Déclencheurs d'événements
│
└─ UI/
   ├─ UIManager.cs          # Coordination de l'interface
   ├─ SpeedUI.cs            # Affichage de l'indicateur de vitesse
   ├─ AltitudeUI.cs         # Affichage de l'indicateur d'altitude
   └─ GameplayUI.cs         # Interface des défis (objectifs, progression)
```

## Flux de données

```
Entrée utilisateur
    ↓
PlaneInput
    ↓
FlightController ──→ EventBus ──→ GameplayManager (PositionUpdated, SpeedChanged)
    ↓                    ↓
    ↓                    ├──→ UI (SpeedChanged, AltitudeChanged, GameplayProgress)
    ↓                    └──→ CameraFollow
    ↓
Collectible/Checkpoint (détection de collision)
    ↓
EventBus (CollectibleCollected, CheckpointReached)
    ↓
GameplayManager → EventBus → UI
```

## Systèmes principaux

### Système de vol (Flight)

Gère toutes les fonctionnalités liées à l'avion :

- **PlaneInput** : Capture et traite les entrées du joueur depuis diverses sources
- **FlightController** : Implémente la physique et la dynamique de vol réaliste
- **PlaneData** : Stocke les spécifications de l'avion (vitesse max, taux de virage, etc.)

### Système EventBus

Système nerveux central de l'application :

- Permet une communication découplée entre les modules
- Les événements sont fortement typés et immuables
- Événements courants :
  - `SpeedChanged` : Déclenché quand la vitesse de l'avion change
  - `AltitudeChanged` : Déclenché quand l'altitude de l'avion change
  - `PositionUpdated` : Déclenché pour le suivi de la caméra
  - `CollectibleCollected` : Déclenché lors de la collecte d'un objet
  - `CheckpointReached` : Déclenché au passage d'un checkpoint
  - `ChallengeStarted` / `ChallengeCompleted` : Début et fin d'un défi
  - `RaceFinished` : Fin d'une course avec le temps
  - `EventTriggered` : Événement mondial déclenché

### Système de caméra (Camera)

Offre une expérience cinématographique :

- Suivi fluide avec décalage configurable
- Répond à la position/orientation de l'avion via EventBus
- Aucune référence directe à l'objet avion

### Système d'environnement (Environment)

Crée l'immersion atmosphérique :

- Conditions météorologiques dynamiques
- Cycle jour/nuit avec transitions d'éclairage réalistes
- Effets audio et visuels environnementaux

### Système de gameplay (Gameplay)

Gère toute la logique de gameplay et des défis :

- **GameplayManager** :
  - Coordonne tous les défis actifs
  - Écoute les événements de collecte/checkpoints via EventBus
  - Gère la progression et la complétion des défis
  - Émet des événements pour notifier l'UI

- **Collectibles** :
  - `Collectible.cs` : Classe de base (position, type, récompense)
  - `CollectibleTrigger.cs` : Détecte la collision avec l'avion et émet `CollectibleCollected`
  - `CollectibleSpawner.cs` : Place les collectibles dans le monde

- **Races** :
  - `RaceChallenge.cs` : Logique de course (démarrage, validation du parcours)
  - `Checkpoint.cs` : Valide le passage et émet `CheckpointReached`
  - `RaceTimer.cs` : Chronomètre et gestion du temps

- **Events** :
  - `WorldEvent.cs` : Événements scriptés (apparition d'objets, changements météo)
  - `EventTrigger.cs` : Zones déclencheuses basées sur la position de l'avion

**Principe clé** : Le système Gameplay ne référence jamais directement l'avion. Il écoute les événements `PositionUpdated`, `SpeedChanged` pour détecter les conditions de défi.

### Système d'interface (UI)

Affiche les informations de vol et de défis :

- Écoute l'EventBus pour des mises à jour en temps réel
- Les éléments HUD se mettent à jour indépendamment
- **GameplayUI** : Affiche les objectifs actifs, la progression, les notifications
- Séparation nette avec la logique de jeu

## Lignes directrices de développement

### Ajouter de nouvelles fonctionnalités

1. Identifier à quel système appartient la fonctionnalité
2. Créer des événements pour toute donnée devant être partagée
3. Utiliser l'EventBus pour toute communication inter-système
4. Garder les systèmes indépendants

### Convention de nommage des événements

- Utiliser le passé composé : `SpeedChanged`, `LandingCompleted`
- Être spécifique : `EngineStarted` plutôt que `StateChanged`

### Organisation du code

- Une classe par fichier
- Garder les scripts focalisés sur une seule responsabilité
- Utiliser des namespaces pour organiser les classes liées

## Exemple d'implémentation : Course avec checkpoints

```
1. L'avion vole et émet PositionUpdated via EventBus
2. Checkpoint écoute PositionUpdated, détecte la proximité
3. Checkpoint émet CheckpointReached(checkpointId, timestamp)
4. RaceChallenge écoute CheckpointReached
   - Valide l'ordre des checkpoints
   - Si dernier checkpoint → émet RaceFinished(totalTime)
5. GameplayManager écoute RaceFinished
   - Met à jour la progression
   - Émet GameplayCompleted
6. UI écoute GameplayCompleted et affiche la victoire
```

## Points importants pour les défis

### Détection de collision/proximité

Les `Collectible` et `Checkpoint` utilisent :
- **Trigger Colliders** sur les GameObjects dans le monde
- **OnTriggerEnter** détecte l'avion (vérifie le tag "Player")
- Émet immédiatement un événement via EventBus

### Gestion de la progression

Le `GameplayManager` maintient :
- Liste des défis actifs
- État de progression de chaque défi
- Sauvegarde automatique via événements

### Spawn dynamique

Le `CollectibleSpawner` peut :
- Placer des collectibles à des positions fixes (niveau design)
- Générer procéduralement des collectibles
- Réagir aux événements météo pour modifier le placement

## Améliorations futures

- **Système audio** : Sons de moteur, effets de vent, notification de défi
- **Système de sauvegarde** : Progression des défis et statistiques
- **Personnalisation** : Apparences d'avion et modifications
- **Leaderboards** : Meilleurs temps de course
- **Mission narrative** : Chaînes de défis avec histoire
