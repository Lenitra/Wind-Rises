# Core - EventBus

Système de communication événementielle centralisé pour Wind Rises.

## Principe

L'EventBus permet aux différents systèmes du jeu de communiquer sans se connaître directement. C'est le système nerveux central de l'architecture.

### Avantages
- **Découplage total** : Aucun système ne référence directement un autre
- **Extensibilité** : Facile d'ajouter de nouveaux événements
- **Testabilité** : Chaque système peut être testé indépendamment
- **Performance** : Dictionnaire de delegates pour un dispatch rapide

## Utilisation basique

```csharp
using WindRises.Core;

public class ExampleScript : MonoBehaviour
{
    // S'abonner dans OnEnable
    void OnEnable()
    {
        EventBus.Instance.Subscribe<SpeedChanged>(OnSpeedChanged);
        EventBus.Instance.Subscribe<AltitudeChanged>(OnAltitudeChanged);
    }

    // TOUJOURS se désabonner dans OnDisable
    void OnDisable()
    {
        EventBus.Instance.Unsubscribe<SpeedChanged>(OnSpeedChanged);
        EventBus.Instance.Unsubscribe<AltitudeChanged>(OnAltitudeChanged);
    }

    // Callbacks
    void OnSpeedChanged(SpeedChanged evt)
    {
        Debug.Log($"Vitesse: {evt.Speed} / {evt.MaxSpeed}");
    }

    void OnAltitudeChanged(AltitudeChanged evt)
    {
        Debug.Log($"Altitude: {evt.Altitude}m");
    }

    // Publier un événement
    void Update()
    {
        EventBus.Instance.Publish(new SpeedChanged
        {
            Speed = 120f,
            MaxSpeed = 200f
        });
    }
}
```

## Événements disponibles

### Flight (Vol)

**SpeedChanged**
```csharp
EventBus.Instance.Publish(new SpeedChanged
{
    Speed = 150f,
    MaxSpeed = 200f
});
```

**AltitudeChanged**
```csharp
EventBus.Instance.Publish(new AltitudeChanged
{
    Altitude = 500f
});
```

**PositionUpdated**
```csharp
EventBus.Instance.Publish(new PositionUpdated
{
    Position = transform.position,
    Rotation = transform.rotation
});
```

### Gameplay (Défis)

**CollectibleCollected**
```csharp
EventBus.Instance.Publish(new CollectibleCollected
{
    Id = "collectible_001",
    Position = transform.position,
    Points = 100
});
```

**CheckpointReached**
```csharp
EventBus.Instance.Publish(new CheckpointReached
{
    Id = "checkpoint_1",
    Index = 0,
    Time = Time.time
});
```

**GameplayStarted / GameplayCompleted**
```csharp
EventBus.Instance.Publish(new GameplayStarted
{
    Id = "race_001",
    Type = "Race"
});

EventBus.Instance.Publish(new GameplayCompleted
{
    Id = "race_001",
    Success = true,
    Time = 45.5f,
    Score = 1000
});
```

**RaceFinished**
```csharp
EventBus.Instance.Publish(new RaceFinished
{
    Id = "race_001",
    Time = 45.5f,
    NewRecord = true
});
```

### Environment (Environnement)

**WeatherChanged**
```csharp
EventBus.Instance.Publish(new WeatherChanged
{
    Type = "Rain",
    Intensity = 0.7f
});
```

**TimeOfDayChanged**
```csharp
EventBus.Instance.Publish(new TimeOfDayChanged
{
    Time = 14.5f,  // 14h30
    IsNight = false
});
```

## Bonnes pratiques

### ✅ À faire

```csharp
// Toujours utiliser OnEnable/OnDisable
void OnEnable() => EventBus.Instance.Subscribe<SpeedChanged>(OnSpeedChanged);
void OnDisable() => EventBus.Instance.Unsubscribe<SpeedChanged>(OnSpeedChanged);
```

### ❌ À éviter

```csharp
// Ne PAS s'abonner dans Start/Awake
void Start() => EventBus.Instance.Subscribe<SpeedChanged>(OnSpeedChanged);

// Ne PAS oublier de se désabonner
// Risque de fuite mémoire et d'erreurs!
```

## Créer un nouvel événement

1. Ouvrir `GameEvents.cs`
2. Ajouter un struct dans la catégorie appropriée

```csharp
public struct MonNouvelEvenement
{
    public string Data;
    public float Value;
}
```

3. L'utiliser immédiatement

```csharp
EventBus.Instance.Subscribe<MonNouvelEvenement>(OnMonEvenement);
EventBus.Instance.Publish(new MonNouvelEvenement { Data = "test", Value = 42f });
```

## Architecture

```
FlightController
    ↓ Publish(SpeedChanged)
EventBus (dictionnaire central)
    ↓ Dispatch vers tous les abonnés
    ├─→ UIManager (affiche vitesse)
    ├─→ GameplayManager (vérifie conditions)
    └─→ AudioManager (ajuste son moteur)
```

## Debugging

Si un événement ne fonctionne pas :

1. Vérifier que `Subscribe` est appelé dans `OnEnable`
2. Vérifier que `Unsubscribe` est appelé dans `OnDisable`
3. Vérifier l'orthographe exacte du type d'événement
4. Ajouter des `Debug.Log` dans le callback pour confirmer la réception
