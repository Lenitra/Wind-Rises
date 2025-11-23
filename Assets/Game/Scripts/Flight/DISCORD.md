## Comportement de vol

### 1. Vitesse et Accélération

#### Throttle (Gaz)
- **Accélération** : Augmente la vitesse jusqu'à `maxSpeed`
- **Freinage** : Réduit la vitesse activement
- **Limite** : Le throttle seul ne peut pas dépasser `maxSpeed`

#### Gravité
- **En montée** (pitch positif) : Ralentit progressivement l'avion
- **En descente** (pitch négatif) : Accélère l'avion, **peut dépasser `maxSpeed`**
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