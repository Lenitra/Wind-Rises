# Flight Controller - Documentation

## Vue d'ensemble

Le système de vol de Wind Rises est conçu pour offrir une expérience arcade accessible tout en conservant des éléments de réalisme physique. L'avion répond de manière intuitive aux commandes tout en simulant des forces physiques comme la gravité et la résistance de l'air.

---

## Contrôles

### Clavier
- **W/S** (ou **Flèches Haut/Bas**) : Tangage (pitch) - Monter/Descendre
- **A/D** (ou **Flèches Gauche/Droite**) : Roulis (roll) - S'incliner et tourner
- **Espace** : Accélération
- **Shift Gauche** : Freinage
- **Q/E** : Lacet (yaw) manuel - *Non utilisé par défaut*

### Manette
- **Stick Gauche** : Tangage et Roulis
- **R2** : Accélération
- **L2** : Freinage

---

## Comportement de vol

### 1. Vitesse et Accélération

#### Throttle (Gaz)
- **Accélération** : Augmente la vitesse jusqu'à `maxSpeed`
- **Freinage** : Réduit la vitesse activement
- **Limite** : Le throttle seul ne peut pas dépasser `maxSpeed`

#### Gravité
- **En montée** (pitch positif) : Ralentit progressivement l'avion
- **En descente** (pitch négatif) : Accélère l'avion, **peut dépasser `maxSpeed`**
- **Vol horizontal** : Pas d'effet
- **Limite basse** : Ne peut pas faire descendre sous `minSpeed`

#### Frottement de l'air (Linear Drag)
- Ralentit constamment l'avion proportionnellement à sa vitesse
- Plus l'avion va vite, plus le frottement est important
- Plafonné à 100% à `maxSpeed`
- Ne peut pas faire descendre sous `minSpeed`

#### Interaction des forces
En piqué avec le throttle à fond :
1. Le **throttle** s'arrête d'accélérer à `maxSpeed`
2. La **gravité** continue d'accélérer au-delà de `maxSpeed`
3. Le **drag** ralentit proportionnellement à la vitesse
4. Résultat : L'avion peut atteindre des vitesses très élevées en piqué prolongé

---

### 2. Rotations

#### Pitch (Tangage)
- Contrôlé directement par **W/S** (ou stick vertical)
- Fait pivoter l'avion vers le haut ou le bas
- Influence l'effet de gravité sur la vitesse

#### Roll (Roulis)
- Contrôlé directement par **A/D** (ou stick horizontal)
- L'avion s'incline visuellement à gauche ou à droite
- **Stabilisation automatique** : Retourne progressivement à l'horizontale quand on relâche les commandes
  - Force de stabilisation contrôlée par `rollStabilizationForce`

#### Yaw (Lacet) Automatique
- **Généré automatiquement par le roll**, pas de contrôle direct
- L'avion tourne dans la direction de son inclinaison
- **Proportionnel au roll** :
  - À **0°** ou **180°** (horizontal) → Pas de virage
  - À **±90°** (incliné au max) → Virage maximum
  - Utilise une fonction sinusoïdale pour une transition naturelle
- Force contrôlée par `rollTurnInfluence`
- Le virage s'effectue **autour de l'axe vertical global** (reste parallèle au sol)

---

### 3. Mouvement

L'avion se déplace toujours dans sa direction **forward** (avant) :
```
Vélocité = transform.forward × vitesse_actuelle
```

Le mouvement suit exactement l'orientation de l'avion, quelle que soit son inclinaison.

---

## Paramètres configurables (PlaneData)

### Vitesse
| Paramètre | Description | Valeur par défaut |
|-----------|-------------|-------------------|
| `minSpeed` | Vitesse minimale de l'avion | 5 m/s |
| `maxSpeed` | Vitesse maximale (throttle seul) | 30 m/s |
| `acceleration` | Vitesse d'accélération | 3 m/s² |
| `deceleration` | Vitesse de freinage | 7.5 m/s² |

### Contrôles
| Paramètre | Description | Valeur par défaut |
|-----------|-------------|-------------------|
| `pitchSpeed` | Vitesse de rotation en tangage | 80°/s |
| `rollSpeed` | Vitesse de rotation en roulis | 120°/s |

### Stabilisation
| Paramètre | Description | Valeur par défaut |
|-----------|-------------|-------------------|
| `rollStabilizationForce` | Force de retour automatique à l'horizontal | 0.5 |
| `rollTurnInfluence` | Force de virage basée sur le roll | 30°/s |

