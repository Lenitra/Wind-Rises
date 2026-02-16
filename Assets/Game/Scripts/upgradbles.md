# Plan d'amélioration - Wind Rises

## 📋 Vue d'ensemble du projet

### Architecture actuelle
```
Assets/Game/Scripts/
├── Camera/          → FollowCam (suivi caméra)
├── Core/            → EventBus + GameEvents
├── Environement/    → TerrainCollisions (crash detection)
└── Flight/          → FlightController, FlightRecorder, PlaneInput, PlaneData
```

---

## 🎯 Systèmes à améliorer/ajouter

### 1. ⭐ Système de progression & upgrades (PRIORITÉ HAUTE)

#### Problème actuel
- Aucun système de progression
- PlaneData est statique (pas d'évolution)
- Pas de mécanisme de déblocage/amélioration

#### Améliorations proposées
- [ ] **UpgradeSystem.cs** : Gestionnaire central des upgrades
  - Catalogue d'upgrades disponibles (ScriptableObject)
  - Sauvegarde de la progression
  - Application dynamique sur PlaneData

- [ ] **PlayerProgression.cs** : Suivi de la progression du joueur
  - XP/Level system
  - Monnaie virtuelle (collecte en vol)
  - Déblocage de zones/avions

- [ ] **Upgrades disponibles** :
  - Moteur : `maxSpeed`, `acceleration`
  - Aérodynamisme : `maneuverability`, `linearDrag`
  - Stabilité : `rollStabilization`, `gravityInfluence`
  - Système de fuel (nouveau)
  - Système de boost (nouveau)

**Fichiers à créer** :
- `Core/UpgradeSystem.cs`
- `Core/PlayerProgression.cs`
- `Core/UpgradeData.cs` (ScriptableObject)
- `Flight/PlaneUpgradeApplier.cs`

---

### 2. 💾 Système de sauvegarde (PRIORITÉ HAUTE)

#### Problème actuel
- Aucune persistance des données
- Progression perdue à chaque redémarrage

#### Améliorations proposées
- [ ] **SaveSystem.cs** : Gestionnaire de sauvegarde
  - Sauvegarde JSON locale
  - Chiffrement basique (optionnel)
  - Auto-save périodique

- [ ] **SaveData.cs** : Structure de données
  - Progression joueur
  - Upgrades achetés
  - Meilleurs temps/scores
  - Settings

**Fichiers à créer** :
- `Core/SaveSystem.cs`
- `Core/SaveData.cs`

---

### 3. 🎮 Interface utilisateur (PRIORITÉ HAUTE)

#### Problème actuel
- UI debug basique uniquement (OnGUI)
- Pas de HUD immersif
- Pas de menu d'upgrades

#### Améliorations proposées
- [ ] **HUD** :
  - Vitesse/Altitude/Fuel visuels
  - Mini-carte/boussole
  - Indicateur d'objectif

- [ ] **Menu Upgrades** :
  - Shop d'améliorations
  - Aperçu des stats
  - Comparaison avant/après

- [ ] **Menu Pause** :
  - Continuer/Restart/Options
  - Keybindings customisables

**Dossiers à créer** :
- `UI/HUD/`
- `UI/Menus/`
- `UI/Components/`

---

### 4. 🎨 Feedbacks visuels & audio (PRIORITÉ MOYENNE)

#### Problème actuel
- Pas de feedback audio
- Pas d'effets visuels (particules, trails)
- EventBus défini mais peu exploité

#### Améliorations proposées
- [ ] **AudioManager.cs** : Gestion centralisée du son
  - Musique adaptative (vitesse, altitude)
  - SFX : moteur, vent, crash, collectibles

- [ ] **VFXManager.cs** : Effets visuels
  - Trails d'avion (couleur selon vitesse)
  - Particules crash
  - Effets de vitesse (motion blur, FOV)

- [ ] **CameraShake.cs** : Extension de FollowCam
  - Shake lors de crash/turbulences
  - Smooth zoom selon vitesse

**Fichiers à créer** :
- `Audio/AudioManager.cs`
- `VFX/VFXManager.cs`
- `Camera/CameraShake.cs` (extension de FollowCam)

---

### 5. 🌍 Environnement & gameplay (PRIORITÉ MOYENNE)

#### Problème actuel
- EventBus définit `WeatherChanged`, `TimeOfDayChanged` mais non utilisés
- TerrainCollisions uniquement
- Pas de système de collectibles actif

#### Améliorations proposées
- [ ] **WeatherSystem.cs** : Météo dynamique
  - Vent qui affecte le vol
  - Turbulences (déjà paramétré dans PlaneData mais non utilisé)
  - Visibilité réduite (brouillard)

- [ ] **DayNightCycle.cs** : Cycle jour/nuit
  - Changement de lighting progressif
  - Difficulté variable (nuit = plus dur)

- [ ] **CollectibleSystem.cs** : Objets à collecter en vol
  - Pièces/énergie
  - Checkpoints (déjà défini dans GameEvents)
  - Power-ups temporaires

- [ ] **ObstacleSpawner.cs** : Obstacles dynamiques
  - Oiseaux, ballons, cerfs-volants
  - Zones dangereuses

**Fichiers à créer** :
- `Environment/WeatherSystem.cs`
- `Environment/DayNightCycle.cs`
- `Gameplay/CollectibleSystem.cs`
- `Gameplay/ObstacleSpawner.cs`

---

### 6. 🔧 Refactoring du FlightController (PRIORITÉ BASSE)

#### Problème actuel
- FlightController monolithique (~250 lignes)
- Mélange physique + inputs + debug
- Yaw input défini mais jamais utilisé dans HandleRotation()

#### Améliorations proposées
- [ ] **Séparer les responsabilités** :
  - `FlightPhysics.cs` : Gestion physique pure (speed, gravity, drag)
  - `FlightManeuvers.cs` : Rotations (pitch, roll, yaw)
  - `FlightController.cs` : Orchestration uniquement

- [ ] **Ajouter le contrôle Yaw** :
  - PlaneInput lit déjà Q/E mais c'est ignoré
  - Ajouter un paramètre `yawSpeed` dans PlaneData
  - Implémenter dans HandleRotation()

- [ ] **Système de fuel** :
  - Nouveau paramètre dans PlaneData
  - Consommation variable selon throttle
  - Event `FuelDepleted` dans GameEvents

**Fichiers à refactorer/créer** :
- Refactor : `Flight/FlightController.cs`
- Créer : `Flight/FlightPhysics.cs`
- Créer : `Flight/FlightManeuvers.cs`
- Modifier : `Flight/PlaneData.cs` (ajouter yawSpeed, fuelCapacity)

---

### 7. 📊 Système de missions & scoring (PRIORITÉ BASSE)

#### Problème actuel
- GameEvents définit `CheckpointReached`, `RaceFinished` mais non utilisés
- Pas de boucle de gameplay claire

#### Améliorations proposées
- [ ] **MissionSystem.cs** : Missions variées
  - Course contre la montre
  - Collecte d'objets
  - Acrobaties (looping, tonneau)

- [ ] **ScoreManager.cs** : Calcul de score
  - Temps de vol
  - Collectibles
  - Multiplicateur combo

- [ ] **LeaderboardSystem.cs** : Classements
  - Local leaderboard
  - Online (optionnel avec backend)

**Fichiers à créer** :
- `Gameplay/MissionSystem.cs`
- `Gameplay/ScoreManager.cs`
- `Gameplay/LeaderboardSystem.cs`
- `Core/MissionData.cs` (ScriptableObject)

---

## 🚀 Roadmap de développement suggérée

### Phase 1 : Fondations (Sprint 1-2)
1. Système de sauvegarde
2. Système d'upgrades (base)
3. PlayerProgression

### Phase 2 : Gameplay (Sprint 3-4)
4. CollectibleSystem
5. HUD basique
6. Menu Upgrades

### Phase 3 : Immersion (Sprint 5-6)
7. AudioManager + SFX de base
8. VFXManager + particules
9. WeatherSystem

### Phase 4 : Contenu (Sprint 7-8)
10. MissionSystem
11. ScoreManager
12. DayNightCycle

### Phase 5 : Polish (Sprint 9-10)
13. Refactoring FlightController
14. CameraShake & effets caméra
15. Leaderboard

---

## 📝 Notes techniques importantes

### EventBus - Opportunités non exploitées
L'EventBus existe mais est sous-utilisé. Voici les événements à exploiter :

**Déjà définis mais non publiés** :
- `CollectibleCollected` → À utiliser avec CollectibleSystem
- `CheckpointReached` → À utiliser avec MissionSystem
- `RaceFinished` → À utiliser avec MissionSystem
- `WeatherChanged` → À utiliser avec WeatherSystem
- `TimeOfDayChanged` → À utiliser avec DayNightCycle

**À ajouter** :
```csharp
public struct UpgradePurchased { string UpgradeId; int Cost; }
public struct FuelDepleted { float TimeToRefuel; }
public struct SpeedMilestoneReached { float Speed; }
public struct AltitudeMilestoneReached { float Altitude; }
```

### PlaneData - Extensions suggérées
```csharp
[Header("Fuel & Energy")]
public float fuelCapacity = 100f;
public float fuelConsumptionRate = 1f;
public float boostMultiplier = 2f;
public float boostDuration = 3f;

[Header("Advanced Controls")]
public float yawSpeed = 40f; // Actuellement manquant !

[Header("Damage & Durability")]
public float maxHealth = 100f;
public float crashDamage = 25f;
```

---

## 🔍 Bugs/Problèmes identifiés

### Bugs mineurs
1. **PlaneInput.cs:122-136** : Le Yaw input est lu mais jamais utilisé
   - Ligne 42 : `Yaw` est exposé
   - FlightController ne l'utilise jamais
   - **Fix** : Implémenter dans HandleRotation() ou supprimer

2. **TerrainCollisions.cs:39-40** : Variables statiques pour état partagé
   - Peut causer des problèmes avec plusieurs terrains
   - **Fix** : Singleton pattern ou instance unique garantie

### Améliorations de performance potentielles
1. **FlightRecorder.cs** : List<PlaneState> grandit indéfiniment si maxRecordedStates = 0
   - **Fix** : Forcer une limite raisonnable (ex: 300 max)

2. **FollowCam.cs** : Recalcule flatForward/flatRight à chaque LateUpdate
   - **Fix** : Cache si target n'a pas bougé significativement

---

## ✅ Points forts de l'architecture actuelle

1. **Séparation claire** : Namespaces bien organisés
2. **EventBus** : Bonne base pour découplage
3. **ScriptableObjects** : PlaneData permet facilement multi-avions
4. **FlightRecorder** : Système de rollback bien implémenté et innovant
5. **Documentation** : Bons commentaires/tooltips

---

## 🎓 Recommandations globales

### Architecture
- Continuer à utiliser ScriptableObjects pour data (UpgradeData, MissionData)
- Exploiter l'EventBus pour découplage UI/Gameplay
- Créer un dossier `Core/Managers/` pour les singletons (Audio, Save, VFX)

### Performance
- Pooling pour collectibles/particules
- Occlusion culling pour environnement
- LOD pour avion si modèle complexe

### Maintenabilité
- Tests unitaires pour SaveSystem
- Tests unitaires pour UpgradeSystem
- Documentation API pour systèmes complexes

---

*Dernière mise à jour : 2026-02-16*
*Analysé par : Claude Sonnet 4.5*