### Physique
| Paramètre | Description | Valeur par défaut |
|-----------|-------------|-------------------|
| `linearDrag` | Frottement de l'air (résistance) | 0.5 |
| `gravityInfluence` | Force de la gravité sur la vitesse | 10 |
| `turbulenceIntensity` | Intensité des turbulences *(à implémenter)* | 0 |
| `turbulenceFrequency` | Fréquence des turbulences *(à implémenter)* | 1 |

---

## Système de physique

### Rigidbody
- **Interpolation** : Activée pour éliminer les saccades visuelles
- **Gravité Unity** : Désactivée (gérée manuellement)
- **Contrôle** : Via `rb.MoveRotation()` pour les rotations
- **Mouvement** : Via `rb.linearVelocity` pour le déplacement

### Ordre d'exécution (FixedUpdate)
1. **HandleSpeed()** : Application du throttle
2. **HandleGravity()** : Effet de la gravité selon l'inclinaison
3. **HandleDrag()** : Frottement de l'air
4. **HandleRotation()** : Pitch, Roll, Yaw
5. **ApplyMovement()** : Application de la vélocité finale

---

## Debug UI

Panneau affiché en haut à gauche quand `showDebugInfo` est activé :

```
=== FLIGHT DEBUG ===
Speed: 25.3 m/s (91 km/h)
Throttle: 1.00
Roll: -15.2°
Pitch: 8.7°
Yaw: 145.3°
Altitude: 127.5 m
```



---

## Architecture du code

### FlightController.cs
Script principal qui gère tout le comportement de vol.

**Méthodes principales** :
- `HandleSpeed()` : Gestion du throttle
- `HandleGravity()` : Simulation de la gravité
- `HandleDrag()` : Résistance de l'air
- `HandleRotation()` : Pitch, Roll, Yaw
- `ApplyMovement()` : Application finale de la vélocité

### PlaneData.cs
ScriptableObject contenant tous les paramètres configurables. Permet de créer différents types d'avions avec des comportements variés.

### PlaneInput.cs
Gère les entrées clavier et manette via le nouveau Input System de Unity.

---

## Conseils de tuning

### Pour un avion plus arcade
- ↑ `rollSpeed` : Virages plus réactifs
- ↑ `rollTurnInfluence` : Virages plus serrés
- ↑ `rollStabilizationForce` : Retour à l'horizontal plus rapide
- ↓ `gravityInfluence` : Moins d'impact de la gravité

### Pour un avion plus simulation
- ↓ `rollSpeed` : Virages plus lents
- ↓ `rollTurnInfluence` : Virages plus larges
- ↓ `rollStabilizationForce` : L'avion garde son inclinaison
- ↑ `gravityInfluence` : Gravité plus réaliste
- ↑ `linearDrag` : Plus de résistance de l'air

### Pour un avion rapide
- ↑ `maxSpeed` : Vitesse de pointe plus élevée
- ↑ `acceleration` : Atteint la vitesse max plus vite
- ↓ `linearDrag` : Moins de frottement

### Pour un avion lourd
- ↓ `pitchSpeed` + `rollSpeed` : Réactions plus lentes
- ↑ `gravityInfluence` : Perd plus de vitesse en montée
- ↓ `rollStabilizationForce` : Se stabilise moins vite

---

## Notes techniques

### Référentiels de rotation
Le système utilise deux référentiels différents :

1. **Référentiel local** (pitch + roll) :
   - Les rotations s'appliquent autour des axes de l'avion
   - Permet à l'avion de s'incliner et monter/descendre naturellement

2. **Référentiel monde** (yaw automatique) :
   - La rotation s'applique autour de l'axe Y global (vertical)
   - Garantit que les virages restent horizontaux par rapport au sol

Cette approche hybride crée un comportement arcade intuitif tout en conservant un aspect visuel réaliste.

### Formule du yaw automatique
```csharp
float rollFactor = sin(|roll|)
float yaw = -(roll / 90°) × rollFactor × rollTurnInfluence
```

Le facteur sinusoïdal crée une courbe naturelle :
- Maximum à ±90° (avion sur le flanc)
- Minimum à 0° et 180° (avion à plat ou retourné)

---

## Évolutions futures

- [ ] Système de turbulences atmosphériques
- [ ] Effets de décrochage (stall) à basse vitesse
- [ ] Système de dégâts et résistance structurelle
- [ ] Effets sonores réactifs à la vitesse et aux manœuvres
- [ ] Trainées visuelles et effets de particules
